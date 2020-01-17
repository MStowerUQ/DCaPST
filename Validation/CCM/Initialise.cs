using DCAPST.Canopy;
using DCAPST.Interfaces;
using DCAPST;

namespace Validation.CCM
{
    public static class Initialise
    {        
        public static PathwayParameters NewWheat()
        {
            double PsiFactor = 1.0;

            var canopy = new CanopyParameters()
            {
                Type = CanopyType.CCM,

                Ca = 370,
                ConvexityFactor = 0.7,
                DiffusivitySolubilityRatio = 0.047,

                DiffuseExtCoeff = 0.78,
                DiffuseExtCoeffNIR = 0.8,
                DiffuseReflectionCoeff = 0.036,
                DiffuseReflectionCoeffNIR = 0.389,

                LeafAngle = 60,
                LeafScatteringCoeff = 0.15,
                LeafScatteringCoeffNIR = 0.8,                
                LeafWidth = 0.05,

                SLNRatioTop = 1.3,
                StructuralN = 14,

                Windspeed = 1.5,
                WindSpeedExtinction = 1.5
            };

            var j = new ValParameters()
            {
                TMin = 0.0,
                TOpt = 30.0,
                TMax = 45.0,
                C = 0.911017958600129,
                Beta = 1.0
            };

            var g = new ValParameters()
            {
                TMin = 0.0,
                TOpt = 29.2338417788683,
                TMax = 45.0,
                C = 0.875790608584141,
                Beta = 1.0
            };

            var CPath = new PathwayParameters()
            {
                Canopy = canopy,                

                Vpr_l = 400,
                SpectralCorrectionFactor = 0.15,
                Alpha = 0.1,
                Gbs_CO2 = 0.5,

                PsiVc = 1.1 * PsiFactor,
                PsiJ = 1.9484 * PsiFactor,
                PsiRd = 0.0 * PsiFactor,
                PsiVp = 0.373684157583268 * PsiFactor,
                PsiGm = 0.00412 * PsiFactor,

                Phi = 0.75,
                CiCaRatio = 0.7,

                KcP25 = 273.422964228666,
                KcTEa = 93720.0,
                KoP25 = 165824.064155384,
                KoTEa = 33600.0,

                VcTEa = 65330.0,
                VcMax_VoMaxP25 = 4.59217066521612,
                VcMax_VoMaxTEa = 35713.1987127717,
                KpP25 = 75.0,
                KpTEa = 36300.0,

                VpMaxTEa = 57043.2677590512,

                RdTEa = 46390.0,

                J = j,
                Gm = g                
            };
            CPath.X = CPath.Phi / (3.0 + CPath.Phi);
            CPath.Fcyc = 0.25 * CPath.Phi;
            CPath.z = (3.0 - CPath.Fcyc) / (4.0 * (1.0 - CPath.Fcyc));                      

            return CPath;
        }
    }
}
