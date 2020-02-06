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
            MesophyllCO2 = intercellularCO2 - CO2Rate / MesophyllCO2ConductanceAtT;
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
                x1 = VcMaxT,
                x2 = Kc / Ko,
                x3 = Kc,
                x4 = VpMaxT / (MesophyllCO2 + Kp),
                x5 = 0.0,
                x6 = 0.0,
                x7 = ChloroplasticCO2 * VcMaxT / (ChloroplasticCO2 + Kc * (1 + ChloroplasticO2 / Ko)),
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
                x6 = 0.0,
                x7 = ChloroplasticCO2 * VcMaxT / (ChloroplasticCO2 + Kc * (1 + ChloroplasticO2 / Ko)),
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
                x1 = (1 - path.MesophyllElectronTransportFraction) * path.ATPProductionElectronTransportFactor * ElectronTransportRate / 3.0,
                x2 = 7.0 / 3.0 * G_,
                x3 = 0.0,
                x4 = 0.0,
                x5 = path.MesophyllElectronTransportFraction * path.ATPProductionElectronTransportFactor * ElectronTransportRate / path.ExtraATPCost,
                x6 = 0.0,
                x7 = ChloroplasticCO2 * (1 - path.MesophyllElectronTransportFraction) * path.ATPProductionElectronTransportFactor * ElectronTransportRate / (3 * ChloroplasticCO2 + 7 * G_ * ChloroplasticO2),
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
