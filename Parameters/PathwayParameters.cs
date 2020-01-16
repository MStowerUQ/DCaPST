using System;
using DCAPST.Canopy;
using DCAPST.Interfaces;

namespace DCAPST
{
    public abstract class PathwayParameters : IPathwayParameters
    {
        public ICanopyParameters Canopy { get; set; } 

        public double CiCaRatio { get; set; } = 0.7;
        public double CiCaRatioIntercept { get; set; } = 0.90;
        public double CiCaRatioSlope { get; set; } = -0.12;
        public double Fcyc { get; set; } = 0;        
        public double PsiRd { get; set; } = 0.0175;        
        public double PsiVc { get; set; } = 1.75;        
        public double PsiJ { get; set; } = 3.20;        
        public double PsiVp { get; set; } = 3.39;
        public double PsiGm { get; set; } = 0.005296;
        public double X { get; set; } = 0.4;
        public double z { get; set; } = 0.0;

        // KineticParams       
        public double F2 { get; set; } = 0.75;
        public double F1 { get; set; } = 0.95;
        public double Phi { get; set; } = 2;

        // Curvilinear Temperature Model
        public double KcP25 { get; set; } = 267.9295;
        public double KcTEa { get; set; } = 0.0;
        public double KoP25 { get; set; } = 164991.8069;        
        public double KoTEa { get; set; } = 0.0;
        public double VcTEa { get; set; } = 0.0;
        public double JMaxC { get; set; } = 0.7991;
        public double JTMax { get; set; } = 42.9922;
        public double JTMin { get; set; } = 0.0;
        public double JTOpt { get; set; } = 31.2390;
        public double JBeta { get; set; } = 1;
        public double VcMax_VoMaxP25 { get; set; } = 4.1672;
        public double VcMax_VoMaxTEa { get; set; } = 0.0;
        public double KpP25 { get; set; } = 160.1404;
        public double KpTEa { get; set; } = 0.0;
        public double VpMaxTEa { get; set; } = 0.0;
        public double RdTEa { get; set; } = 0.0;
        public double GmC { get; set; } = 0.5626;
        public double GmTMax { get; set; } = 42.7227;
        public double GmTMin { get; set; } = 0.0;
        public double GmTOpt { get; set; } = 33.2424;
        public double GmBeta { get; set; } = 1;

        public double F { get; set; }
        public double Alpha { get; set; }
        public double Vpr_l { get; set; }
        public double Gbs_CO2 { get; set; }

        public AssimilationParameters GetAssimilationParams(PartialCanopy canopy)
        {
            if (canopy.Type == CanopyType.Ac1) 
                return GetAc1Params(canopy);
            else if (canopy.Type == CanopyType.Ac2) 
                return GetAc2Params(canopy);
            else 
                return GetAjParams(canopy);
        }

        protected abstract AssimilationParameters GetAc1Params(PartialCanopy canopy);
        protected abstract AssimilationParameters GetAc2Params(PartialCanopy canopy);
        protected abstract AssimilationParameters GetAjParams(PartialCanopy canopy);
        
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
