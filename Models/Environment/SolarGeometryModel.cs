using System;
using DCAPST.Interfaces;

namespace DCAPST.Environment
{
    public class SolarGeometryModel : ISolarGeometry
    {
        public double Latitude { get; private set; }
        public double SolarDeclination { get; private set; }
        public double SunsetAngle { get; private set; }

        public double SolarConstant { get; } = 1360;

        public double DayOfYear { get; private set; }        
        public double DayLength { get; private set; }
        public double Sunrise { get; private set; }
        public double Sunset { get; private set; }        

        public SolarGeometryModel(double dayOfYear, double latitude)
        {
            if (dayOfYear < 1 || 366 < dayOfYear) throw new Exception("Day of year must lie between 1 and 366");
            if (latitude < -90.0 || 90.0 < latitude) throw new Exception("Latitude cannot exceed 90 degrees");

            DayOfYear = dayOfYear;
            Latitude = latitude.ToRadians();

            SolarDeclination = CalcSolarDeclination();
            SunsetAngle = CalcSunsetAngle();
            DayLength = 2 * SunsetAngle.ToDegrees() / 15;
            Sunrise = 12.0 - DayLength / 2.0;
            Sunset = 12.0 + DayLength / 2.0;
        }     

        private double CalcSolarDeclination() => 23.45.ToRadians() * Math.Sin(2 * Math.PI * (284 + DayOfYear) / 365);

        private double CalcSunsetAngle() => Math.Acos(-1 * Math.Tan(Latitude) * Math.Tan(SolarDeclination));
        
        public double SunAngle(double hour)
        {
            var angle = Math.Asin(Math.Sin(Latitude) * Math.Sin(SolarDeclination)
                + Math.Cos(Latitude)
                * Math.Cos(SolarDeclination)
                * Math.Cos(Math.PI / 12.0 * DayLength * (((hour - Sunrise) / DayLength) - 0.5)));
            return angle;
        }
       
    }
}
