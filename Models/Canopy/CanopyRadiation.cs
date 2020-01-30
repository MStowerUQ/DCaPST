using System;

namespace DCAPST.Canopy
{
    public class CanopyRadiation
    {
        public double DiffuseExtCoeff { get; set; }
        public double BeamExtinctionCoeff { get; set; }
        public double BeamReflectionCoeff => 1 - Math.Exp(-2 * ReflectionCoefficientHorizontal * BeamExtinctionCoeff / (1 + BeamExtinctionCoeff));
        public double DiffuseReflectionCoeff { get; set; }
        public double LeafScatteringCoeff { get; set; }

        public double BeamScatteredBeam => BeamExtinctionCoeff * Math.Pow(1 - LeafScatteringCoeff, 0.5);
        public double DiffuseScatteredDiffuse => DiffuseExtCoeff * Math.Pow(1 - LeafScatteringCoeff, 0.5);
        public double ReflectionCoefficientHorizontal => (1 - Math.Pow(1 - LeafScatteringCoeff, 0.5)) / (1 + Math.Pow(1 - LeafScatteringCoeff, 0.5));

        // Note: In the case of n layers, the layerLAI is totalLAI / n. 
        // The LAIAccum of the nth layer is n * layerLAI
        // LAIAccum 0 is (n - 1) * layerLAI
        public double LAIAccum { get; set; }
        public double LAIAccum0 { get; set; }

        public CanopyRadiation(int layers, double layerLAI)
        {
            LAIAccum = layers * layerLAI;
            LAIAccum0 = (layers - 1) * layerLAI;
        }

        public double CalculateTotalRadiation(double direct, double diffuse)
        {
            var a = (1 - BeamReflectionCoeff) * direct * CalcExp(BeamScatteredBeam);
            var b = (1 - DiffuseReflectionCoeff) * diffuse * CalcExp(DiffuseScatteredDiffuse);

            return a + b;
        }

        public double CalcSunlitRadiation(double direct, double diffuse)
        {
            var dir = CalculateDirectSunlit(direct);
            var dif = CalculateDiffuseSunlit(diffuse);
            var sct = CalculateScatteredSunlit(direct);

            return dir + dif + sct;
        }

        public double CalculateDirectSunlit(double direct)
        {
            return (1 - LeafScatteringCoeff) * direct * CalcExp(BeamExtinctionCoeff);
        }

        public double CalculateDiffuseSunlit(double diffuse)
        {
            var DSD_BEC = DiffuseScatteredDiffuse + BeamExtinctionCoeff;            
            var radiation = (1 - DiffuseReflectionCoeff) * diffuse * CalcExp(DSD_BEC) * (DiffuseScatteredDiffuse / DSD_BEC);

            return radiation;
        }

        public double CalculateScatteredSunlit(double direct)
        {
            var BSB_BEC = BeamScatteredBeam + BeamExtinctionCoeff;
            if (BSB_BEC == 0) return 0;
            
            var dir = (1 - BeamReflectionCoeff) * CalcExp(BSB_BEC) * (BeamScatteredBeam / BSB_BEC); // Integral of direct     
            var diff = (1 - LeafScatteringCoeff) * CalcExp(2 * BeamExtinctionCoeff) / 2.0;    // Integral of diffuse

            var radiation = direct * (dir - diff);

            return radiation;
        }

        public double CalculateSunlitLAI()
        {
            return CalcExp(BeamExtinctionCoeff) / BeamExtinctionCoeff;
        }

        public double CalculateAccumInterceptedRadn()
        {
            return 1 - Math.Exp(-BeamExtinctionCoeff * LAIAccum);
        }

        public double CalcExp(double x)
        {
            var a = Math.Exp(-x * LAIAccum0);
            var b = Math.Exp(-x * LAIAccum);

            return a - b;
        }
    }
}
