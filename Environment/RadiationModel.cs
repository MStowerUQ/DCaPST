using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace LayerCanopyPhotosynthesis.Environment
{
    public class RadiationModel
    {
        public double SolarRadiation { get; set; }
        public double ExtraTerrestrialRadiation { get; set; }
        public double FracDiffuseATM { get; set; } = 0.1725;
        public double RPAR { get; set; } = 0.5;
        public double TotalIncidentRadiation { get; set; }
        public double DirectRadiation { get; set; }
        public double DiffuseRadiation { get; set; }
        public double DirectRadiationPAR { get; set; }
        public double DiffuseRadiationPAR { get; set; }

        public TableFunction Ios { get; set; }
        public TableFunction Idirs { get; set; }
        public TableFunction Idiffs { get; set; }
        public TableFunction Ios_PAR { get; set; }
        public TableFunction Idirs_PAR { get; set; }
        public TableFunction Idiffs_PAR { get; set; }
        public TableFunction Ratios { get; set; }

        public SolarGeometryModel Solar;

        private readonly double[] times = Enumerable.Range(0, 24).Select(i => (double)i).ToArray();

        public RadiationModel(SolarGeometryModel solar, double solarRadiation)
        {
            Solar = solar;

            SolarRadiation = solarRadiation;
            ExtraTerrestrialRadiation = Solar.CalcExtraTerrestrialRadiation();

            CalcRatios();
            CalcIncidentRadns();
            CalcDiffuseRadns();
            CalcDirectRadns();
            ConvertRadiationsToPAR();
        }

        public void CalcRatios()
        {
            double ratio = SolarRadiation / ExtraTerrestrialRadiation;

            var ratios = times.Select(t => ratio).ToArray();
            Ratios = new TableFunction(times, ratios);
        }

        public void UpdateIncidentRadiation(double hour)
        {
            TotalIncidentRadiation = Ios.Value(hour);

            DirectRadiation = Idirs.Value(hour);
            DiffuseRadiation = Idiffs.Value(hour);

            DiffuseRadiationPAR = Idiffs_PAR.Value(hour);
            DirectRadiationPAR = Idirs_PAR.Value(hour);
        }

        public double CalcInstantaneousIncidentRadiation(double hour) =>
            ExtraTerrestrialRadiation * Ratios.Value(hour) * Math.PI * Math.Sin(Math.PI * (hour - Solar.Sunrise) / Solar.DayLength) / (2 * Solar.DayLength * 3600);

        void CalcIncidentRadns()
        {
            double dawn = Math.Floor(12 - Solar.DayLength / 2.0);
            double dusk = Math.Ceiling(12 + Solar.DayLength / 2.0);

            var ios = times.Select(t => (t < dawn || dusk < t) ? 0 : Math.Max(CalcInstantaneousIncidentRadiation(t), 0)).ToArray();
            
            Ios = new TableFunction(times, ios, false);
        }

        void CalcDiffuseRadns()
        {
            var diffs = times.Select(t =>
            {
                var diff = Math.Max(FracDiffuseATM * Solar.SolarConstant * Math.Sin(Solar.SunAngle(t).Rad) / 1000000, 0);
                var value = Ios.Value(t);
                return diff > value ? value : diff;
            }).ToArray();

            Idiffs = new TableFunction(times, diffs, false);
        }

        void CalcDirectRadns()
        {
            var dirs = times.Select(t => Ios.Value(t) - Idiffs.Value(t)).ToArray();
            Idirs = new TableFunction(times, dirs, false);
        }

        private void ConvertRadiationsToPAR()
        {            
            var idiff_par = times.Select(t => Idiffs.Value(t) * RPAR * 4.25 * 1E6).ToArray();
            var idir_par = times.Select(t => Idirs.Value(t) * RPAR * 4.56 * 1E6).ToArray();
            var io_par = times.Select(t => idiff_par[(int)t] + idir_par[(int)t]).ToArray();

            Ios_PAR = new TableFunction(times, io_par, false);
            Idiffs_PAR = new TableFunction(times, idiff_par, false);
            Idirs_PAR = new TableFunction(times, idir_par, false);
        }

    }
}
