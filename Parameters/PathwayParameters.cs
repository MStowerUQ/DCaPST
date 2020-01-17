using DCAPST.Interfaces;

namespace DCAPST
{   
    public class PathwayParameters : IPathwayParameters
    {
        public ICanopyParameters Canopy { get; set; } 

        public double CiCaRatio { get; set; }
        public double Fcyc { get; set; }        
        public double PsiRd { get; set; }        
        public double PsiVc { get; set; }        
        public double PsiJ { get; set; }        
        public double PsiVp { get; set; }
        public double PsiGm { get; set; }
        public double X { get; set; }
        public double z { get; set; }
       
        public double Phi { get; set; }

        public double KcP25 { get; set; }
        public double KcTEa { get; set; }
        public double KoP25 { get; set; }        
        public double KoTEa { get; set; }
        public double VcTEa { get; set; }
        
        public double VcMax_VoMaxP25 { get; set; }
        public double VcMax_VoMaxTEa { get; set; }
        public double KpP25 { get; set; }
        public double KpTEa { get; set; }
        public double VpMaxTEa { get; set; }
        public double RdTEa { get; set; }

        public ValParameters J { get; set; }
        public ValParameters Gm { get; set; }

        public double SpectralCorrectionFactor { get; set; }
        public double Alpha { get; set; }
        public double Vpr_l { get; set; }
        public double Gbs_CO2 { get; set; }       
    }
}
