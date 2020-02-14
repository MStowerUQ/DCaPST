using System.Collections.Generic;
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

        public ParameterRates At25C { get; private set; }

        public double AbsorbedRadiation { get; set; }
        public double PhotonCount { get; set; }
        public double CO2AssimilationRate { get; set; }
        public double WaterUse { get; set; }

        public PartialCanopy(ICanopyParameters canopy)
        {
            Canopy = canopy;
            At25C = new ParameterRates();
        }

        public void CalculatePhotosynthesis(ITemperature temperature, WaterParameters Params)
        {
            partials = new List<IAssimilation>
            {
                CreateAssimilation(AssimilationType.Ac1),
                (Canopy.Type != CanopyType.C3) ? CreateAssimilation(AssimilationType.Ac2) : null,
                CreateAssimilation(AssimilationType.Aj)
            };

            // Determine initial results            
            foreach (var p in partials)
            {
                p.Path.LeafTemperature = temperature.AirTemperature;
                
                p.UpdateAssimilation(temperature, Params);
                if (p.Path.CO2Rate == 0 || p.Path.WaterUse == 0) return;                
            }

            // Store the initial results in case the subsequent updates fail
            var initialA = partials.Select(s => s.Path.CO2Rate).ToArray();
            var initialWater = partials.Select(s => s.Path.WaterUse).ToArray();

            // Do not try to update assimilation if the initial value is too low
            if (!partials.Any(s => s.Path.CO2Rate < 0.5))
            {
                for (int n = 0; n < 3; n++)
                {
                    foreach (var p in partials)
                    {
                        p.UpdateAssimilation(temperature, Params);
                        // If the additional updates fail, the minimum amongst the initial values is taken
                        if (p.Path.CO2Rate == 0 || p.Path.WaterUse == 0)
                        {
                            CO2AssimilationRate = initialA.Min();
                            WaterUse = initialWater.Min();
                            return;
                        }
                    }
                }
            }

            CO2AssimilationRate = partials.Min(p => p.Path.CO2Rate);
            WaterUse = partials.Min(p => p.Path.WaterUse);
        }

        private IAssimilation CreateAssimilation(AssimilationType type)
        {
            if (Canopy.Type == CanopyType.C3) return new ParametersC3(type, this);
            else if (Canopy.Type == CanopyType.C4) return new ParametersC4(type, this);
            else return new ParametersCCM(type, this);
        }
    }
}
