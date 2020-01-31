using DCAPST.Canopy;
using DCAPST.Interfaces;

namespace DCAPST
{
    public class CalculatorC4 : AssimilationCalculator
    {
        public CalculatorC4(IAssimilation assimilation, IPartialCanopy partial) : base(assimilation, partial)
        { }

        protected override AssimilationParameters GetAc1Params()
        {
            var param = new AssimilationParameters()
            {
                x1 = VcMaxT,
                x2 = Kc / Ko,
                x3 = Kc,
                x4 = VpMaxT / (assimilation.MesophyllCO2 + Kp),
                x5 = 0.0,
                x6 = 1.0,
                x7 = 0.0,
                x8 = 1.0,
                x9 = 1.0,

                m = MesophyllRespiration,
                t = G_,
                sb = 0.1 / canopy.DiffusivitySolubilityRatio,
                j = Gbs,
                e = canopy.OxygenPartialPressure,
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

                m = MesophyllRespiration,
                t = G_,
                sb = 0.1 / canopy.DiffusivitySolubilityRatio,
                j = Gbs,
                e = canopy.OxygenPartialPressure,
                R = RdT
            };

            return param;
        }

        protected override AssimilationParameters GetAjParams()
        {
            var param = new AssimilationParameters()
            {
                x1 = (1.0 - path.MesophyllElectronTransportFraction) * ElectronTransportRate / 3.0,
                x2 = 7.0 / 3.0 * G_,
                x3 = 0.0,
                x4 = 0.0,
                x5 = path.MesophyllElectronTransportFraction * ElectronTransportRate / path.ExtraATPCost,
                x6 = 1.0,
                x7 = 0.0,
                x8 = 1.0,
                x9 = 1.0,

                m = MesophyllRespiration,
                t = G_,
                sb = 0.1 / canopy.DiffusivitySolubilityRatio,
                j = Gbs,
                e = canopy.OxygenPartialPressure,
                R = RdT
            };

            return param;
        }
    }
}
