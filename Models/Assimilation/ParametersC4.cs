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
            Path.MesophyllCO2 = intercellularCO2 - CO2Rate / Path.Current.GmT;
        }

        protected override AssimilationCalculator GetAc1Calculator()
        {
            var x = new double[9];

            x[0] = Path.Current.VcMaxT;
            x[1] = Path.Current.Kc / Path.Current.Ko;
            x[2] = Path.Current.Kc;
            x[3] = Path.Current.VpMaxT / (Path.MesophyllCO2 + Path.Current.Kp);
            x[4] = 0.0;
            x[5] = 1.0;
            x[6] = 0.0;
            x[7] = 1.0;
            x[8] = 1.0;

            var param = new AssimilationCalculator()
            {
                X = x,

                m = Path.Current.GmRd,
                t = Path.Current.Gamma,
                sb = 0.1 / canopy.DiffusivitySolubilityRatio,
                j = Gbs,
                e = canopy.OxygenPartialPressure,
                R = Path.Current.RdT
            };

            return param;
        }

        protected override AssimilationCalculator GetAc2Calculator()
        {
            var x = new double[9];

            x[0] = Path.Current.VcMaxT;
            x[1] = Path.Current.Kc / Path.Current.Ko;
            x[2] = Path.Current.Kc;
            x[3] = 0.0;
            x[4] = Vpr;
            x[5] = 1.0;
            x[6] = 0.0;
            x[7] = 1.0;
            x[8] = 1.0;

            var param = new AssimilationCalculator()
            {
                X = x,

                m = Path.Current.GmRd,
                t = Path.Current.Gamma,
                sb = 0.1 / canopy.DiffusivitySolubilityRatio,
                j = Gbs,
                e = canopy.OxygenPartialPressure,
                R = Path.Current.RdT
            };

            return param;
        }

        protected override AssimilationCalculator GetAjCalculator()
        {
            var x = new double[9];

            x[0] = (1.0 - pway.MesophyllElectronTransportFraction) * Path.Current.J / 3.0;
            x[1] = 7.0 / 3.0 * Path.Current.Gamma;
            x[2] = 0.0;
            x[3] = 0.0;
            x[4] = pway.MesophyllElectronTransportFraction * Path.Current.J / pway.ExtraATPCost;
            x[5] = 1.0;
            x[6] = 0.0;
            x[7] = 1.0;
            x[8] = 1.0;

            var param = new AssimilationCalculator()
            {
                X = x,

                m = Path.Current.GmRd,
                t = Path.Current.Gamma,
                sb = 0.1 / canopy.DiffusivitySolubilityRatio,
                j = Gbs,
                e = canopy.OxygenPartialPressure,
                R = Path.Current.RdT
            };

            return param;
        }
    }
}
