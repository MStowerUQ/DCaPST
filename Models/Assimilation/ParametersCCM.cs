using System;

using DCAPST.Canopy;
using DCAPST.Interfaces;

namespace DCAPST
{
    public class ParametersCCM : Assimilation
    {
        public ParametersCCM(AssimilationType type, IPartialCanopy partial) : base(type, partial)
        { }

        public override void UpdateMesophyllCO2(double intercellularCO2, double CO2Rate)
        {
            Path.MesophyllCO2 = intercellularCO2 - CO2Rate / Path.Current.GmT;
        }

        public override void UpdateChloroplasticO2(double CO2Rate)
        {
            Path.ChloroplasticO2 = pway.PS2ActivityInBundleSheathFraction * CO2Rate / (canopy.DiffusivitySolubilityRatio * Gbs) + canopy.OxygenPartialPressure;
        }

        public override void UpdateChloroplasticCO2(double CO2Rate)
        {
            var a = (Path.MesophyllCO2 * Calculator.X[3] + Calculator.X[4] - Calculator.X[5] * CO2Rate - Calculator.m - Calculator.X[6]);
            Path.ChloroplasticCO2 = Path.MesophyllCO2 + a * Calculator.X[7] / Gbs;
        }

        protected override AssimilationCalculator GetAc1Calculator()
        {
            var x = new double[9];

            x[0] = Path.Current.VcMaxT;
            x[1] = Path.Current.Kc / Path.Current.Ko;
            x[2] = Path.Current.Kc;
            x[3] = Path.Current.VpMaxT / (Path.MesophyllCO2 + Path.Current.Kp);
            x[4] = 0.0;
            x[5] = 0.0;
            x[6] = Path.ChloroplasticCO2 * Path.Current.VcMaxT / (Path.ChloroplasticCO2 + Path.Current.Kc * (1 + Path.ChloroplasticO2 / Path.Current.Ko));
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
            x[5] = 0.0;
            x[6] = Path.ChloroplasticCO2 * Path.Current.VcMaxT / (Path.ChloroplasticCO2 + Path.Current.Kc * (1 + Path.ChloroplasticO2 / Path.Current.Ko));
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

            x[0] = (1 - pway.MesophyllElectronTransportFraction) * pway.ATPProductionElectronTransportFactor * Path.Current.J / 3.0;
            x[1] = 7.0 / 3.0 * Path.Current.Gamma;
            x[2] = 0.0;
            x[3] = 0.0;
            x[4] = pway.MesophyllElectronTransportFraction * pway.ATPProductionElectronTransportFactor * Path.Current.J / base.pway.ExtraATPCost;
            x[5] = 0.0;
            x[6] = Path.ChloroplasticCO2 * (1 - pway.MesophyllElectronTransportFraction) * pway.ATPProductionElectronTransportFactor * Path.Current.J / (3 * Path.ChloroplasticCO2 + 7 * Path.Current.Gamma * Path.ChloroplasticO2);
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
