using DCAPST.Interfaces;

namespace DCAPST
{
    /// <summary>
    /// Tracks the state of an assimilation type
    /// </summary>
    public class Assimilation : IAssimilation
    {
        public AssimilationType Type { get; set; }

        private IPartialCanopy partial;
        private ICanopyParameters canopy;
        private IPathwayParameters path;
        private AssimilationParameters aParams;

        /// <summary>
        /// The rate at which CO2 is assimilated
        /// </summary>
        public double CO2Rate { get; set; }

        /// <summary>
        /// The water required to maintain the CO2 rate
        /// </summary>
        public double WaterUse { get; set; }

        /// <summary>
        /// The temperature of the leaf in which assimilation is occuring
        /// </summary>
        public double LeafTemperature { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double IntercellularCO2 { get; set; }

        public Assimilation(AssimilationType type, IPartialCanopy partial)
        {
            Type = type;

            this.partial = partial;
            canopy = partial.Canopy;
            path = partial.Canopy.Pathway;

            aParams = AssimilationParameters.Create(this, partial);
        }
        
        /// <summary>
        /// Updates the state of the assimilation
        /// </summary>
        public void UpdateAssimilation(ILeafWaterInteraction leafWater, WaterParameters water)
        {
            double resistance;

            aParams.Current.Temperature = LeafTemperature;
            aParams.PrepareCalculator();
            // If there is no limit on the water supply
            if (!water.limited)
            {
                IntercellularCO2 = path.IntercellularToAirCO2Ratio * canopy.AirCO2;

                aParams.Calculator.p = IntercellularCO2;
                aParams.Calculator.q = 1 / aParams.Current.GmT;

                CO2Rate = aParams.Calculator.CalculateAssimilation();

                resistance = leafWater.UnlimitedWaterResistance(CO2Rate, canopy.AirCO2, IntercellularCO2);
                WaterUse = leafWater.HourlyWaterUse(resistance, partial.AbsorbedRadiation);
            }
            // If water supply is limited
            else
            {
                WaterUse = water.maxHourlyT * water.fraction;
                var WaterUseMolsSecond = WaterUse / 18 * 1000 / 3600;

                resistance = leafWater.LimitedWaterResistance(WaterUse, partial.AbsorbedRadiation);
                var Gt = leafWater.TotalLeafCO2Conductance(resistance);

                aParams.Calculator.p = canopy.AirCO2 - WaterUseMolsSecond * canopy.AirCO2 / (Gt + WaterUseMolsSecond / 2.0);
                aParams.Calculator.q = 1 / (Gt + WaterUseMolsSecond / 2) + 1.0 / aParams.Current.GmT;

                CO2Rate = aParams.Calculator.CalculateAssimilation();

                if (!(aParams is ParametersC3))
                    IntercellularCO2 = ((Gt - WaterUseMolsSecond / 2.0) * canopy.AirCO2 - CO2Rate) / (Gt + WaterUseMolsSecond / 2.0);
            }

            aParams.UpdateMesophyllCO2(IntercellularCO2, CO2Rate);
            aParams.UpdateChloroplasticO2(CO2Rate);
            aParams.UpdateChloroplasticCO2(CO2Rate);

            // New leaf temperature
            LeafTemperature = (leafWater.LeafTemperature(resistance, partial.AbsorbedRadiation) + LeafTemperature) / 2.0;

            // If the assimilation is not sensible
            if (double.IsNaN(CO2Rate) || CO2Rate <= 0.0 || double.IsNaN(WaterUse) || WaterUse <= 0.0)
            {
                CO2Rate = 0;
                WaterUse = 0;
            }
        }
    }
}
