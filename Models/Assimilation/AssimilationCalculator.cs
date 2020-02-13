using System;
using DCAPST.Interfaces;

namespace DCAPST
{   
    public class AssimilationCalculator
    {
        // These are all just dummy variables
        public double p;
        public double q;

        public double[] X;

        public double m;
        public double t;
        public double sb;
        public double j;
        public double e;
        public double R;

        public double CalculateAssimilation()
        {
            if (X.Length != 9) throw new Exception("Invalid assimilation terms");

            var n1 = R - X[0];
            var n2 = m - p * X[3];
            var n3 = X[4] - X[6];

            var a1 = j * q - sb * X[1] * X[8];
            var a2 = (q * X[3] + X[5]) * X[7];            

            var b0 = q * n1 - p;
            var b1 = sb * X[8] * (R * X[1] - t * X[0]);
            var b2 = j * (b0 - e * X[1] - X[2]);
            var b3 = a2 * n1 + (n2 - n3) * X[7];

            var c1 = X[7] * (n1 * n2 + n3 * X[0] - X[6] * R);
            var c2 = j * (p * n1 + e * (t * X[0] + X[1] * R) + R * X[2]);            

            var a = a1 + a2;
            var b = b1 + b2 + b3;
            var c = c1 - c2;

            return SolveQuadratic(a, b, c);
        }

        private static double SolveQuadratic(double a, double b, double c)
        {
            var root = b * b - 4 * a * c;
            return (-b - Math.Sqrt(root)) / (2 * a);
        }
    }
}
