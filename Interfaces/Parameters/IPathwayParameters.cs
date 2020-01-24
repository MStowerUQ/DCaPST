namespace DCAPST.Interfaces
{
    public interface IPathwayParameters
    {
        double SpectralCorrectionFactor { get; set; }
        double PS2ActivityInBundleSheathFraction { get; set; }
        double PEPRegenerationPerLeaf { get; set; }
        double BundleSheathCO2ConductancePerLeaf { get; set; }

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

        double RubiscoCarboxylationMMConstant25 { get; set; }
        double RubiscoCarboxylationMMConstantTemperatureResponseFactor { get; set; }
        double RubiscoOxygenationMMConstant25 { get; set; }
        double RubiscoOxygenationMMConstantTemperatureResponseFactor { get; set; }
        double RubiscoActivityTemperatureResponseFactor { get; set; }

        double RubiscoCarboxylationToOxygenation25 { get; set; }
        double RubiscoCarboxylationToOxygenationTemperatureResponseFactor { get; set; }
        double PEPcMMConstant25 { get; set; }
        double PEPcMMConstantTemperatureResponseFactor { get; set; }
        double PEPcActivityTemperatureResponseFactor { get; set; }
        double RespirationTemperatureResponseFactor { get; set; }

        ValParameters ElectronTransportRateParams { get; set; }
        ValParameters MesophyllCO2ConductanceParams { get; set; }
    }
}
