using System;
using System.Collections.Generic;
using System.Linq;

using DCAPST.Canopy;
using DCAPST.Environment;
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

        public PhotosynthesisModel(ISolarGeometry solar, IRadiation radiation, ITemperature temperature, IPathwayParameters pathway)
        {
            Solar = solar;
            Radiation = radiation;
            Temperature = temperature;

            int layers = 1;
            if (layers <= 0) throw new Exception("There must be at least 1 layer");

            Canopy = new TotalCanopy(pathway, layers);

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
            // TODO: Total canopy -> CanopySection ?
            // Note: In the potential case, we assume unlimited water and therefore supply = demand
            CalculatePotential(out double intercepted, out double[] assimilations, out double[] sunlitDemand, out double[] shadedDemand);
            var waterSupply = sunlitDemand.Zip(shadedDemand, (x, y) => x + y).ToArray();
            var potential = assimilations.Sum();
            var totalDemand = waterSupply.Sum();

            // ACTUAL CALCULATIONS
            // Limit water to supply available from Apsim
            double maxHourlyT = Math.Min(waterSupply.Max(), MaxHourlyTRate);
            waterSupply = waterSupply.Select(w => w > maxHourlyT ? maxHourlyT : w).ToArray();

            var limitedSupply = CalculateWaterSupplyLimits(soilWater, maxHourlyT, totalDemand, waterSupply);

            var actual = (soilWater > totalDemand) ? potential : CalculateActual(limitedSupply, sunlitDemand, shadedDemand);

            double[] results = new double[5];
            results[0] = actual * 3600 / 1000000 * 44 * B * 100 / ((1 + RootShootRatio) * 100);
            results[1] = totalDemand;
            results[2] = (soilWater < totalDemand) ? limitedSupply.Sum() : waterSupply.Sum();
            results[3] = intercepted;
            results[4] = potential * 3600 / 1000000 * 44 * B * 100 / ((1 + RootShootRatio) * 100);

            return results;
        }

        // TODO: temp has more or less been moved into Temperature.AirTemperature, there should be simplifications that can
        // can be made as a result of this
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
            var CPath = Canopy.CPath;
            var temp = Temperature.AirTemperature;

            bool invalidTemp = temp > CPath.JTMax || temp < CPath.JTMin || temp > CPath.GmTMax || temp < CPath.GmTMin;
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

                // Note: double array values default to 0.0, which is the intended case if initialisation fails
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

            Canopy.ResetPartials();
            Canopy.CalcLAI();
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
        /// must be calculated. This is done by partitioning the water demand into each hour, then trimming the supply
        /// of any hour which demands more water than some maxHourlyT. 
        /// 
        /// If this brings the water demand too far below the supply, it is adjusted upwards again, by a smaller margin.
        /// This process is repeated until the difference between the demand and supply is within some minor tolerance.
        /// </summary>
        private double[] CalculateWaterSupplyLimits(double soilWaterAvail, double maxHourlyT, double totalDemand, double[] supply)
        {
            if (soilWaterAvail > 0.0001)
            {
                if (totalDemand > soilWaterAvail)
                {
                    double tolerance = 0.000001;

                    double max = maxHourlyT;
                    double min = 0;
                    double average = 0;

                    double averageSum = supply.Sum();
                    double maxSum = 0;

                    // While averageTotal is outside some tolerance of the soilwater
                    while ((soilWaterAvail + tolerance) < averageSum || averageSum < (soilWaterAvail - tolerance))
                    {
                        average = (max + min) / 2;

                        // Select the sum of the supplied water, trimming any values greater than the average
                        averageSum = supply.Select(d => d > average ? average : d).Sum();

                        // Select the sum of the supplied water, trimming values greater than the max value
                        maxSum = supply.Select(d => d > max ? max : d).Sum();

                        // If available water is between the average and high values, adjust the min value upwards
                        if (averageSum < soilWaterAvail && soilWaterAvail < maxSum)
                            min = average;
                        // Otherwise, if available water is between the average and min values, adjust the max value downwards
                        else if (min < soilWaterAvail && soilWaterAvail < averageSum)
                            max = average;
                    }
                    return supply.Select(d => d > average ? average : d).ToArray();
                }
                else
                    return supply;
            }
            else 
                return supply.Select(d => 0.0).ToArray();
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
