﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DCAPST.Interfaces;

namespace DCAPST.Canopy
{
    public class Transpiration
    {
        /// <summary>
        /// Parameters describing the canopy
        /// </summary>
        public ICanopyParameters Canopy { get; private set; }

        public IPathwayParameters Pathway { get; private set; }

        /// <summary>
        /// Models the leaf water interaction
        /// </summary>
        public IWaterInteraction Water { get; }

        /// <summary>
        /// Models how the leaf responds to different temperatures
        /// </summary>
        public TemperatureResponse Leaf { get; set; }

        public bool limited;
        public double BoundaryHeatConductance;
        public double maxHourlyT;
        public double fraction;

        public double[] SunlitDemand { get; }

        public double[] ShadedDemand { get; }

        /// <summary>
        /// Resistance to water
        /// </summary>
        public double Resistance { get; private set; }

        public Transpiration(
            ICanopyParameters canopy,
            IPathwayParameters pathway,
            IWaterInteraction water,
            TemperatureResponse leaf,
            int iterations
        )
        {
            Canopy = canopy;
            Pathway = pathway;
            Water = water;
            Leaf = leaf;

            SunlitDemand = new double[iterations];
            ShadedDemand = new double[iterations];
        }

        public void SetConditions(ParameterRates At25C, double temperature, double photons, double radiation)
        {
            Leaf.SetConditions(At25C, temperature, photons);
            Water.SetConditions(temperature, BoundaryHeatConductance, radiation);
        }

        public void UpdatePathway(IAssimilation assimilation, AssimilationPathway pathway)
        {
            var func = assimilation.GetFunction(pathway, Leaf);

            if (limited)
            {
                var molarMassWater = 18;
                var g_to_kg = 1000;
                var hrs_to_seconds = 3600;

                pathway.WaterUse = maxHourlyT * fraction;
                var WaterUseMolsSecond = pathway.WaterUse / molarMassWater * g_to_kg / hrs_to_seconds;

                Resistance = Water.LimitedWaterResistance(pathway.WaterUse);
                var Gt = Water.TotalCO2Conductance(Resistance);

                func.Ci = Canopy.AirCO2 - WaterUseMolsSecond * Canopy.AirCO2 / (Gt + WaterUseMolsSecond / 2.0);
                func.Rm = 1 / (Gt + WaterUseMolsSecond / 2) + 1.0 / Leaf.GmT;

                pathway.CO2Rate = func.Value();

                assimilation.UpdateIntercellularCO2(pathway, Gt, WaterUseMolsSecond);
            }
            else
            {
                pathway.IntercellularCO2 = Pathway.IntercellularToAirCO2Ratio * Canopy.AirCO2; 
                
                func.Ci = pathway.IntercellularCO2;
                func.Rm = 1 / Leaf.GmT;

                pathway.CO2Rate = func.Value();

                Resistance = Water.UnlimitedWaterResistance(pathway.CO2Rate, Canopy.AirCO2, pathway.IntercellularCO2);
                pathway.WaterUse = Water.HourlyWaterUse(Resistance);
            }

            assimilation.UpdatePartialPressures(pathway, Leaf, func);
        }

        public void UpdateTemperature(AssimilationPathway pathway)
        {
            // New leaf temperature
            var leafTemp = Water.LeafTemperature(Resistance);
            pathway.Temperature = (leafTemp + pathway.Temperature) / 2.0;            
        }
    }
}
