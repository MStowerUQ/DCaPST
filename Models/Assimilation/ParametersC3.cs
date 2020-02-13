using System;

using DCAPST.Canopy;
using DCAPST.Interfaces;

namespace DCAPST
{
    public class ParametersC3 : Assimilation
    {
        public ParametersC3(AssimilationType type, IPartialCanopy partial) : base(type, partial)
        { }

        protected override AssimilationCalculator GetAc1Calculator()
        {
            var param = new AssimilationCalculator()
            {
                x1 = Current.VcMaxT,
                x2 = Current.Kc / Current.Ko,
                x3 = Current.Kc,
                x4 = 0.0,
                x5 = 0.0,
                x6 = 0.0,
                x7 = 0.0,
                x8 = 0.0,
                x9 = 0.0,

                m = Current.GmRd,
                t = Current.Gamma,
                sb = 0.0,
                j = 1.0,
                e = canopy.OxygenPartialPressure,
                R = Current.RdT
            };

            return param;
        }

        protected override AssimilationCalculator GetAc2Calculator()
        {
            throw new Exception("The C3 model does not use the Ac2 pathway");
        }

        protected override AssimilationCalculator GetAjCalculator()
        {
            var param = new AssimilationCalculator()
            {
                x1 = Current.J / 4,
                x2 = 2 * Current.Gamma,
                x3 = 0.0,
                x4 = 0.0,
                x5 = 0.0,
                x6 = 0.0,
                x7 = 0.0,
                x8 = 0.0,
                x9 = 0.0,

                m = Current.GmRd,
                t = Current.Gamma,
                sb = 0.0,
                j = 1.0,
                e = canopy.OxygenPartialPressure,
                R = Current.RdT
            };

            return param;
        }
    }
}
