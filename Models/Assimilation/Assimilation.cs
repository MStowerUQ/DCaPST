using System;
using System.Collections.Generic;
using System.Linq;
using DCAPST.Environment;
using DCAPST.Interfaces;

namespace DCAPST
{
    /// <summary>
    /// Tracks the state of an assimilation type
    /// </summary>
    public abstract class Assimilation : IAssimilation
    {       
        protected IPartialCanopy partial;
        protected ICanopyParameters canopy;
        protected IPathwayParameters pway;
        protected AssimilationFunction Calculator;

        protected List<AssimilationPathway> pathways;

        public double Gbs => pway.BundleSheathCO2ConductancePerLeaf * partial.LAI;
        public double Vpr => pway.PEPRegenerationPerLeaf * partial.LAI;

        public Assimilation(IPartialCanopy partial)
        {
            this.partial = partial;
            canopy = partial.Canopy;
            pway = partial.Canopy.Pathway;

            pathways = new List<AssimilationPathway>()
            {
                /*Ac1*/ new AssimilationPathway(partial) { Type = AssimilationType.Ac1 },
                /*Ac2*/ this is ParametersC3 ? null : new AssimilationPathway(partial) { Type = AssimilationType.Ac2 },
                /*Aj */ new AssimilationPathway(partial) { Type = AssimilationType.Aj }
            };
        }
        
        public void UpdateAssimilation(ITemperature temperature, WaterParameters water)
        {
            pathways.ForEach(p => UpdatePathway(temperature, water, p));
        }

        public double GetCO2Rate() => pathways.Min(p => p.CO2Rate);

        public double GetWaterUse() => pathways.Min(p => p.WaterUse);

        /// <summary>
        /// Updates the state of the assimilation
        /// </summary>
        private void UpdatePathway(ITemperature temperature, WaterParameters water, AssimilationPathway pathway)
        {
            if (pathway == null) return;

            if (pathway.Current.Temperature == 0) pathway.Current.Temperature = temperature.AirTemperature;

            var leafWater = new LeafWaterInteractionModel(temperature, pathway.Current.Temperature, water.BoundaryHeatConductance);

            double resistance;

            PrepareCalculator(pathway);
            // If there is no limit on the water supply
            if (!water.limited)
            {
                pathway.IntercellularCO2 = pway.IntercellularToAirCO2Ratio * canopy.AirCO2;

                Calculator.Ci = pathway.IntercellularCO2;
                Calculator.Rm = 1 / pathway.Current.GmT;

                pathway.CO2Rate = Calculator.Value();

                resistance = leafWater.UnlimitedWaterResistance(pathway.CO2Rate, canopy.AirCO2, pathway.IntercellularCO2);
                pathway.WaterUse = leafWater.HourlyWaterUse(resistance, partial.AbsorbedRadiation);
            }
            // If water supply is limited
            else
            {
                pathway.WaterUse = water.maxHourlyT * water.fraction;
                var WaterUseMolsSecond = pathway.WaterUse / 18 * 1000 / 3600;

                resistance = leafWater.LimitedWaterResistance(pathway.WaterUse, partial.AbsorbedRadiation);
                var Gt = leafWater.TotalLeafCO2Conductance(resistance);

                Calculator.Ci = canopy.AirCO2 - WaterUseMolsSecond * canopy.AirCO2 / (Gt + WaterUseMolsSecond / 2.0);
                Calculator.Rm = 1 / (Gt + WaterUseMolsSecond / 2) + 1.0 / pathway.Current.GmT;

                pathway.CO2Rate = Calculator.Value();

                UpdateIntercellularCO2(pathway, Gt, WaterUseMolsSecond);
            }

            UpdateMesophyllCO2(pathway);
            UpdateChloroplasticO2(pathway);
            UpdateChloroplasticCO2(pathway);

            // New leaf temperature
            pathway.Current.Temperature = (leafWater.LeafTemperature(resistance, partial.AbsorbedRadiation) + pathway.Current.Temperature) / 2.0;

            // If the assimilation is not sensible
            if (double.IsNaN(pathway.CO2Rate) || pathway.CO2Rate <= 0.0 || double.IsNaN(pathway.WaterUse) || pathway.WaterUse <= 0.0)
            {
                pathway.CO2Rate = 0;
                pathway.WaterUse = 0;
            }
        }

        private void PrepareCalculator(AssimilationPathway path)
        {
            if (path.Type == AssimilationType.Ac1) Calculator = GetAc1Function(path);
            else if (path.Type == AssimilationType.Ac2) Calculator = GetAc2Function(path);
            else Calculator = GetAjFunction(path);
        }

        protected virtual void UpdateIntercellularCO2(AssimilationPathway path, double gt, double waterUseMolsSecond) { /*C4 & CCM overwrite this.*/ }
        protected virtual void UpdateMesophyllCO2(AssimilationPathway path) { /*C4 & CCM overwrite this.*/ }
        protected virtual void UpdateChloroplasticO2(AssimilationPathway path) { /*CCM overwrites this.*/ }
        protected virtual void UpdateChloroplasticCO2(AssimilationPathway path) { /*CCM overwrites this.*/ }

        protected abstract AssimilationFunction GetAc1Function(AssimilationPathway path);
        protected abstract AssimilationFunction GetAc2Function(AssimilationPathway path);
        protected abstract AssimilationFunction GetAjFunction(AssimilationPathway path);
    }
}
