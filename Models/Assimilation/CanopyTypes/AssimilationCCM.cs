using System;

using DCAPST.Canopy;
using DCAPST.Interfaces;

namespace DCAPST
{
    public class AssimilationCCM : Assimilation
    {
        public AssimilationCCM(IPartialCanopy partial) : base(partial)
        { }

        protected override void UpdateIntercellularCO2(AssimilationPathway pathway, double gt, double waterUseMolsSecond)
        {
            pathway.IntercellularCO2 = ((gt - waterUseMolsSecond / 2.0) * canopy.AirCO2 - pathway.CO2Rate) / (gt + waterUseMolsSecond / 2.0);
        }

        protected override void UpdateMesophyllCO2(AssimilationPathway pathway)
        {
            pathway.MesophyllCO2 = pathway.IntercellularCO2 - pathway.CO2Rate / pathway.Leaf.GmT;
        }

        protected override void UpdateChloroplasticO2(AssimilationPathway pathway)
        {
            pathway.ChloroplasticO2 = pway.PS2ActivityInBundleSheathFraction * pathway.CO2Rate / (canopy.DiffusivitySolubilityRatio * Gbs) + canopy.OxygenPartialPressure;
        }

        protected override void UpdateChloroplasticCO2(AssimilationPathway pathway)
        {
            var a = (pathway.MesophyllCO2 * calculator.X[3] + calculator.X[4] - calculator.X[5] * pathway.CO2Rate - calculator.m - calculator.X[6]);
            pathway.ChloroplasticCO2 = pathway.MesophyllCO2 + a * calculator.X[7] / Gbs;
        }

        protected override AssimilationFunction GetAc1Function(AssimilationPathway pathway)
        {
            var x = new double[9];

            x[0] = pathway.Leaf.VcMaxT;
            x[1] = pathway.Leaf.Kc / pathway.Leaf.Ko;
            x[2] = pathway.Leaf.Kc;
            x[3] = pathway.Leaf.VpMaxT / (pathway.MesophyllCO2 + pathway.Leaf.Kp);
            x[4] = 0.0;
            x[5] = 0.0;
            x[6] = pathway.ChloroplasticCO2 * pathway.Leaf.VcMaxT / (pathway.ChloroplasticCO2 + pathway.Leaf.Kc * (1 + pathway.ChloroplasticO2 / pathway.Leaf.Ko));
            x[7] = 1.0;
            x[8] = 1.0;

            var func = new AssimilationFunction()
            {
                X = x,

                m = pathway.Leaf.GmRd,
                t = pathway.Leaf.Gamma,
                sb = 0.1 / canopy.DiffusivitySolubilityRatio,
                j = Gbs,
                e = canopy.OxygenPartialPressure,
                R = pathway.Leaf.RdT
            };

            return func;
        }

        protected override AssimilationFunction GetAc2Function(AssimilationPathway pathway)
        {
            var x = new double[9];

            x[0] = pathway.Leaf.VcMaxT;
            x[1] = pathway.Leaf.Kc / pathway.Leaf.Ko;
            x[2] = pathway.Leaf.Kc;
            x[3] = 0.0;
            x[4] = Vpr;
            x[5] = 0.0;
            x[6] = pathway.ChloroplasticCO2 * pathway.Leaf.VcMaxT / (pathway.ChloroplasticCO2 + pathway.Leaf.Kc * (1 + pathway.ChloroplasticO2 / pathway.Leaf.Ko));
            x[7] = 1.0;
            x[8] = 1.0;

            var func = new AssimilationFunction()
            {
                X = x,

                m = pathway.Leaf.GmRd,
                t = pathway.Leaf.Gamma,
                sb = 0.1 / canopy.DiffusivitySolubilityRatio,
                j = Gbs,
                e = canopy.OxygenPartialPressure,
                R = pathway.Leaf.RdT
            };

            return func;
        }

        protected override AssimilationFunction GetAjFunction(AssimilationPathway pathway)
        {
            var x = new double[9];

            x[0] = (1 - pway.MesophyllElectronTransportFraction) * pway.ATPProductionElectronTransportFactor * pathway.Leaf.J / 3.0;
            x[1] = 7.0 / 3.0 * pathway.Leaf.Gamma;
            x[2] = 0.0;
            x[3] = 0.0;
            x[4] = pway.MesophyllElectronTransportFraction * pway.ATPProductionElectronTransportFactor * pathway.Leaf.J / pway.ExtraATPCost;
            x[5] = 0.0;
            x[6] = pathway.ChloroplasticCO2 * (1 - pway.MesophyllElectronTransportFraction) * pway.ATPProductionElectronTransportFactor * pathway.Leaf.J / (3 * pathway.ChloroplasticCO2 + 7 * pathway.Leaf.Gamma * pathway.ChloroplasticO2);
            x[7] = 1.0;
            x[8] = 1.0;

            var param = new AssimilationFunction()
            {
                X = x,

                m = pathway.Leaf.GmRd,
                t = pathway.Leaf.Gamma,
                sb = 0.1 / canopy.DiffusivitySolubilityRatio,
                j = Gbs,
                e = canopy.OxygenPartialPressure,
                R = pathway.Leaf.RdT
            };

            return param;
        }
    }
}
