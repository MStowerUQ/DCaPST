using System;
using DCAPST.Interfaces;

namespace DCAPST
{   
    public class AssimilationFunction
    {
        // These are all just dummy variables
        public double Ci;
        public double Rm;

        public double[] X;

        public double m;
        public double t;
        public double sb;
        public double j;
        public double e;
        public double R;

        /// <summary>
        /// Solves the assimilation function
        /// </summary>
        public double Value()
        {
            if (X.Length != 9) throw new Exception("Invalid assimilation terms");

            double m = this.m;
            double t = this.t;
            double sb = this.sb;
            double j = this.j;
            double e = this.e;
            double R = this.R;

            var n1 = R - X[0];
            var n2 = m - Ci * X[3];
            var n3 = X[4] - X[6];

            var a1 = j * Rm - sb * X[1] * X[8];
            var a2 = (Rm * X[3] + X[5]) * X[7];            

            var b0 = Rm * n1 - Ci;
            var b1 = sb * X[8] * (R * X[1] - t * X[0]);
            var b2 = j * (b0 - e * X[1] - X[2]);
            var b3 = a2 * n1 + (n2 - n3) * X[7];

            var c1 = X[7] * (n1 * n2 + n3 * X[0] - X[6] * R);
            var c2 = j * (Ci * n1 + e * (t * X[0] + X[1] * R) + R * X[2]);            

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
