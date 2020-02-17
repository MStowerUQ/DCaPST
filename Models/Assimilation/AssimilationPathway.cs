﻿using System;
using DCAPST.Interfaces;

namespace DCAPST
{
    public enum AssimilationType { Ac1, Ac2, Aj }

    public class AssimilationPathway
    {
        public AssimilationType Type { get; set; }

        public LeafParameters Leaf { get; set; }

        /// <summary>
        /// The rate at which CO2 is assimilated
        /// </summary>
        public double CO2Rate { get; set; }

        /// <summary>
        /// The water required to maintain the CO2 rate
        /// </summary>
        public double WaterUse { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double IntercellularCO2 { get; set; }

        public double MesophyllCO2 { get; set; }
        public double ChloroplasticCO2 { get; set; }
        public double ChloroplasticO2 { get; set; }

        public AssimilationPathway(IPartialCanopy partial)
        {
            MesophyllCO2 = partial.Canopy.AirCO2 * partial.Canopy.Pathway.IntercellularToAirCO2Ratio;
            ChloroplasticCO2 = MesophyllCO2 + 20;
            ChloroplasticO2 = 210000;

            Leaf = new LeafParameters(partial);
        }
    }
}
