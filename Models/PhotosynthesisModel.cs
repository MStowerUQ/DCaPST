﻿using System;
using System.Linq;

using DCAPST.Canopy;
using DCAPST.Interfaces;

namespace DCAPST
{
    public class PhotosynthesisModel
    {
        public ISolarGeometry Solar { get; set; }
        public IRadiation Radiation { get; set; }
        public ITemperature Temperature { get; set; }

        public TotalCanopy Canopy;

        public double B { get; set; } = 0.409;

        private readonly double start = 6.0;
        private readonly double end = 18.0;
        private readonly double timestep = 1.0;
        private readonly int iterations;

        public PhotosynthesisModel(ISolarGeometry solar, IRadiation radiation, ITemperature temperature, ICanopyParameters canopy)
        {
            Solar = solar;
            Radiation = radiation;
            Temperature = temperature;

            int layers = 1;
            if (layers <= 0) throw new Exception("There must be at least 1 layer");

            Canopy = new TotalCanopy(canopy, layers);

            iterations = (int)Math.Floor(1.0 + ((end - start) / timestep));
        }

        public double[] DailyRun(
            double lai,
            double SLN, 
            double soilWater, 
            double RootShootRatio, 
            double MaxHourlyTRate = 100)
        {            
            Canopy.Initialise(lai, SLN);

            // POTENTIAL CALCULATIONS
            // Note: In the potential case, we assume unlimited water and therefore supply = demand
            CalculatePotential(out double intercepted, out double[] assimilations, out double[] sunlitDemand, out double[] shadedDemand);
            var waterSupply = sunlitDemand.Zip(shadedDemand, (x, y) => x + y).ToArray();
            var potential = assimilations.Sum();
            var totalDemand = waterSupply.Sum();

            // ACTUAL CALCULATIONS
            // Limit water to supply available from Apsim
            double maxHourlyT = Math.Min(waterSupply.Max(), MaxHourlyTRate);
            waterSupply = waterSupply.Select(w => w > maxHourlyT ? maxHourlyT : w).ToArray();

            var limitedSupply = CalculateWaterSupplyLimits(soilWater, maxHourlyT, waterSupply);

            var actual = (soilWater > totalDemand) ? potential : CalculateActual(limitedSupply, sunlitDemand, shadedDemand);

            double[] results = new double[5];
            results[0] = actual * 3600 / 1000000 * 44 * B * 100 / ((1 + RootShootRatio) * 100);
            results[1] = totalDemand;
            results[2] = (soilWater < totalDemand) ? limitedSupply.Sum() : waterSupply.Sum();
            results[3] = intercepted;
            results[4] = potential * 3600 / 1000000 * 44 * B * 100 / ((1 + RootShootRatio) * 100);

            return results;
        }

        private bool TryInitiliase(double time)
        {
            Temperature.UpdateAirTemperature(time);
            Radiation.UpdateHourlyRadiation(time);
            var sunAngle = Solar.SunAngle(time).Rad;            
            Canopy.CalcCanopyStructure(sunAngle);

            return IsSensible();
        }

        private bool IsSensible()
        {
            var CPath = Canopy.Canopy;
            var temp = Temperature.AirTemperature;

            bool invalidTemp = temp > CPath.Pathway.J.TMax || temp < CPath.Pathway.J.TMin || temp > CPath.Pathway.Gm.TMax || temp < CPath.Pathway.Gm.TMin;
            bool invalidRadn = Radiation.TotalIncidentRadiation <= double.Epsilon;

            if (invalidTemp || invalidRadn)
                return false;
            else
                return true;
        }        

        public void CalculatePotential(out double intercepted, out double[] assimilations, out double[] sunlitDemand, out double[] shadedDemand)
        {
            // Water demands
            intercepted = 0.0;
            sunlitDemand = new double[iterations];
            shadedDemand = new double[iterations];
            assimilations = new double[iterations];

            for (int i = 0; i < iterations; i++)
            {
                double time = start + i * timestep;

                // Note: double arrays default value is 0.0, which is the intended case if initialisation fails
                if (!TryInitiliase(time)) continue;

                intercepted += Radiation.TotalIncidentRadiation * Canopy.PropnInterceptedRadns * 3600;

                DoHourlyCalculation();

                sunlitDemand[i] = Canopy.Sunlit.WaterUse;
                shadedDemand[i] = Canopy.Shaded.WaterUse;
                assimilations[i] = Canopy.Sunlit.A + Canopy.Shaded.A;
            }
        }

        public double CalculateActual(double[] waterSupply, double[] sunlitDemand, double[] shadedDemand)
        {
            double assimilation = 0.0;
            for (int i = 0; i < iterations; i++)
            {
                double time = start + i * timestep;

                // Note: double array values default to 0.0, which is the intended case if initialisation fails
                if (!TryInitiliase(time)) continue;

                double total = sunlitDemand[i] + shadedDemand[i];
                DoHourlyCalculation(waterSupply[i], sunlitDemand[i] / total, shadedDemand[i] / total);

                assimilation += Canopy.Sunlit.A + Canopy.Shaded.A;
            }
            return assimilation;
        }

        public void DoHourlyCalculation(double maxHourlyT = -1, double sunFraction = 0, double shadeFraction = 0)
        {
            var Params = new PhotosynthesisParams
            {
                maxHourlyT = maxHourlyT,
                limited = false
            };
            if (maxHourlyT != -1) Params.limited = true;

            Canopy.Run(Radiation);

            var gbh = Canopy.CalcGbh();
            var sunlitGbh = Canopy.CalcSunlitGbh();

            Params.Gbh = sunlitGbh;
            Params.fraction = sunFraction;
            Canopy.Sunlit.CalcPartialPhotosynthesis(Temperature, Params);

            Params.Gbh = gbh - sunlitGbh;
            Params.fraction = shadeFraction;
            Canopy.Shaded.CalcPartialPhotosynthesis(Temperature, Params);
        }

        /// <summary>
        /// In the case where there is greater water demand than supply allows, the water supply limit for each hour
        /// must be calculated. 
        /// 
        /// This is done by adjusting the maximum rate of water supply each hour, until the total water demand across
        /// the day is within some tolerance of the actual water available, as we want to make use of all the 
        /// accessible water.
        /// </summary>
        private double[] CalculateWaterSupplyLimits(double soilWaterAvail, double maxHourlyT, double[] demand)
        {
            double initialDemand = demand.Sum();
            if (soilWaterAvail < 0.0001) return demand.Select(d => 0.0).ToArray();
            if (initialDemand < soilWaterAvail) return demand;
            
            double maxDemandRate = maxHourlyT;
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
            return demand.Select(d => d > averageDemandRate ? averageDemandRate : d).ToArray();
        }
    }

    public struct PhotosynthesisParams
    {
        public bool limited;
        public double Gbh;
        public double maxHourlyT;
        public double fraction;
    }
}
