using System;
using DCAPST.Interfaces;

namespace DCAPST.Environment
{
    public class TemperatureModel : ITemperature
    {
        public ISolarGeometry Solar;

        public double AbsoluteTemperature { get; } = 273;
        public double AtmosphericPressure { get; set; } = 1.01325;
        public double MaxTemperature { get; set; }
        public double MinTemperature { get; set; }

        public double XLag { get; set; } = 1.8;
        public double YLag { get; set; } = 2.2;
        public double ZLag { get; set; } = 1;

        public double AirTemperature { get; set; }
        public double AirMolarDensity => ((AtmosphericPressure * 100000) / (287 * (AirTemperature + 273))) * (1000 / 28.966);

        public TemperatureModel(ISolarGeometry solar, double maxTemperature, double minTemperature)
        {
            Solar = solar ?? throw new Exception("The solar geometry model cannot be null");

            if (maxTemperature < minTemperature) throw new Exception("The maximum cannot be less than the minimum");

            MaxTemperature = maxTemperature;
            MinTemperature = minTemperature;            

            // Initialise the air temperature at 6 AM
            UpdateAirTemperature(6.0);
        }

        public void UpdateAirTemperature(double time)
        {
            if (time < 0 || 24 < time) throw new Exception("The time must be between 0 and 24");

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
