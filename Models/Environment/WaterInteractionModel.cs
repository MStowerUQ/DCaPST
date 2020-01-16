using System;
using DCAPST.Interfaces;

namespace DCAPST.Environment
{
    public class WaterInteractionModel : IWaterInteraction
    {
        private ITemperature Temp;

        private double LeafTemp;   
        private double Gbh;

        // Constants
        private double sigma = 0.0000000567;
        private double rcp = 1200;
        private double psychrometricConstant = 0.066;
        private double lambda = 2447000;

        public WaterInteractionModel(ITemperature temperature, double leafTemp, double gbh)
        {
            Temp = temperature ?? throw new Exception("The temperature model cannot be null");
            LeafTemp = leafTemp;
            Gbh = (gbh != 0) ? gbh : throw new Exception("Gbh cannot be 0");
        }        

        public double Gbw => Gbh / 0.92;
        public double Rbh => 1 / Gbh;
        public double GbCO2 => Temp.AtmosphericPressure * Temp.Rair * Gbw / 1.37;        

        //This should be: HEnergyBalance - * (LeafTemp__[i]-PM.EnvModel.GetTemp(PM.Time));
        private double BnUp => 8 * sigma * Math.Pow(Temp.AirTemperature + Temp.AbsoluteTemperature, 3) * (LeafTemp - Temp.AirTemperature);
        private double VPTLeaf => 0.61365 * Math.Exp(17.502 * LeafTemp / (240.97 + LeafTemp));
        private double VPTAir => 0.61365 * Math.Exp(17.502 * Temp.AirTemperature / (240.97 + Temp.AirTemperature));
        private double VPTAir_1 => 0.61365 * Math.Exp(17.502 * (Temp.AirTemperature + 1) / (240.97 + (Temp.AirTemperature + 1)));
        private double VPTMin => 0.61365 * Math.Exp(17.502 * Temp.MinTemperature / (240.97 + Temp.MinTemperature));

        private double s => VPTAir_1 - VPTAir;
        private double VPD_la => VPTLeaf - VPTMin;  

        public double CalcUnlimitedRtw(double Assimilation, double Ca, double Ci)
        {
            double Wl = VPTLeaf / (Temp.AtmosphericPressure * 100) * 1000;
            double Wa = VPTMin / (Temp.AtmosphericPressure * 100) * 1000;

            double a = 1 / GbCO2;
            double d = (Wl - Wa) / (1000 - (Wl + Wa) / 2) * (Ca + Ci) / 2;
            double e = Assimilation;
            double f = Ca - Ci;

            double m = 1.37; //Constant
            double n = 1.6;  //Constant

            double a_lump = e * a * m + e * a * n + d * m * n - f * m;
            double b_lump = e * m * (e * Math.Pow(a, 2) * n + a * d * m * n - a * f * n);
            double d_lump = m * Assimilation;
            
            // Unlimited water gsCO2
            double gsCO2 = 2 * d_lump / (Math.Pow((Math.Pow(a_lump, 2) - 4 * b_lump), 0.5) - a_lump);
            double Gtw = 1 / (1 / (1.37 * GbCO2) + 1 / (1.6 * gsCO2));
            
            double rtw = Temp.Rair / Gtw * Temp.AtmosphericPressure;
            return rtw;
        }

        public double CalcLimitedRtw(double availableWater, double Rn)
        {
            double ekg = lambda * availableWater / 3600;
            double rtw = (s * Rbh * (Rn - BnUp - ekg) + VPD_la * rcp) / (ekg * psychrometricConstant);
            return rtw;
        }

        public double HourlyWaterUse(double rtw, double Rn)
        {
            double a_lump = s * (Rn - BnUp) + VPD_la * rcp / Rbh;
            double b_lump = s + psychrometricConstant * rtw / Rbh;
            double lambdaE = a_lump / b_lump;

            return (lambdaE / lambda) * 3600;
        }

        public double CalcGt(double rtw)
        {
            // Limited water gsCO2
            var gsCO2 = Temp.Rair * (Temp.AtmosphericPressure / (rtw - (1 / Gbw))) / 1.6;
            return 1 / (1 / GbCO2 + 1 / gsCO2);
        }

        public double CalcTemperature(double rtw, double Rn)
        {
            double a = psychrometricConstant * (Rn - BnUp) * rtw / rcp - VPD_la;
            double d = s + psychrometricConstant * rtw / Rbh;

            double tDelta = a / d;

            return Temp.AirTemperature + tDelta;
        }
    }
}
