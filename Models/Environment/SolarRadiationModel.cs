using System;
using DCAPST.Interfaces;

namespace DCAPST.Environment
{
    public class SolarRadiationModel : ISolarRadiation
    {
        public ISolarGeometry Solar;

        public double FracDiffuseATM { get; } = 0.1725;
        
        /// <summary>
        /// PAR energy fraction
        /// </summary>
        public double RPAR { get; set; }

        public double Total { get; private set; }
        public double Direct { get; private set; }
        public double Diffuse { get; private set; }
        public double DirectPAR { get; private set; }
        public double DiffusePAR { get; private set; }

        /// <summary>
        /// The radiation measured across a day
        /// </summary>
        private readonly double daily;

        public SolarRadiationModel(ISolarGeometry solar, double dailyRadiation)
        {
            Solar = solar ?? throw new Exception();
            daily = (dailyRadiation >= 0) ? dailyRadiation : throw new Exception();

            // Initialise radiation at 6 AM
            UpdateRadiationValues(6.0);
        }

        public void UpdateRadiationValues(double time)
        {
            if (time < 0 || 24 < time) throw new Exception("Time must be between 0 and 24");
            //if (RPAR <= 0) throw new Exception("RPAR must be greater than 0");
            //if (RPAR > 1) throw new Exception("RPAR must not exceed 1.0");

            Total = CurrentTotal(time);
            Diffuse = CurrentDiffuse(time);
            Direct = Total - Diffuse;

            // Photon count
            DiffusePAR = Diffuse * RPAR * 4.25 * 1E6;
            DirectPAR = Direct * RPAR * 4.56 * 1E6;
        }

        /// <summary>
        /// Finds the total radiation value at the current time
        /// </summary>
        private double CurrentTotal(double time)
        {
            double dawn = Math.Floor(Solar.Sunrise);
            double dusk = Math.Ceiling(Solar.Sunset);

            if (time < dawn || dusk < time) return 0;

            var theta = Math.PI * (time - Solar.Sunrise) / Solar.DayLength;
            var factor = Math.Sin(theta) * Math.PI / 2;
            var radiation = daily / (Solar.DayLength * 3600);
            var incident = radiation * factor;

            if (incident < 0) return 0;

            return incident;
        }

        /// <summary>
        /// Finds the diffuse radiation value at the current time
        /// </summary>
        private double CurrentDiffuse(double time)
        {
            var diffuse = Math.Max(FracDiffuseATM * Solar.SolarConstant * Math.Sin(Solar.SunAngle(time)) / 1000000, 0);

            if (diffuse > Total)
                return Total;
            else
                return diffuse;
        }

    }
}
