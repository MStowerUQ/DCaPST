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
        protected AssimilationCalculator Calculator;

        protected List<Pathway> pathways;

        public double Gbs => pway.BundleSheathCO2ConductancePerLeaf * partial.LAI;
        public double Vpr => pway.PEPRegenerationPerLeaf * partial.LAI;

        public Assimilation(IPartialCanopy partial)
        {
            this.partial = partial;
            canopy = partial.Canopy;
            pway = partial.Canopy.Pathway;

            pathways = new List<Pathway>()
            {
                /*Ac1*/ new Pathway(partial) { Type = AssimilationType.Ac1 },
                /*Ac2*/ this is ParametersC3 ? null : new Pathway(partial) { Type = AssimilationType.Ac2 },
                /*Aj */ new Pathway(partial) { Type = AssimilationType.Aj }
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
        private void UpdatePathway(ITemperature temperature, WaterParameters water, Pathway path)
        {
            if (path == null) return;

            if (path.Current.Temperature == 0) path.Current.Temperature = temperature.AirTemperature;

            var leafWater = new LeafWaterInteractionModel(temperature, path.Current.Temperature, water.BoundaryHeatConductance);

            double resistance;

            PrepareCalculator(path);
            // If there is no limit on the water supply
            if (!water.limited)
            {
                path.IntercellularCO2 = pway.IntercellularToAirCO2Ratio * canopy.AirCO2;

                Calculator.p = path.IntercellularCO2;
                Calculator.q = 1 / path.Current.GmT;

                path.CO2Rate = Calculator.CalculateAssimilation();

                resistance = leafWater.UnlimitedWaterResistance(path.CO2Rate, canopy.AirCO2, path.IntercellularCO2);
                path.WaterUse = leafWater.HourlyWaterUse(resistance, partial.AbsorbedRadiation);
            }
            // If water supply is limited
            else
            {
                path.WaterUse = water.maxHourlyT * water.fraction;
                var WaterUseMolsSecond = path.WaterUse / 18 * 1000 / 3600;

                resistance = leafWater.LimitedWaterResistance(path.WaterUse, partial.AbsorbedRadiation);
                var Gt = leafWater.TotalLeafCO2Conductance(resistance);

                Calculator.p = canopy.AirCO2 - WaterUseMolsSecond * canopy.AirCO2 / (Gt + WaterUseMolsSecond / 2.0);
                Calculator.q = 1 / (Gt + WaterUseMolsSecond / 2) + 1.0 / path.Current.GmT;

                path.CO2Rate = Calculator.CalculateAssimilation();

                UpdateIntercellularCO2(path, Gt, WaterUseMolsSecond);
            }

            UpdateMesophyllCO2(path);
            UpdateChloroplasticO2(path);
            UpdateChloroplasticCO2(path);

            // New leaf temperature
            path.Current.Temperature = (leafWater.LeafTemperature(resistance, partial.AbsorbedRadiation) + path.Current.Temperature) / 2.0;

            // If the assimilation is not sensible
            if (double.IsNaN(path.CO2Rate) || path.CO2Rate <= 0.0 || double.IsNaN(path.WaterUse) || path.WaterUse <= 0.0)
            {
                path.CO2Rate = 0;
                path.WaterUse = 0;
            }
        }

        private void PrepareCalculator(Pathway path)
        {
            if (path.Type == AssimilationType.Ac1) Calculator = GetAc1Calculator(path);
            else if (path.Type == AssimilationType.Ac2) Calculator = GetAc2Calculator(path);
            else Calculator = GetAjCalculator(path);
        }

        protected virtual void UpdateIntercellularCO2(Pathway path, double gt, double waterUseMolsSecond) { /*C4 & CCM overwrite this.*/ }
        protected virtual void UpdateMesophyllCO2(Pathway path) { /*C4 & CCM overwrite this.*/ }
        protected virtual void UpdateChloroplasticO2(Pathway path) { /*CCM overwrites this.*/ }
        protected virtual void UpdateChloroplasticCO2(Pathway path) { /*CCM overwrites this.*/ }

        protected abstract AssimilationCalculator GetAc1Calculator(Pathway path);
        protected abstract AssimilationCalculator GetAc2Calculator(Pathway path);
        protected abstract AssimilationCalculator GetAjCalculator(Pathway path);
    }
}
