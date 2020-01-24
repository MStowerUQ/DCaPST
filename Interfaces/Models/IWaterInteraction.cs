﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAPST.Interfaces
{
    public interface IWaterInteraction
    {
        double CalcUnlimitedRtw(double Assimilation, double Ca, double Ci);

        double CalcLimitedRtw(double wateruse, double Rn);

        double CalcTotalLeafCO2Conductance(double rtw);

        double CalcLeafTemperature(double rtw, double Rn);

        double HourlyWaterUse(double rtw, double Rn);
    }
}
