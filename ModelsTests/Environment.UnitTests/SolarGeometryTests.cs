using System;
using NUnit.Framework;
using DCAPST.Environment;

namespace ModelsTests.Environment.UnitTests
{
    [TestFixture]
    public class SolarGeometryTests
    {
        private SolarGeometryModel solar;

        [SetUp]
        public void SetUp()
        {
            solar = new SolarGeometryModel(144, 18.3);
        }

        [TestCase(-16, 45.0)]
        [TestCase(0, 45.0)]
        [TestCase(392, 45.0)]
        [TestCase(50, 95.0)]
        [TestCase(-50, -95.0)]
        public void BadInputTests(double day, double lat)
        {
            Assert.Throws<Exception>(() => new SolarGeometryModel(day, lat));
        }

        [Test]
        public void SolarDeclinationTest()
        {
            var expected = 20.731383108171872;
            var actual = solar.SolarDeclination.Deg;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SunsetAngleTest()
        {
            var expected = 97.190868688685228;
            var actual = solar.SunsetAngle.Deg;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void DayLengthTest()
        {
            var expected = 12.958782491824698;
            var actual = solar.DayLength;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SunriseTest()
        {
            var expected = 5.5206087540876512;
            var actual = solar.Sunrise;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SunsetTest()
        {
            var expected = 18.47939124591235;
            var actual = solar.Sunset;
            Assert.AreEqual(expected, actual);
        }

        [TestCase(1.0, -48.291971830796477)]
        [TestCase(6.5, 13.12346022737003)]
        [TestCase(12.7, 79.811435134587384)]
        [TestCase(22.8, -47.167281573443965)]
        public void SunAngleTest(double hour, double expected)
        {
            var actual = solar.SunAngle(hour).Deg;
            Assert.AreEqual(expected, actual);
        }
    }
}
