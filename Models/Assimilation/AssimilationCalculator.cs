using System;
using DCAPST.Canopy;
using DCAPST.Interfaces;

namespace DCAPST
{
    public abstract class AssimilationCalculator
    {
        public ICanopyParameters Canopy;
        public IPathwayParameters Path;
        public PartialCanopy Partial;       

        public double Cm { get; set; }
        public double Cc { get; set; }
        public double Oc { get; set; }

        public double OxygenPartialPressure { get; set; } = 210000;

        public double LeafTemperature;

        public AssimilationCalculator(ICanopyParameters canopy, PartialCanopy partial, Assimilation assimilation)
        {
            Canopy = canopy;
            Path = canopy.Pathway;
            Partial = partial;

            LeafTemperature = assimilation.LeafTemperature;
            Cm = assimilation.Cm;
            Cc = assimilation.Cc;
            Oc = assimilation.Oc;
        }
        
        public double VcMaxT => TemperatureFunction.Val2(LeafTemperature, Partial.VcMax25, Path.VcTEa);
        public double RdT => TemperatureFunction.Val2(LeafTemperature, Partial.Rd25, Path.RdTEa);
        public double JMaxT => TemperatureFunction.Val(LeafTemperature, Partial.JMax25, Path.J);
        public double VpMaxT => TemperatureFunction.Val2(LeafTemperature, Partial.VpMax25, Path.VpMaxTEa);

        public double Kc => TemperatureFunction.Val2(LeafTemperature, Path.KcP25, Path.KcTEa);
        public double Ko => TemperatureFunction.Val2(LeafTemperature, Path.KoP25, Path.KoTEa);
        public double VcVo => TemperatureFunction.Val2(LeafTemperature, Path.VcMax_VoMaxP25, Path.VcMax_VoMaxTEa);
        public double Kp => TemperatureFunction.Val2(LeafTemperature, Path.KpP25, Path.KpTEa);

        public double Ja => (1.0 - Path.SpectralCorrectionFactor) / 2.0;
        private double JaXRad => Ja * Partial.Rad.TotalIrradiance;
        public double J =>
            (JaXRad + JMaxT - Math.Pow(Math.Pow(JaXRad + JMaxT, 2) - 4 * Canopy.ConvexityFactor * JMaxT * JaXRad, 0.5))
            / (2 * Canopy.ConvexityFactor);

        public double ScO => Ko / Kc * VcVo;
        public double G_ => 0.5 / ScO;
        public double Rm => RdT * 0.5;
        public double Gbs => Path.Gbs_CO2 * Partial.LAI;
        public double Vpr => Path.Vpr_l * Partial.LAI;

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
