using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAPST.Interfaces
{
    public interface ISolarGeometry
    {
        double Sunrise { get; }
        double Sunset { get; }
        double DayLength { get; }
        double SolarConstant { get; }

        Angle SunAngle(double time);
    }
}
