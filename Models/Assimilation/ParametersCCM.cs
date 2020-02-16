using System;

using DCAPST.Canopy;
using DCAPST.Interfaces;

namespace DCAPST
{
    public class ParametersCCM : Assimilation
    {
        public ParametersCCM(IPartialCanopy partial) : base(partial)
        { }

        protected override void UpdateIntercellularCO2(Pathway path, double gt, double waterUseMolsSecond)
        {
            path.IntercellularCO2 = ((gt - waterUseMolsSecond / 2.0) * canopy.AirCO2 - path.CO2Rate) / (gt + waterUseMolsSecond / 2.0);
        }

        protected override void UpdateMesophyllCO2(Pathway path)
        {
            path.MesophyllCO2 = path.IntercellularCO2 - path.CO2Rate / path.Current.GmT;
        }

        protected override void UpdateChloroplasticO2(Pathway path)
        {
            path.ChloroplasticO2 = pway.PS2ActivityInBundleSheathFraction * path.CO2Rate / (canopy.DiffusivitySolubilityRatio * Gbs) + canopy.OxygenPartialPressure;
        }

        protected override void UpdateChloroplasticCO2(Pathway path)
        {
            var a = (path.MesophyllCO2 * Calculator.X[3] + Calculator.X[4] - Calculator.X[5] * path.CO2Rate - Calculator.m - Calculator.X[6]);
            path.ChloroplasticCO2 = path.MesophyllCO2 + a * Calculator.X[7] / Gbs;
        }

        protected override AssimilationCalculator GetAc1Calculator(Pathway path)
        {
            var x = new double[9];

            x[0] = path.Current.VcMaxT;
            x[1] = path.Current.Kc / path.Current.Ko;
            x[2] = path.Current.Kc;
            x[3] = path.Current.VpMaxT / (path.MesophyllCO2 + path.Current.Kp);
            x[4] = 0.0;
            x[5] = 0.0;
            x[6] = path.ChloroplasticCO2 * path.Current.VcMaxT / (path.ChloroplasticCO2 + path.Current.Kc * (1 + path.ChloroplasticO2 / path.Current.Ko));
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
            x[5] = 0.0;
            x[6] = path.ChloroplasticCO2 * path.Current.VcMaxT / (path.ChloroplasticCO2 + path.Current.Kc * (1 + path.ChloroplasticO2 / path.Current.Ko));
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

            x[0] = (1 - pway.MesophyllElectronTransportFraction) * pway.ATPProductionElectronTransportFactor * path.Current.J / 3.0;
            x[1] = 7.0 / 3.0 * path.Current.Gamma;
            x[2] = 0.0;
            x[3] = 0.0;
            x[4] = pway.MesophyllElectronTransportFraction * pway.ATPProductionElectronTransportFactor * path.Current.J / pway.ExtraATPCost;
            x[5] = 0.0;
            x[6] = path.ChloroplasticCO2 * (1 - pway.MesophyllElectronTransportFraction) * pway.ATPProductionElectronTransportFactor * path.Current.J / (3 * path.ChloroplasticCO2 + 7 * path.Current.Gamma * path.ChloroplasticO2);
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
