using DCAPST.Canopy;
using DCAPST.Interfaces;

namespace DCAPST
{
    public class CalculatorCCM : AssimilationCalculator
    {
        public CalculatorCCM(ICanopyParameters path, PartialCanopy partial, Assimilation assimilation) : base(path, partial, assimilation)
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
                x6 = 0.0,
                x7 = Cc * VcMaxT / (Cc + Kc * (1 + Oc / Ko)),
                x8 = 1.0,
                x9 = 1.0,

                m = Rm,
                t = G_,
                sb = 0.1 / Canopy.DiffusivitySolubilityRatio,
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
                x6 = 0.0,
                x7 = Cc * VcMaxT / (Cc + Kc * (1 + Oc / Ko)),
                x8 = 1.0,
                x9 = 1.0,

                m = Rm,
                t = G_,
                sb = 0.1 / Canopy.DiffusivitySolubilityRatio,
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
                x1 = (1 - Canopy.Pathway.X) * Canopy.Pathway.z * J / 3.0,
                x2 = 7.0 / 3.0 * G_,
                x3 = 0.0,
                x4 = 0.0,
                x5 = Canopy.Pathway.X * Canopy.Pathway.z * J / Canopy.Pathway.Phi,
                x6 = 0.0,
                x7 = Cc * (1 - Canopy.Pathway.X) * Canopy.Pathway.z * J / (3 * Cc + 7 * G_ * Oc),
                x8 = 1.0,
                x9 = 1.0,

                m = Rm,
                t = G_,
                sb = 0.1 / Canopy.DiffusivitySolubilityRatio,
                j = Gbs,
                e = OxygenPartialPressure,
                R = RdT
            };

            return param;
        }
    }
}
