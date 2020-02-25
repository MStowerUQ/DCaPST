using System;
using System.Collections.Generic;
using System.Linq;
using DCAPST.Interfaces;

namespace DCAPST.Canopy
{
    /// <summary>
    /// Models a subsection of the canopy (used for distinguishing between sunlit and shaded)
    /// </summary>
    public class PartialCanopy : IPartialCanopy
    {
        /// <summary>
        /// Parameters describing the canopy
        /// </summary>
        public ICanopyParameters Canopy { get; private set; }       

        public IPathwayParameters Pathway { get; private set; }

        /// <summary>
        /// Models the leaf water interaction
        /// </summary>
        public ILeafWaterInteraction LeafWater { get; }

        IAssimilation assimilation;

        /// <summary>
        /// A group of parameters valued at the reference temperature of 25 Celsius
        /// </summary>
        public ParameterRates At25C { get; private set; } = new ParameterRates();

        /// <summary>
        /// Models how the leaf responds to different temperatures
        /// </summary>
        public LeafTemperatureResponseModel Leaf { get; set; }

        /// <summary>
        /// The leaf area index of this part of the canopy
        /// </summary>
        public double LAI { get; set; }

        /// <summary>
        /// The sunlight absorbed by the canopy over a period of time
        /// </summary>
        public double AbsorbedRadiation { get; set; }

        /// <summary>
        /// The number of photons which reached the canopy over a period of time
        /// </summary>
        public double PhotonCount { get; set; }
        
        /// <summary>
        /// CO2 assimilation rate over a period of time
        /// </summary>
        public double CO2AssimilationRate { get; set; }
        
        /// <summary>
        /// Water used during photosynthesis
        /// </summary>
        public double WaterUse { get; set; }        

        /// <summary>
        /// The possible assimilation pathways
        /// </summary>
        protected List<AssimilationPathway> pathways;

        public PartialCanopy(
            ICanopyParameters canopy,
            IPathwayParameters pathway,
            ILeafWaterInteraction leafWater,
            IAssimilation assimilation,
            LeafTemperatureResponseModel leaf
        )
        {
            Canopy = canopy;
            Pathway = pathway;
            LeafWater = leafWater;
            this.assimilation = assimilation;
            Leaf = leaf;
        }

        /// <summary>
        /// Calculates the CO2 assimilated by the partial canopy during photosynthesis,
        /// and the water used by the process
        /// </summary>
        public void DoPhotosynthesis(ITemperature temperature, WaterParameters Params)
        {
            pathways = new List<AssimilationPathway>()
            {
                /*Ac1*/ new AssimilationPathway(this, Pathway) { Type = PathwayType.Ac1 },
                /*Ac2*/ assimilation is AssimilationC3 ? null : new AssimilationPathway(this, Pathway) { Type = PathwayType.Ac2 },
                /*Aj */ new AssimilationPathway(this, Pathway) { Type = PathwayType.Aj }
            };
            pathways.ForEach(p => p.Temperature = temperature.AirTemperature);

            // Determine initial results
            UpdateAssimilation(Params);

            // Store the initial results in case the subsequent updates fail
            CO2AssimilationRate = GetCO2Rate();
            WaterUse = GetWaterUse();
            
            if (CO2AssimilationRate == 0 || WaterUse == 0) return;

            // Only update assimilation if the initial value is large enough
            if (CO2AssimilationRate >= 0.5)
            {
                for (int n = 0; n < 3; n++)
                {
                    UpdateAssimilation(Params);

                    // If the additional updates fail, the minimum amongst the initial values is taken
                    if (GetCO2Rate() == 0 || GetWaterUse() == 0) return;                    
                }
            }

            // If three iterations pass without failing, update the values to the final result
            CO2AssimilationRate = GetCO2Rate();
            WaterUse = GetWaterUse();
        }

        /// <summary>
        /// Finds the CO2 assimilation rate
        /// </summary>
        public double GetCO2Rate() => pathways.Min(p => p.CO2Rate);

        /// <summary>
        /// Finds the water used during CO2 assimilation
        /// </summary>
        public double GetWaterUse() => pathways.Min(p => p.WaterUse);

        /// <summary>
        /// Recalculates the assimilation values for each pathway
        /// </summary>
        public void UpdateAssimilation(WaterParameters water) => pathways.ForEach(p => UpdatePathway(water, p));

        /// <summary>
        /// Updates the state of an assimilation pathway
        /// </summary>
        private void UpdatePathway(WaterParameters water, AssimilationPathway pathway)
        {
            if (pathway == null) return;

            Leaf.SetConditions(At25C, pathway.Temperature, PhotonCount);
            LeafWater.SetConditions(pathway.Temperature, water.BoundaryHeatConductance);

            double resistance;

            var func = assimilation.GetFunction(pathway, Leaf);
            if (!water.limited) /* Unlimited water calculation */
            {
                pathway.IntercellularCO2 = Pathway.IntercellularToAirCO2Ratio * Canopy.AirCO2;

                func.Ci = pathway.IntercellularCO2;
                func.Rm = 1 / Leaf.GmT;

                pathway.CO2Rate = func.Value();

                resistance = LeafWater.UnlimitedWaterResistance(pathway.CO2Rate, Canopy.AirCO2, pathway.IntercellularCO2);
                pathway.WaterUse = LeafWater.HourlyWaterUse(resistance, AbsorbedRadiation);
            }
            else /* Limited water calculation */
            {
                pathway.WaterUse = water.maxHourlyT * water.fraction;
                var WaterUseMolsSecond = pathway.WaterUse / 18 * 1000 / 3600;

                resistance = LeafWater.LimitedWaterResistance(pathway.WaterUse, AbsorbedRadiation);
                var Gt = LeafWater.TotalCO2Conductance(resistance);

                func.Ci = Canopy.AirCO2 - WaterUseMolsSecond * Canopy.AirCO2 / (Gt + WaterUseMolsSecond / 2.0);
                func.Rm = 1 / (Gt + WaterUseMolsSecond / 2) + 1.0 / Leaf.GmT;

                pathway.CO2Rate = func.Value();

                assimilation.UpdateIntercellularCO2(pathway, Gt, WaterUseMolsSecond);
            }
            assimilation.UpdatePartialPressures(pathway, Leaf, func);

            // New leaf temperature
            pathway.Temperature = (LeafWater.LeafTemperature(resistance, AbsorbedRadiation) + pathway.Temperature) / 2.0;

            // If the assimilation is not sensible zero the values
            if (double.IsNaN(pathway.CO2Rate) || pathway.CO2Rate <= 0.0 || double.IsNaN(pathway.WaterUse) || pathway.WaterUse <= 0.0)
            {
                pathway.CO2Rate = 0;
                pathway.WaterUse = 0;
            }
        }

    }
}
