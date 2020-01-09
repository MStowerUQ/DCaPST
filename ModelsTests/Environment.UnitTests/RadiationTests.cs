using System;
using NUnit.Framework;
using DCAPST.Environment;

namespace ModelsTests.Environment.UnitTests
{
    [TestFixture]
    public class RadiationTests
    {
        private static RadiationModel Radiation;

        [SetUp]
        public void SetUp()
        {
            var solar = new SolarGeometryModel(144, 18.3);
            Radiation = new RadiationModel(solar, 16.5);
        }

        [TestCase(null, 0)]
        //[TestCase(new SolarGeometryModel(144, 18.3), -10)]
        public void InvalidConstructorTests(SolarGeometryModel solar, double radiation)
        {
            Assert.Throws<Exception>(() => new RadiationModel(solar, radiation));
        }

        [TestCase(-2.3)]
        [TestCase(24.7)]
        public void InvalidTimeTests(double time)
        {
            Assert.Throws<Exception>(() => Radiation.UpdateHourlyRadiation(time));
        }

        [TestCase(0.0, 0.0)]
        [TestCase(6.0, 6.4422088216489629E-05)]
        [TestCase(12.0, 0.00055556786827918008)]
        [TestCase(18.0, 6.44220882164897E-05)]
        [TestCase(24.0, 0.0)]
        public void IncidentRadiationTest(double time, double expected)
        {
            Radiation.UpdateHourlyRadiation(time);
            var actual = Radiation.TotalIncidentRadiation;

            Assert.AreEqual(expected, actual);
        }

        [TestCase(0.0, 0.0)]
        [TestCase(6.0, 2.60756259519616E-05)]
        [TestCase(12.0, 0.00023438879978105451)]
        [TestCase(18.0, 2.6075625951961505E-05)]
        [TestCase(24.0, 0.0)]
        public void DiffuseRadiationTest(double time, double expected)
        {
            Radiation.UpdateHourlyRadiation(time);
            var actual = Radiation.DiffuseRadiation;

            Assert.AreEqual(expected, actual);
        }

        [TestCase(0.0, 0.0)]
        [TestCase(6.0, 3.8346462264528029E-05)]
        [TestCase(12.0, 0.00032117906849812557)]
        [TestCase(18.0, 3.8346462264528192E-05)]
        [TestCase(24.0, 0.0)]
        public void DirectRadiationTest(double time, double expected)
        {
            Radiation.UpdateHourlyRadiation(time);
            var actual = Radiation.DirectRadiation;

            Assert.AreEqual(expected, actual);
        }

        [TestCase(0.0, 0.0)]
        [TestCase(6.0, 55.4107051479184)]
        [TestCase(12.0, 498.07619953474079)]
        [TestCase(18.0, 55.4107051479182)]
        [TestCase(24.0, 0.0)]
        public void DiffuseRadiationParTest(double time, double expected)
        {
            Radiation.UpdateHourlyRadiation(time);
            var actual = Radiation.DiffuseRadiationPAR;

            Assert.AreEqual(expected, actual);
        }

        [TestCase(0.0, 0.0)]
        [TestCase(6.0, 87.4299339631239)]
        [TestCase(12.0, 732.28827617572631)]
        [TestCase(18.0, 87.42993396312427)]
        [TestCase(24.0, 0.0)]
        public void DirectRadiationParTest(double time, double expected)
        {
            Radiation.UpdateHourlyRadiation(time);
            var actual = Radiation.DirectRadiationPAR;

            Assert.AreEqual(expected, actual);
        }
    }
}
