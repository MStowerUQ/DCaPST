using System;
using System.Collections.Generic;
using System.Text;

using LayerCanopyPhotosynthesis.Canopy;
using LayerCanopyPhotosynthesis;

namespace Validation.C4
{
    public static class Initialise
    {
        public static Photosynthesis NewC4()
        {
            double PsiFactor = 0.4;

            //Set the parameters
            var canopy = new CanopyParameters()
            {
                Ca = 363,
                U0 = 1.5,
                LeafAngle = 60,
                DiffuseExtCoeff = 0.78,
                DiffuseExtCoeffNIR = 0.8,
                LeafScatteringCoeff = 0.15,
                LeafScatteringCoeffNIR = 0.8,
                DiffuseReflectionCoeff = 0.036,
                DiffuseReflectionCoeffNIR = 0.389,
                LeafWidth = 0.15,
                Ku = 1.5,
                Vpr_l = 120,
                Theta = 0.7,
                F = 0.15,
                Alpha = 0.1,
                Constant = 0.047,
                Gbs_CO2 = 0.003,
                Sigma = 0.0000000567,
                Rcp = 1200,
                G = 0.066,
                Lambda = 2447000,
            };

            var CPath = new PathwayParametersC4()
            {
                Canopy = canopy,

                SLNRatioTop = 1.3,      // Ratio of the specific leaf nitrogen at the top of the canopy to that of the canopy average
                StructuralN = 14,       // Specific leaf nitrogen below which Vcamx, Jmax and Vpmax are zero

                PsiVc = 0.465 * PsiFactor,      // Slope of linear relationship between Vcmax per leaf area at 25°C and specific leaf nitrogen
                PsiJ = 2.7 * PsiFactor,         // Slope of linear relationship between Jmax per leaf area at 25°C and specific leaf nitrogen
                PsiRd = 0.0 * PsiFactor,        // Slope of linear relationship between Rd per leaf area at 25°C and specific leaf nitrogen
                PsiVp = 1.55 * PsiFactor,       // Slope of linear relationship between Vpmax per leaf area at 25°C and specific leaf nitrogen 
                PsiGm = 0.0135 * PsiFactor,     // Slope of linear relationship between gm per leaf area at 25°C and specific leaf nitrogen

                Phi = 2,                // extra energy (ATP) cost required from processes other than the C3 cycle
                X = 0.4,                // Fraction of electron transport partitioned to mesophyll chloroplasts
                CiCaRatio = 0.45,       // ratio of intercellular to air CO2 partial pressure

                KcP25 = 1210,
                KcTEa = 64200,
                KoP25 = 292000,
                KoTEa = 10500,

                VcTEa = 78000,
                VcMax_VoMaxP25 = 5.51328906454566,
                VcMax_VoMaxTEa = 21265.4029552906,
                KpP25 = 75,
                KpTEa = 36300,

                VpMaxTEa = 57043.2677590512,

                RdTEa = 46390,

                JTMin = 0,
                JTOpt = 37.8649150880407,
                JTMax = 55,
                JMaxC = 0.711229539802063,
                JBeta = 1,

                GmTMin = 0,
                GmTOpt = 42,
                GmTMax = 55,
                GmC = 0.462820450976839,
                GmBeta = 1,
            };

            var Model = new Photosynthesis(CPath);
            
            //Model.B = 0.409;     //BiomassConversionCoefficient - CO2-to-biomass conversion efficiency
            //Model.Radiation.RPAR = 0.5;     //RPAR - Fraction of PAR energy to that of the total solar
            //Model.Temperature.AtmosphericPressure = 1.01325;            

            return Model;
        }
    }
}
