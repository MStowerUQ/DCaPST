using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayerCanopyPhotosynthesis.Environment
{
    public class WaterInteractionModel
    {
        // TODO: Handle these variables better, they're just being dumped in to get it working

        private double AirTemp;
        private double LeafTemp;
        private double AbsTemp;
        private double MinTemp;

        private double Atm;
        private double Rair;

        private double Gbh;

        private double Rcp = 1200;
        private double PsychrometricConstant = 0.066;
        private double Sigma = 5.67E-08;
        private double Lambda = 2447000;

        public WaterInteractionModel(TemperatureModel Temp, PathwayParameters CPath, double leafTemp, double gbh)
        {
            AirTemp = Temp.AirTemperature;            
            AbsTemp = Temp.AbsoluteTemperature;
            MinTemp = Temp.MinTemperature;
            Atm = Temp.AtmosphericPressure;
            Rair = Temp.Rair;

            Sigma = CPath.Canopy.Sigma;
            Rcp = CPath.Canopy.Rcp;
            PsychrometricConstant = CPath.Canopy.G;
            Lambda = CPath.Canopy.Lambda;

            LeafTemp = leafTemp;

            Gbh = gbh;
        }        

        public double Gbw => Gbh / 0.92;
        public double Rbh => 1 / Gbh;
        public double GbCO2 => Atm * Rair * Gbw / 1.37;        

        //This should be: HEnergyBalance - * (LeafTemp__[i]-PM.EnvModel.GetTemp(PM.Time));
        private double BnUp => 8 * Sigma * Math.Pow(AirTemp + AbsTemp, 3) * (LeafTemp - AirTemp);
        private double VPTLeaf => 0.61365 * Math.Exp(17.502 * LeafTemp / (240.97 + LeafTemp));
        private double VPTAir => 0.61365 * Math.Exp(17.502 * AirTemp / (240.97 + AirTemp));
        private double VPTAir_1 => 0.61365 * Math.Exp(17.502 * (AirTemp + 1) / (240.97 + (AirTemp + 1)));
        private double VPTMin => 0.61365 * Math.Exp(17.502 * MinTemp / (240.97 + MinTemp));

        private double s => VPTAir_1 - VPTAir;
        private double VPD_la => VPTLeaf - VPTMin;  

        public double CalcUnlimitedRtw(double Assimilation, double Ca, double Ci)
        {
            double Wl = VPTLeaf / (Atm * 100) * 1000;
            double Wa = VPTMin / (Atm * 100) * 1000;

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
            
            double rtw = Rair / Gtw * Atm;
            return rtw;
        }

        public double CalcLimitedRtw(double availableWater, double Rn)
        {
            double ekg = Lambda * availableWater / 3600;
            double rtw = (s * Rbh * (Rn - BnUp - ekg) + VPD_la * Rcp) / (ekg * PsychrometricConstant);
            return rtw;
        }

        public double HourlyWaterUse(double rtw, double Rn)
        {
            double a_lump = s * (Rn - BnUp) + VPD_la * Rcp / Rbh;
            double b_lump = s + PsychrometricConstant * rtw / Rbh;
            double lambdaE = a_lump / b_lump;

            return (lambdaE / Lambda) * 3600;
        }

        public double CalcGt(double rtw)
        {
            // Limited water gsCO2
            var gsCO2 = Rair * (Atm / (rtw - (1 / Gbw))) / 1.6;
            return 1 / (1 / GbCO2 + 1 / gsCO2);
        }

        public double CalcTemperature(double rtw, double Rn)
        {
            double a = PsychrometricConstant * (Rn - BnUp) * rtw / Rcp - VPD_la;
            double d = s + PsychrometricConstant * rtw / Rbh;

            double tDelta = a / d;

            return AirTemp + tDelta;
        }
    }
}
