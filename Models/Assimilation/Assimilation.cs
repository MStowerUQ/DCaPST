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
        public ILeafWaterInteraction LeafWater { get; }

        protected List<AssimilationPathway> pathways;        

        public double Gbs => pway.BundleSheathConductance * partial.LAI;
        public double Vpr => pway.PEPRegeneration * partial.LAI;

        public Assimilation(IPartialCanopy partial, ITemperature temperature)
        {
            this.partial = partial;
            canopy = partial.Canopy;
            pway = partial.Canopy.Pathway;

            pathways = new List<AssimilationPathway>()
            {
                /*Ac1*/ new AssimilationPathway(partial) { Type = AssimilationType.Ac1 },
                /*Ac2*/ this is AssimilationC3 ? null : new AssimilationPathway(partial) { Type = AssimilationType.Ac2 },
                /*Aj */ new AssimilationPathway(partial) { Type = AssimilationType.Aj }
            };
            pathways.ForEach(p => p.Leaf.Temperature = temperature.AirTemperature);

            LeafWater = new LeafWaterInteractionModel(temperature);
        }
        
        public void UpdateAssimilation(WaterParameters water) => pathways.ForEach(p => UpdatePathway(water, p));        

        public double GetCO2Rate() => pathways.Min(p => p.CO2Rate);

        public double GetWaterUse() => pathways.Min(p => p.WaterUse);

        /// <summary>
        /// Updates the state of the assimilation
        /// </summary>
        private void UpdatePathway(WaterParameters water, AssimilationPathway pathway)
        {
            if (pathway == null) return;

            LeafWater.SetConditions(pathway.Leaf.Temperature, water.BoundaryHeatConductance);

            double resistance;

            var func = GetFunction(pathway);
            if (!water.limited) /* Unlimited water calculation */
            {
                pathway.IntercellularCO2 = pway.IntercellularToAirCO2Ratio * canopy.AirCO2;

                func.Ci = pathway.IntercellularCO2;
                func.Rm = 1 / pathway.Leaf.GmT;

                pathway.CO2Rate = func.Value();

                resistance = LeafWater.UnlimitedWaterResistance(pathway.CO2Rate, canopy.AirCO2, pathway.IntercellularCO2);
                pathway.WaterUse = LeafWater.HourlyWaterUse(resistance, partial.AbsorbedRadiation);
            }
            else /* Limited water calculation */
            {
                pathway.WaterUse = water.maxHourlyT * water.fraction;
                var WaterUseMolsSecond = pathway.WaterUse / 18 * 1000 / 3600;

                resistance = LeafWater.LimitedWaterResistance(pathway.WaterUse, partial.AbsorbedRadiation);
                var Gt = LeafWater.TotalLeafCO2Conductance(resistance);

                func.Ci = canopy.AirCO2 - WaterUseMolsSecond * canopy.AirCO2 / (Gt + WaterUseMolsSecond / 2.0);
                func.Rm = 1 / (Gt + WaterUseMolsSecond / 2) + 1.0 / pathway.Leaf.GmT;

                pathway.CO2Rate = func.Value();

                UpdateIntercellularCO2(pathway, Gt, WaterUseMolsSecond);
            }

            UpdateMesophyllCO2(pathway);
            UpdateChloroplasticO2(pathway);
            UpdateChloroplasticCO2(pathway, func);

            // New leaf temperature
            pathway.Leaf.Temperature = (LeafWater.LeafTemperature(resistance, partial.AbsorbedRadiation) + pathway.Leaf.Temperature) / 2.0;

            // If the assimilation is not sensible zero the values
            if (double.IsNaN(pathway.CO2Rate) || pathway.CO2Rate <= 0.0 || double.IsNaN(pathway.WaterUse) || pathway.WaterUse <= 0.0)
            {
                pathway.CO2Rate = 0;
                pathway.WaterUse = 0;
            }
        }

        private AssimilationFunction GetFunction(AssimilationPathway pathway)
        {
            if (pathway.Type == AssimilationType.Ac1) return GetAc1Function(pathway);
            else if (pathway.Type == AssimilationType.Ac2) return GetAc2Function(pathway);
            else return GetAjFunction(pathway);
        }

        /// <summary>
        /// Updates the intercellular CO2 parameter
        /// </summary>
        protected virtual void UpdateIntercellularCO2(AssimilationPathway pathway, double gt, double waterUseMolsSecond) 
        { /*C4 & CCM overwrite this.*/ }

        /// <summary>
        /// Updates the mesophyll CO2 parameter
        /// </summary>
        protected virtual void UpdateMesophyllCO2(AssimilationPathway pathway) 
        { /*C4 & CCM overwrite this.*/ }

        /// <summary>
        /// Updates the chloroplastic O2 parameter
        /// </summary>
        protected virtual void UpdateChloroplasticO2(AssimilationPathway pathway) 
        { /*CCM overwrites this.*/ }

        /// <summary>
        /// Updates the chloroplastic CO2 parameter
        /// </summary>
        protected virtual void UpdateChloroplasticCO2(AssimilationPathway pathway, AssimilationFunction func) 
        { /*CCM overwrites this.*/ }

        /// <summary>
        /// Retrieves a function describing assimilation along the Ac1 pathway
        /// </summary>
        protected abstract AssimilationFunction GetAc1Function(AssimilationPathway pathway);

        /// <summary>
        /// Retrieves a function describing assimilation along the Ac2 pathway
        /// </summary>
        protected abstract AssimilationFunction GetAc2Function(AssimilationPathway pathway);

        /// <summary>
        /// Retrieves a function describing assimilation along the Aj pathway
        /// </summary>
        protected abstract AssimilationFunction GetAjFunction(AssimilationPathway pathway);
    }
}
