﻿using System.Collections.Generic;
using System.Linq;
using DCAPST.Environment;
using DCAPST.Interfaces;

namespace DCAPST.Canopy
{
    public class PartialCanopy : IPartialCanopy
    {
        public ICanopyParameters Canopy { get; set; }
        public List<IAssimilation> partials;

        public double LAI { get; set; }

        public double RubiscoActivity25 { get; set; }
        public double Rd25 { get; set; }
        public double JMax25 { get; set; }
        public double PEPcActivity25 { get; set; }
        public double MesophyllCO2Conductance25 { get; set; }

        public double AbsorbedRadiation { get; set; }
        public double PhotonCount { get; set; }
        public double CO2AssimilationRate { get; set; }
        public double WaterUse { get; set; }

        public PartialCanopy(ICanopyParameters canopy)
        {
            Canopy = canopy;            
        }

        public void CalculatePhotosynthesis(ITemperature temperature, PhotosynthesisParams Params)
        {
            partials = new List<IAssimilation>
            {
                new Assimilation(AssimilationType.Ac1, this),
                (Canopy.Type != CanopyType.C3) ? new Assimilation(AssimilationType.Ac2, this) : null,
                new Assimilation(AssimilationType.Aj, this)
            };

            // Determine initial results            
            foreach (var p in partials)
            {
                p.LeafTemperature = temperature.AirTemperature;
                var water = new LeafWaterInteractionModel(temperature, p.LeafTemperature, Params.BoundaryHeatConductance);

                if (!p.TryUpdateAssimilation(water, Params))
                {
                    CO2AssimilationRate = 0;
                    WaterUse = 0;
                    return;
                }
            }

            // Store the initial results in case the subsequent updates fail
            var initialA = partials.Select(s => s.CO2AssimilationRate);
            var initialWater = partials.Select(s => s.WaterUse);

            // Do not try to update assimilation if the initial value is too low
            if (!partials.Any(s => s.CO2AssimilationRate < 0.5))
            {
                for (int n = 0; n < 3; n++)
                {
                    foreach (var p in partials)
                    {
                        var water = new LeafWaterInteractionModel(temperature, p.LeafTemperature, Params.BoundaryHeatConductance);

                        if (!p.TryUpdateAssimilation(water, Params))
                        {
                            CO2AssimilationRate = initialA.Min();
                            WaterUse = initialWater.Min();
                            return;
                        }
                    }
                }
            }

            CO2AssimilationRate = partials.Min(p => p.CO2AssimilationRate);
            WaterUse = partials.Min(p => p.WaterUse);
        }        
    }
}
