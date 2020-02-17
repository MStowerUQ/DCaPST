using System.Collections.Generic;
using System.Linq;
using DCAPST.Environment;
using DCAPST.Interfaces;

namespace DCAPST.Canopy
{
    public class PartialCanopy : IPartialCanopy
    {
        public ICanopyParameters Canopy { get; set; }
        public IAssimilation assimilation;

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
            assimilation = CreateAssimilation();

            // Determine initial results
            assimilation.UpdateAssimilation(temperature, Params);

            // Store the initial results in case the subsequent updates fail
            var initialA = assimilation.GetCO2Rate();
            var initialWater = assimilation.GetWaterUse();
            
            if (initialA == 0 || initialWater == 0) return;

            // Only update assimilation if the initial value is large enough
            if (initialA >= 0.5)
            {
                for (int n = 0; n < 3; n++)
                {
                    assimilation.UpdateAssimilation(temperature, Params);

                    // If the additional updates fail, the minimum amongst the initial values is taken
                    if (assimilation.GetCO2Rate() == 0 || assimilation.GetWaterUse() == 0)
                    {
                        CO2AssimilationRate = initialA;
                        WaterUse = initialWater;
                        return;
                    }
                }
            }
            CO2AssimilationRate = assimilation.GetCO2Rate();
            WaterUse = assimilation.GetWaterUse();
        }

        private IAssimilation CreateAssimilation()
        {
            if (Canopy.Type == CanopyType.C3) return new AssimilationC3(this);
            else if (Canopy.Type == CanopyType.C4) return new AssimilationC4(this);
            else return new AssimilationCCM(this);
        }
    }
}
