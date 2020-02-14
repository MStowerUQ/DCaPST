using System;

using DCAPST.Canopy;
using DCAPST.Interfaces;

namespace DCAPST
{
    public class ParametersC3 : Assimilation
    {
        public ParametersC3(IPartialCanopy partial) : base(partial)
        { }

        protected override AssimilationCalculator GetAc1Calculator(Pathway path)
        {
            var x = new double[9];

            x[0] = path.Current.VcMaxT;
            x[1] = path.Current.Kc / path.Current.Ko;
            x[2] = path.Current.Kc;
            x[3] = 0.0;
            x[4] = 0.0;
            x[5] = 0.0;
            x[6] = 0.0;
            x[7] = 0.0;
            x[8] = 0.0;

            var param = new AssimilationCalculator()
            {
                X = x,

                m = path.Current.GmRd,
                t = path.Current.Gamma,
                sb = 0.0,
                j = 1.0,
                e = canopy.OxygenPartialPressure,
                R = path.Current.RdT
            };

            return param;
        }

        protected override AssimilationCalculator GetAc2Calculator(Pathway path)
        {
            throw new Exception("The C3 model does not use the Ac2 pathway");
        }

        protected override AssimilationCalculator GetAjCalculator(Pathway path)
        {
            var x = new double[9];

            x[0] =path.Current.J / 4;
            x[1] = 2 * path.Current.Gamma;
            x[2] = 0.0;
            x[3] = 0.0;
            x[4] = 0.0;
            x[5] = 0.0;
            x[6] = 0.0;
            x[7] = 0.0;
            x[8] = 0.0;

            var param = new AssimilationCalculator()
            {                
                m = path.Current.GmRd,
                t = path.Current.Gamma,
                sb = 0.0,
                j = 1.0,
                e = canopy.OxygenPartialPressure,
                R = path.Current.RdT
            };

            return param;
        }
    }
}
