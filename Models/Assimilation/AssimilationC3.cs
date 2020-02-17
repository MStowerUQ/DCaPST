using System;

using DCAPST.Canopy;
using DCAPST.Interfaces;

namespace DCAPST
{
    public class ParametersC3 : Assimilation
    {
        public ParametersC3(IPartialCanopy partial) : base(partial)
        { }

        protected override AssimilationFunction GetAc1Function(AssimilationPathway pathway)
        {
            var x = new double[9];

            x[0] = pathway.Current.VcMaxT;
            x[1] = pathway.Current.Kc / pathway.Current.Ko;
            x[2] = pathway.Current.Kc;
            x[3] = 0.0;
            x[4] = 0.0;
            x[5] = 0.0;
            x[6] = 0.0;
            x[7] = 0.0;
            x[8] = 0.0;

            var param = new AssimilationFunction()
            {
                X = x,

                m = pathway.Current.GmRd,
                t = pathway.Current.Gamma,
                sb = 0.0,
                j = 1.0,
                e = canopy.OxygenPartialPressure,
                R = pathway.Current.RdT
            };

            return param;
        }

        protected override AssimilationFunction GetAc2Function(AssimilationPathway pathway)
        {
            throw new Exception("The C3 model does not use the Ac2 pathway");
        }

        protected override AssimilationFunction GetAjFunction(AssimilationPathway pathway)
        {
            var x = new double[9];

            x[0] = pathway.Current.J / 4.0;
            x[1] = 2.0 * pathway.Current.Gamma;
            x[2] = 0.0;
            x[3] = 0.0;
            x[4] = 0.0;
            x[5] = 0.0;
            x[6] = 0.0;
            x[7] = 0.0;
            x[8] = 0.0;

            var param = new AssimilationFunction()
            {                
                m = pathway.Current.GmRd,
                t = pathway.Current.Gamma,
                sb = 0.0,
                j = 1.0,
                e = canopy.OxygenPartialPressure,
                R = pathway.Current.RdT
            };

            return param;
        }
    }
}
