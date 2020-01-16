using System;
using System.Collections.Generic;
using System.Text;

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

            //Set the parameters
            var canopy = new CanopyParameters()
            {
                Type = CanopyType.CCM,

                Ca = 370,
                Windspeed = 1.5,
                LeafAngle = 60,
                DiffuseExtCoeff = 0.78,
                DiffuseExtCoeffNIR = 0.8,
                LeafScatteringCoeff = 0.15,
                LeafScatteringCoeffNIR = 0.8,
                DiffuseReflectionCoeff = 0.036,
                DiffuseReflectionCoeffNIR = 0.389,
                LeafWidth = 0.05,
                WindSpeedExtinction = 1.5,                
                
                ConvexityFactor = 0.7,
                
                DiffusivitySolubilityRatio = 0.047,

                SLNRatioTop = 1.3,
                StructuralN = 14,
            };

            var CPath = new PathwayParametersCCM()
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
                X = 0.75 / 3.75,
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

                JTMin = 0.0,
                JTOpt = 30.0,
                JTMax = 45.0,
                JMaxC = 0.911017958600129,
                JBeta = 1.0,

                GmTMin = 0.0,
                GmTOpt = 29.2338417788683,
                GmTMax = 45.0,
                GmC = 0.875790608584141,
                GmBeta = 1.0,
            };
            CPath.Fcyc = 0.25 * CPath.Phi;
            CPath.z = (3.0 - CPath.Fcyc) / (4.0 * (1.0 - CPath.Fcyc));                      

            return CPath;
        }
    }
}
