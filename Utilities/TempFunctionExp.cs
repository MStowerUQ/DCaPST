using System;

namespace DCAPST
{
    public static class TemperatureFunction
    {
        public static double Val(double temp, double P25, ValParameters p)
        {
            double alpha = Math.Log(2) / (Math.Log((p.TMax - p.TMin) / (p.TOpt - p.TMin)));
            double numerator = 2 * Math.Pow((temp - p.TMin), alpha) * Math.Pow((p.TOpt - p.TMin), alpha) - Math.Pow((temp - p.TMin), 2 * alpha);
            double denominator = Math.Pow((p.TOpt - p.TMin), 2 * alpha);
            double funcT = P25 * Math.Pow(numerator / denominator, p.Beta) / p.C;

            return funcT;
        }

        public static double Val2(double temp, double P25, double tMin)
        {
            return P25 * Math.Exp(tMin * (temp + 273 - 298.15) / (298.15 * 8.314 * (temp + 273)));
        }
    }

    public struct ValParameters
    {
        public double C;
        public double TMax;
        public double TMin;
        public double TOpt;
        public double Beta;
    }
}
