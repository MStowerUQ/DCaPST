﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DCAPST.Canopy;
using DCAPST.Environment;
using DCAPST.Interfaces;

namespace DCAPST.Utilities
{
    /// <summary>
    /// This classic exists to simplify the interaction with Apsim Classic
    /// </summary>
    public static class Classic
    {
        public static CanopyParameters SetUpCanopy(
            CanopyType type,
            double airCO2,
            double curvatureFactor,
            double diffusivitySolubilityRatio,
            double airO2,
            double diffuseExtCoeff,
            double diffuseExtCoeffNIR,
            double diffuseReflectionCoeff,
            double diffuseReflectionCoeffNIR,
            double leafAngle,
            double leafScatteringCoeff,
            double leafScatteringCoeffNIR,
            double leafWidth,
            double slnRatioTop,
            double minimumN,
            double windspeed,
            double windSpeedExtinction
        )
        {
            var CP = new CanopyParameters
            {
                Type = type,
                AirCO2 = airCO2,
                CurvatureFactor = curvatureFactor,
                DiffusivitySolubilityRatio = diffusivitySolubilityRatio,
                AirO2 = airO2,
                DiffuseExtCoeff = diffuseExtCoeff,
                DiffuseExtCoeffNIR = diffuseExtCoeffNIR,
                DiffuseReflectionCoeff = diffuseReflectionCoeff,
                DiffuseReflectionCoeffNIR = diffuseReflectionCoeffNIR,
                LeafAngle = leafAngle,
                LeafScatteringCoeff = leafScatteringCoeff,
                LeafScatteringCoeffNIR = leafScatteringCoeffNIR,
                LeafWidth = leafWidth,
                SLNRatioTop = slnRatioTop,
                MinimumN = minimumN,
                Windspeed = windspeed,
                WindSpeedExtinction = windSpeedExtinction
            };

            return CP;
        }

        public static PathwayParameters SetUpPathway(
            double jTMin,
            double jTOpt,
            double jTMax,
            double jC,
            double jBeta,
            double gTMin,
            double gTOpt,
            double gTMax,
            double gC,
            double gBeta,
            double KcAt25,
            double KcFactor,
            double KoAt25,
            double KoFactor,
            double VcVoAt25,
            double VcVoFactor,
            double KpAt25,
            double KpFactor,
            double VcFactor,
            double RdFactor,
            double VpFactor,
            double pepRegeneration,
            double spectralCorrectionFactor,
            double ps2ActivityFraction,
            double bundleSheathConductance,
            double maxRubiscoActivitySLNRatio,
            double maxElectronTransportSLNRatio,
            double respirationSLNRatio,
            double maxPEPcActivitySLNRatio,
            double mesophyllCO2ConductanceSLNRatio,
            double extraATPCost,
            double mesophyllElectronTransportFraction,
            double intercellularToAirCO2Ratio
        )
        {
            var j = new LeafTemperatureParameters
            {
                TMin = jTMin,
                TOpt = jTOpt,
                TMax = jTMax,
                C = jC,
                Beta = jBeta
            };

            var g = new LeafTemperatureParameters
            {
                TMin = gTMin,
                TOpt = gTOpt,
                TMax = gTMax,
                C = gC,
                Beta = gBeta,
            };

            var Kc = new TemperatureResponseValues
            {
                At25 = KcAt25,
                Factor = KcFactor
            };

            var Ko = new TemperatureResponseValues
            {
                At25 = KoAt25,
                Factor = KoFactor
            };

            var VcVo = new TemperatureResponseValues
            {
                At25 = VcVoAt25,
                Factor = VcVoFactor
            };

            var Kp = new TemperatureResponseValues
            {
                At25 = KpAt25,
                Factor = KpFactor
            };

            var Vc = new TemperatureResponseValues
            {
                Factor = VcFactor
            };

            var Rd = new TemperatureResponseValues
            {
                Factor = RdFactor
            };

            var Vp = new TemperatureResponseValues
            {
                Factor = VpFactor
            };

            var PP = new PathwayParameters
            {
                PEPRegeneration = pepRegeneration,
                SpectralCorrectionFactor = spectralCorrectionFactor,
                PS2ActivityFraction = ps2ActivityFraction,
                BundleSheathConductance = bundleSheathConductance,
                MaxRubiscoActivitySLNRatio = maxRubiscoActivitySLNRatio,
                MaxElectronTransportSLNRatio = maxElectronTransportSLNRatio,
                RespirationSLNRatio = respirationSLNRatio,
                MaxPEPcActivitySLNRatio = maxPEPcActivitySLNRatio,
                MesophyllCO2ConductanceSLNRatio = mesophyllCO2ConductanceSLNRatio,
                ExtraATPCost = extraATPCost,
                MesophyllElectronTransportFraction = mesophyllElectronTransportFraction,
                IntercellularToAirCO2Ratio = intercellularToAirCO2Ratio,
                RubiscoCarboxylation = Kc,
                RubiscoOxygenation = Ko,
                RubiscoCarboxylationToOxygenation = VcVo,
                PEPc = Kp,
                RubiscoActivity = Vc,
                Respiration = Rd,
                PEPcActivity = Vp,
                ElectronTransportRateParams = j,
                MesophyllCO2ConductanceParams = g
            };
            
            // Only used in CCM
            PP.MesophyllElectronTransportFraction = PP.ExtraATPCost / (3.0 + PP.ExtraATPCost);
            PP.FractionOfCyclicElectronFlow = 0.25 * PP.ExtraATPCost;
            PP.ATPProductionElectronTransportFactor = (3.0 - PP.FractionOfCyclicElectronFlow) / (4.0 * (1.0 - PP.FractionOfCyclicElectronFlow));

            return PP;
        }

        public static DCAPSTModel SetUpModel(
            ICanopyParameters CP, 
            IPathwayParameters PP, 
            int DOY, 
            double latitude, 
            double maxT, 
            double minT, 
            double radn)
        {
            // Model the solar geometry
            var SG = new SolarGeometry
            {
                Latitude = latitude.ToRadians(),
                DayOfYear = DOY
            };

            // Model the solar radiation
            var SR = new SolarRadiation(SG)
            {
                Daily = radn,
                RPAR = 0.5
            };

            // Model the environmental temperature
            var TM = new Temperature(SG)
            {
                MaxTemperature = maxT,
                MinTemperature = minT,
                AtmosphericPressure = 1.01325
            };

            // Model the leaf water interaction and temperature response
            var water = new WaterInteraction(TM);
            var response = new TemperatureResponse(CP, PP);

            // Model the pathways
            var SunlitAc1 = new AssimilationPathway(CP, PP);
            var SunlitAc2 = new AssimilationPathway(CP, PP);
            var SunlitAj = new AssimilationPathway(CP, PP);

            var ShadedAc1 = new AssimilationPathway(CP, PP);
            var ShadedAc2 = new AssimilationPathway(CP, PP);
            var ShadedAj = new AssimilationPathway(CP, PP);

            // Model the assimilation
            var C4 = new AssimilationC4(CP, PP);
            var sunlit = new AssimilationArea(SunlitAc1, SunlitAc2, SunlitAj, C4);
            var shaded = new AssimilationArea(ShadedAc1, ShadedAc2, ShadedAj, C4);

            // Model the canopy
            var CS = new CanopyAttributes(CP, PP, sunlit, shaded);

            // Model the photosynthesis
            var DM = new DCAPSTModel(SG, SR, TM, PP, CP, CS, water, response)
            {
                B = 0.409
            };

            return DM;
        }
    }
}
