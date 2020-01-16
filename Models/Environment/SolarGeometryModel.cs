using System;
using DCAPST.Interfaces;

namespace DCAPST.Environment
{
    public class SolarGeometryModel : ISolarGeometry
    {
        public Angle Latitude { get; private set; }
        public Angle SolarDeclination { get; private set; }
        public Angle SunsetAngle { get; private set; }

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
            Latitude = new Angle(latitude, AngleType.Deg);

            SolarDeclination = CalcSolarDeclination();
            SunsetAngle = CalcSunsetAngle();
            DayLength = (SunsetAngle.Deg / 15) * 2;
            Sunrise = 12.0 - DayLength / 2.0;
            Sunset = 12.0 + DayLength / 2.0;
        }     

        private Angle CalcSolarDeclination() => new Angle(23.45 * Math.Sin(2 * Math.PI * (284 + DayOfYear) / 365), AngleType.Deg);

        private Angle CalcSunsetAngle() => new Angle(Math.Acos(-1 * Math.Tan(Latitude.Rad) * Math.Tan(SolarDeclination.Rad)), AngleType.Rad);
        
        public Angle SunAngle(double hour)
        {
            var angle = Math.Asin(Math.Sin(Latitude.Rad) * Math.Sin(SolarDeclination.Rad)
                + Math.Cos(Latitude.Rad)
                * Math.Cos(SolarDeclination.Rad)
                * Math.Cos(Math.PI / 12.0 * DayLength * (((hour - Sunrise) / DayLength) - 0.5)));
            return new Angle(angle, AngleType.Rad);
        }
       
    }
}
