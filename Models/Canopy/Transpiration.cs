using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAPST.Canopy
{
    public class Transpiration
    {
        public bool limited;
        public double BoundaryHeatConductance;
        public double maxHourlyT;
        public double fraction;

        public double[] SunlitDemand { get; }

        public double[] ShadedDemand { get; }

        public Transpiration(int iterations)
        {
            SunlitDemand = new double[iterations];
            ShadedDemand = new double[iterations];
        }
    }
}
