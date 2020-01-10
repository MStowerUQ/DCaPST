using System;
using System.Collections.Generic;
using NUnit.Framework;
using DCAPST.Environment;
using DCAPST.Interfaces;
using Moq;
using DCAPST;

namespace ModelsTests.Environment.UnitTests
{
    [TestFixture]
    public class RadiationTests
    {    
        [SetUp]
        public void SetUp()
        {
            //var solar = new SolarGeometryModel(144, 18.3);
        }

        public Mock<ISolarGeometry> SetupMockSolar(double time, double sunAngle)
        {
            Mock<ISolarGeometry> mock = new Mock<ISolarGeometry>(MockBehavior.Loose);
            mock.Setup(s => s.Sunrise).Returns(5.5206087540876512);
            mock.Setup(s => s.Sunset).Returns(18.47939124591235);
            mock.Setup(s => s.DayLength).Returns(12.958782491824698);
            mock.Setup(s => s.SolarConstant).Returns(1360);            
            mock.Setup(s => s.SunAngle(6.0)).Returns(new Angle(0.111379441989282, AngleType.Rad));

            Angle angle = new Angle(sunAngle, AngleType.Rad);
            mock.Setup(s => s.SunAngle(time)).Returns(angle);

            return mock;
        }

        [TestCase(null, 0)]
        //[TestCase(, -10)]
        public void Constructor_IfInvalidArguments_ThrowsException(ISolarGeometry solar, double radiation)
        {
            Assert.Throws<Exception>(() => new RadiationModel(solar, radiation));
        }

        [TestCase(-2.3, -0.66955090392859185)]
        [TestCase(24.7, -0.86629147258044892)]
        public void HourlyRadiation_WhenTimeOutOfBounds_ThrowsException(double time, double sunAngle)
        {
            // Arrange            
            var mock = SetupMockSolar(time, sunAngle);
            var radiation = new RadiationModel(mock.Object, 16.5);

            // Act

            // Assert
            Assert.Throws<Exception>(() => radiation.UpdateHourlyRadiation(time));
            mock.VerifyAll();            
        }

        [TestCaseSource(typeof(RadiationTestData), "IncidentRadiationTestData")]
        //[TestCase(0.0, 0.0, -0.88957017994999932)]
        //[TestCase(6, 6.4422088216489629E-05, 0.111379441989282)]
        //[TestCase(12, 0.00055556786827918008, 1.5283606861799228)]
        //[TestCase(18, 6.44220882164897E-05, 0.1113794419892816)]
        //[TestCase(24, 0.0, -0.88957017994999932)]
        public void IncidentRadiation_GivenValidInput_MatchesExpectedValue(double time, double expected, double sunAngle)
        {
            // Arrange
            var mock = SetupMockSolar(time, sunAngle);
            var radiation = new RadiationModel(mock.Object, 16.5);

            // Act
            radiation.UpdateHourlyRadiation(time);
            var actual = radiation.TotalIncidentRadiation;

            // Assert
            Assert.AreEqual(expected, actual);
            mock.VerifyAll();
        }

        [TestCaseSource(typeof(RadiationTestData), "DiffuseRadiationTestData")]
        public void DiffuseRadiation_GivenValidInput_MatchesExpectedValue(double time, double expected, double sunAngle)
        {
            // Arrange
            var mock = SetupMockSolar(time, sunAngle);
            var radiation = new RadiationModel(mock.Object, 16.5);

            // Act
            radiation.UpdateHourlyRadiation(time);
            var actual = radiation.DiffuseRadiation;

            // Assert
            Assert.AreEqual(expected, actual);
            mock.VerifyAll();
        }

        [TestCaseSource(typeof(RadiationTestData), "DirectRadiationTestData")]
        public void DirectRadiation_GivenValidInput_MatchesExpectedValue(double time, double expected, double sunAngle)
        {
            // Arrange
            var mock = SetupMockSolar(time, sunAngle);
            var radiation = new RadiationModel(mock.Object, 16.5);

            // Act
            radiation.UpdateHourlyRadiation(time);
            var actual = radiation.DirectRadiation;

            // Assert
            Assert.AreEqual(expected, actual);
            mock.VerifyAll();
        }

        [TestCaseSource(typeof(RadiationTestData), "DiffuseRadiationParTestData")]
        public void DiffuseRadiationPAR_GivenValidInput_MatchesExpectedValue(double time, double expected, double sunAngle)
        {
            // Arrange
            var mock = SetupMockSolar(time, sunAngle);
            var radiation = new RadiationModel(mock.Object, 16.5);

            // Act
            radiation.UpdateHourlyRadiation(time);
            var actual = radiation.DiffuseRadiationPAR;

            // Assert
            Assert.AreEqual(expected, actual);
            mock.VerifyAll();
        }

        [TestCaseSource(typeof(RadiationTestData), "DirectRadiationParTestData")]
        public void DirectRadiationPAR_GivenValidInput_MatchesExpectedValue(double time, double expected, double sunAngle)
        {
            // Arrange
            var mock = SetupMockSolar(time, sunAngle);
            var radiation = new RadiationModel(mock.Object, 16.5);

            // Act
            radiation.UpdateHourlyRadiation(time);
            var actual = radiation.DirectRadiationPAR;

            // Assert
            Assert.AreEqual(expected, actual);
            mock.VerifyAll();
        }
    }
}
