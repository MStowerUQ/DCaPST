namespace DCAPST.Interfaces
{
    public interface IPathwayParameters
    {       
        double IntercellularToAirCO2Ratio { get; set; }
        double FractionOfCyclicElectronFlow { get; set; }
        double RespirationSLNRatio { get; set; }
        double MaxRubiscoActivitySLNRatio { get; set; }
        double MaxElectronTransportSLNRatio { get; set; }
        double MaxPEPcActivitySLNRatio { get; set; }
        double MesophyllCO2ConductanceSLNRatio { get; set; }
        double MesophyllElectronTransportFraction { get; set; }
        double ATPProductionElectronTransportFactor { get; set; }
 
        double ExtraATPCost { get; set; }

        TemperatureResponse RubiscoCarboxylation { get; set; }
        TemperatureResponse RubiscoOxygenation { get; set; }        
        TemperatureResponse RubiscoCarboxylationToOxygenation { get; set; }
        TemperatureResponse RubiscoActivity { get; set; }

        TemperatureResponse PEPc { get; set; }
        TemperatureResponse PEPcActivity { get; set; }
        TemperatureResponse Respiration { get; set; }

        ValParameters ElectronTransportRateParams { get; set; }
        ValParameters MesophyllCO2ConductanceParams { get; set; }

        double SpectralCorrectionFactor { get; set; }
        double PS2ActivityInBundleSheathFraction { get; set; }
        double PEPRegenerationPerLeaf { get; set; }
        double BundleSheathCO2ConductancePerLeaf { get; set; }
    }
}
