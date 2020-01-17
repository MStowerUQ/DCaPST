using DCAPST.Canopy;
using DCAPST.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAPST
{
    public class CalculatorC4 : AssimilationCalculator
    {
        public CalculatorC4(IPathwayParameters path, PartialCanopy partial, Assimilation assimilation) : base(path, partial, assimilation)
        { }

        protected override AssimilationParameters GetAc1Params()
        {
            var param = new AssimilationParameters()
            {
                x1 = VcMaxT,
                x2 = Kc / Ko,
                x3 = Kc,
                x4 = VpMaxT / (Cm + Kp),
                x5 = 0.0,
                x6 = 1.0,
                x7 = 0.0,
                x8 = 1.0,
                x9 = 1.0,

                m = Rm,
                t = G_,
                sb = 0.1 / CPath.Canopy.DiffusivitySolubilityRatio,
                j = Gbs,
                e = OxygenPartialPressure,
                R = RdT
            };

            return param;
        }

        protected override AssimilationParameters GetAc2Params()
        {
            var param = new AssimilationParameters()
            {
                x1 = VcMaxT,
                x2 = Kc / Ko,
                x3 = Kc,
                x4 = 0.0,
                x5 = Vpr,
                x6 = 1.0,
                x7 = 0.0,
                x8 = 1.0,
                x9 = 1.0,

                m = Rm,
                t = G_,
                sb = 0.1 / CPath.Canopy.DiffusivitySolubilityRatio,
                j = Gbs,
                e = OxygenPartialPressure,
                R = RdT
            };

            return param;
        }

        protected override AssimilationParameters GetAjParams()
        {
            var param = new AssimilationParameters()
            {
                x1 = (1.0 - CPath.X) * J / 3.0,
                x2 = 7.0 / 3.0 * G_,
                x3 = 0.0,
                x4 = 0.0,
                x5 = CPath.X * J / CPath.Phi,
                x6 = 1.0,
                x7 = 0.0,
                x8 = 1.0,
                x9 = 1.0,

                m = Rm,
                t = G_,
                sb = 0.1 / CPath.Canopy.DiffusivitySolubilityRatio,
                j = Gbs,
                e = OxygenPartialPressure,
                R = RdT
            };

            return param;
        }
    }
}
