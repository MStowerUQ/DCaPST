using System;

using DCAPST.Canopy;
using DCAPST.Interfaces;

namespace DCAPST
{
    public class ParametersC3 : AssimilationParameters
    {
        public ParametersC3(IAssimilation assimilation, IPartialCanopy partial) : base(assimilation, partial)
        { }

        protected override AssimilationCalculator GetAc1Calculator()
        {
            var param = new AssimilationCalculator()
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

                m = MesophyllRespiration,
                t = G_,
                sb = 0.0,
                j = 1.0,
                e = canopy.OxygenPartialPressure,
                R = RdT
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
                x1 = ElectronTransportRate / 4,
                x2 = 2 * G_,
                x3 = 0.0,
                x4 = 0.0,
                x5 = 0.0,
                x6 = 0.0,
                x7 = 0.0,
                x8 = 0.0,
                x9 = 0.0,

                m = MesophyllRespiration,
                t = G_,
                sb = 0.0,
                j = 1.0,
                e = canopy.OxygenPartialPressure,
                R = RdT
            };

            return param;
        }
    }
}
