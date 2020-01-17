using System;
using DCAPST.Canopy;
using DCAPST.Interfaces;

namespace DCAPST
{
    public abstract class PathwayParameters : IPathwayParameters
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

        public AssimilationParameters GetAssimilationParams(Assimilation canopy)
        {            
            if (canopy.Type == AssimilationType.Ac1) 
                return GetAc1Params(canopy);
            else if (canopy.Type == AssimilationType.Ac2) 
                return GetAc2Params(canopy);
            else 
                return GetAjParams(canopy);
        }

        protected abstract AssimilationParameters GetAc1Params(Assimilation canopy);
        protected abstract AssimilationParameters GetAc2Params(Assimilation canopy);
        protected abstract AssimilationParameters GetAjParams(Assimilation canopy);
        
    }

    public class AssimilationParameters
    {
        public double p;
        public double q;

        public double x1;
        public double x2;
        public double x3;
        public double x4;
        public double x5;
        public double x6;
        public double x7;
        public double x8;
        public double x9;

        public double m;
        public double t;
        public double sb;
        public double j;
        public double e;
        public double R;

        public double CalculateAssimilation()
        {
            var n1 = R - x1;
            var n2 = m - p * x4;
            var n3 = x5 - x7;

            var a1 = j * q - sb * x2 * x9;
            var a2 = (q * x4 + x6) * x8;            

            var b0 = q * n1 - p;
            var b1 = sb * x9 * (R * x2 - t * x1);
            var b2 = j * (b0 - e * x2 - x3);
            var b3 = n1 * a2 + (n2 - n3) * x8;            

            var c1 = j * (-p * n1 - e * (t * x1 + x2 * R) - R * x3);
            var c2 = x8 * (n1 * n2 + n3 * x1 - x7 * R);

            var a = a1 + a2;
            var b = b1 + b2 + b3;
            var c = c1 + c2;

            return SolveQuadratic(a, b, c);
        }

        public static double SolveQuadratic(double a, double b, double c)
        {
            var root = b * b - 4 * a * c;
            return (-b - Math.Sqrt(root)) / (2 * a);
        }
    }
}
