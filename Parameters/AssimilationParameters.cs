using System;
using DCAPST.Interfaces;

namespace DCAPST
{   
    public class AssimilationParameters
    {
        // These are all just dummy variables
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
