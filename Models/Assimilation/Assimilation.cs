using DCAPST.Interfaces;

namespace DCAPST
{
    /// <summary>
    /// Tracks the state of an assimilation type
    /// </summary>
    public abstract class Assimilation : IAssimilation
    {
        public AssimilationType Type { get; set; }

        protected IPartialCanopy partial;
        protected ICanopyParameters canopy;
        protected IPathwayParameters path;
        protected AssimilationCalculator Calculator;

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

        public double MesophyllCO2 { get; set; }
        public double ChloroplasticCO2 { get; set; }
        public double ChloroplasticO2 { get; set; }

        public LeafTemperatureFunction Current { get; }

        public double Gbs => path.BundleSheathCO2ConductancePerLeaf * partial.LAI;
        public double Vpr => path.PEPRegenerationPerLeaf * partial.LAI;

        public Assimilation(AssimilationType type, IPartialCanopy partial)
        {
            Type = type;

            this.partial = partial;
            canopy = partial.Canopy;
            path = partial.Canopy.Pathway;

            MesophyllCO2 = canopy.AirCO2 * path.IntercellularToAirCO2Ratio;
            ChloroplasticCO2 = MesophyllCO2 + 20;
            ChloroplasticO2 = 210000;

            Current = new LeafTemperatureFunction(partial);
        }
        
        /// <summary>
        /// Updates the state of the assimilation
        /// </summary>
        public void UpdateAssimilation(ILeafWaterInteraction leafWater, WaterParameters water)
        {
            double resistance;

            Current.Temperature = LeafTemperature;
            PrepareCalculator();
            // If there is no limit on the water supply
            if (!water.limited)
            {
                IntercellularCO2 = path.IntercellularToAirCO2Ratio * canopy.AirCO2;

                Calculator.p = IntercellularCO2;
                Calculator.q = 1 / Current.GmT;

                CO2Rate = Calculator.CalculateAssimilation();

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

                Calculator.p = canopy.AirCO2 - WaterUseMolsSecond * canopy.AirCO2 / (Gt + WaterUseMolsSecond / 2.0);
                Calculator.q = 1 / (Gt + WaterUseMolsSecond / 2) + 1.0 / Current.GmT;

                CO2Rate = Calculator.CalculateAssimilation();

                if (!(this is ParametersC3))
                    IntercellularCO2 = ((Gt - WaterUseMolsSecond / 2.0) * canopy.AirCO2 - CO2Rate) / (Gt + WaterUseMolsSecond / 2.0);
            }

            UpdateMesophyllCO2(IntercellularCO2, CO2Rate);
            UpdateChloroplasticO2(CO2Rate);
            UpdateChloroplasticCO2(CO2Rate);

            // New leaf temperature
            LeafTemperature = (leafWater.LeafTemperature(resistance, partial.AbsorbedRadiation) + LeafTemperature) / 2.0;

            // If the assimilation is not sensible
            if (double.IsNaN(CO2Rate) || CO2Rate <= 0.0 || double.IsNaN(WaterUse) || WaterUse <= 0.0)
            {
                CO2Rate = 0;
                WaterUse = 0;
            }
        }

        private void PrepareCalculator()
        {
            if (Type == AssimilationType.Ac1) Calculator = GetAc1Calculator();
            else if (Type == AssimilationType.Ac2) Calculator = GetAc2Calculator();
            else Calculator = GetAjCalculator();
        }

        public virtual void UpdateMesophyllCO2(double intercellularCO2, double CO2Rate) { /*C4 & CCM overwrite this.*/ }
        public virtual void UpdateChloroplasticO2(double CO2Rate) { /*CCM overwrites this.*/ }
        public virtual void UpdateChloroplasticCO2(double CO2Rate) { /*CCM overwrites this.*/ }

        protected abstract AssimilationCalculator GetAc1Calculator();
        protected abstract AssimilationCalculator GetAc2Calculator();
        protected abstract AssimilationCalculator GetAjCalculator();
    }
}
