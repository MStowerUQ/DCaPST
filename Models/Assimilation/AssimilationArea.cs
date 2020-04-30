using System;
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
        protected double CO2AssimilationRate { get; set; }
        
        /// <summary>
        /// Water used during photosynthesis
        /// </summary>
        protected double WaterUse { get; set; }        

        /// <summary>
        /// The possible assimilation pathways
        /// </summary>
        protected List<AssimilationPathway> pathways;

        //public List<double> WaterDemands { get; set; } = new List<double>();

        //public List<double> AssimilationRates { get; set; } = new List<double>();

        public AreaAlphaValues Alpha { get; private set; }

        public AreaBetaValues Beta { get; } = new AreaBetaValues();

        public AssimilationArea(
            AssimilationPathway Ac1,
            AssimilationPathway Ac2,
            AssimilationPathway Aj,
            IAssimilation assimilation
        )
        {
            pathways = new List<AssimilationPathway>();

            // Always include Ac1
            Ac1.Type = PathwayType.Ac1;
            pathways.Add(Ac1);

            // Conditionally include Ac2
            Ac2.Type = PathwayType.Ac2;
            if (!(assimilation is AssimilationC3)) pathways.Add(Ac2);

            // Always include Aj
            Aj.Type = PathwayType.Aj;
            pathways.Add(Aj);

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
            // Initialise at the current temperature
            pathways.ForEach(p => p.SetConditions(temperature.AirTemperature, LAI));

            // Determine initial results
            UpdateAssimilation(transpiration);

            // Store the initial results in case the subsequent updates fail
            CO2AssimilationRate = GetCO2Rate();
            WaterUse = GetWaterUse();

            // Only attempt to converge result if there is sufficient assimilation
            if (CO2AssimilationRate < 0.5 || WaterUse == 0) goto Finally;

            // Repeat calculation 3 times to let solution converge
            for (int n = 0; n < 3; n++)
            {
                UpdateAssimilation(transpiration);

                // If the additional updates fail, stop the process (meaning the initial results used)
                if (GetCO2Rate() == 0 || GetWaterUse() == 0) goto Finally;
            }

            // Update results only if convergence succeeds
            CO2AssimilationRate = GetCO2Rate();
            WaterUse = GetWaterUse();            

        Finally:
            RecordAlphaValues();
            //AssimilationRates.Add(CO2AssimilationRate);
            //WaterDemands.Add(WaterUse);
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

        private void RecordAlphaValues()
        {
            Alpha = new AreaAlphaValues();

            foreach (var p in pathways)
            {
                if (p.Type == PathwayType.Ac1) Alpha.Ac1 = p.CO2Rate;
                else if (p.Type == PathwayType.Ac2) Alpha.Ac2 = p.CO2Rate;
                else Alpha.Aj = p.CO2Rate;
            }

            Alpha.A = CO2AssimilationRate;
            Alpha.E = WaterUse;
        }
    }

    public class AreaAlphaValues
    {
        public double A { get; set; }

        public double Ac1 { get; set; }

        public double Ac2 { get; set; }

        public double Aj { get; set; }

        public double gsCO2 { get; set; }

        public double E { get; set; }

        public override string ToString()
        {
            // This intentionally excludes A
            return $"{Ac1:F6},{Ac2:F6},{Aj:F6},{gsCO2:F6},{E:F6}";
        }

        public string Header(string pre = "", string suf = "")
        {
            return $"{pre}_Ac1_{suf}, {pre}_Ac2_{suf}, {pre}_Aj_{suf}, {pre}_gsCO2_{suf}, {pre}_E_{suf}";
        }
    }

    public class AreaBetaValues
    {
        public double Ci { get; set; }

        public double Cm { get; set; }

        public double Cc { get; set; }

        public double Tl { get; set; }

        public override string ToString()
        {
            return $"{Ci},{Cm},{Cc},{Tl}";
        }
    }
}
