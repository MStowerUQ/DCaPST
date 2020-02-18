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
        public TemperatureResponse RubiscoCarboxylation { get; set; }
        public TemperatureResponse RubiscoOxygenation { get; set; }
        public TemperatureResponse RubiscoCarboxylationToOxygenation { get; set; }
        public TemperatureResponse RubiscoActivity { get; set; }

        public TemperatureResponse PEPc { get; set; }
        public TemperatureResponse PEPcActivity { get; set; }
        public TemperatureResponse Respiration { get; set; }

        public LeafTemperatureParameters ElectronTransportRateParams { get; set; }
        public LeafTemperatureParameters MesophyllCO2ConductanceParams { get; set; }

        public double SpectralCorrectionFactor { get; set; }
        // PS2: Photosystem 2
        public double PS2ActivityFraction { get; set; }
        public double PEPRegeneration { get; set; }
        public double BundleSheathConductance { get; set; }       
    }

    public struct TemperatureResponse
    {
        /// <summary>
        /// The value of the temperature response factor for a given parameter
        /// </summary>
        public double Factor;

        /// <summary>
        /// The value of the temperature response factor at 25 degrees
        /// </summary>
        public double At25;
    }
}
