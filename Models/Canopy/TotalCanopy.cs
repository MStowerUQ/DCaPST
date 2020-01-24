using System;
using DCAPST.Environment;
using DCAPST.Interfaces;

namespace DCAPST.Canopy
{
    public class TotalCanopy : BaseCanopy
    {        
        public PartialCanopy Sunlit { get; private set; }
        public PartialCanopy Shaded { get; private set; }

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
            Canopy.SLNAv = sln;

            // CalcLeafNitrogenDistribution
            var SLNTop = Canopy.SLNAv * Canopy.SLNRatioTop;
            LeafNTopCanopy = SLNTop * 1000 / 14;

            var NcAv = Canopy.SLNAv * 1000 / 14;
            NAllocationCoeff = -1 * Math.Log((NcAv - Canopy.StructuralN) / (LeafNTopCanopy - Canopy.StructuralN)) * 2;

            WindSpeed = Canopy.Windspeed;
            WindSpeedExtinction = Canopy.WindSpeedExtinction;
            LeafAngle = new Angle(Canopy.LeafAngle, AngleType.Deg);
            LeafWidth = Canopy.LeafWidth;

            var layerLAI = LAI / Layers;

            Rad = new CanopyRadiation(Layers, layerLAI)
            {
                DiffuseExtCoeff = Canopy.DiffuseExtCoeff,
                LeafScatteringCoeff = Canopy.LeafScatteringCoeff,
                DiffuseReflectionCoeff = Canopy.DiffuseReflectionCoeff
            };

            PAR = new CanopyRadiation(Layers, layerLAI)
            {
                DiffuseExtCoeff = Canopy.DiffuseExtCoeff,
                LeafScatteringCoeff = Canopy.LeafScatteringCoeff,
                DiffuseReflectionCoeff = Canopy.DiffuseReflectionCoeff
            };

            NIR = new CanopyRadiation(Layers, layerLAI)
            {
                DiffuseExtCoeff = Canopy.DiffuseExtCoeffNIR,
                LeafScatteringCoeff = Canopy.LeafScatteringCoeffNIR,
                DiffuseReflectionCoeff = Canopy.DiffuseReflectionCoeffNIR
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
            Sunlit = new PartialCanopy(Canopy, Layers, LAI / Layers);
            Shaded = new PartialCanopy(Canopy, Layers, LAI / Layers);

            // TODO: This mess can be cleaned up with better structure, just getting it working
            Sunlit.NIR.BeamExtinctionCoeff = Sunlit.PAR.BeamExtinctionCoeff = Sunlit.Rad.BeamExtinctionCoeff = Rad.BeamExtinctionCoeff;
            Shaded.NIR.BeamExtinctionCoeff = Shaded.PAR.BeamExtinctionCoeff = Shaded.Rad.BeamExtinctionCoeff = Rad.BeamExtinctionCoeff;
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
            var a = 0.5 * WindSpeedExtinction + Rad.BeamExtinctionCoeff;
            var b = 0.01 * Math.Pow(WindSpeed / LeafWidth, 0.5);
            var c = 1 - Math.Exp(-a * LAI);            

            return b * c / a;
        }

        public void CalcLAI()
        {
            Sunlit.LAI = Rad.CalculateSunlitLAI();
            Shaded.LAI = LAI - Sunlit.LAI;
        }

        public void CalcCanopyStructure(double sunAngleRadians)
        {        
            // Beam Extinction Coefficient
            if (sunAngleRadians > 0)
                Rad.BeamExtinctionCoeff = CalcShadowProjection(sunAngleRadians) / Math.Sin(sunAngleRadians);
            else
                Rad.BeamExtinctionCoeff = 0;

            NIR.BeamExtinctionCoeff = PAR.BeamExtinctionCoeff = Rad.BeamExtinctionCoeff;

            // Intercepted radiation
            PropnInterceptedRadns = Rad.CalculateAccumInterceptedRadn();
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
            // water interaction uses energy
            // photosynthesis uses photons

            // These values are energy
            PAR.Direct = radiation.DirectRadiation * 0.5 * 1000000;
            PAR.Diffuse = radiation.DiffuseRadiation * 0.5 * 1000000;
            NIR.Direct = radiation.DirectRadiation * 0.5 * 1000000;
            NIR.Diffuse = radiation.DiffuseRadiation * 0.5 * 1000000;           

            // photons
            Rad.TotalIrradiance = Rad.CalculateTotalRadiation(radiation.DirectRadiationPAR, radiation.DiffuseRadiationPAR);
            
            // energy
            PAR.TotalIrradiance = PAR.CalculateTotalRadiation(PAR.Direct, PAR.Diffuse);
            NIR.TotalIrradiance = NIR.CalculateTotalRadiation(NIR.Direct, NIR.Diffuse);

            // Sunlit
            // photons
            CalcSunlitRadiation(Sunlit.Rad, radiation.DirectRadiationPAR, radiation.DiffuseRadiationPAR);
            
            // energy
            CalcSunlitRadiation(Sunlit.PAR, PAR.Direct, PAR.Diffuse);
            CalcSunlitRadiation(Sunlit.NIR, NIR.Direct, NIR.Diffuse);

            // Shaded
            // photons
            Shaded.Rad.TotalIrradiance = Rad.TotalIrradiance - Sunlit.Rad.TotalIrradiance;
            
            // energy
            Shaded.PAR.TotalIrradiance = PAR.TotalIrradiance - Sunlit.PAR.TotalIrradiance;
            Shaded.NIR.TotalIrradiance = NIR.TotalIrradiance - Sunlit.NIR.TotalIrradiance;
        }

        public void CalcSunlitRadiation(CanopyRadiation rad, double direct, double diffuse)
        {
            rad.Direct = rad.CalculateDirectSunlit(direct);
            rad.Diffuse = rad.CalculateDiffuseSunlit(diffuse);
            rad.Scattered = rad.CalculateScatteredSunlit(direct);

            rad.TotalIrradiance = rad.Direct + rad.Diffuse + rad.Scattered;
        }

        public void CalcMaximumRates()
        {
            var nTerm = NAllocationCoeff + (Rad.BeamExtinctionCoeff * LAI);
            
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
            var exp = Rad.CalcExp(nTerm / LAI);

            return factor * exp / nTerm;
        }
        
    }
}
