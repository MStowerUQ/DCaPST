﻿using System;

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
            MesophyllCO2 = intercellularCO2 - CO2Rate / Current.GmT;
        }

        public override void UpdateChloroplasticO2(double CO2Rate)
        {
            ChloroplasticO2 = path.PS2ActivityInBundleSheathFraction * CO2Rate / (canopy.DiffusivitySolubilityRatio * Gbs) + canopy.OxygenPartialPressure;
        }

        public override void UpdateChloroplasticCO2(double CO2Rate)
        {
            var a = (MesophyllCO2 * Calculator.X[3] + Calculator.X[4] - Calculator.X[5] * CO2Rate - Calculator.m - Calculator.X[6]);
            ChloroplasticCO2 = MesophyllCO2 + a * Calculator.X[7] / Gbs;
        }

        protected override AssimilationCalculator GetAc1Calculator()
        {
            var x = new double[9];

            x[0] = Current.VcMaxT;
            x[1] = Current.Kc / Current.Ko;
            x[2] = Current.Kc;
            x[3] = Current.VpMaxT / (MesophyllCO2 + Current.Kp);
            x[4] = 0.0;
            x[5] = 0.0;
            x[6] = ChloroplasticCO2 * Current.VcMaxT / (ChloroplasticCO2 + Current.Kc * (1 + ChloroplasticO2 / Current.Ko));
            x[7] = 1.0;
            x[8] = 1.0;

            var param = new AssimilationCalculator()
            {
                X = x,

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
            var x = new double[9];

            x[0] = Current.VcMaxT;
            x[1] = Current.Kc / Current.Ko;
            x[2] = Current.Kc;
            x[3] = 0.0;
            x[4] = Vpr;
            x[5] = 0.0;
            x[6] = ChloroplasticCO2 * Current.VcMaxT / (ChloroplasticCO2 + Current.Kc * (1 + ChloroplasticO2 / Current.Ko));
            x[7] = 1.0;
            x[8] = 1.0;

            var param = new AssimilationCalculator()
            {
                X = x,

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
            var x = new double[9];

            x[0] = (1 - path.MesophyllElectronTransportFraction) * path.ATPProductionElectronTransportFactor * Current.J / 3.0;
            x[1] = 7.0 / 3.0 * Current.Gamma;
            x[2] = 0.0;
            x[3] = 0.0;
            x[4] = path.MesophyllElectronTransportFraction * path.ATPProductionElectronTransportFactor * Current.J / path.ExtraATPCost;
            x[5] = 0.0;
            x[6] = ChloroplasticCO2 * (1 - path.MesophyllElectronTransportFraction) * path.ATPProductionElectronTransportFactor * Current.J / (3 * ChloroplasticCO2 + 7 * Current.Gamma * ChloroplasticO2);
            x[7] = 1.0;
            x[8] = 1.0;

            var param = new AssimilationCalculator()
            {
                X = x,

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
