using System;

namespace DCAPST.Environment
{
    public class RadiationModel
    {
        public double FracDiffuseATM { get; set; } = 0.1725;
        public double RPAR { get; set; } = 0.5;

        public double ExtraTerrestrialRadiation { get; set; }
        public double TotalIncidentRadiation { get; private set; }
        public double DirectRadiation { get; private set; }
        public double DiffuseRadiation { get; private set; }
        public double DirectRadiationPAR { get; private set; }
        public double DiffuseRadiationPAR { get; private set; }

        public double Ratio { get; set; }

        public SolarGeometryModel Solar;

        public RadiationModel(SolarGeometryModel solar, double solarRadiation)
        {
            Solar = solar;
            ExtraTerrestrialRadiation = Solar.CalcExtraTerrestrialRadiation();
            Ratio = solarRadiation / ExtraTerrestrialRadiation;

            // Initialise radiation at 6 AM
            UpdateHourlyRadiation(6.0);
        }

        public void UpdateHourlyRadiation(double time)
        {
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

            var x = Math.PI * (time - Solar.Sunrise) / Solar.DayLength;
            var y = Math.PI * Math.Sin(x) / (2 * Solar.DayLength * 3600);
            var radiation = ExtraTerrestrialRadiation * Ratio * y;

            if (radiation < 0) return 0;

            return radiation;
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
