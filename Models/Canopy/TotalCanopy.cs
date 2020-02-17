using System;
using DCAPST.Interfaces;

namespace DCAPST.Canopy
{
    /// <summary>
    /// Models a complete canopy
    /// </summary>
    public class TotalCanopy : ITotalCanopy
    {
        /// <summary>
        /// The initial parameters of the canopy
        /// </summary>
        public ICanopyParameters Canopy { get; set; }
        
        /// <summary>
        /// The part of the canopy in sunlight
        /// </summary>
        public IPartialCanopy Sunlit { get; private set; }

        /// <summary>
        /// The part of the canopy in shade
        /// </summary>
        public IPartialCanopy Shaded { get; private set; }

        /// <summary>
        /// The radiation absorbed by the canopy
        /// </summary>
        private CanopyRadiation Absorbed { get; set; }

        /// <summary>
        /// Leaf area index of the canopy
        /// </summary>
        private double LAI { get; set; }

        /// <summary>
        /// The leaf angle (radians)
        /// </summary>
        private double LeafAngle { get; set; }
        private double LeafWidth { get; set; }
        private double LeafNTopCanopy { get; set; }

        private double WindSpeed { get; set; }
        private double WindSpeedExtinction { get; set; }

        private double NAllocationCoeff { get; set; }

        public double InterceptedRadiation { get; set; }

        /// <summary>
        /// The number of layers in the canopy
        /// </summary>
        private int Layers { get; }

        public TotalCanopy(ICanopyParameters canopy, int layers)
        {
            Canopy = canopy;
            Layers = layers;

            WindSpeed = canopy.Windspeed;
            WindSpeedExtinction = canopy.WindSpeedExtinction;
            LeafAngle = canopy.LeafAngle.ToRadians();
            LeafWidth = canopy.LeafWidth;
        }

        public void InitialiseDay(double lai, double sln)
        {
            LAI = lai;

            var SLNTop = sln * Canopy.SLNRatioTop;
            LeafNTopCanopy = SLNTop * 1000 / 14;

            var NcAv = sln * 1000 / 14;
            NAllocationCoeff = -1 * Math.Log((NcAv - Canopy.StructuralN) / (LeafNTopCanopy - Canopy.StructuralN)) * 2;           

            Absorbed = new CanopyRadiation(Layers, LAI)
            {
                DiffuseExtCoeff = Canopy.DiffuseExtCoeff,
                LeafScatteringCoeff = Canopy.LeafScatteringCoeff,
                DiffuseReflectionCoeff = Canopy.DiffuseReflectionCoeff
            };         
        }

        public void PerformTimeAdjustment(ISolarRadiation radiation)
        {
            ResetPartials();
            CalcLAI();
            CalcAbsorbedRadiations(radiation);
            CalcMaximumRates();
        }

        private void ResetPartials()
        {
            // Reset the partial canopies
            Sunlit = new PartialCanopy(Canopy);
            Shaded = new PartialCanopy(Canopy);
        }

        public void CalcLAI()
        {
            Sunlit.LAI = Absorbed.CalculateSunlitLAI();
            Shaded.LAI = LAI - Sunlit.LAI;
        }

        private void CalcAbsorbedRadiations(ISolarRadiation radiation)
        {
            // Set parameters
            Absorbed.DiffuseExtCoeff = Canopy.DiffuseExtCoeff;
            Absorbed.LeafScatteringCoeff = Canopy.LeafScatteringCoeff;
            Absorbed.DiffuseReflectionCoeff = Canopy.DiffuseReflectionCoeff;

            // Photon calculations (used by photosynthesis)
            var photons = Absorbed.CalcTotalRadiation(radiation.DirectPAR, radiation.DiffusePAR);
            Sunlit.PhotonCount = Absorbed.CalcSunlitRadiation(radiation.DirectPAR, radiation.DiffusePAR);
            Shaded.PhotonCount = photons - Sunlit.PhotonCount;

            // Energy calculations (used by water interaction)
            var PARDirect = radiation.Direct * 0.5 * 1000000;
            var PARDiffuse = radiation.Diffuse * 0.5 * 1000000;
            var NIRDirect = radiation.Direct * 0.5 * 1000000;
            var NIRDiffuse = radiation.Diffuse * 0.5 * 1000000;

            var PARTotalIrradiance = Absorbed.CalcTotalRadiation(PARDirect, PARDiffuse);
            var SunlitPARTotalIrradiance = Absorbed.CalcSunlitRadiation(PARDirect, PARDiffuse);
            var ShadedPARTotalIrradiance = PARTotalIrradiance - SunlitPARTotalIrradiance;

            // Adjust parameters for NIR calculations
            Absorbed.DiffuseExtCoeff = Canopy.DiffuseExtCoeffNIR;
            Absorbed.LeafScatteringCoeff = Canopy.LeafScatteringCoeffNIR;
            Absorbed.DiffuseReflectionCoeff = Canopy.DiffuseReflectionCoeffNIR;

            var NIRTotalIrradiance = Absorbed.CalcTotalRadiation(NIRDirect, NIRDiffuse);
            var SunlitNIRTotalIrradiance = Absorbed.CalcSunlitRadiation(NIRDirect, NIRDiffuse);
            var ShadedNIRTotalIrradiance = NIRTotalIrradiance - SunlitNIRTotalIrradiance;

            Sunlit.AbsorbedRadiation = SunlitPARTotalIrradiance + SunlitNIRTotalIrradiance;
            Shaded.AbsorbedRadiation = ShadedPARTotalIrradiance + ShadedNIRTotalIrradiance;
        }

        private void CalcMaximumRates()
        {
            var coefficient = NAllocationCoeff;
            var sunlitCoefficient = NAllocationCoeff + (Absorbed.BeamExtinctionCoeff * LAI);

            var RubiscoActivity25 = CalcMaximumRate(Canopy.Pathway.MaxRubiscoActivitySLNRatio, coefficient);
            Sunlit.At25C.VcMax = CalcMaximumRate(Canopy.Pathway.MaxRubiscoActivitySLNRatio, sunlitCoefficient);
            Shaded.At25C.VcMax = RubiscoActivity25 - Sunlit.At25C.VcMax;

            var Rd25 = CalcMaximumRate(Canopy.Pathway.RespirationSLNRatio, coefficient);
            Sunlit.At25C.Rd = CalcMaximumRate(Canopy.Pathway.RespirationSLNRatio, sunlitCoefficient);
            Shaded.At25C.Rd = Rd25 - Sunlit.At25C.Rd;

            var JMax25 = CalcMaximumRate(Canopy.Pathway.MaxElectronTransportSLNRatio, coefficient);
            Sunlit.At25C.JMax = CalcMaximumRate(Canopy.Pathway.MaxElectronTransportSLNRatio, sunlitCoefficient);
            Shaded.At25C.JMax = JMax25 - Sunlit.At25C.JMax;

            var PEPcActivity25 = CalcMaximumRate(Canopy.Pathway.MaxPEPcActivitySLNRatio, coefficient);
            Sunlit.At25C.VpMax = CalcMaximumRate(Canopy.Pathway.MaxPEPcActivitySLNRatio, sunlitCoefficient);
            Shaded.At25C.VpMax = PEPcActivity25 - Sunlit.At25C.VpMax;

            var MesophyllCO2Conductance25 = CalcMaximumRate(Canopy.Pathway.MesophyllCO2ConductanceSLNRatio, coefficient);
            Sunlit.At25C.Gm = CalcMaximumRate(Canopy.Pathway.MesophyllCO2ConductanceSLNRatio, sunlitCoefficient);
            Shaded.At25C.Gm = MesophyllCO2Conductance25 - Sunlit.At25C.Gm;
        }

        private double CalcMaximumRate(double psi, double coefficient)
        {
            var factor = LAI * (LeafNTopCanopy - Canopy.StructuralN) * psi;
            var exp = Absorbed.CalcExp(coefficient / LAI);

            return factor * exp / coefficient;
        }

        public double CalcBoundaryHeatConductance()
        {
            var a = 0.5 * WindSpeedExtinction;
            var b = 0.01 * Math.Pow(WindSpeed / LeafWidth, 0.5);
            var c = 1 - Math.Exp(-a * LAI);

            return b * c / a;
        }

        public double CalcSunlitBoundaryHeatConductance()
        {
            var a = 0.5 * WindSpeedExtinction + Absorbed.BeamExtinctionCoeff;
            var b = 0.01 * Math.Pow(WindSpeed / LeafWidth, 0.5);
            var c = 1 - Math.Exp(-a * LAI);            

            return b * c / a;
        }
                
        public void CalcCanopyStructure(double sunAngle)
        {        
            // Beam Extinction Coefficient
            if (sunAngle > 0)
                Absorbed.BeamExtinctionCoeff = CalcShadowProjection(sunAngle) / Math.Sin(sunAngle);
            else
                Absorbed.BeamExtinctionCoeff = 0;

            // Intercepted radiation
            InterceptedRadiation = Absorbed.CalcInterceptedRadiation();

            // TODO: Make this work with multiple layers 
            // (by subtracting the accumulated intercepted radiation of the previous layers) e.g:
            // InterceptedRadiation_1 = Absorbed.CalcInterceptedRadiation() - InterceptedRadiation_0;
        }

        private double CalcShadowProjection(double sunAngle)
        {
            if (LeafAngle <= sunAngle)
            {
                return Math.Cos(LeafAngle) * Math.Sin(sunAngle);
            }
            else
            {
                double theta = Math.Acos(1 / Math.Tan(LeafAngle) * Math.Tan(sunAngle));

                var a = 2 / Math.PI * Math.Sin(LeafAngle) * Math.Cos(sunAngle) * Math.Sin(theta);
                var b = (1 - theta * 2 / Math.PI) * Math.Cos(LeafAngle) * Math.Sin(sunAngle);
                return a + b;
            }
        }
    }
}
