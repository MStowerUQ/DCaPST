using DCAPST.Canopy;
using DCAPST.Interfaces;

namespace DCAPST
{
    public class ParametersC4 : Assimilation
    {
        public ParametersC4(AssimilationType type, IPartialCanopy partial) : base(type, partial)
        { }

        public override void UpdateMesophyllCO2(Pathway path)
        {
            path.MesophyllCO2 = path.IntercellularCO2 - path.CO2Rate / path.Current.GmT;
        }

        protected override AssimilationCalculator GetAc1Calculator(Pathway path)
        {
            var x = new double[9];

            x[0] = path.Current.VcMaxT;
            x[1] = path.Current.Kc / path.Current.Ko;
            x[2] = path.Current.Kc;
            x[3] = path.Current.VpMaxT / (path.MesophyllCO2 + path.Current.Kp);
            x[4] = 0.0;
            x[5] = 1.0;
            x[6] = 0.0;
            x[7] = 1.0;
            x[8] = 1.0;

            var param = new AssimilationCalculator()
            {
                X = x,

                m = path.Current.GmRd,
                t = path.Current.Gamma,
                sb = 0.1 / canopy.DiffusivitySolubilityRatio,
                j = Gbs,
                e = canopy.OxygenPartialPressure,
                R = path.Current.RdT
            };

            return param;
        }

        protected override AssimilationCalculator GetAc2Calculator(Pathway path)
        {
            var x = new double[9];

            x[0] = path.Current.VcMaxT;
            x[1] = path.Current.Kc / path.Current.Ko;
            x[2] = path.Current.Kc;
            x[3] = 0.0;
            x[4] = Vpr;
            x[5] = 1.0;
            x[6] = 0.0;
            x[7] = 1.0;
            x[8] = 1.0;

            var param = new AssimilationCalculator()
            {
                X = x,

                m = path.Current.GmRd,
                t = path.Current.Gamma,
                sb = 0.1 / canopy.DiffusivitySolubilityRatio,
                j = Gbs,
                e = canopy.OxygenPartialPressure,
                R = path.Current.RdT
            };

            return param;
        }

        protected override AssimilationCalculator GetAjCalculator(Pathway path)
        {
            var x = new double[9];

            x[0] = (1.0 - pway.MesophyllElectronTransportFraction) * path.Current.J / 3.0;
            x[1] = 7.0 / 3.0 * path.Current.Gamma;
            x[2] = 0.0;
            x[3] = 0.0;
            x[4] = pway.MesophyllElectronTransportFraction * path.Current.J / pway.ExtraATPCost;
            x[5] = 1.0;
            x[6] = 0.0;
            x[7] = 1.0;
            x[8] = 1.0;

            var param = new AssimilationCalculator()
            {
                X = x,

                m = path.Current.GmRd,
                t = path.Current.Gamma,
                sb = 0.1 / canopy.DiffusivitySolubilityRatio,
                j = Gbs,
                e = canopy.OxygenPartialPressure,
                R = path.Current.RdT
            };

            return param;
        }
    }
}
