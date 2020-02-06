using DCAPST.Interfaces;

namespace DCAPST
{
    public class Assimilation : IAssimilation
    {
        public AssimilationType Type { get; set; }

        public IPartialCanopy Partial;
        public ICanopyParameters Canopy;
        public IPathwayParameters Path;              

        public double CO2Rate { get; set; }
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

        public void TryUpdateAssimilation(ILeafWaterInteraction Water, PhotosynthesisParams Params)
        {
            AssimilationParameters param;

            // TODO: There might be a better structure for this, since it's only used to
            // determine the assimilation parameters
            if (Canopy.Type == CanopyType.C3) 
                param = new ParametersC3(this, Partial);
            else if (Canopy.Type == CanopyType.C4) 
                param = new ParametersC4(this, Partial);
            else 
                param = new ParametersCCM(this, Partial);

            var calc = param.GetCalculator();

            double resistance;

            // If there is no limit on the water supply
            if (!Params.limited)
            {
                IntercellularCO2 = Path.IntercellularToAirCO2Ratio * Canopy.AirCO2;

                calc.p = IntercellularCO2;
                calc.q = 1 / MesophyllCO2ConductanceAtT;

                CO2Rate = calc.CalculateAssimilation();
                resistance = Water.UnlimitedWaterResistance(CO2Rate, Canopy.AirCO2, IntercellularCO2);
                WaterUse = Water.HourlyWaterUse(resistance, Partial.AbsorbedRadiation);
            }
            // If water supply is limited
            else
            {
                WaterUse = Params.maxHourlyT * Params.fraction;
                var WaterUseMolsSecond = WaterUse / 18 * 1000 / 3600;

                resistance = Water.LimitedWaterResistance(WaterUse, Partial.AbsorbedRadiation);
                var Gt = Water.TotalLeafCO2Conductance(resistance);

                calc.p = Canopy.AirCO2 - WaterUseMolsSecond * Canopy.AirCO2 / (Gt + WaterUseMolsSecond / 2.0);
                calc.q = 1 / (Gt + WaterUseMolsSecond / 2) + 1.0 / MesophyllCO2ConductanceAtT;

                CO2Rate = calc.CalculateAssimilation();

                if (!(param is ParametersC3))
                    IntercellularCO2 = ((Gt - WaterUseMolsSecond / 2.0) * Canopy.AirCO2 - CO2Rate) / (Gt + WaterUseMolsSecond / 2.0);
            }

            // C4 & CCM
            if (!(param is ParametersC3))
                MesophyllCO2 = IntercellularCO2 - CO2Rate / MesophyllCO2ConductanceAtT;

            // CCM ONLY
            if (param is ParametersCCM)
            {
                ChloroplasticO2 = Path.PS2ActivityInBundleSheathFraction * CO2Rate / (Canopy.DiffusivitySolubilityRatio * param.Gbs) + Canopy.OxygenPartialPressure;
                ChloroplasticCO2 = MesophyllCO2 + (MesophyllCO2 * calc.x4 + calc.x5 - calc.x6 * CO2Rate - calc.m - calc.x7) * calc.x8 / param.Gbs;
            }

            // New leaf temperature
            LeafTemperature = (Water.LeafTemperature(resistance, Partial.AbsorbedRadiation) + LeafTemperature) / 2.0;

            // If the assimilation is not sensible
            if (double.IsNaN(CO2Rate) || CO2Rate <= 0.0 || double.IsNaN(WaterUse) || WaterUse <= 0.0)
            {
                CO2Rate = 0;
                WaterUse = 0;
            }
        }
    }
}
