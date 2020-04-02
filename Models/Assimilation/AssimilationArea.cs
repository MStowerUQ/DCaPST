﻿using System;
using System.Collections.Generic;
using System.Linq;
using DCAPST.Interfaces;

namespace DCAPST.Canopy
{
    /// <summary>
    /// Models a subsection of the canopy (used for distinguishing between sunlit and shaded)
    /// </summary>
    public class AssimilationArea : IAssimilationArea
    {
        /// <summary>
        /// Parameters describing the canopy
        /// </summary>
        public ICanopyParameters Canopy { get; private set; }       

        public IPathwayParameters Pathway { get; private set; }        

        IAssimilation assimilation;

        /// <summary>
        /// A group of parameters valued at the reference temperature of 25 Celsius
        /// </summary>
        public ParameterRates At25C { get; private set; } = new ParameterRates();


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

        public AssimilationArea(
            ICanopyParameters canopy,
            IPathwayParameters pathway,
            IAssimilation assimilation
        )
        {
            Canopy = canopy;
            Pathway = pathway;
            this.assimilation = assimilation;
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
        /// Calculates the CO2 assimilated by the partial canopy during photosynthesis,
        /// and the water used by the process
        /// </summary>
        public void DoPhotosynthesis(ITemperature temperature, Transpiration transpiration)
        {
            // Create the pathways
            pathways = new List<AssimilationPathway>();
            /*Ac1*/ pathways.Add(new AssimilationPathway(this, Pathway) { Type = PathwayType.Ac1 });
            /*Ac2*/ if (!(assimilation is AssimilationC3)) pathways.Add(new AssimilationPathway(this, Pathway) { Type = PathwayType.Ac2 });
            /*Aj */ pathways.Add(new AssimilationPathway(this, Pathway) { Type = PathwayType.Aj });
            
            // Initialise the temperature
            pathways.ForEach(p => p.Temperature = temperature.AirTemperature);

            // Determine initial results
            UpdateAssimilation(transpiration);

            // Store the initial results in case the subsequent updates fail
            CO2AssimilationRate = GetCO2Rate();
            WaterUse = GetWaterUse();

            // Only attempt to converge result if there is sufficient assimilation
            if (CO2AssimilationRate < 0.5 || WaterUse == 0) return;

            // Repeat calculation 3 times to let solution converge
            for (int n = 0; n < 3; n++)
            {
                UpdateAssimilation(transpiration);

                // If the additional updates fail, stop the process (meaning the initial results used)
                if (GetCO2Rate() == 0 || GetWaterUse() == 0) return;
            }

            // Update results only if convergence succeeds
            CO2AssimilationRate = GetCO2Rate();
            WaterUse = GetWaterUse();
        }

        /// <summary>
        /// Recalculates the assimilation values for each pathway
        /// </summary>
        public void UpdateAssimilation(Transpiration t)
        {
            foreach (var p in pathways)
            {
                t.SetConditions(At25C, p.Temperature, PhotonCount, AbsorbedRadiation);                
                t.UpdatePathway(assimilation, p);
                t.UpdateTemperature(p);

                // If the assimilation is not sensible zero the values
                if (double.IsNaN(p.CO2Rate) || p.CO2Rate <= 0.0 || double.IsNaN(p.WaterUse) || p.WaterUse <= 0.0)
                {
                    p.CO2Rate = 0;
                    p.WaterUse = 0;
                }
            }
        }
    }
}
