using System;
using DCAPST.Interfaces;

namespace DCAPST.Canopy
{
    public class TotalCanopy : ITotalCanopy
    {
        public ICanopyParameters Canopy { get; set; }
        public double LAI { get; set; }

        public IPartialCanopy Sunlit { get; private set; }
        public IPartialCanopy Shaded { get; private set; }

        public CanopyRadiation Absorbed { get; private set; }

        public Angle LeafAngle { get; set; } 
        public double LeafWidth { get; set; }
        public double LeafNTopCanopy { get; set; }

        public double WindSpeed { get; set; }
        public double WindSpeedExtinction { get; set; }

        public double NAllocationCoeff { get; set; }

        public double PropnInterceptedRadns { get; set; }

        public int Layers { get; }

        public TotalCanopy(ICanopyParameters canopy, int layers)
        {
            Canopy = canopy;
            Layers = layers;

            WindSpeed = canopy.Windspeed;
            WindSpeedExtinction = canopy.WindSpeedExtinction;
            LeafAngle = new Angle(canopy.LeafAngle, AngleType.Deg);
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
            var photons = Absorbed.CalculateTotalRadiation(radiation.DirectPAR, radiation.DiffusePAR);
            Sunlit.PhotonCount = Absorbed.CalcSunlitRadiation(radiation.DirectPAR, radiation.DiffusePAR);
            Shaded.PhotonCount = photons - Sunlit.PhotonCount;

            // Energy calculations (used by water interaction)
            var PARDirect = radiation.Direct * 0.5 * 1000000;
            var PARDiffuse = radiation.Diffuse * 0.5 * 1000000;
            var NIRDirect = radiation.Direct * 0.5 * 1000000;
            var NIRDiffuse = radiation.Diffuse * 0.5 * 1000000;

            var PARTotalIrradiance = Absorbed.CalculateTotalRadiation(PARDirect, PARDiffuse);
            var SunlitPARTotalIrradiance = Absorbed.CalcSunlitRadiation(PARDirect, PARDiffuse);
            var ShadedPARTotalIrradiance = PARTotalIrradiance - SunlitPARTotalIrradiance;

            // Adjust parameters for NIR calculations
            Absorbed.DiffuseExtCoeff = Canopy.DiffuseExtCoeffNIR;
            Absorbed.LeafScatteringCoeff = Canopy.LeafScatteringCoeffNIR;
            Absorbed.DiffuseReflectionCoeff = Canopy.DiffuseReflectionCoeffNIR;

            var NIRTotalIrradiance = Absorbed.CalculateTotalRadiation(NIRDirect, NIRDiffuse);
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
            Sunlit.RubiscoActivity25 = CalcMaximumRate(Canopy.Pathway.MaxRubiscoActivitySLNRatio, sunlitCoefficient);
            Shaded.RubiscoActivity25 = RubiscoActivity25 - Sunlit.RubiscoActivity25;

            var Rd25 = CalcMaximumRate(Canopy.Pathway.RespirationSLNRatio, coefficient);
            Sunlit.Rd25 = CalcMaximumRate(Canopy.Pathway.RespirationSLNRatio, sunlitCoefficient);
            Shaded.Rd25 = Rd25 - Sunlit.Rd25;

            var JMax25 = CalcMaximumRate(Canopy.Pathway.MaxElectronTransportSLNRatio, coefficient);
            Sunlit.JMax25 = CalcMaximumRate(Canopy.Pathway.MaxElectronTransportSLNRatio, sunlitCoefficient);
            Shaded.JMax25 = JMax25 - Sunlit.JMax25;

            var PEPcActivity25 = CalcMaximumRate(Canopy.Pathway.MaxPEPcActivitySLNRatio, coefficient);
            Sunlit.PEPcActivity25 = CalcMaximumRate(Canopy.Pathway.MaxPEPcActivitySLNRatio, sunlitCoefficient);
            Shaded.PEPcActivity25 = PEPcActivity25 - Sunlit.PEPcActivity25;

            var MesophyllCO2Conductance25 = CalcMaximumRate(Canopy.Pathway.MesophyllCO2ConductanceSLNRatio, coefficient);
            Sunlit.MesophyllCO2Conductance25 = CalcMaximumRate(Canopy.Pathway.MesophyllCO2ConductanceSLNRatio, sunlitCoefficient);
            Shaded.MesophyllCO2Conductance25 = MesophyllCO2Conductance25 - Sunlit.MesophyllCO2Conductance25;
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
            PropnInterceptedRadns = Absorbed.CalculateAccumInterceptedRadn();

            // TODO: Make this work with multiple layers 
            // (by subtracting the accumulated intercepted radiation of the previous layers) e.g:
            //PropnInterceptedRadns = Rad.CalculateAccumInterceptedRadn() - PropnInterceptedRadns0;
        }

        private double CalcShadowProjection(double sunAngle)
        {
            if (LeafAngle.Rad <= sunAngle)
            {
                return Math.Cos(LeafAngle.Rad) * Math.Sin(sunAngle);
            }
            else
            {
                double value = Math.Acos(1 / Math.Tan(LeafAngle.Rad) * Math.Tan(sunAngle));
                Angle theta = new Angle(value, AngleType.Rad);

                var a = 2 / Math.PI * Math.Sin(LeafAngle.Rad) * Math.Cos(sunAngle) * Math.Sin(theta.Rad);
                var b = (1 - theta.Deg / 90) * Math.Cos(LeafAngle.Rad) * Math.Sin(sunAngle);
                return a + b;
            }
        }
    }
}
