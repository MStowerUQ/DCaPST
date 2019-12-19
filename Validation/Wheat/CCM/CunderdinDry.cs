using System;
using NUnit.Framework;

namespace Validation.Wheat
{
    [TestFixture]
    public class BW5_GxE
    {
        private double epsilon = 0.0000000000001;
        
        public void Validate
        (
            int DOY, 
            double latitude, 
            double maxT, 
            double minT, 
            double radn, 
            double RootShootRatio, 
            double SLN, 
            double SWAvailable, 
            double lai,
            double expectedBIOshootDAY,
            double expectedEcanDemand,
            double expectedEcanSupply,
            double expectedRadIntDcaps,
            double expectedBIOshootDAYPot
        )
        {
            var PM = Initialise.NewCCM();

            var dcaps = PM.DailyRun(DOY, latitude, maxT, minT, radn, lai, SLN, SWAvailable, RootShootRatio);

            double BIOshootDAY = dcaps[0];            
            double EcanDemand = dcaps[1];
            double EcanSupply = dcaps[2];
            double RadIntDcaps = dcaps[3];
            double BIOshootDAYPot = dcaps[4];

            Assert.IsTrue(Math.Abs(expectedBIOshootDAY - BIOshootDAY) < epsilon);
            Assert.IsTrue(Math.Abs(expectedEcanDemand - EcanDemand) < epsilon);
            Assert.IsTrue(Math.Abs(expectedEcanSupply - EcanSupply) < epsilon);
            Assert.IsTrue(Math.Abs(expectedRadIntDcaps - RadIntDcaps) < epsilon);
            Assert.IsTrue(Math.Abs(expectedBIOshootDAYPot - BIOshootDAYPot) < epsilon);
        }
    }
}
