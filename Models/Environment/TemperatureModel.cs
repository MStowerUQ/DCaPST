using System;
using DCAPST.Environment;

namespace DCAPST
{
    public class TemperatureModel
    {
        public SolarGeometryModel Solar;
        public RadiationModel Radiation;

        public double AbsoluteTemperature { get; } = 273;
        public double AtmosphericPressure { get; set; } = 1.01325;
        public double MaxTemperature { get; set; }
        public double MinTemperature { get; set; }

        public double XLag { get; set; } = 1.8;
        public double YLag { get; set; } = 2.2;
        public double ZLag { get; set; } = 1;

        public double AirTemperature { get; set; }
        public double Rair => AtmosphericPressure * 100000 / (287 * (AirTemperature + 273)) * 1000 / 28.966;

        public TemperatureModel(SolarGeometryModel solar, double maxTemperature, double minTemperature)
        {
            Solar = solar;
            MaxTemperature = maxTemperature;
            MinTemperature = minTemperature;

            // Initialise the air temperature at 6 AM
            UpdateAirTemperature(6.0);
        }

        public void UpdateAirTemperature(double time)
        {
            double timeOfMinT = 12.0 - Solar.DayLength / 2.0 + ZLag;

            if /*DAY*/ (timeOfMinT < time && time < Solar.Sunset)
            {
                double m = time - timeOfMinT;
                AirTemperature = (MaxTemperature - MinTemperature) * Math.Sin((Math.PI * m) / (Solar.DayLength + 2 * XLag)) + MinTemperature;
            }
            else /*NIGHT*/
            {
                double n = time - Solar.Sunset;
                if (n < 0) n += 24;

                double temperatureDifference = (MaxTemperature - MinTemperature) * Math.Sin(Math.PI * (Solar.DayLength - ZLag) / (Solar.DayLength + 2 * XLag));
                AirTemperature = MinTemperature + temperatureDifference * Math.Exp(-YLag * n / (24.0 - Solar.DayLength));
            }
        }
    }
}
