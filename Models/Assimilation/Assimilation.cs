using DCAPST.Environment;
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
        protected IPathwayParameters pway;
        protected AssimilationCalculator Calculator;

        public Pathway Path { get; }

        public LeafTemperatureFunction Current { get; }

        public double Gbs => pway.BundleSheathCO2ConductancePerLeaf * partial.LAI;
        public double Vpr => pway.PEPRegenerationPerLeaf * partial.LAI;

        public Assimilation(AssimilationType type, IPartialCanopy partial)
        {
            Type = type;

            this.partial = partial;
            canopy = partial.Canopy;
            pway = partial.Canopy.Pathway;

            Path = new Pathway(canopy.AirCO2, pway.IntercellularToAirCO2Ratio);

            Current = new LeafTemperatureFunction(partial);
        }
        
        /// <summary>
        /// Updates the state of the assimilation
        /// </summary>
        public void UpdateAssimilation(ITemperature temperature, WaterParameters water)
        {
            var leafWater = new LeafWaterInteractionModel(temperature, Path.LeafTemperature, water.BoundaryHeatConductance);

            double resistance;

            Current.Temperature = Path.LeafTemperature;
            PrepareCalculator();
            // If there is no limit on the water supply
            if (!water.limited)
            {
                Path.IntercellularCO2 = pway.IntercellularToAirCO2Ratio * canopy.AirCO2;

                Calculator.p = Path.IntercellularCO2;
                Calculator.q = 1 / Current.GmT;

                Path.CO2Rate = Calculator.CalculateAssimilation();

                resistance = leafWater.UnlimitedWaterResistance(Path.CO2Rate, canopy.AirCO2, Path.IntercellularCO2);
                Path.WaterUse = leafWater.HourlyWaterUse(resistance, partial.AbsorbedRadiation);
            }
            // If water supply is limited
            else
            {
                Path.WaterUse = water.maxHourlyT * water.fraction;
                var WaterUseMolsSecond = Path.WaterUse / 18 * 1000 / 3600;

                resistance = leafWater.LimitedWaterResistance(Path.WaterUse, partial.AbsorbedRadiation);
                var Gt = leafWater.TotalLeafCO2Conductance(resistance);

                Calculator.p = canopy.AirCO2 - WaterUseMolsSecond * canopy.AirCO2 / (Gt + WaterUseMolsSecond / 2.0);
                Calculator.q = 1 / (Gt + WaterUseMolsSecond / 2) + 1.0 / Current.GmT;

                Path.CO2Rate = Calculator.CalculateAssimilation();

                if (!(this is ParametersC3))
                    Path.IntercellularCO2 = ((Gt - WaterUseMolsSecond / 2.0) * canopy.AirCO2 - Path.CO2Rate) / (Gt + WaterUseMolsSecond / 2.0);
            }

            UpdateMesophyllCO2(Path.IntercellularCO2, Path.CO2Rate);
            UpdateChloroplasticO2(Path.CO2Rate);
            UpdateChloroplasticCO2(Path.CO2Rate);

            // New leaf temperature
            Path.LeafTemperature = (leafWater.LeafTemperature(resistance, partial.AbsorbedRadiation) + Path.LeafTemperature) / 2.0;

            // If the assimilation is not sensible
            if (double.IsNaN(Path.CO2Rate) || Path.CO2Rate <= 0.0 || double.IsNaN(Path.WaterUse) || Path.WaterUse <= 0.0)
            {
                Path.CO2Rate = 0;
                Path.WaterUse = 0;
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
