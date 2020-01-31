using System;
using DCAPST.Interfaces;

namespace DCAPST.Environment
{
    public class LeafWaterInteractionModel : ILeafWaterInteraction
    {
        private ITemperature Temp;

        // Constants
        private double boltzmannConstant = 0.0000000567;
        private double airVolumetricHeatCapacity = 1200;
        private double psychrometricConstant = 0.066;
        private double latentHeatOfVapourisation = 2447000;

        private double LeafTemp;   
        private double boundaryHeatConductance;

        public LeafWaterInteractionModel(ITemperature temperature, double leafTemp, double gbh)
        {
            Temp = temperature ?? throw new Exception("The temperature model cannot be null");
            LeafTemp = leafTemp;
            boundaryHeatConductance = (gbh != 0) ? gbh : throw new Exception("Gbh cannot be 0");
        }        

        public double BoundaryH20Conductance => boundaryHeatConductance / 0.92;
        public double boundaryHeatResistance => 1 / boundaryHeatConductance;
        public double BoundaryCO2Conductance => Temp.AtmosphericPressure * Temp.AirMolarDensity * BoundaryH20Conductance / 1.37;        
                
        private double OutgoingThermalRadiation => 8 * boltzmannConstant * Math.Pow(Temp.AirTemperature + 273, 3) * (LeafTemp - Temp.AirTemperature);
        // VPT: Vapour Pressure at T
        private double VPTLeaf => 0.61365 * Math.Exp(17.502 * LeafTemp / (240.97 + LeafTemp));
        private double VPTAir => 0.61365 * Math.Exp(17.502 * Temp.AirTemperature / (240.97 + Temp.AirTemperature));
        private double VPTAir_1 => 0.61365 * Math.Exp(17.502 * (Temp.AirTemperature + 1) / (240.97 + (Temp.AirTemperature + 1)));
        private double VPTMin => 0.61365 * Math.Exp(17.502 * Temp.MinTemperature / (240.97 + Temp.MinTemperature));

        private double deltaAirVP => VPTAir_1 - VPTAir;
        // Leaf to air vapour pressure deficit
        private double VPD_la => VPTLeaf - VPTMin;  

        // Rtw : Total leaf resistance to water
        public double UnlimitedWaterResistance(double Assimilation, double Ca, double Ci)
        {
            // Leaf water mol fraction
            double Wl = VPTLeaf / (Temp.AtmosphericPressure * 100) * 1000;
            // Air water mol fraction
            double Wa = VPTMin / (Temp.AtmosphericPressure * 100) * 1000;

            // Boundary CO2 Resistance
            double a = 1 / BoundaryCO2Conductance;

            // dummy variable
            double d = (Wl - Wa) / (1000 - (Wl + Wa) / 2) * (Ca + Ci) / 2;
            double e = Assimilation;
            double f = Ca - Ci;

            // Boundary water diffusion factor
            double m = 1.37;
            // Stomata water diffusion factor
            double n = 1.6;

            // dummy variables
            double a_lump = e * a * m + e * a * n + d * m * n - f * m;
            double b_lump = e * m * (e * Math.Pow(a, 2) * n + a * d * m * n - a * f * n);
            double d_lump = m * Assimilation;
            
            // Stomatal CO2 conductance
            double gsCO2 = 2 * d_lump / (Math.Pow((Math.Pow(a_lump, 2) - 4 * b_lump), 0.5) - a_lump);
            // Total leaf water conductance
            double Gtw = 1 / (1 / (m * BoundaryCO2Conductance) + 1 / (n * gsCO2));
            
            double rtw = Temp.AirMolarDensity / Gtw * Temp.AtmosphericPressure;
            return rtw;
        }

        public double LimitedWaterResistance(double availableWater, double Rn)
        {
            // Transpiration in kilos of water per second
            double ekg = latentHeatOfVapourisation * availableWater / 3600;
            double rtw = (deltaAirVP * boundaryHeatResistance * (Rn - OutgoingThermalRadiation - ekg) + VPD_la * airVolumetricHeatCapacity) / (ekg * psychrometricConstant);
            return rtw;
        }

        public double HourlyWaterUse(double rtw, double Rn)
        {
            // dummy variables
            double a_lump = deltaAirVP * (Rn - OutgoingThermalRadiation) + VPD_la * airVolumetricHeatCapacity / boundaryHeatResistance;
            double b_lump = deltaAirVP + psychrometricConstant * rtw / boundaryHeatResistance;
            double latentHeatLoss = a_lump / b_lump;

            return (latentHeatLoss / latentHeatOfVapourisation) * 3600;
        }

        public double TotalLeafCO2Conductance(double rtw)
        {
            // Limited water gsCO2
            var gsCO2 = Temp.AirMolarDensity * (Temp.AtmosphericPressure / (rtw - (1 / BoundaryH20Conductance))) / 1.6;
            var boundaryCO2Resistance = 1 / BoundaryCO2Conductance;
            var stomatalCO2Resistance = 1 / gsCO2;
            return 1 / (boundaryCO2Resistance + stomatalCO2Resistance);
        }

        public double LeafTemperature(double rtw, double Rn)
        {
            // dummy variables
            double a = psychrometricConstant * (Rn - OutgoingThermalRadiation) * rtw / airVolumetricHeatCapacity - VPD_la;
            double d = deltaAirVP + psychrometricConstant * rtw / boundaryHeatResistance;

            double deltaT = a / d;

            return Temp.AirTemperature + deltaT;
        }
    }
}
