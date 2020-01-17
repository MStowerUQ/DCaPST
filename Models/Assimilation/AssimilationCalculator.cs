using System;
using DCAPST.Canopy;
using DCAPST.Interfaces;

namespace DCAPST
{
    public abstract class AssimilationCalculator
    {
        public IPathwayParameters CPath;
        public PartialCanopy Partial;       

        public double Cm { get; set; }
        public double Cc { get; set; }
        public double Oc { get; set; }

        public double OxygenPartialPressure { get; set; } = 210000;

        public double LeafTemperature;

        public AssimilationCalculator(IPathwayParameters path, PartialCanopy partial, Assimilation assimilation)
        {
            CPath = path;
            Partial = partial;

            LeafTemperature = assimilation.LeafTemperature;
            Cm = assimilation.Cm;
            Cc = assimilation.Cc;
            Oc = assimilation.Oc;
        }
        
        public double VcMaxT => TemperatureFunction.Val2(LeafTemperature, Partial.VcMax25, CPath.VcTEa);
        public double RdT => TemperatureFunction.Val2(LeafTemperature, Partial.Rd25, CPath.RdTEa);
        public double JMaxT => TemperatureFunction.Val(LeafTemperature, Partial.JMax25, CPath.J);
        public double VpMaxT => TemperatureFunction.Val2(LeafTemperature, Partial.VpMax25, CPath.VpMaxTEa);

        public double Kc => TemperatureFunction.Val2(LeafTemperature, CPath.KcP25, CPath.KcTEa);
        public double Ko => TemperatureFunction.Val2(LeafTemperature, CPath.KoP25, CPath.KoTEa);
        public double VcVo => TemperatureFunction.Val2(LeafTemperature, CPath.VcMax_VoMaxP25, CPath.VcMax_VoMaxTEa);
        public double Kp => TemperatureFunction.Val2(LeafTemperature, CPath.KpP25, CPath.KpTEa);

        public double Ja => (1.0 - CPath.SpectralCorrectionFactor) / 2.0;
        private double JaXRad => Ja * Partial.Rad.TotalIrradiance;
        public double J =>
            (JaXRad + JMaxT - Math.Pow(Math.Pow(JaXRad + JMaxT, 2) - 4 * CPath.Canopy.ConvexityFactor * JMaxT * JaXRad, 0.5))
            / (2 * CPath.Canopy.ConvexityFactor);

        public double ScO => Ko / Kc * VcVo;
        public double G_ => 0.5 / ScO;
        public double Rm => RdT * 0.5;
        public double Gbs => CPath.Gbs_CO2 * Partial.LAI;
        public double Vpr => CPath.Vpr_l * Partial.LAI;

        public AssimilationParameters GetAssimilationParams(Assimilation canopy)
        {
            if (canopy.Type == AssimilationType.Ac1) return GetAc1Params();
            else if (canopy.Type == AssimilationType.Ac2) return GetAc2Params();
            else return GetAjParams();
        }

        protected abstract AssimilationParameters GetAc1Params();
        protected abstract AssimilationParameters GetAc2Params();
        protected abstract AssimilationParameters GetAjParams();

    }
}
