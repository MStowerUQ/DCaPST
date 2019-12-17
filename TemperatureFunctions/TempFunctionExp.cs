using System;

namespace LayerCanopyPhotosynthesis
{
    public static class TemperatureFunction
    {
        public static double Val(double temp, double P25, double c, double tMax, double tMin, double tOpt, double beta)
        {
            double alpha = Math.Log(2) / (Math.Log((tMax - tMin) / (tOpt - tMin)));
            double numerator = 2 * Math.Pow((temp - tMin), alpha) * Math.Pow((tOpt - tMin), alpha) - Math.Pow((temp - tMin), 2 * alpha);
            double denominator = Math.Pow((tOpt - tMin), 2 * alpha);
            double funcT = P25 * Math.Pow(numerator / denominator, beta) / c;

            return funcT;
        }

        public static double Val2(double temp, double P25, double tMin)
        {
            return P25 * Math.Exp(tMin * (temp + 273 - 298.15) / (298.15 * 8.314 * (temp + 273)));
        }
    }
}
