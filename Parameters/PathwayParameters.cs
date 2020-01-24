using DCAPST.Interfaces;

namespace DCAPST
{   
    public class PathwayParameters : IPathwayParameters
    {
        public double IntercellularToAirCO2Ratio { get; set; }
        public double FractionOfCyclicElectronFlow { get; set; }        
        public double RespirationSLNRatio { get; set; }        
        public double MaxRubiscoActivitySLNRatio { get; set; }        
        public double MaxElectronTransportSLNRatio { get; set; }        
        public double MaxPEPcActivitySLNRatio { get; set; }
        public double MesophyllCO2ConductanceSLNRatio { get; set; }
        public double MesophyllElectronTransportFraction { get; set; }
        public double ATPProductionElectronTransportFactor { get; set; }
       
        public double ExtraATPCost { get; set; }

        // MM: Michaelis Menten
        // PEPc: Phosphoenolpyruvate carboxylase
        public double RubiscoCarboxylationMMConstant25 { get; set; }
        public double RubiscoCarboxylationMMConstantTemperatureResponseFactor { get; set; }
        public double RubiscoOxygenationMMConstant25 { get; set; }        
        public double RubiscoOxygenationMMConstantTemperatureResponseFactor { get; set; }
        public double RubiscoActivityTemperatureResponseFactor { get; set; }
        
        public double RubiscoCarboxylationToOxygenation25 { get; set; }
        public double RubiscoCarboxylationToOxygenationTemperatureResponseFactor { get; set; }
        public double PEPcMMConstant25 { get; set; }
        public double PEPcMMConstantTemperatureResponseFactor { get; set; }
        public double PEPcActivityTemperatureResponseFactor { get; set; }
        public double RespirationTemperatureResponseFactor { get; set; }

        public ValParameters ElectronTransportRateParams { get; set; }
        public ValParameters MesophyllCO2ConductanceParams { get; set; }

        public double SpectralCorrectionFactor { get; set; }
        // PS2: Photosystem 2
        public double PS2ActivityInBundleSheathFraction { get; set; }
        public double PEPRegenerationPerLeaf { get; set; }
        public double BundleSheathCO2ConductancePerLeaf { get; set; }       
    }
}
