using DCAPST.Canopy;
using DCAPST.Interfaces;

namespace DCAPST
{
    public class ParametersC4 : Assimilation
    {
        public ParametersC4(IPartialCanopy partial) : base(partial)
        { }

        protected override void UpdateIntercellularCO2(AssimilationPathway pathway, double gt, double waterUseMolsSecond)
        {
            pathway.IntercellularCO2 = ((gt - waterUseMolsSecond / 2.0) * canopy.AirCO2 - pathway.CO2Rate) / (gt + waterUseMolsSecond / 2.0);
        }

        protected override void UpdateMesophyllCO2(AssimilationPathway pathway)
        {
            pathway.MesophyllCO2 = pathway.IntercellularCO2 - pathway.CO2Rate / pathway.Current.GmT;
        }

        protected override AssimilationFunction GetAc1Function(AssimilationPathway pathway)
        {
            var x = new double[9];

            x[0] = pathway.Current.VcMaxT;
            x[1] = pathway.Current.Kc / pathway.Current.Ko;
            x[2] = pathway.Current.Kc;
            x[3] = pathway.Current.VpMaxT / (pathway.MesophyllCO2 + pathway.Current.Kp);
            x[4] = 0.0;
            x[5] = 1.0;
            x[6] = 0.0;
            x[7] = 1.0;
            x[8] = 1.0;

            var param = new AssimilationFunction()
            {
                X = x,

                m = pathway.Current.GmRd,
                t = pathway.Current.Gamma,
                sb = 0.1 / canopy.DiffusivitySolubilityRatio,
                j = Gbs,
                e = canopy.OxygenPartialPressure,
                R = pathway.Current.RdT
            };

            return param;
        }

        protected override AssimilationFunction GetAc2Function(AssimilationPathway pathway)
        {
            var x = new double[9];

            x[0] = pathway.Current.VcMaxT;
            x[1] = pathway.Current.Kc / pathway.Current.Ko;
            x[2] = pathway.Current.Kc;
            x[3] = 0.0;
            x[4] = Vpr;
            x[5] = 1.0;
            x[6] = 0.0;
            x[7] = 1.0;
            x[8] = 1.0;

            var param = new AssimilationFunction()
            {
                X = x,

                m = pathway.Current.GmRd,
                t = pathway.Current.Gamma,
                sb = 0.1 / canopy.DiffusivitySolubilityRatio,
                j = Gbs,
                e = canopy.OxygenPartialPressure,
                R = pathway.Current.RdT
            };

            return param;
        }

        protected override AssimilationFunction GetAjFunction(AssimilationPathway pathway)
        {
            var x = new double[9];

            x[0] = (1.0 - pway.MesophyllElectronTransportFraction) * pathway.Current.J / 3.0;
            x[1] = 7.0 / 3.0 * pathway.Current.Gamma;
            x[2] = 0.0;
            x[3] = 0.0;
            x[4] = pway.MesophyllElectronTransportFraction * pathway.Current.J / pway.ExtraATPCost;
            x[5] = 1.0;
            x[6] = 0.0;
            x[7] = 1.0;
            x[8] = 1.0;

            var param = new AssimilationFunction()
            {
                X = x,

                m = pathway.Current.GmRd,
                t = pathway.Current.Gamma,
                sb = 0.1 / canopy.DiffusivitySolubilityRatio,
                j = Gbs,
                e = canopy.OxygenPartialPressure,
                R = pathway.Current.RdT
            };

            return param;
        }
    }
}
