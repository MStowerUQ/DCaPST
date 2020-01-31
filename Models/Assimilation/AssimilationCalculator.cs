using System;
using DCAPST.Interfaces;

namespace DCAPST
{
    public abstract class AssimilationCalculator
    {
        protected readonly ICanopyParameters canopy;        
        protected readonly IPartialCanopy partial;
        protected readonly IAssimilation assimilation;
        protected readonly IPathwayParameters path;

        public AssimilationCalculator(IAssimilation assimilation, IPartialCanopy partial)
        {            
            this.assimilation = assimilation;
            this.partial = partial;
            canopy = partial.Canopy;
            path = canopy.Pathway;
        }
        
        // T: At T Per Leaf
        public double VcMaxT => TemperatureFunction.Val2(assimilation.LeafTemperature, partial.RubiscoActivity25, path.RubiscoActivity.Factor);
        public double RdT => TemperatureFunction.Val2(assimilation.LeafTemperature, partial.Rd25, path.Respiration.Factor);
        public double JMaxT => TemperatureFunction.Val(assimilation.LeafTemperature, partial.JMax25, path.ElectronTransportRateParams);
        public double VpMaxT => TemperatureFunction.Val2(assimilation.LeafTemperature, partial.PEPcActivity25, path.PEPcActivity.Factor);


        // These ones are not per leaf
        public double Kc => TemperatureFunction.Val2(assimilation.LeafTemperature, path.RubiscoCarboxylation.At25, path.RubiscoCarboxylation.Factor);
        public double Ko => TemperatureFunction.Val2(assimilation.LeafTemperature, path.RubiscoOxygenation.At25, path.RubiscoOxygenation.Factor);
        public double VcVo => TemperatureFunction.Val2(assimilation.LeafTemperature, path.RubiscoCarboxylationToOxygenation.At25, path.RubiscoCarboxylationToOxygenation.Factor);
        public double Kp => TemperatureFunction.Val2(assimilation.LeafTemperature, path.PEPc.At25, path.PEPc.Factor);

        private double JFactor => partial.PhotonCount * (1.0 - path.SpectralCorrectionFactor) / 2.0;
        public double ElectronTransportRate =>
            (JFactor + JMaxT - Math.Pow(Math.Pow(JFactor + JMaxT, 2) - 4 * canopy.ConvexityFactor * JMaxT * JFactor, 0.5))
            / (2 * canopy.ConvexityFactor);

        public double RubiscoSpecificityFactor => Ko / Kc * VcVo;
        public double G_ => 0.5 / RubiscoSpecificityFactor;
        public double MesophyllRespiration => RdT * 0.5;
        public double Gbs => path.BundleSheathCO2ConductancePerLeaf * partial.LAI;
        public double Vpr => path.PEPRegenerationPerLeaf * partial.LAI;

        public AssimilationParameters GetAssimilationParams()
        {
            if (assimilation.Type == AssimilationType.Ac1) return GetAc1Params();
            else if (assimilation.Type == AssimilationType.Ac2) return GetAc2Params();
            else return GetAjParams();
        }

        protected abstract AssimilationParameters GetAc1Params();
        protected abstract AssimilationParameters GetAc2Params();
        protected abstract AssimilationParameters GetAjParams();

    }
}
