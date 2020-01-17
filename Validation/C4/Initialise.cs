using DCAPST.Canopy;
using DCAPST.Interfaces;
using DCAPST;

namespace Validation.C4
{
    public static class Initialise
    {
        public static CanopyParameters NewSorghumParameters()
        {
            double PsiFactor = 0.4;
            
            var j = new ValParameters()
            {
                TMin = 0,
                TOpt = 37.8649150880407,
                TMax = 55,
                C = 0.711229539802063,
                Beta = 1
            };

            var g = new ValParameters()
            {
                TMin = 0,
                TOpt = 42,
                TMax = 55,
                C = 0.462820450976839,
                Beta = 1,
            };

            var CPath = new PathwayParameters()
            {
                Vpr_l = 120,
                SpectralCorrectionFactor = 0.15,
                Alpha = 0.1,
                Gbs_CO2 = 0.003,
                
                PsiVc = 0.465 * PsiFactor,
                PsiJ = 2.7 * PsiFactor,
                PsiRd = 0.0 * PsiFactor,
                PsiVp = 1.55 * PsiFactor,
                PsiGm = 0.0135 * PsiFactor,

                Phi = 2,
                X = 0.4,
                CiCaRatio = 0.45,

                KcP25 = 1210,
                KcTEa = 64200,

                KoP25 = 292000,
                KoTEa = 10500,
                
                VcMax_VoMaxP25 = 5.51328906454566,
                VcMax_VoMaxTEa = 21265.4029552906,

                KpP25 = 75,
                KpTEa = 36300,                

                VcTEa = 78000,
                RdTEa = 46390,
                VpMaxTEa = 57043.2677590512,

                J = j,
                Gm = g                
            };

            var canopy = new CanopyParameters()
            {
                Type = CanopyType.C4,

                Pathway = CPath,

                Ca = 363,
                ConvexityFactor = 0.7,
                DiffusivitySolubilityRatio = 0.047,

                DiffuseExtCoeff = 0.78,
                DiffuseExtCoeffNIR = 0.8,
                DiffuseReflectionCoeff = 0.036,
                DiffuseReflectionCoeffNIR = 0.389,

                LeafAngle = 60,
                LeafScatteringCoeff = 0.15,
                LeafScatteringCoeffNIR = 0.8,
                LeafWidth = 0.15,

                SLNRatioTop = 1.3,
                StructuralN = 14,

                Windspeed = 1.5,
                WindSpeedExtinction = 1.5
            };

            return canopy;
        }
    }
}
