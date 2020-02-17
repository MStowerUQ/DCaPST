using System;

namespace DCAPST.Canopy
{
    public class CanopyRadiation
    {
        public double LeafScatteringCoeff { get; set; }

        public double DiffuseExtCoeff { get; set; }
        public double DiffuseReflectionCoeff { get; set; }
        public double DiffuseScatteredDiffuse => DiffuseExtCoeff * Math.Pow(1 - LeafScatteringCoeff, 0.5);

        public double BeamExtinctionCoeff { get; set; }
        public double BeamReflectionCoeff => 1 - Math.Exp(-2 * ReflectionCoefficientHorizontal * BeamExtinctionCoeff / (1 + BeamExtinctionCoeff));
        public double BeamScatteredBeam => BeamExtinctionCoeff * Math.Pow(1 - LeafScatteringCoeff, 0.5);
        
        public double ReflectionCoefficientHorizontal => (1 - Math.Pow(1 - LeafScatteringCoeff, 0.5)) / (1 + Math.Pow(1 - LeafScatteringCoeff, 0.5));

        /// <summary>
        /// The accumulated LAI of all layers up to the Nth layer
        /// </summary>
        private readonly double AccumLAI_1;

        /// <summary>
        /// The accumulated LAI of all layers up to the (N - 1)th layer
        /// </summary>
        private readonly double AccumLAI_0;

        public CanopyRadiation(int layers, double lai)
        {
            var layerLAI = lai / layers;

            AccumLAI_1 = layers * layerLAI;
            AccumLAI_0 = (layers - 1) * layerLAI;
        }

        /// <summary>
        /// Calculates the total radiation absorbed by the canopy
        /// </summary>
        /// <param name="direct">The direct solar radiation</param>
        /// <param name="diffuse">The diffuse solar radiation</param>
        public double CalcTotalRadiation(double direct, double diffuse)
        {
            var a = (1 - BeamReflectionCoeff) * direct * CalcExp(BeamScatteredBeam);
            var b = (1 - DiffuseReflectionCoeff) * diffuse * CalcExp(DiffuseScatteredDiffuse);

            return a + b;
        }

        /// <summary>
        /// Calculates the total radiation absorbed by the sunlit part of the canopy
        /// </summary>
        /// <param name="direct">The direct solar radiation</param>
        /// <param name="diffuse">The diffuse solar radiation</param>
        /// <returns></returns>
        public double CalcSunlitRadiation(double direct, double diffuse)
        {
            var dir = CalcSunlitDirect(direct);
            var dif = CalcSunlitDiffuse(diffuse);
            var sct = CalcSunlitScattered(direct);

            return dir + dif + sct;
        }

        /// <summary>
        /// Calculates the direct radiation absorbed by the sunlit canopy
        /// </summary>
        /// <param name="direct">The direct solar radiation</param>
        private double CalcSunlitDirect(double direct)
        {
            return (1 - LeafScatteringCoeff) * direct * CalcExp(BeamExtinctionCoeff);
        }

        /// <summary>
        /// Calculates the diffuse radiation absorbed by the sunlit canopy
        /// </summary>
        /// <param name="diffuse">The diffuse solar radiation</param>
        private double CalcSunlitDiffuse(double diffuse)
        {
            var DSD_BEC = DiffuseScatteredDiffuse + BeamExtinctionCoeff;            
            var radiation = (1 - DiffuseReflectionCoeff) * diffuse * CalcExp(DSD_BEC) * (DiffuseScatteredDiffuse / DSD_BEC);

            return radiation;
        }

        /// <summary>
        /// Calculates the scattered radiation absorbed by the sunlit canopy
        /// </summary>
        /// <param name="direct">The direct solar radiation</param>
        private double CalcSunlitScattered(double direct)
        {
            var BSB_BEC = BeamScatteredBeam + BeamExtinctionCoeff;
            if (BSB_BEC == 0) return 0;
            
            var dir = (1.0 - BeamReflectionCoeff) * CalcExp(BSB_BEC) * (BeamScatteredBeam / BSB_BEC); // Integral of direct     
            var dif = (1.0 - LeafScatteringCoeff) * CalcExp(2 * BeamExtinctionCoeff) / 2.0;    // Integral of diffuse

            var radiation = direct * (dir - dif);

            return radiation;
        }

        /// <summary>
        /// Calculates the LAI of the sunlit canopy
        /// </summary>
        public double CalculateSunlitLAI()
        {
            return CalcExp(BeamExtinctionCoeff) / BeamExtinctionCoeff;
        }

        /// <summary>
        /// Calculates the total intercepted radiation
        /// </summary>
        /// <returns></returns>
        public double CalcInterceptedRadiation()
        {
            return 1.0 - Math.Exp(-BeamExtinctionCoeff * AccumLAI_1);
        }

        public double CalcExp(double x)
        {
            var a = Math.Exp(-x * AccumLAI_0);
            var b = Math.Exp(-x * AccumLAI_1);

            return a - b;
        }
    }
}
