using System;
using DCAPST.Interfaces;

namespace DCAPST.Environment
{
    public class RadiationModel : IRadiation
    {
        public ISolarGeometry Solar;

        public double FracDiffuseATM = 0.1725;
        public double RPAR { get; set; } // TODO: Parameterise (I)

        public double TotalIncidentRadiation { get; private set; }
        public double DirectRadiation { get; private set; }
        public double DiffuseRadiation { get; private set; }
        public double DirectRadiationPAR { get; private set; }
        public double DiffuseRadiationPAR { get; private set; }

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
            DiffuseRadiation = CalcDiffuseRadiation(time);
            DirectRadiation = TotalIncidentRadiation - DiffuseRadiation;
            DiffuseRadiationPAR = DiffuseRadiation * RPAR * 4.25 * 1E6;
            DirectRadiationPAR = DirectRadiation * RPAR * 4.56 * 1E6;
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
