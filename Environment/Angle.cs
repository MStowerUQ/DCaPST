using System;
using System.ComponentModel;

namespace DCAPST
{
    public enum AngleType { Deg, Rad };
    
    public class Angle
    {
        private double _rad;
        private double _deg;

        public Angle(double val, AngleType type)
        {
            if (type == AngleType.Deg)
            {
                _deg = val;
                _rad = DegToRad(val);
            }
            else
            {
                _rad = val;
                _deg = RadToDeg(val);
            }
        }

        double DegToRad(double degs)
        {
            return degs * Math.PI / 180.0;
        }

        double RadToDeg(double rads)
        {
            return rads * 180.0 / Math.PI;
        }        

        public double Rad
        {
            get { return _rad; }
            set
            {
                _rad = value;
                _deg = RadToDeg(value);
            }
        }

        public double Deg
        {
            get { return _deg; }
            set
            {
                _deg = value;
                _rad = DegToRad(value);

            }
        }
    }
}
