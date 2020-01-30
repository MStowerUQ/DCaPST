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
            Cm = assimilation.MesophyllCO2;
            Cc = assimilation.ChloroplasticCO2;
            Oc = assimilation.ChloroplasticO2;
        }
        
        // T: At T Per Leaf
        public double VcMaxT => TemperatureFunction.Val2(LeafTemperature, Partial.VcMax25, Path.RubiscoActivity.Factor);
        public double RdT => TemperatureFunction.Val2(LeafTemperature, Partial.Rd25, Path.Respiration.Factor);
        public double JMaxT => TemperatureFunction.Val(LeafTemperature, Partial.JMax25, Path.ElectronTransportRateParams);
        public double VpMaxT => TemperatureFunction.Val2(LeafTemperature, Partial.VpMax25, Path.PEPcActivity.Factor);


        // These ones are not per leaf
        public double Kc => TemperatureFunction.Val2(LeafTemperature, Path.RubiscoCarboxylation.At25, Path.RubiscoCarboxylation.Factor);
        public double Ko => TemperatureFunction.Val2(LeafTemperature, Path.RubiscoOxygenation.At25, Path.RubiscoOxygenation.Factor);
        public double VcVo => TemperatureFunction.Val2(LeafTemperature, Path.RubiscoCarboxylationToOxygenation.At25, Path.RubiscoCarboxylationToOxygenation.Factor);
        public double Kp => TemperatureFunction.Val2(LeafTemperature, Path.PEPc.At25, Path.PEPc.Factor);

        private double JFactor => Partial.PhotonCount * (1.0 - Path.SpectralCorrectionFactor) / 2.0;
        public double ElectronTransportRate =>
            (JFactor + JMaxT - Math.Pow(Math.Pow(JFactor + JMaxT, 2) - 4 * Canopy.ConvexityFactor * JMaxT * JFactor, 0.5))
            / (2 * Canopy.ConvexityFactor);

        public double RubiscoSpecificityFactor => Ko / Kc * VcVo;
        public double G_ => 0.5 / RubiscoSpecificityFactor;
        public double MesophyllRespiration => RdT * 0.5;
        public double Gbs => Path.BundleSheathCO2ConductancePerLeaf * Partial.LAI;
        public double Vpr => Path.PEPRegenerationPerLeaf * Partial.LAI;

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
