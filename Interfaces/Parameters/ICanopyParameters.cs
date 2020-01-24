namespace DCAPST.Interfaces
{
    public enum CanopyType { C3, C4, CCM }

    public interface ICanopyParameters
    {
        CanopyType Type { get; set; }

        IPathwayParameters Pathway { get; set; }

        double AirCO2 { get; set; }
        double Windspeed { get; set; }
        double LeafAngle { get; set; }
        double DiffuseExtCoeff { get; set; }
        double DiffuseExtCoeffNIR { get; set; }
        double LeafScatteringCoeff { get; set; }
        double LeafScatteringCoeffNIR { get; set; }
        double DiffuseReflectionCoeff { get; set; }
        double DiffuseReflectionCoeffNIR { get; set; }
        double LeafWidth { get; set; }
        double WindSpeedExtinction { get; set; }
        double ConvexityFactor { get; set; }
        double DiffusivitySolubilityRatio { get; set; }

        double StructuralN { get; set; }
        double SLNRatioTop { get; set; }
        double SLNAv { get; set; }
    }
}
