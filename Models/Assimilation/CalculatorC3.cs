using DCAPST.Canopy;
using DCAPST.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAPST
{
    public class CalculatorC3 : AssimilationCalculator
    {
        public CalculatorC3(IPathwayParameters path, PartialCanopy partial, Assimilation assimilation) : base(path, partial, assimilation)
        { }

        protected override AssimilationParameters GetAc1Params()
        {
            var param = new AssimilationParameters()
            {
                x1 = VcMaxT,
                x2 = Kc / Ko,
                x3 = Kc,
                x4 = 0.0,
                x5 = 0.0,
                x6 = 0.0,
                x7 = 0.0,
                x8 = 0.0,
                x9 = 0.0,

                m = Rm,
                t = G_,
                sb = 0.0,
                j = 1.0,
                e = OxygenPartialPressure,
                R = RdT
            };

            return param;
        }

        protected override AssimilationParameters GetAc2Params()
        {
            var param = new AssimilationParameters()
            {
                x1 = 0.0,
                x2 = 0.0,
                x3 = 0.0,
                x4 = 0.0,
                x5 = 0.0,
                x6 = 0.0,
                x7 = 0.0,
                x8 = 0.0,
                x9 = 0.0,

                m = 0.0,
                t = 0.0,
                sb = 0.0,
                j = 0.0,
                e = 0.0,
                R = 0.0
            };

            return param;
        }

        protected override AssimilationParameters GetAjParams()
        {
            var param = new AssimilationParameters()
            {
                x1 = J / 4,
                x2 = 2 * G_,
                x3 = 0.0,
                x4 = 0.0,
                x5 = 0.0,
                x6 = 0.0,
                x7 = 0.0,
                x8 = 0.0,
                x9 = 0.0,

                m = Rm,
                t = G_,
                sb = 0.0,
                j = 1.0,
                e = OxygenPartialPressure,
                R = RdT
            };

            return param;
        }
    }
}
