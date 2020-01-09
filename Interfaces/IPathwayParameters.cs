using DCAPST.Canopy;

namespace DCAPST.Interfaces
{
    public interface IPathwayParameters
    {
        ICanopyParameters Canopy { get; set; }

        double StructuralN { get; set; }
        double SLNRatioTop { get; set; }
        double SLNAv { get; set; }

        double CiCaRatio { get; set; }
        double CiCaRatioIntercept { get; set; }
        double CiCaRatioSlope { get; set; }
        double Fcyc { get; set; }
        double PsiRd { get; set; }
        double PsiVc { get; set; }
        double PsiJ { get; set; }
        double PsiVp { get; set; }
        double PsiGm { get; set; }
        double X { get; set; }
        double z { get; set; }

        // KineticParams       
        double F2 { get; set; }
        double F1 { get; set; }
        double Phi { get; set; }

        // Curvilinear Temperature Model
        double KcP25 { get; set; }
        double KcTEa { get; set; }
        double KoP25 { get; set; }
        double KoTEa { get; set; }
        double VcTEa { get; set; }
        double JMaxC { get; set; }
        double JTMax { get; set; }
        double JTMin { get; set; }
        double JTOpt { get; set; }
        double JBeta { get; set; }
        double VcMax_VoMaxP25 { get; set; }
        double VcMax_VoMaxTEa { get; set; }
        double KpP25 { get; set; }
        double KpTEa { get; set; }
        double VpMaxTEa { get; set; }
        double RdTEa { get; set; }
        double GmC { get; set; }
        double GmTMax { get; set; }
        double GmTMin { get; set; }
        double GmTOpt { get; set; }
        double GmBeta { get; set; }

        AssimilationParameters GetAssimilationParams(PartialCanopy canopy);

        double CalculateAssimilation(AssimilationParameters s);
    }
}
