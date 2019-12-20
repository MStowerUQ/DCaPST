using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

using DCAPST.Environment;

namespace DCAPST
{
    public class TemperatureModel
    {
        public TableFunction Temps { get; set; }

        public SolarGeometryModel Solar;
        public RadiationModel Radiation;

        public double AbsoluteTemperature = 273;
        public double AtmosphericPressure { get; set; } = 1.01325;
        public double MaxTemperature { get; set; }
        public double MinTemperature { get; set; }
        public double MinRelativeHumidity { get; set; }
        public double MaxRelativeHumidity { get; set; } = -1;

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

            CalcTemps();            
        }

        public double GetTemp(double time) => Temps.Value(time - 1);       

        public void CalcTemps()
        {
            var hours = new double[24];
            var temperatures = new double[24];
            
            double timeOfMinT = 12.0 - Solar.DayLength / 2.0 + ZLag;              
            
            for (int time = 1; time <= 24; time++)
            {                
                double temperature;

                if /*DAY*/ (time >= timeOfMinT && time < Solar.Sunset)
                {
                    double m = time - timeOfMinT;
                    temperature = (MaxTemperature - MinTemperature) * Math.Sin((Math.PI * m) / (Solar.DayLength + 2 * XLag)) + MinTemperature;
                }
                else /*NIGHT*/
                {
                    double n = time - Solar.Sunset;
                    if (n < 0)
                        n += 24;

                    double temperatureDifference = (MaxTemperature - MinTemperature) * Math.Sin(Math.PI * (Solar.DayLength - ZLag) / (Solar.DayLength + 2 * XLag));
                    temperature = MinTemperature + temperatureDifference * Math.Exp(-YLag * n / (24.0 - Solar.DayLength));
                }
                hours[time - 1] = (time - 1);
                temperatures[time - 1] = temperature;
            }
            Temps = new TableFunction(hours, temperatures, false);
        }
    }
}
