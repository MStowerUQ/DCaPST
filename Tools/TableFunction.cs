using System;

namespace Utilities
{
    public class TableFunction
    {
        private bool _flatEnds;

        public double[] YVals { get; set; }

        public double[] XVals { get; set; }

        public TableFunction(double[] x, double[] y, bool flatEnds = true)
        {
            XVals = x;
            YVals = y;

            _flatEnds = flatEnds;
        }
        //--------------------------------------------------------------------
        // Get the y value of the function
        //--------------------------------------------------------------------

        public double Value(double v)
        {
            // Find which sector of the function that v falls in
            int sector;

            if (!_flatEnds)
            {
                if (XVals.Length == 0)
                {
                    throw (new Exception("Array has no data"));
                }

                if (v < XVals[0] || v > XVals[XVals.Length - 1])
                {
                    throw (new Exception("X value is outside the bounds of the Array"));
                }
            }

            for (sector = 0; sector < XVals.Length; sector++)
            {
                if (v <= XVals[sector])
                {
                    break;
                }
            }

            if (sector == 0)
            {
                return YVals[0];
            }

            if (sector == XVals.Length)
            {
                return YVals[YVals.Length - 1];
            }

            if (Math.Abs(v - XVals[sector]) < Double.Epsilon)
            {
                return YVals[sector];
            }

            double slope;
            try
            {

                slope = (Math.Abs(XVals[sector] - XVals[sector - 1]) < Double.Epsilon) ? 0 :
                                 (YVals[sector] - YVals[sector - 1]) / (XVals[sector] - XVals[sector - 1]);
            }
            catch(DivideByZeroException)
            {
                slope = 0;
            }

            return YVals[sector - 1] + slope * (v - XVals[sector - 1]);
        }
    }
}
