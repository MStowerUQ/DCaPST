﻿using System;
using System.Linq;

using DCAPST.Canopy;
using DCAPST.Interfaces;

namespace DCAPST
{
    public class PhotosynthesisModel
    {
        public ISolarGeometry Solar { get; set; }
        public ISolarRadiation Radiation { get; set; }
        public ITemperature Temperature { get; set; }
        public ITotalCanopy Canopy { get; set; }

        /// <summary>
        /// Biochemical Conversion & Maintenance Respiration
        /// </summary>
        public double B { get; set; } = 0.409;

        private readonly double start = 6.0;
        private readonly double end = 18.0;
        private readonly double timestep = 1.0;
        private readonly int iterations;

        public PhotosynthesisModel(ISolarGeometry solar, ISolarRadiation radiation, ITemperature temperature, ICanopyParameters canopy)
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
            Canopy.InitialiseDay(lai, SLN);

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
            Radiation.UpdateRadiationValues(time);
            var sunAngle = Solar.SunAngle(time);            
            Canopy.CalcCanopyStructure(sunAngle);

            return IsSensible();
        }

        /// <summary>
        /// Check if the basic conditions for photosynthesis to occur are met
        /// </summary>
        private bool IsSensible()
        {
            var CPath = Canopy.Canopy;
            var temp = Temperature.AirTemperature;

            bool[] tempConditions = new bool[4]
            {
                temp > CPath.Pathway.ElectronTransportRateParams.TMax,
                temp < CPath.Pathway.ElectronTransportRateParams.TMin,
                temp > CPath.Pathway.MesophyllCO2ConductanceParams.TMax,
                temp < CPath.Pathway.MesophyllCO2ConductanceParams.TMin
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

                intercepted += Radiation.Total * Canopy.PropnInterceptedRadns * 3600;

                DoHourlyCalculation();

                sunlitDemand[i] = Canopy.Sunlit.WaterUse;
                shadedDemand[i] = Canopy.Shaded.WaterUse;
                assimilations[i] = Canopy.Sunlit.CO2AssimilationRate + Canopy.Shaded.CO2AssimilationRate;
            }
        }

        /// <summary>
        /// Determine the total biomass that can be assimilated under the actual conditions 
        /// </summary>
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

                assimilation += Canopy.Sunlit.CO2AssimilationRate + Canopy.Shaded.CO2AssimilationRate;
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

            Canopy.PerformTimeAdjustment(Radiation);

            var heat = Canopy.CalcBoundaryHeatConductance();
            var sunlitHeat = Canopy.CalcSunlitBoundaryHeatConductance();

            Params.BoundaryHeatConductance = sunlitHeat;
            Params.fraction = sunFraction;
            Canopy.Sunlit.CalculatePhotosynthesis(Temperature, Params);

            Params.BoundaryHeatConductance = heat - sunlitHeat;
            Params.fraction = shadeFraction;
            Canopy.Shaded.CalculatePhotosynthesis(Temperature, Params);
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
        public double BoundaryHeatConductance;
        public double maxHourlyT;
        public double fraction;
    }
}
