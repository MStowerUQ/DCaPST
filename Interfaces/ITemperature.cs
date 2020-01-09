using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAPST.Interfaces
{
    public interface ITemperature
    {
        double AtmosphericPressure { get; }
        double Rair { get; }

        double AirTemperature { get; }
        double AbsoluteTemperature { get; }
        double MinTemperature { get; }

        void UpdateAirTemperature(double time);
    }
}
