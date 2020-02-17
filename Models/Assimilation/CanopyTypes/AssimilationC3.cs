using System;

using DCAPST.Canopy;
using DCAPST.Interfaces;

namespace DCAPST
{
    public class AssimilationC3 : Assimilation
    {
        public AssimilationC3(IPartialCanopy partial) : base(partial)
        { }

        protected override AssimilationFunction GetAc1Function(AssimilationPathway pathway)
        {
            var x = new double[9];

            x[0] = pathway.Leaf.VcMaxT;
            x[1] = pathway.Leaf.Kc / pathway.Leaf.Ko;
            x[2] = pathway.Leaf.Kc;
            x[3] = 0.0;
            x[4] = 0.0;
            x[5] = 0.0;
            x[6] = 0.0;
            x[7] = 0.0;
            x[8] = 0.0;

            var param = new AssimilationFunction()
            {
                X = x,

                m = pathway.Leaf.GmRd,
                t = pathway.Leaf.Gamma,
                sb = 0.0,
                j = 1.0,
                e = canopy.OxygenPartialPressure,
                R = pathway.Leaf.RdT
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

            x[0] = pathway.Leaf.J / 4.0;
            x[1] = 2.0 * pathway.Leaf.Gamma;
            x[2] = 0.0;
            x[3] = 0.0;
            x[4] = 0.0;
            x[5] = 0.0;
            x[6] = 0.0;
            x[7] = 0.0;
            x[8] = 0.0;

            var param = new AssimilationFunction()
            {                
                m = pathway.Leaf.GmRd,
                t = pathway.Leaf.Gamma,
                sb = 0.0,
                j = 1.0,
                e = canopy.OxygenPartialPressure,
                R = pathway.Leaf.RdT
            };

            return param;
        }
    }
}
