using System;
using DCAPST.Interfaces;

namespace DCAPST.Environment
{
    public class RadiationModel : IRadiation
    {
        public ISolarGeometry Solar;

        public double FracDiffuseATM { get; } = 0.1725;
        
        /// <summary>
        /// PAR energy fraction
        /// </summary>
        public double RPAR { get; set; }

        public double TotalIncidentRadiation { get; private set; }
        public double Direct { get; private set; }
        public double Diffuse { get; private set; }
        public double DirectPAR { get; private set; }
        public double DiffusePAR { get; private set; }

        public double DailyRadiation { get; private set; }

        public RadiationModel(ISolarGeometry solar, double dailyRadiation)
        {
            Solar = solar ?? throw new Exception();
            DailyRadiation = (dailyRadiation >= 0) ? dailyRadiation : throw new Exception();

            // Initialise radiation at 6 AM
            UpdateHourlyRadiation(6.0);
        }

        public void UpdateHourlyRadiation(double time)
        {
            if (time < 0 || 24 < time) throw new Exception("Time must be between 0 and 24");

            TotalIncidentRadiation = CalcTotalIncidentRadiation(time);
            Diffuse = CalcDiffuseRadiation(time);
            Direct = TotalIncidentRadiation - Diffuse;

            // Photon count
            DiffusePAR = Diffuse * RPAR * 4.25 * 1E6;
            DirectPAR = Direct * RPAR * 4.56 * 1E6;
        }

        private double CalcTotalIncidentRadiation(double time)
        {
            double dawn = Math.Floor(Solar.Sunrise);
            double dusk = Math.Ceiling(Solar.Sunset);

            if (time < dawn || dusk < time) return 0;

            var theta = Math.PI * (time - Solar.Sunrise) / Solar.DayLength;
            var factor = Math.Sin(theta) * Math.PI / 2;
            var radiation = DailyRadiation / (Solar.DayLength * 3600);
            var incident = radiation * factor;

            if (incident < 0) return 0;

            return incident;
        }

        private double CalcDiffuseRadiation(double time)
        {
            var diffuse = Math.Max(FracDiffuseATM * Solar.SolarConstant * Math.Sin(Solar.SunAngle(time).Rad) / 1000000, 0);

            if (diffuse > TotalIncidentRadiation)
                return TotalIncidentRadiation;
            else
                return diffuse;
        }

    }
}
