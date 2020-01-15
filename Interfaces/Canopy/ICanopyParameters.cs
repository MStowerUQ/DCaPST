namespace DCAPST.Interfaces
{
    public interface ICanopyParameters
    {
        IPathwayParameters Path { get; set; }

        double Ca { get; set; }
        double U0 { get; set; }
        double LeafAngle { get; set; }
        double DiffuseExtCoeff { get; set; }
        double DiffuseExtCoeffNIR { get; set; }
        double LeafScatteringCoeff { get; set; }
        double LeafScatteringCoeffNIR { get; set; }
        double DiffuseReflectionCoeff { get; set; }
        double DiffuseReflectionCoeffNIR { get; set; }
        double LeafWidth { get; set; }
        double Ku { get; set; }
        double EmpiricalSpectralCorrectionFactor { get; set; }        
        double DiffusivitySolubilityRatio { get; set; }

        double StructuralN { get; set; }
        double SLNRatioTop { get; set; }
        double SLNAv { get; set; }
    }
}
