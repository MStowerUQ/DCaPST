﻿using DCAPST.Canopy;
using DCAPST.Interfaces;
using DCAPST;

namespace Validation.C4
{
    public static class Initialise
    {
        public static CanopyParameters NewSorghumParameters()
        {
            double PsiFactor = 0.4;
            
            var j = new LeafTemperatureParameters()
            {
                TMin = 0,
                TOpt = 37.8649150880407,
                TMax = 55,
                C = 0.711229539802063,
                Beta = 1
            };

            var g = new LeafTemperatureParameters()
            {
                TMin = 0,
                TOpt = 42,
                TMax = 55,
                C = 0.462820450976839,
                Beta = 1,
            };

            var rubiscoCarboxylation = new TemperatureResponseValues()
            {
                At25 = 1210,
                Factor = 64200
            };

            var rubiscoOxygenation = new TemperatureResponseValues()
            {
                At25 = 292000,
                Factor = 10500
            };

            var rubiscoCarboxylationToOxygenation = new TemperatureResponseValues()
            {
                At25 = 5.51328906454566,
                Factor = 21265.4029552906
            };

            var pepc = new TemperatureResponseValues()
            {
                At25 = 75,
                Factor = 36300
            };

            var rubiscoActivity = new TemperatureResponseValues()
            {
                Factor = 78000
            };

            var respiration = new TemperatureResponseValues()
            {
                Factor = 46390
            };

            var pepcActivity = new TemperatureResponseValues()
            {
                Factor = 57043.2677590512
            };

            var CPath = new PathwayParameters()
            {
                PEPRegeneration = 120,
                SpectralCorrectionFactor = 0.15,
                PS2ActivityFraction = 0.1,
                BundleSheathConductance = 0.003,
                
                MaxRubiscoActivitySLNRatio = 0.465 * PsiFactor,
                MaxElectronTransportSLNRatio = 2.7 * PsiFactor,
                RespirationSLNRatio = 0.0 * PsiFactor,
                MaxPEPcActivitySLNRatio = 1.55 * PsiFactor,
                MesophyllCO2ConductanceSLNRatio = 0.0135 * PsiFactor,

                ExtraATPCost = 2,
                MesophyllElectronTransportFraction = 0.4,
                IntercellularToAirCO2Ratio = 0.45,

                RubiscoCarboxylation = rubiscoCarboxylation,
                RubiscoOxygenation = rubiscoOxygenation,                
                RubiscoCarboxylationToOxygenation = rubiscoCarboxylationToOxygenation,
                PEPc = pepc,
                RubiscoActivity = rubiscoActivity,
                Respiration = respiration,
                PEPcActivity = pepcActivity,

                ElectronTransportRateParams = j,
                MesophyllCO2ConductanceParams = g
            };            

            var canopy = new CanopyParameters()
            {
                Type = CanopyType.C4,

                Pathway = CPath,

                AirCO2 = 363,
                CurvatureFactor = 0.7,
                DiffusivitySolubilityRatio = 0.047,
                AirO2 = 210000,

                DiffuseExtCoeff = 0.78,
                DiffuseExtCoeffNIR = 0.8,
                DiffuseReflectionCoeff = 0.036,
                DiffuseReflectionCoeffNIR = 0.389,

                LeafAngle = 60,
                LeafScatteringCoeff = 0.15,
                LeafScatteringCoeffNIR = 0.8,
                LeafWidth = 0.15,

                SLNRatioTop = 1.3,
                MinimumN = 14,

                Windspeed = 1.5,
                WindSpeedExtinction = 1.5
            };

            return canopy;
        }
    }
}
