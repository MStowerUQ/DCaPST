using DCAPST.Canopy;
using DCAPST.Interfaces;

namespace DCAPST
{
    public class ParametersC4 : Assimilation
    {
        public ParametersC4(AssimilationType type, IPartialCanopy partial) : base(type, partial)
        { }

        public override void UpdateMesophyllCO2(double intercellularCO2, double CO2Rate)
        {
            MesophyllCO2 = intercellularCO2 - CO2Rate / Current.GmT;
        }

        protected override AssimilationCalculator GetAc1Calculator()
        {
            var param = new AssimilationCalculator()
            {
                x1 = Current.VcMaxT,
                x2 = Current.Kc / Current.Ko,
                x3 = Current.Kc,
                x4 = Current.VpMaxT / (MesophyllCO2 + Current.Kp),
                x5 = 0.0,
                x6 = 1.0,
                x7 = 0.0,
                x8 = 1.0,
                x9 = 1.0,

                m = Current.GmRd,
                t = Current.Gamma,
                sb = 0.1 / canopy.DiffusivitySolubilityRatio,
                j = Gbs,
                e = canopy.OxygenPartialPressure,
                R = Current.RdT
            };

            return param;
        }

        protected override AssimilationCalculator GetAc2Calculator()
        {
            var param = new AssimilationCalculator()
            {
                x1 = Current.VcMaxT,
                x2 = Current.Kc / Current.Ko,
                x3 = Current.Kc,
                x4 = 0.0,
                x5 = Vpr,
                x6 = 1.0,
                x7 = 0.0,
                x8 = 1.0,
                x9 = 1.0,

                m = Current.GmRd,
                t = Current.Gamma,
                sb = 0.1 / canopy.DiffusivitySolubilityRatio,
                j = Gbs,
                e = canopy.OxygenPartialPressure,
                R = Current.RdT
            };

            return param;
        }

        protected override AssimilationCalculator GetAjCalculator()
        {
            var param = new AssimilationCalculator()
            {
                x1 = (1.0 - path.MesophyllElectronTransportFraction) * Current.J / 3.0,
                x2 = 7.0 / 3.0 * Current.Gamma,
                x3 = 0.0,
                x4 = 0.0,
                x5 = path.MesophyllElectronTransportFraction * Current.J / path.ExtraATPCost,
                x6 = 1.0,
                x7 = 0.0,
                x8 = 1.0,
                x9 = 1.0,

                m = Current.GmRd,
                t = Current.Gamma,
                sb = 0.1 / canopy.DiffusivitySolubilityRatio,
                j = Gbs,
                e = canopy.OxygenPartialPressure,
                R = Current.RdT
            };

            return param;
        }
    }
}
