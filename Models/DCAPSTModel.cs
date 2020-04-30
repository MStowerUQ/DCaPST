using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DCAPST.Canopy;
using DCAPST.Interfaces;

namespace DCAPST
{
    public class DCAPSTModel : IPhotosynthesisModel
    {
        /// <summary>
        /// The solar geometry
        /// </summary>
        private ISolarGeometry Solar { get; set; }

        /// <summary>
        /// The solar radiation
        /// </summary>
        private ISolarRadiation Radiation { get; set; }

        /// <summary>
        /// The environmental temperature
        /// </summary>
        private ITemperature Temperature { get; set; }
        
        /// <summary>
        /// The canopy undergoing photosynthesis
        /// </summary>
        private ICanopyAttributes Canopy { get; set; }

        private IPathwayParameters pathway;

        Transpiration transpiration;

        public bool PrintIntervalValues { get; set; } = false;

        public string IntervalResults { get; private set; } = "";

        /// <summary>
        /// Biochemical Conversion & Maintenance Respiration
        /// </summary>
        public double B { get; set; } = 0.409;

        public double PotentialBiomass { get; private set; }
        public double ActualBiomass { get; private set; }
        public double WaterDemanded { get; private set; }
        public double WaterSupplied { get; private set; }
        public double InterceptedRadiation { get; private set; }

        private readonly double start = 6.0;
        private readonly double end = 18.0;
        private readonly double timestep = 1.0;

        private List<IntervalValues> Intervals = new List<IntervalValues>();

        //private int iterations;

        public DCAPSTModel(
            ISolarGeometry solar, 
            ISolarRadiation radiation, 
            ITemperature temperature, 
            IPathwayParameters pathway,
            ICanopyAttributes canopy,
            Transpiration trans
        )
        {
            Solar = solar;
            Radiation = radiation;
            Temperature = temperature;
            this.pathway = pathway;
            Canopy = canopy;
            transpiration = trans;
        }

        /// <summary>
        /// Calculates the potential and actual biomass growth of a canopy across the span of a day,
        /// as well as the water requirements for both cases.
        /// </summary>
        public void DailyRun(
            double lai,
            double SLN, 
            double soilWater, 
            double RootShootRatio,
            double biolimit = 0,
            double reduction = 0
        )
        {
            for (double x = start; x <= end; x += timestep) Intervals.Add(new IntervalValues() { Time = x });

            Solar.Initialise();
            Canopy.InitialiseDay(lai, SLN);

            // UNLIMITED POTENTIAL CALCULATIONS
            // Note: In the potential case, we assume unlimited water and therefore supply = demand
            transpiration.Limited = false;
            var potential = CalculatePotential();
            var waterDemands = Intervals.Select(i => i.Sunlit.E + i.Shaded.E).ToList();

            // BIO-LIMITED CALCULATIONS

            transpiration.Limited = true;

            // Check if the plant is biologically self-limiting
            if (biolimit > 0)
            {
                // Percentile reduction
                if (reduction > 0)
                {
                    waterDemands = waterDemands.Select(w => ReductionFunction(w, biolimit, reduction)).ToList();
                }
                // Truncation
                else
                {
                    // Reduce to the flat biological limit
                    waterDemands = waterDemands.Select(w => Math.Min(w, biolimit)).ToList();
                }               

                potential = CalculateLimited(waterDemands);
            }

            if (PrintIntervalValues)
            {
                IntervalResults = Solar.DayOfYear.ToString() + ",";
                IntervalResults += string.Join(",", Intervals.Select(i => i.ToString()));
            }

            // ACTUAL CALCULATIONS
            var totalDemand = waterDemands.Sum();
            var limitedSupply = CalculateWaterSupplyLimits(soilWater, waterDemands);

            var actual = (soilWater > totalDemand) ? potential : CalculateActual(limitedSupply.ToArray());

            var hrs_to_seconds = 3600;

            // 1,000,000 mmol to mol
            // 44 mol wt CO2

            ActualBiomass = actual * hrs_to_seconds / 1000000 * 44 * B / (1 + RootShootRatio);
            PotentialBiomass = potential * hrs_to_seconds / 1000000 * 44 * B / (1 + RootShootRatio);
            WaterDemanded = totalDemand;
            WaterSupplied = (soilWater < totalDemand) ? limitedSupply.Sum() : waterDemands.Sum();

            if (PrintIntervalValues) IntervalResults += "," + string.Join(",", Intervals.Select(i => i.ToString()));
        }        

        private double RatioFunction(double A, double B)
        {
            var total = A + B;

            return A / total;
        }

        private double ReductionFunction(double water, double limit, double percent)
        {
            if (water < limit) return water;

            // Find amount of water past the limit
            var excess = water - limit;

            // Reduce the excess by the percentage
            var reduced = excess * percent;

            return limit + reduced;
        }

        /// <summary>
        /// Attempt to initialise models based on the current time, and test if they are sensible
        /// </summary>
        private bool TryInitiliase(double time)
        {
            Temperature.UpdateAirTemperature(time);
            Radiation.UpdateRadiationValues(time);
            var sunAngle = Solar.SunAngle(time);            
            Canopy.DoSolarAdjustment(sunAngle);

            return IsSensible();
        }

        /// <summary>
        /// Tests if the basic conditions for photosynthesis to occur are met
        /// </summary>
        private bool IsSensible()
        {
            var CPath = Canopy.Canopy;
            var temp = Temperature.AirTemperature;

            bool[] tempConditions = new bool[4]
            {
                temp > pathway.ElectronTransportRateParams.TMax,
                temp < pathway.ElectronTransportRateParams.TMin,
                temp > pathway.MesophyllCO2ConductanceParams.TMax,
                temp < pathway.MesophyllCO2ConductanceParams.TMin
            };

            bool invalidTemp = tempConditions.Any(b => b == true);
            bool invalidRadn = Radiation.Total <= double.Epsilon;

            if (invalidTemp || invalidRadn)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Determine the total potential biomass for the day under ideal conditions
        /// </summary>
        public double CalculatePotential()
        {
            foreach (var I in Intervals)
            {
                // Note: double arrays default value is 0.0, which is the intended case if initialisation fails
                if (!TryInitiliase(I.Time))
                {
                    I.Sunlit.A = 0;
                    I.Shaded.A = 0;

                    I.Sunlit.E = 0;
                    I.Shaded.E = 0;

                    continue;
                }

                InterceptedRadiation += Radiation.Total * Canopy.GetInterceptedRadiation() * 3600;

                DoTimestepUpdate(I);
            }

            return Intervals.Select(i => i.Sunlit.A + i.Shaded.A).Sum();
        }

        public double CalculateLimited(IEnumerable<double> demands)
        {
            var ratios = Intervals.Select(i => RatioFunction(i.Sunlit.E, i.Shaded.E));
            double[] sunlitDemand = demands.Zip(ratios, (d, ratio) => d * ratio).ToArray();
            double[] shadedDemand = demands.Zip(ratios, (d, ratio) => d * (1 - ratio)).ToArray();

            foreach (var I in Intervals)
            {
                // Note: double array values default to 0.0, which is the intended case if initialisation fails
                if (!TryInitiliase(I.Time))
                {
                    I.Sunlit.A = 0;
                    I.Shaded.A = 0;

                    I.Sunlit.E = 0;
                    I.Shaded.E = 0;

                    continue;
                }

                int i = Intervals.IndexOf(I);
                
                double total = sunlitDemand[i] + shadedDemand[i];
                transpiration.HourlyMax = total;
                DoTimestepUpdate(I, sunlitDemand[i] / total, shadedDemand[i] / total);
            }

            return Intervals.Select(i => i.Sunlit.A + i.Shaded.A).Sum();
        }

        /// <summary>
        /// Determine the total biomass that can be assimilated under the actual conditions 
        /// </summary>
        public double CalculateActual(double[] waterSupply)
        {
            double[] sunlitDemand = Intervals.Select(i => i.Sunlit.E).ToArray();
            double[] shadedDemand = Intervals.Select(i => i.Shaded.E).ToArray();

            foreach (var I in Intervals)
            {
                // Note: double array values default to 0.0, which is the intended case if initialisation fails
                if (!TryInitiliase(I.Time))
                {
                    I.Sunlit.A = 0;
                    I.Shaded.A = 0;

                    I.Sunlit.E = 0;
                    I.Shaded.E = 0;

                    continue;
                }

                int i = Intervals.IndexOf(I);

                transpiration.HourlyMax = waterSupply[i];
                double total = sunlitDemand[i] + shadedDemand[i];
                DoTimestepUpdate(I, sunlitDemand[i] / total, shadedDemand[i] / total);
            }

            return Intervals.Select(i => i.Sunlit.A + i.Shaded.A).Sum();
        }

        /// <summary>
        /// Updates the model to a new timestep
        /// </summary>
        public void DoTimestepUpdate(IntervalValues interval, double sunFraction = 0, double shadeFraction = 0)
        {
            Canopy.DoTimestepAdjustment(Radiation);

            var totalHeat = Canopy.CalcBoundaryHeatConductance();
            var sunlitHeat = Canopy.CalcSunlitBoundaryHeatConductance();
            
            var shadedHeat =  (totalHeat == sunlitHeat) ? double.Epsilon : totalHeat - sunlitHeat;

            PerformPhotosynthesis(Canopy.Sunlit, sunlitHeat, sunFraction);
            interval.Sunlit = Canopy.Sunlit.Alpha;            

            PerformPhotosynthesis(Canopy.Shaded, shadedHeat, shadeFraction);
            interval.Shaded = Canopy.Shaded.Alpha;
        }

        public void PerformPhotosynthesis(IAssimilationArea area, double gbh, double fraction)
        {
            transpiration.BoundaryHeatConductance = gbh;
            transpiration.Fraction = fraction;
            area.DoPhotosynthesis(Temperature, transpiration);
        }

        /// <summary>
        /// In the case where there is greater water demand than supply allows, the water supply limit for each hour
        /// must be calculated. 
        /// 
        /// This is done by adjusting the maximum rate of water supply each hour, until the total water demand across
        /// the day is within some tolerance of the actual water available, as we want to make use of all the 
        /// accessible water.
        /// </summary>
        private IEnumerable<double> CalculateWaterSupplyLimits(double soilWaterAvail, IEnumerable<double> demand)
        {
            double initialDemand = demand.Sum();

            if (initialDemand < soilWaterAvail) return demand;            
            
            if (soilWaterAvail < 0.0001) return demand.Select(d => 0.0);            
            
            double maxDemandRate = demand.Max();
            double minDemandRate = 0;
            double averageDemandRate = 0;

            double dailyDemand = initialDemand;

            // While the daily demand is outside some tolerance of the available water
            while (dailyDemand < (soilWaterAvail - 0.000001) || (0.000001 + soilWaterAvail) < dailyDemand)
            {
                averageDemandRate = (maxDemandRate + minDemandRate) / 2;

                // Find the total daily demand when the hourly rate is limited to the average rate
                dailyDemand = demand.Select(d => d > averageDemandRate ? averageDemandRate : d).Sum();

                // Find the total daily demand when the hourly rate is limited to the maximum rate
                var maxDemand = demand.Select(d => d > maxDemandRate ? maxDemandRate : d).Sum();

                // If there is more water available than is being demanded, adjust the minimum demand upwards
                if (dailyDemand < soilWaterAvail) minDemandRate = averageDemandRate;
                // Else, there is less water available than is being demanded, so adjust the maximum demand downwards
                else maxDemandRate = averageDemandRate;
            }
            return demand.Select(d => d > averageDemandRate ? averageDemandRate : d);
        }

        public string PrintResultHeader()
        {
            var builder = new StringBuilder();
            builder.Append("DoY,");
            Intervals.ForEach(i => builder.Append(i.PrintHeader("Pot_") + ","));
            Intervals.ForEach(i => builder.Append(i.PrintHeader("") + ","));
            return builder.ToString();
        }
    }

    public class IntervalValues
    {
        /// <summary>
        /// The time of the interval
        /// </summary>
        public double Time { get; set; }

        public AreaAlphaValues Sunlit { get; set; } = new AreaAlphaValues();

        public AreaAlphaValues Shaded { get; set; } = new AreaAlphaValues();

        public override string ToString()
        {            
            return $"{Sunlit},{Shaded}";
        }

        public string PrintHeader(string prefix)
        {
            return $"{Sunlit.Header($"{prefix}sun", $"at {Time}")},{Shaded.Header($"{prefix}sh", $"at {Time}")}";
        }
    }
}
