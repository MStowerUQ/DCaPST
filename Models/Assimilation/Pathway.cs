using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAPST
{
    public class Pathway
    {
        /// <summary>
        /// The rate at which CO2 is assimilated
        /// </summary>
        public double CO2Rate { get; set; }

        /// <summary>
        /// The water required to maintain the CO2 rate
        /// </summary>
        public double WaterUse { get; set; }

        /// <summary>
        /// The temperature of the leaf in which assimilation is occuring
        /// </summary>
        public double LeafTemperature { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double IntercellularCO2 { get; set; }

        public double MesophyllCO2 { get; set; }
        public double ChloroplasticCO2 { get; set; }
        public double ChloroplasticO2 { get; set; }

        public Pathway(double Ca, double CiCaRatio)
        {
            MesophyllCO2 = Ca * CiCaRatio;
            ChloroplasticCO2 = MesophyllCO2 + 20;
            ChloroplasticO2 = 210000;
        }
    }
}
