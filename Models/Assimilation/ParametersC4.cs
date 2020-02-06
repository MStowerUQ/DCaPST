using DCAPST.Canopy;
using DCAPST.Interfaces;

namespace DCAPST
{
    public class ParametersC4 : AssimilationParameters
    {
        public ParametersC4(IAssimilation assimilation, IPartialCanopy partial) : base(assimilation, partial)
        { }

        public override void UpdateMesophyllCO2(double intercellularCO2, double CO2Rate)
        {
            MesophyllCO2 = intercellularCO2 - CO2Rate / MesophyllCO2ConductanceAtT;
        }

        protected override AssimilationCalculator GetAc1Calculator()
        {
            var param = new AssimilationCalculator()
            {
                x1 = VcMaxT,
                x2 = Kc / Ko,
                x3 = Kc,
                x4 = VpMaxT / (MesophyllCO2 + Kp),
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

        protected override AssimilationCalculator GetAc2Calculator()
        {
            var param = new AssimilationCalculator()
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

        protected override AssimilationCalculator GetAjCalculator()
        {
            var param = new AssimilationCalculator()
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
