using System;

using DCAPST.Canopy;
using DCAPST.Interfaces;

namespace DCAPST
{
    public class ParametersCCM : AssimilationParameters
    {
        public ParametersCCM(IAssimilation assimilation, IPartialCanopy partial) : base(assimilation, partial)
        { }

        public override void UpdateMesophyllCO2(double intercellularCO2, double CO2Rate)
        {
            MesophyllCO2 = intercellularCO2 - CO2Rate / Current.GmT;
        }

        public override void UpdateChloroplasticO2(double CO2Rate)
        {
            ChloroplasticO2 = path.PS2ActivityInBundleSheathFraction * CO2Rate / (canopy.DiffusivitySolubilityRatio * Gbs) + canopy.OxygenPartialPressure;
        }

        public override void UpdateChloroplasticCO2(double CO2Rate)
        {
            var a = (MesophyllCO2 * Calculator.x4 + Calculator.x5 - Calculator.x6 * CO2Rate - Calculator.m - Calculator.x7);
            ChloroplasticCO2 = MesophyllCO2 + a * Calculator.x8 / Gbs;
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
                x6 = 0.0,
                x7 = ChloroplasticCO2 * Current.VcMaxT / (ChloroplasticCO2 + Current.Kc * (1 + ChloroplasticO2 / Current.Ko)),
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
                x6 = 0.0,
                x7 = ChloroplasticCO2 * Current.VcMaxT / (ChloroplasticCO2 + Current.Kc * (1 + ChloroplasticO2 / Current.Ko)),
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
                x1 = (1 - path.MesophyllElectronTransportFraction) * path.ATPProductionElectronTransportFactor * Current.J / 3.0,
                x2 = 7.0 / 3.0 * Current.Gamma,
                x3 = 0.0,
                x4 = 0.0,
                x5 = path.MesophyllElectronTransportFraction * path.ATPProductionElectronTransportFactor * Current.J / path.ExtraATPCost,
                x6 = 0.0,
                x7 = ChloroplasticCO2 * (1 - path.MesophyllElectronTransportFraction) * path.ATPProductionElectronTransportFactor * Current.J / (3 * ChloroplasticCO2 + 7 * Current.Gamma * ChloroplasticO2),
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
