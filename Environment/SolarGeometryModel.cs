﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace LayerCanopyPhotosynthesis.Environment
{
    public class SolarGeometryModel
    {
        public Angle Latitude { get; set; }
        public double DayOfYear { get; set; }
        public Angle SolarDeclination { get; set; }
        public double SolarConstant { get; set; } = 1360;   
        public Angle SunsetAngle { get; set; }
        public double RadiusVector { get; set; }
        public double DayLength { get; set; }
        public double Sunrise { get; set; }
        public double Sunset { get; set; }        

        public SolarGeometryModel(double dayOfYear, double latitude)
        {
            DayOfYear = dayOfYear;
            Latitude = new Angle(latitude, AngleType.Deg);

            SolarDeclination = CalcSolarDeclination();
            SunsetAngle = CalcSunsetAngle();
            DayLength = (SunsetAngle.Deg / 15) * 2;
            Sunrise = 12.0 - DayLength / 2.0;
            Sunset = 12.0 + DayLength / 2.0;
        }

        public double CalcExtraTerrestrialRadiation() => 24.0 / Math.PI
            * (3600.0 * SolarConstant / Math.Pow(1.0 / Math.Sqrt(1 + (0.033 * Math.Cos(360.0 * DayOfYear / 365.0))), 2))
            * (SunsetAngle.Rad * Math.Sin(Latitude.Rad) * Math.Sin(SolarDeclination.Rad) + Math.Sin(SunsetAngle.Rad) * Math.Cos(Latitude.Rad) * Math.Cos(SolarDeclination.Rad))
            / 1000000.0;            

        public Angle CalcSolarDeclination() => new Angle(23.45 * Math.Sin(2 * Math.PI * (284 + DayOfYear) / 365), AngleType.Deg);

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