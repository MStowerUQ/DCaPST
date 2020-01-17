using DCAPST.Canopy;
using DCAPST.Interfaces;

namespace DCAPST
{
    public enum AssimilationType { Ac1, Ac2, Aj }

    public class Assimilation
    {
        public AssimilationType Type;

        public ICanopyParameters Canopy;
        public IPathwayParameters Path;
        public PartialCanopy Partial;       

        public double A { get; set; } = 0.0;
        public double WaterUse { get; set; } = 0.0;
        public double LeafTemperature { get; set; }

        public double Ci { get; set; }
        public double Cm { get; set; }
        public double Cc { get; set; }
        public double Oc { get; set; }

        public double OxygenPartialPressure { get; set; } = 210000;

        public Assimilation(AssimilationType type, ICanopyParameters canopy, PartialCanopy partial)
        {
            Type = type;

            Canopy = canopy;
            Path = canopy.Pathway;
            Partial = partial;

            Cm = Canopy.Ca * Path.CiCaRatio;
            Cc = Cm + 20;
            Oc = 210000;
        }
        
        public double GmT => TemperatureFunction.Val(LeafTemperature, Partial.Gm25, Path.Gm);

        public bool CalculateAssimilation(IWaterInteraction Water, PhotosynthesisParams Params)
        {
            AssimilationCalculator calc;

            if (Canopy.Type == CanopyType.C3) 
                calc = new CalculatorC3(Canopy, Partial, this);
            else if (Canopy.Type == CanopyType.C4) 
                calc = new CalculatorC4(Canopy, Partial, this);
            else 
                calc = new CalculatorCCM(Canopy, Partial, this);

            var aparam = calc.GetAssimilationParams(this);

            double Rn = Partial.PAR.TotalIrradiance + Partial.NIR.TotalIrradiance;
            double rtw;

            if (!Params.limited)
            {
                Ci = Path.CiCaRatio * Canopy.Ca;

                aparam.p = Ci;
                aparam.q = 1 / GmT;

                A = aparam.CalculateAssimilation();
                rtw = Water.CalcUnlimitedRtw(A, Canopy.Ca, Ci);
                WaterUse = Water.HourlyWaterUse(rtw, Rn);
            }
            else
            {
                WaterUse = Params.maxHourlyT * Params.fraction;
                var WaterUseMolsSecond = WaterUse / 18 * 1000 / 3600;

                rtw = Water.CalcLimitedRtw(WaterUse, Rn);
                var Gt = Water.CalcGt(rtw);

                aparam.p = Canopy.Ca - WaterUseMolsSecond * Canopy.Ca / (Gt + WaterUseMolsSecond / 2.0);
                aparam.q = 1 / (Gt + WaterUseMolsSecond / 2) + 1.0 / GmT;

                A = aparam.CalculateAssimilation();

                if (!(calc is CalculatorC3))
                    Ci = ((Gt - WaterUseMolsSecond / 2.0) * Canopy.Ca - A) / (Gt + WaterUseMolsSecond / 2.0);
            }

            // C4 & CCM
            if (!(calc is CalculatorC3))
                Cm = Ci - A / GmT;

            // CCM ONLY
            if (calc is CalculatorCCM)
            {
                Oc = Path.Alpha * A / (Canopy.DiffusivitySolubilityRatio * calc.Gbs) + OxygenPartialPressure;
                Cc = Cm + (Cm * aparam.x4 + aparam.x5 - aparam.x6 * A - aparam.m - aparam.x7) * aparam.x8 / calc.Gbs;
            }

            // New leaf temperature
            LeafTemperature = (Water.CalcTemperature(rtw, Rn) + LeafTemperature) / 2.0;

            // If the assimilation is not sensible
            if (double.IsNaN(A) || A <= 0.0)
                return false;
            // If the water use is not sensible
            else if (double.IsNaN(WaterUse) || WaterUse <= 0.0)
                return false;
            else
                return true;
        }

        public void ZeroVariables()
        {
            A = 0;
            WaterUse = 0;
        }

    }
}
