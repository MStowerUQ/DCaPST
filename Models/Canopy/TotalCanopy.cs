using System;
using DCAPST.Environment;
using DCAPST.Interfaces;

namespace DCAPST.Canopy
{
    public class TotalCanopy : BaseCanopy
    {        
        public PartialCanopy Sunlit { get; private set; }
        public PartialCanopy Shaded { get; private set; }

        public CanopyRadiation Absorbed;

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
        }

        public void Initialise(double lai, double sln)
        {
            LAI = lai;

            // CalcLeafNitrogenDistribution
            var SLNTop = sln * Canopy.SLNRatioTop;
            LeafNTopCanopy = SLNTop * 1000 / 14;

            var NcAv = sln * 1000 / 14;
            NAllocationCoeff = -1 * Math.Log((NcAv - Canopy.StructuralN) / (LeafNTopCanopy - Canopy.StructuralN)) * 2;

            WindSpeed = Canopy.Windspeed;
            WindSpeedExtinction = Canopy.WindSpeedExtinction;
            LeafAngle = new Angle(Canopy.LeafAngle, AngleType.Deg);
            LeafWidth = Canopy.LeafWidth;

            var layerLAI = LAI / Layers;

            Absorbed = new CanopyRadiation(Layers, layerLAI)
            {
                DiffuseExtCoeff = Canopy.DiffuseExtCoeff,
                LeafScatteringCoeff = Canopy.LeafScatteringCoeff,
                DiffuseReflectionCoeff = Canopy.DiffuseReflectionCoeff
            };         
        }

        public void Run(IRadiation radiation)
        {
            ResetPartials();
            CalcLAI();
            CalcAbsorbedRadiations(radiation);
            CalcMaximumRates();
        }

        public void ResetPartials()
        {
            // Reset the partial canopies
            Sunlit = new PartialCanopy(Canopy);
            Shaded = new PartialCanopy(Canopy);
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

        public void CalcLAI()
        {
            Sunlit.LAI = Absorbed.CalculateSunlitLAI();
            Shaded.LAI = LAI - Sunlit.LAI;
        }

        public void CalcCanopyStructure(double sunAngleRadians)
        {        
            // Beam Extinction Coefficient
            if (sunAngleRadians > 0)
                Absorbed.BeamExtinctionCoeff = CalcShadowProjection(sunAngleRadians) / Math.Sin(sunAngleRadians);
            else
                Absorbed.BeamExtinctionCoeff = 0;

            // Intercepted radiation
            PropnInterceptedRadns = Absorbed.CalculateAccumInterceptedRadn();
            // TODO: Make this work with multiple layers
            //PropnInterceptedRadns = Rad.CalculateAccumInterceptedRadn() - PropnInterceptedRadns0;
        }

        private double CalcShadowProjection(double sunAngleRadians)
        {
            if (LeafAngle.Rad <= sunAngleRadians)
            {
                return Math.Cos(LeafAngle.Rad) * Math.Sin(sunAngleRadians);
            }
            else
            {
                double value = Math.Acos(1 / Math.Tan(LeafAngle.Rad) * Math.Tan(sunAngleRadians));
                Angle theta = new Angle(value, AngleType.Rad);

                return 2 / Math.PI * Math.Sin(LeafAngle.Rad) * Math.Cos(sunAngleRadians) * Math.Sin(theta.Rad)
                    + ((1 - theta.Deg / 90) * Math.Cos(LeafAngle.Rad) * Math.Sin(sunAngleRadians));
            }
        }

        public void CalcAbsorbedRadiations(IRadiation radiation)
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

        public void CalcMaximumRates()
        {
            var nTerm = NAllocationCoeff + (Absorbed.BeamExtinctionCoeff * LAI);
            
            VcMax25 = CalcMaximumRate(Canopy.Pathway.MaxRubiscoActivitySLNRatio, NAllocationCoeff);
            Sunlit.VcMax25 = CalcMaximumRate(Canopy.Pathway.MaxRubiscoActivitySLNRatio, nTerm);
            Shaded.VcMax25 = VcMax25 - Sunlit.VcMax25;

            Rd25 = CalcMaximumRate(Canopy.Pathway.RespirationSLNRatio, NAllocationCoeff);
            Sunlit.Rd25 = CalcMaximumRate(Canopy.Pathway.RespirationSLNRatio, nTerm);
            Shaded.Rd25 = Rd25 - Sunlit.Rd25;

            JMax25 = CalcMaximumRate(Canopy.Pathway.MaxElectronTransportSLNRatio, NAllocationCoeff);
            Sunlit.JMax25 = CalcMaximumRate(Canopy.Pathway.MaxElectronTransportSLNRatio, nTerm);
            Shaded.JMax25 = JMax25 - Sunlit.JMax25;

            VpMax25 = CalcMaximumRate(Canopy.Pathway.MaxPEPcActivitySLNRatio, NAllocationCoeff);
            Sunlit.VpMax25 = CalcMaximumRate(Canopy.Pathway.MaxPEPcActivitySLNRatio, nTerm);
            Shaded.VpMax25 = VpMax25 - Sunlit.VpMax25;

            Gm25 = CalcMaximumRate(Canopy.Pathway.MesophyllCO2ConductanceSLNRatio, NAllocationCoeff);
            Sunlit.Gm25 = CalcMaximumRate(Canopy.Pathway.MesophyllCO2ConductanceSLNRatio, nTerm);
            Shaded.Gm25 = Gm25 - Sunlit.Gm25;
        }

        // Total: nTerm = NAllocationCoeff
        // Sunlit: nTerm = NAllocationCoeff + (BeamExtCoeff * LAI)
        public double CalcMaximumRate(double psi, double nTerm)
        {
            var factor = LAI * (LeafNTopCanopy - Canopy.StructuralN) * psi;
            var exp = Absorbed.CalcExp(nTerm / LAI);

            return factor * exp / nTerm;
        }
        
    }
}
