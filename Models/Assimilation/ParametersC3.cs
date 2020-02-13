﻿using System;

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
            var x = new double[9];

            x[0] = Current.VcMaxT;
            x[1] = Current.Kc / Current.Ko;
            x[2] = Current.Kc;
            x[3] = 0.0;
            x[4] = 0.0;
            x[5] = 0.0;
            x[6] = 0.0;
            x[7] = 0.0;
            x[8] = 0.0;

            var param = new AssimilationCalculator()
            {
                X = x,

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
            var x = new double[9];

            x[0] = Current.J / 4;
            x[1] = 2 * Current.Gamma;
            x[2] = 0.0;
            x[3] = 0.0;
            x[4] = 0.0;
            x[5] = 0.0;
            x[6] = 0.0;
            x[7] = 0.0;
            x[8] = 0.0;

            var param = new AssimilationCalculator()
            {                
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
