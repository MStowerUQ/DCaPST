using System;
using DCAPST.Environment;
using DCAPST.Interfaces;

namespace DCAPST.Canopy
{
    public class PartialCanopy : BaseCanopy
    {        
        public CanopyType Type { get; set; }

        public double A { get; set; } = 0.0;
        public double WaterUse { get; set; } = 0.0;
        public double LeafTemperature { get; set; }

        public double Ci { get; set; }
        public double Cm { get; set; }       
        public double Cc { get; set; }
        public double Oc { get; set; }

        // CONSTANTS   
        public double OxygenPartialPressure { get; set; } = 210000;

        // CONSTANT FUNCTIONS
        public double VcMaxT => TemperatureFunction.Val2(LeafTemperature, VcMax25, CPath.VcTEa);
        public double RdT => TemperatureFunction.Val2(LeafTemperature, Rd25, CPath.RdTEa);
        public double JMaxT => TemperatureFunction.Val(LeafTemperature, JMax25, CPath.JMaxC, CPath.JTMax, CPath.JTMin, CPath.JTOpt, CPath.JBeta);
        public double VpMaxT => TemperatureFunction.Val2(LeafTemperature, VpMax25, CPath.VpMaxTEa);
        public double GmT => TemperatureFunction.Val(LeafTemperature, Gm25, CPath.GmC, CPath.GmTMax, CPath.GmTMin, CPath.GmTOpt, CPath.GmBeta);
        
        public double Kc => TemperatureFunction.Val2(LeafTemperature, CPath.KcP25, CPath.KcTEa);
        public double Ko => TemperatureFunction.Val2(LeafTemperature, CPath.KoP25, CPath.KoTEa);
        public double VcVo => TemperatureFunction.Val2(LeafTemperature, CPath.VcMax_VoMaxP25, CPath.VcMax_VoMaxTEa);
        public double Kp => TemperatureFunction.Val2(LeafTemperature, CPath.KpP25, CPath.KpTEa);

        public double Ja => (1.0 - CPath.SpectralCorrectionFactor) / 2.0;
        private double JaXRad => Ja * Rad.TotalIrradiance;
        public double J =>
            (JaXRad + JMaxT - Math.Pow(Math.Pow(JaXRad + JMaxT, 2) - 4 * CPath.Canopy.ConvexityFactor * JMaxT * JaXRad, 0.5))
            / (2 * CPath.Canopy.ConvexityFactor);        

        public double ScO => Ko / Kc * VcVo;
        public double G_ => 0.5 / ScO;       
        public double K_ => Kc * (1 + OxygenPartialPressure / Ko);       
        public double Rm => RdT * 0.5;
        public double Gbs => CPath.Gbs_CO2 * LAI;
        public double Vpr => CPath.Vpr_l * LAI;

        public PartialCanopy(IPathwayParameters cPath, CanopyType type, int layers, double layerLAI)
        {
            CPath = cPath;
            Type = type;

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

            Cm = CPath.Canopy.Ca * CPath.CiCaRatio;
            Cc = Cm + 20;
            Oc = 210000;
        }

        public bool TryCalculatePhotosynthesis(IWaterInteraction Water, PhotosynthesisParams Params)
        {
            //var Water = new WaterInteractionModel(Temp, CPath, LeafTemperature, Params.Gbh);

            var aparam = CPath.GetAssimilationParams(this);
            
            double Rn = PAR.TotalIrradiance + NIR.TotalIrradiance;
            double rtw;

            if (!Params.limited)
            {
                Ci = CPath.CiCaRatio * CPath.Canopy.Ca;                
                
                aparam.p = Ci;
                aparam.q = 1 / GmT;

                A = aparam.CalculateAssimilation();
                rtw = Water.CalcUnlimitedRtw(A, CPath.Canopy.Ca, Ci);
                WaterUse = Water.HourlyWaterUse(rtw, Rn);
            }
            else
            {
                WaterUse = Params.maxHourlyT * Params.fraction;
                var WaterUseMolsSecond = WaterUse / 18 * 1000 / 3600;

                rtw = Water.CalcLimitedRtw(WaterUse, Rn);
                var Gt = Water.CalcGt(rtw);                

                aparam.p = CPath.Canopy.Ca - WaterUseMolsSecond * CPath.Canopy.Ca / (Gt + WaterUseMolsSecond / 2.0);
                aparam.q = 1 / (Gt + WaterUseMolsSecond / 2) + 1.0 / GmT;

                A = aparam.CalculateAssimilation();

                if (!(CPath is PathwayParametersC3)) 
                    Ci = ((Gt - WaterUseMolsSecond / 2.0) * CPath.Canopy.Ca - A) / (Gt + WaterUseMolsSecond / 2.0);
            }

            // C4 & CCM
            if (!(CPath is PathwayParametersC3))
                Cm = Ci - A / GmT;

            // CCM ONLY
            if (CPath is PathwayParametersCCM)
            {
                Oc = CPath.Alpha * A / (CPath.Canopy.DiffusivitySolubilityRatio * Gbs) + OxygenPartialPressure;
                Cc = Cm + (Cm * aparam.x4 + aparam.x5 - aparam.x6 * A - aparam.m - aparam.x7) * aparam.x8 / Gbs;
            }

            // New leaf temperature
            LeafTemperature = (Water.CalcTemperature(rtw, Rn) + LeafTemperature) / 2.0;

            // If the assimilation is not sensible
            if (double.IsNaN(A) || A <= 0.0)
                return false;
            // If the water use is not sensible
            else if (double.IsNaN(WaterUse) || WaterUse <= 0.0)
                return false;
            else
                return true;
        }

        public void ZeroVariables()
        {
            A = 0;
            WaterUse = 0;            
        }
    }
}
