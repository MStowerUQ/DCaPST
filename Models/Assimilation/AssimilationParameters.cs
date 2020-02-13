using System;
using DCAPST.Interfaces;

namespace DCAPST
{
    /// <summary>
    /// Manages the parameters used to calculate assimilation
    /// </summary>
    public abstract class AssimilationParameters
    {
        protected readonly ICanopyParameters canopy;        
        protected readonly IPartialCanopy partial;
        protected readonly IAssimilation assimilation;
        protected readonly IPathwayParameters path;
        protected AssimilationCalculator Calculator;

        public AssimilationParameters(IAssimilation assimilation, IPartialCanopy partial)
        {            
            this.assimilation = assimilation;
            this.partial = partial;
            canopy = partial.Canopy;
            path = canopy.Pathway;

            MesophyllCO2 = canopy.AirCO2 * path.IntercellularToAirCO2Ratio;
            ChloroplasticCO2 = MesophyllCO2 + 20;
            ChloroplasticO2 = 210000;            
        }

        public static AssimilationParameters Create(IAssimilation assimilation, IPartialCanopy partial)
        {
            if (partial.Canopy.Type == CanopyType.C3)
                return new ParametersC3(assimilation, partial);
            else if (partial.Canopy.Type == CanopyType.C4)
                return new ParametersC4(assimilation, partial);
            else
                return new ParametersCCM(assimilation, partial);
        }
        
        public double MesophyllCO2 { get; set; }
        public double ChloroplasticCO2 { get; set; }
        public double ChloroplasticO2 { get; set; }

        // T: At T Per Leaf
        public double VcMaxT => TemperatureFunction.Val2(assimilation.LeafTemperature, partial.At25C.VcMax, path.RubiscoActivity.Factor);
        public double RdT => TemperatureFunction.Val2(assimilation.LeafTemperature, partial.At25C.Rd, path.Respiration.Factor);
        public double JMaxT => TemperatureFunction.Val(assimilation.LeafTemperature, partial.At25C.JMax, path.ElectronTransportRateParams);
        public double VpMaxT => TemperatureFunction.Val2(assimilation.LeafTemperature, partial.At25C.VpMax, path.PEPcActivity.Factor);
        public double MesophyllCO2ConductanceAtT => TemperatureFunction.Val(assimilation.LeafTemperature, partial.At25C.Gm, path.MesophyllCO2ConductanceParams);


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

        private void PrepareCalculator()
        {
            if (assimilation.Type == AssimilationType.Ac1) Calculator = GetAc1Calculator();
            else if (assimilation.Type == AssimilationType.Ac2) Calculator = GetAc2Calculator();
            else Calculator = GetAjCalculator();            
        }

        public double GetUnlimitedAssimilation(double intercellularCO2)
        {
            PrepareCalculator();
            Calculator.p = intercellularCO2;
            Calculator.q = 1 / MesophyllCO2ConductanceAtT;

            return Calculator.CalculateAssimilation();
        }

        public double GetLimitedAssimilation(double waterUseMolsSecond, double Gt)
        {
            PrepareCalculator();
            Calculator.p = canopy.AirCO2 - waterUseMolsSecond * canopy.AirCO2 / (Gt + waterUseMolsSecond / 2.0);
            Calculator.q = 1 / (Gt + waterUseMolsSecond / 2) + 1.0 / MesophyllCO2ConductanceAtT;

            return Calculator.CalculateAssimilation();
        }

        public virtual void UpdateMesophyllCO2(double intercellularCO2, double CO2Rate) { /*C4 & CCM overwrite this.*/ }
        public virtual void UpdateChloroplasticO2(double CO2Rate) { /*CCM overwrites this.*/ }
        public virtual void UpdateChloroplasticCO2(double CO2Rate) { /*CCM overwrites this.*/ }

        protected abstract AssimilationCalculator GetAc1Calculator();
        protected abstract AssimilationCalculator GetAc2Calculator();
        protected abstract AssimilationCalculator GetAjCalculator();
    }
}
