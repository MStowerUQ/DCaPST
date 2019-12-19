using System;
using System.Collections.Generic;
using System.Text;

using LayerCanopyPhotosynthesis.Canopy;
using LayerCanopyPhotosynthesis;

namespace Validation.Wheat
{
    public static class Initialise
    {        
        public static Photosynthesis NewCCM()
        {
            double PsiFactor = 1.0;

            //Set the parameters
            var canopy = new CanopyParameters()
            {
                Ca = 370,
                U0 = 1.5,
                LeafAngle = 60,
                DiffuseExtCoeff = 0.78,
                DiffuseExtCoeffNIR = 0.8,
                LeafScatteringCoeff = 0.15,
                LeafScatteringCoeffNIR = 0.8,
                DiffuseReflectionCoeff = 0.036,
                DiffuseReflectionCoeffNIR = 0.389,
                LeafWidth = 0.05,
                Ku = 1.5,
                
                Vpr_l = 400,
                Theta = 0.7,
                F = 0.15,
                Alpha = 0.1,
                Constant = 0.047,
                
                Gbs_CO2 = 0.5,

                Sigma = 0.0000000567,
                Rcp = 1200,

                G = 0.066,
                Lambda = 2447000,
            };

            var CPath = new PathwayParametersC4()
            {
                Canopy = canopy,

                SLNRatioTop = 1.3,
                StructuralN = 14,

                PsiVc = 1.1 * PsiFactor,
                PsiJ = 1.85 * PsiFactor,
                PsiRd = 0.0 * PsiFactor,
                PsiVp = 1.0 * PsiFactor,
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

            var Model = new Photosynthesis(CPath);
            
            //Model.B = 0.409;     //BiomassConversionCoefficient - CO2-to-biomass conversion efficiency
            //Model.Radiation.RPAR = 0.5;     //RPAR - Fraction of PAR energy to that of the total solar
            //Model.Temperature.AtmosphericPressure = 1.01325;            

            return Model;
        }
    }
}
