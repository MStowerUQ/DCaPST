using System;
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
        public Mock<ISolarGeometry> SetupMockSolar(double time, double sunAngle)
        {
            Mock<ISolarGeometry> mock = new Mock<ISolarGeometry>(MockBehavior.Strict);
            mock.Setup(s => s.Sunrise).Returns(5.5206087540876512).Verifiable();
            mock.Setup(s => s.Sunset).Returns(18.47939124591235).Verifiable();
            mock.Setup(s => s.DayLength).Returns(12.958782491824698).Verifiable();
            mock.Setup(s => s.SolarConstant).Returns(1360).Verifiable();            
            mock.Setup(s => s.SunAngle(6.0)).Returns(new Angle(0.111379441989282, AngleType.Rad)).Verifiable();

            Angle angle = new Angle(sunAngle, AngleType.Rad);
            mock.Setup(s => s.SunAngle(time)).Returns(angle);            

            return mock;
        }

        [TestCaseSource(typeof(RadiationTestData), "ConstructorTestCases")]
        public void Constructor_IfInvalidArguments_ThrowsException(ISolarGeometry solar, double radiation)
        {
            Assert.Throws<Exception>(() => new RadiationModel(solar, radiation));
        }

        [TestCaseSource(typeof(RadiationTestData), "HourlyRadiationTestCases")]
        public void HourlyRadiation_WhenTimeOutOfBounds_ThrowsException(double time, double sunAngle)
        {
            // Arrange            
            var mock = SetupMockSolar(time, sunAngle);
            var radiation = new RadiationModel(mock.Object, 16.5) { RPAR = 0.5 };

            // Act

            // Assert
            Assert.Throws<Exception>(() => radiation.UpdateHourlyRadiation(time));
            mock.Verify();            
        }

        [TestCaseSource(typeof(RadiationTestData), "IncidentRadiationTestCases")]
        public void IncidentRadiation_GivenValidInput_MatchesExpectedValue(double time, double expected, double sunAngle)
        {
            // Arrange
            var mock = SetupMockSolar(time, sunAngle);
            var radiation = new RadiationModel(mock.Object, 16.5) { RPAR = 0.5 };

            // Act
            radiation.UpdateHourlyRadiation(time);
            var actual = radiation.TotalIncidentRadiation;

            // Assert
            Assert.AreEqual(expected, actual);
            mock.VerifyAll();
        }

        [TestCaseSource(typeof(RadiationTestData), "DiffuseRadiationTestCases")]
        public void DiffuseRadiation_GivenValidInput_MatchesExpectedValue(double time, double expected, double sunAngle)
        {
            // Arrange
            var mock = SetupMockSolar(time, sunAngle);
            var radiation = new RadiationModel(mock.Object, 16.5) { RPAR = 0.5 };

            // Act
            radiation.UpdateHourlyRadiation(time);
            var actual = radiation.DiffuseRadiation;

            // Assert
            Assert.AreEqual(expected, actual);
            mock.VerifyAll();
        }

        [TestCaseSource(typeof(RadiationTestData), "DirectRadiationTestCases")]
        public void DirectRadiation_GivenValidInput_MatchesExpectedValue(double time, double expected, double sunAngle)
        {
            // Arrange
            var mock = SetupMockSolar(time, sunAngle);
            var radiation = new RadiationModel(mock.Object, 16.5) { RPAR = 0.5 };

            // Act
            radiation.UpdateHourlyRadiation(time);
            var actual = radiation.DirectRadiation;

            // Assert
            Assert.AreEqual(expected, actual);
            mock.VerifyAll();
        }

        [TestCaseSource(typeof(RadiationTestData), "DiffuseRadiationParTestCases")]
        public void DiffuseRadiationPAR_GivenValidInput_MatchesExpectedValue(double time, double expected, double sunAngle)
        {
            // Arrange
            var mock = SetupMockSolar(time, sunAngle);
            var radiation = new RadiationModel(mock.Object, 16.5) { RPAR = 0.5 };

            // Act
            radiation.UpdateHourlyRadiation(time);
            var actual = radiation.DiffuseRadiationPAR;

            // Assert
            Assert.AreEqual(expected, actual);
            mock.VerifyAll();
        }

        [TestCaseSource(typeof(RadiationTestData), "DirectRadiationParTestCases")]
        public void DirectRadiationPAR_GivenValidInput_MatchesExpectedValue(double time, double expected, double sunAngle)
        {
            // Arrange
            var mock = SetupMockSolar(time, sunAngle);
            var radiation = new RadiationModel(mock.Object, 16.5) { RPAR = 0.5 };

            // Act
            radiation.UpdateHourlyRadiation(time);
            var actual = radiation.DirectRadiationPAR;

            // Assert
            Assert.AreEqual(expected, actual);
            mock.VerifyAll();
        }
    }
}
