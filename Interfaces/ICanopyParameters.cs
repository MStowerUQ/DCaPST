namespace DCAPST.Interfaces
{
    public interface ICanopyParameters
    {
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
        double Vpr_l { get; set; }
        double Theta { get; set; }
        double F { get; set; }
        double Alpha { get; set; }
        double Constant { get; set; }
        double Gbs_CO2 { get; set; }
        double Sigma { get; set; }
        double Rcp { get; set; }
        double PsychrometricConstant { get; set; }
        double Lambda { get; set; }
    }
}
