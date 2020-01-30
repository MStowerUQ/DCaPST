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

        public double CO2AssimilationRate { get; set; } = 0.0;
        public double WaterUse { get; set; } = 0.0;
        public double LeafTemperature { get; set; }

        public double IntercellularCO2 { get; set; }
        public double MesophyllCO2 { get; set; }
        public double ChloroplasticCO2 { get; set; }
        public double ChloroplasticO2 { get; set; }

        public double OxygenPartialPressure { get; set; } = 210000;

        public Assimilation(AssimilationType type, ICanopyParameters canopy, PartialCanopy partial)
        {
            Type = type;

            Canopy = canopy;
            Path = canopy.Pathway;
            Partial = partial;

            MesophyllCO2 = Canopy.AirCO2 * Path.IntercellularToAirCO2Ratio;
            ChloroplasticCO2 = MesophyllCO2 + 20;
            ChloroplasticO2 = 210000;
        }
        
        // Per Leaf
        public double MesophyllCO2ConductanceAtT => TemperatureFunction.Val(LeafTemperature, Partial.Gm25, Path.MesophyllCO2ConductanceParams);

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

            double rtw;

            if (!Params.limited)
            {
                IntercellularCO2 = Path.IntercellularToAirCO2Ratio * Canopy.AirCO2;

                aparam.p = IntercellularCO2;
                aparam.q = 1 / MesophyllCO2ConductanceAtT;

                CO2AssimilationRate = aparam.CalculateAssimilation();
                rtw = Water.CalcUnlimitedRtw(CO2AssimilationRate, Canopy.AirCO2, IntercellularCO2);
                WaterUse = Water.HourlyWaterUse(rtw, Partial.AbsorbedRadiation);
            }
            else
            {
                WaterUse = Params.maxHourlyT * Params.fraction;
                var WaterUseMolsSecond = WaterUse / 18 * 1000 / 3600;

                rtw = Water.CalcLimitedRtw(WaterUse, Partial.AbsorbedRadiation);
                var Gt = Water.CalcTotalLeafCO2Conductance(rtw);

                aparam.p = Canopy.AirCO2 - WaterUseMolsSecond * Canopy.AirCO2 / (Gt + WaterUseMolsSecond / 2.0);
                aparam.q = 1 / (Gt + WaterUseMolsSecond / 2) + 1.0 / MesophyllCO2ConductanceAtT;

                CO2AssimilationRate = aparam.CalculateAssimilation();

                if (!(calc is CalculatorC3))
                    IntercellularCO2 = ((Gt - WaterUseMolsSecond / 2.0) * Canopy.AirCO2 - CO2AssimilationRate) / (Gt + WaterUseMolsSecond / 2.0);
            }

            // C4 & CCM
            if (!(calc is CalculatorC3))
                MesophyllCO2 = IntercellularCO2 - CO2AssimilationRate / MesophyllCO2ConductanceAtT;

            // CCM ONLY
            if (calc is CalculatorCCM)
            {
                ChloroplasticO2 = Path.PS2ActivityInBundleSheathFraction * CO2AssimilationRate / (Canopy.DiffusivitySolubilityRatio * calc.Gbs) + OxygenPartialPressure;
                ChloroplasticCO2 = MesophyllCO2 + (MesophyllCO2 * aparam.x4 + aparam.x5 - aparam.x6 * CO2AssimilationRate - aparam.m - aparam.x7) * aparam.x8 / calc.Gbs;
            }

            // New leaf temperature
            LeafTemperature = (Water.CalcLeafTemperature(rtw, Partial.AbsorbedRadiation) + LeafTemperature) / 2.0;

            // If the assimilation is not sensible
            if (double.IsNaN(CO2AssimilationRate) || CO2AssimilationRate <= 0.0)
                return false;
            // If the water use is not sensible
            else if (double.IsNaN(WaterUse) || WaterUse <= 0.0)
                return false;
            else
                return true;
        }

        public void ZeroVariables()
        {
            CO2AssimilationRate = 0;
            WaterUse = 0;
        }

    }
}
