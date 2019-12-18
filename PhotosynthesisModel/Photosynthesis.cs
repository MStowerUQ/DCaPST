using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LayerCanopyPhotosynthesis.Canopy;
using LayerCanopyPhotosynthesis.Environment;

namespace LayerCanopyPhotosynthesis
{
    public class Photosynthesis
    {
        public SolarGeometryModel Solar { get; set; }
        public RadiationModel Radiation { get; set; }
        public TemperatureModel Temperature { get; set; }

        public List<TotalCanopy> Canopies = new List<TotalCanopy>();

        public double B { get; set; } = 0.409;

        private readonly int start = 6;
        private readonly int end = 18;
        private int RunTime => 1 + end - start;

        public Photosynthesis(PathwayParameters pathway)
        { 
            int layers = 1;
            if (layers <= 0) throw new Exception("There must be at least 1 layer");

            Canopies.Add(new TotalCanopy(CanopyType.Ac1, pathway, layers));
            
            if (!(pathway is PathwayParametersC3)) 
                Canopies.Add(new TotalCanopy(CanopyType.Ac2, pathway, layers));
            
            Canopies.Add(new TotalCanopy(CanopyType.Aj, pathway, layers));
        }

        public double[] DailyRun(
            int DOY, 
            double latitude, 
            double maxT, 
            double minT, 
            double radn,
            double lai,
            double SLN, 
            double soilWater, 
            double RootShootRatio, 
            double MaxHourlyTRate = 100)
        {
            // INITIALISE VALUES
            Solar = new SolarGeometryModel(DOY, latitude);
            Radiation = new RadiationModel(Solar, radn) { RPAR = 0.5};
            Temperature = new TemperatureModel(Solar, maxT, minT) { AtmosphericPressure = 1.01325 };

            Canopies.ForEach(c => c.Initialise(lai, SLN));

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
            results[2] = (soilWater > totalDemand) ? limitedSupply.Sum() : waterSupply.Sum();
            results[3] = intercepted;
            results[4] = potential * 3600 / 1000000 * 44 * B * 100 / ((1 + RootShootRatio) * 100);

            return results;
        }

        // TODO: temp has more or less been moved into Temperature.AirTemperature, there should be simplifications that can
        // can be made as a result of this
        private bool TryInitiliase(int time)
        {
            double temp = Temperature.GetTemp(time);
            Temperature.AirTemperature = temp;

            Radiation.UpdateIncidentRadiation(time);
            var sunAngle = Solar.SunAngle(time).Rad;            
            Canopies.ForEach(c => { c.CalcCanopyStructure(sunAngle); });

            return IsSensible(time, temp);
        }

        private bool IsSensible(int time, double temp)
        {
            var CPath = Canopies.First().CPath;
            bool invalidTemp = temp > CPath.JTMax || temp < CPath.JTMin || temp > CPath.GmTMax || temp < CPath.GmTMin;
            bool invalidRadn = Radiation.Ios.Value(time) <= double.Epsilon;

            if (invalidTemp || invalidRadn)
                return false;
            else
                return true;
        }        

        public void CalculatePotential(out double intercepted, out double[] assimilations, out double[] sunlitDemand, out double[] shadedDemand)
        {
            // Water demands
            intercepted = 0.0;
            sunlitDemand = new double[RunTime];
            shadedDemand = new double[RunTime];
            assimilations = new double[RunTime];

            for (int i = 0; i < RunTime; i++)
            {
                int time = start + i;

                // Note: double array values default to 0.0, which is the intended case if initialisation fails
                if (!TryInitiliase(time)) continue;

                intercepted += Radiation.Ios.Value(time) * Canopies.First().PropnInterceptedRadns * 3600;

                DoHourlyCalculation();

                var sunlits = Canopies.Select(c => c.Sunlit);
                var shadeds = Canopies.Select(c => c.Shaded);

                sunlitDemand[i] = sunlits.Select(s => s.WaterUse).Min();
                shadedDemand[i] = shadeds.Select(s => s.WaterUse).Min();
                assimilations[i] = sunlits.Select(s => s.A).Min() + shadeds.Select(s => s.A).Min();
            }
        }

        public double CalculateActual(double[] waterSupply, double[] sunlitDemand, double[] shadedDemand)
        {
            double assimilation = 0.0;
            for (int i = 0; i < RunTime; i++)
            {
                int time = start + i;

                // Note: double array values default to 0.0, which is the intended case if initialisation fails
                if (!TryInitiliase(time)) continue;

                double total = sunlitDemand[i] + shadedDemand[i];
                DoHourlyCalculation(waterSupply[i], sunlitDemand[i] / total, shadedDemand[i] / total);

                var sunlits = Canopies.Select(c => c.Sunlit);
                var shadeds = Canopies.Select(c => c.Shaded);

                assimilation += sunlits.Select(s => s.A).Min() + shadeds.Select(s => s.A).Min();
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

            Canopies.ForEach(c => 
            {
                c.ResetPartials();
                c.CalcLAI();
                c.Run(Radiation);
            });

            var gbh = Canopies.First().CalcGbh();
            var sunlitGbh = Canopies.First().CalcSunlitGbh();

            Params.Gbh = sunlitGbh;
            Params.fraction = sunFraction;
            CalcPartialPhotosynthesis(Canopies.Select(c => c.Sunlit).ToList(), Params);

            Params.Gbh = gbh - sunlitGbh;
            Params.fraction = shadeFraction;
            CalcPartialPhotosynthesis(Canopies.Select(c => c.Shaded).ToList(), Params);
        }

        public void CalcPartialPhotosynthesis(List<PartialCanopy> partials, PhotosynthesisParams Params)
        {
            // Initialise the leaf temperature as the air temperature
            partials.ForEach(p => p.LeafTemperature = Temperature.AirTemperature);

            // Determine initial results
            var test = partials.Select(s => s.TryCalculatePhotosynthesis(Temperature, Params)).ToList();

            var initialA = partials.Select(s => s.A).ToArray();
            var initialWater = partials.Select(s => s.WaterUse).ToArray();

            // If any calculation fails, all results are zeroed
            if (test.Any(b => b == false))
            {
                partials.ForEach(s => s.ZeroVariables());
                return;
            }

            // Do not proceed if there is any insufficient assimilation
            if (!partials.Any(s => s.A < 0.5))
            {
                for (int n = 0; n < 3; n++)
                {                    
                    test = partials.Select(s => s.TryCalculatePhotosynthesis(Temperature, Params)).ToList();

                    // If any calculation fails, all results are set to the value calculated initially
                    if (test.Any(b => b == false))
                    {
                        partials.Select((s, index) =>
                        {
                            s.A = initialA[index];
                            s.WaterUse = initialWater[index];
                            return s;
                        });
                        break;
                    }
                }
            }
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
