using DCAPST.Interfaces;

namespace DCAPST
{
    public class Assimilation : IAssimilation
    {
        public AssimilationType Type { get; set; }

        public IPartialCanopy Partial;
        public ICanopyParameters Canopy;
        public IPathwayParameters Path;              

        public double CO2AssimilationRate { get; set; }
        public double WaterUse { get; set; }
        public double LeafTemperature { get; set; }

        public double IntercellularCO2 { get; set; }
        public double MesophyllCO2 { get; set; }
        public double ChloroplasticCO2 { get; set; }
        public double ChloroplasticO2 { get; set; }

        public Assimilation(AssimilationType type, IPartialCanopy partial)
        {
            Type = type;

            Partial = partial;
            Canopy = partial.Canopy;
            Path = partial.Canopy.Pathway;           

            MesophyllCO2 = Canopy.AirCO2 * Path.IntercellularToAirCO2Ratio;
            ChloroplasticCO2 = MesophyllCO2 + 20;
            ChloroplasticO2 = 210000;
        }
        
        // Per Leaf
        public double MesophyllCO2ConductanceAtT => TemperatureFunction.Val(LeafTemperature, Partial.MesophyllCO2Conductance25, Path.MesophyllCO2ConductanceParams);

        public bool TryUpdateAssimilation(ILeafWaterInteraction Water, PhotosynthesisParams Params)
        {
            AssimilationCalculator calc;

            // TODO: There might be a better structure for this, since it's only used to
            // determine the assimilation parameters
            if (Canopy.Type == CanopyType.C3) 
                calc = new CalculatorC3(this, Partial);
            else if (Canopy.Type == CanopyType.C4) 
                calc = new CalculatorC4(this, Partial);
            else 
                calc = new CalculatorCCM(this, Partial);

            var aparam = calc.GetAssimilationParams();

            double resistance;

            // If there is no limit on the water supply
            if (!Params.limited)
            {
                IntercellularCO2 = Path.IntercellularToAirCO2Ratio * Canopy.AirCO2;

                aparam.p = IntercellularCO2;
                aparam.q = 1 / MesophyllCO2ConductanceAtT;

                CO2AssimilationRate = aparam.CalculateAssimilation();
                resistance = Water.UnlimitedWaterResistance(CO2AssimilationRate, Canopy.AirCO2, IntercellularCO2);
                WaterUse = Water.HourlyWaterUse(resistance, Partial.AbsorbedRadiation);
            }
            // If water supply is limited
            else
            {
                WaterUse = Params.maxHourlyT * Params.fraction;
                var WaterUseMolsSecond = WaterUse / 18 * 1000 / 3600;

                resistance = Water.LimitedWaterResistance(WaterUse, Partial.AbsorbedRadiation);
                var Gt = Water.TotalLeafCO2Conductance(resistance);

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
                ChloroplasticO2 = Path.PS2ActivityInBundleSheathFraction * CO2AssimilationRate / (Canopy.DiffusivitySolubilityRatio * calc.Gbs) + Canopy.OxygenPartialPressure;
                ChloroplasticCO2 = MesophyllCO2 + (MesophyllCO2 * aparam.x4 + aparam.x5 - aparam.x6 * CO2AssimilationRate - aparam.m - aparam.x7) * aparam.x8 / calc.Gbs;
            }

            // New leaf temperature
            LeafTemperature = (Water.LeafTemperature(resistance, Partial.AbsorbedRadiation) + LeafTemperature) / 2.0;

            // If the assimilation is not sensible
            if (double.IsNaN(CO2AssimilationRate) || CO2AssimilationRate <= 0.0)
                return false;
            // If the water use is not sensible
            else if (double.IsNaN(WaterUse) || WaterUse <= 0.0)
                return false;
            else
                return true;
        }
    }
}
