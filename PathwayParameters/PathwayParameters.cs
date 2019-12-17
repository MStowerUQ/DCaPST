using System;
using LayerCanopyPhotosynthesis.Canopy;
namespace LayerCanopyPhotosynthesis
{
    public abstract class PathwayParameters
    {
        public double StructuralN { get; set; } = 25;
        public double SLNRatioTop { get; set; } = 1.32;
        public double SLNAv { get; set; } = 1.45;

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

        public static double CalculateAssimilation(AssimilationParameters s)
        {
            double a, b, d;

            a = s.b * s.R * s.x_2 * s.x_9
                - s.b * s.t * s.x_1 * s.x_9
                - s.j * s.p
                + s.j * s.q * s.R
                - s.j * s.q * s.x_1
                - s.j * s.e * s.x_2
                - s.j * s.x_3 + s.m * s.x_8
                - s.p * s.x_4 * s.x_8
                + s.q * s.R * s.x_4 * s.x_8
                - s.q * s.x_1 * s.x_4 * s.x_8
                + s.R * s.x_6 * s.x_8
                - s.x_1 * s.x_6 * s.x_8
                - s.x_5 * s.x_8
                + s.x_7 * s.x_8;

            d = -s.b * s.x_2 * s.x_9
                + s.j * s.q
                + s.q * s.x_4 * s.x_8
                + s.x_6 * s.x_8;

            b = d *
                (-s.j * s.p * s.R
                + s.j * s.p * s.x_1
                - s.j * s.e * s.R * s.x_2
                - s.j * s.R * s.x_3
                - s.j * s.e * s.t * s.x_1
                + s.m * s.R * s.x_8
                - s.m * s.x_1 * s.x_8
                - s.p * s.R * s.x_4 * s.x_8
                + s.p * s.x_1 * s.x_4 * s.x_8
                - s.R * s.x_7 * s.x_8
                + s.x_1 * s.x_5 * s.x_8
                - s.x_1 * s.x_7 * s.x_8);

            // TODO: Explain the concept of factoring common terms to the person who wrote this

            return SolveQuadratic(a, b, d);
        }

        public static double SolveQuadratic(double a, double b, double d)
        {
            return (-1 * Math.Pow((Math.Pow(a, 2) - 4 * b), 0.5) - a) / (2 * d);
        }
    }

    public struct AssimilationParameters
    {
        public double p;
        public double q;

        public double x_1;
        public double x_2;
        public double x_3;
        public double x_4;
        public double x_5;
        public double x_6;
        public double x_7;
        public double x_8;
        public double x_9;

        public double m;
        public double t;
        public double b;
        public double j;
        public double e;
        public double R;
    }
}
