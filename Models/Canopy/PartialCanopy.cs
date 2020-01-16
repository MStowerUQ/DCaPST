using System;
using System.Collections.Generic;
using System.Linq;
using DCAPST.Environment;
using DCAPST.Interfaces;

namespace DCAPST.Canopy
{
    public class PartialCanopy : BaseCanopy
    {
        public List<PartialAssimilation> partials = new List<PartialAssimilation>();

        public double A { get; set; } = 0.0;
        public double WaterUse { get; set; } = 0.0;
        public double LeafTemperature { get; set; }        

        public PartialCanopy(IPathwayParameters cPath, int layers, double layerLAI)
        {
            CPath = cPath;

            Rad = new AbsorbedRadiation(layers, layerLAI)
            {
                DiffuseExtCoeff = CPath.Canopy.DiffuseExtCoeff,
                LeafScatteringCoeff = CPath.Canopy.LeafScatteringCoeff,
                DiffuseReflectionCoeff = CPath.Canopy.DiffuseReflectionCoeff
            };

            PAR = new AbsorbedRadiation(layers, layerLAI)
            {
                DiffuseExtCoeff = CPath.Canopy.DiffuseExtCoeff,
                LeafScatteringCoeff = CPath.Canopy.LeafScatteringCoeff,
                DiffuseReflectionCoeff = CPath.Canopy.DiffuseReflectionCoeff
            };

            NIR = new AbsorbedRadiation(layers, layerLAI)
            {
                DiffuseExtCoeff = CPath.Canopy.DiffuseExtCoeffNIR,
                LeafScatteringCoeff = CPath.Canopy.LeafScatteringCoeffNIR,
                DiffuseReflectionCoeff = CPath.Canopy.DiffuseReflectionCoeffNIR
            };

            partials.Add(new PartialAssimilation(AssimilationType.Ac1, cPath, this));
            if (CPath.Canopy.Type != CanopyType.C3) partials.Add(new PartialAssimilation(AssimilationType.Ac2, cPath, this));
            partials.Add(new PartialAssimilation(AssimilationType.Aj, cPath, this));
        }

        public void CalcPartialPhotosynthesis(ITemperature temperature, PhotosynthesisParams Params)
        {
            // Initialise the leaf temperature as the air temperature
            partials.ForEach(p => p.LeafTemperature = temperature.AirTemperature);

            // Determine initial results            
            var test = partials.Select(s =>
            {
                IWaterInteraction water = new WaterInteractionModel(temperature, s.LeafTemperature, Params.Gbh);
                return s.TryCalculatePhotosynthesis(water, Params);
            }).ToList();

            var initialA = partials.Select(s => s.A).ToArray();
            var initialWater = partials.Select(s => s.WaterUse).ToArray();

            // If any calculation fails, all results are zeroed
            if (test.Any(b => b == false))
            {
                partials.ForEach(s => s.ZeroVariables());
                return;
            }

            // Do not proceed if there is any insufficient assimilation
            if (!partials.Any(s => s.A < 0.5))
            {
                for (int n = 0; n < 3; n++)
                {
                    test = partials.Select(s =>
                    {
                        IWaterInteraction water = new WaterInteractionModel(temperature, s.LeafTemperature, Params.Gbh);
                        return s.TryCalculatePhotosynthesis(water, Params);
                    }).ToList();

                    // If any calculation fails, all results are set to the value calculated initially
                    if (test.Any(b => b == false))
                    {
                        partials.Select((s, index) =>
                        {
                            s.A = initialA[index];
                            s.WaterUse = initialWater[index];
                            return s;
                        });
                        break;
                    }
                }
            }

            A = partials.Min(p => p.A);
            WaterUse = partials.Min(p => p.WaterUse);
        }

        
    }
}
