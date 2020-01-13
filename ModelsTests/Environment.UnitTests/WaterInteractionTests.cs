using System;
using NUnit.Framework;
using DCAPST.Environment;
using DCAPST.Interfaces;
using Moq;
using DCAPST;

namespace ModelsTests.Environment.UnitTests
{
    public class WaterInteractionTests
    {
        [TestCaseSource(typeof(WaterInteractionTestData), "ConstructorTestCases")]
        public void Constructor_IfInvalidArguments_ThrowsException(ITemperature temperature, ICanopyParameters canopy, double leafTemp, double gbh)
        {
            Assert.Throws<Exception>(() => new WaterInteractionModel(temperature, canopy, leafTemp, gbh));
        }

        [Test]
        public void UnlimitedRtw_WhenCalculated_ReturnsExpectedValue()
        {
            // Arrange
            var temperature = new Mock<ITemperature>(MockBehavior.Strict);
            temperature.Setup(t => t.AtmosphericPressure).Returns(1.01325).Verifiable();
            temperature.Setup(t => t.MinTemperature).Returns(16.2).Verifiable();
            temperature.Setup(t => t.Rair).Returns(40.63).Verifiable();

            var canopy = new Mock<ICanopyParameters>();

            var leafTemp = 27.0;
            var gbh = 0.127634;

            var A = 4.5;
            var Ca = 380.0;
            var Ci = 152.0;

            var expected = 1262.0178666386046;

            // Act
            var water = new WaterInteractionModel(temperature.Object, canopy.Object, leafTemp, gbh);
            var actual = water.CalcUnlimitedRtw(A, Ca, Ci);

            // Assert
            Assert.AreEqual(expected, actual);
            temperature.Verify();
        }

        [Test]
        public void LimitedRtw_WhenCalculated_ReturnsExpectedValue()
        {
            // Arrange
            var temperature = new Mock<ITemperature>(MockBehavior.Strict);
            temperature.Setup(t => t.AirTemperature).Returns(27.0).Verifiable();
            temperature.Setup(t => t.AbsoluteTemperature).Returns(273).Verifiable();
            temperature.Setup(t => t.MinTemperature).Returns(16.2).Verifiable();

            var canopy = new Mock<ICanopyParameters>(MockBehavior.Strict);
            canopy.Setup(c => c.Lambda).Returns(2447000).Verifiable();
            canopy.Setup(c => c.Sigma).Returns(0.0000000567).Verifiable();
            canopy.Setup(c => c.Rcp).Returns(1200).Verifiable();
            canopy.Setup(c => c.PsychrometricConstant).Returns(0.066).Verifiable();

            var leafTemp = 27;
            var gbh = 0.127634;

            var available = 0.15;
            var rn = 230;

            var expected = 340.83946167121144;

            // Act
            var water = new WaterInteractionModel(temperature.Object, canopy.Object, leafTemp, gbh);
            var actual = water.CalcLimitedRtw(available, rn);

            // Assert
            Assert.AreEqual(expected, actual);
            temperature.Verify();
            canopy.Verify();
        }

        [Test]
        public void HourlyWaterUse_WhenCalculated_ReturnsExpectedValue()
        {
            // Arrange
            var temperature = new Mock<ITemperature>(MockBehavior.Strict);
            temperature.Setup(t => t.AirTemperature).Returns(27.0).Verifiable();
            temperature.Setup(t => t.AbsoluteTemperature).Returns(273).Verifiable();
            temperature.Setup(t => t.MinTemperature).Returns(16.2).Verifiable();

            var canopy = new Mock<ICanopyParameters>(MockBehavior.Strict);
            canopy.Setup(c => c.Lambda).Returns(2447000).Verifiable();
            canopy.Setup(c => c.Sigma).Returns(0.0000000567).Verifiable();
            canopy.Setup(c => c.Rcp).Returns(1200).Verifiable();
            canopy.Setup(c => c.PsychrometricConstant).Returns(0.066).Verifiable();

            var leafTemp = 27;
            var gbh = 0.127634;

            var rtw = 700;
            var rn = 320;

            var expected = 0.080424818708166368;

            // Act
            var water = new WaterInteractionModel(temperature.Object, canopy.Object, leafTemp, gbh);
            var actual = water.HourlyWaterUse(rtw, rn);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Gt_WhenCalculated_ReturnsExpectedValue()
        {
            // Arrange
            var temperature = new Mock<ITemperature>(MockBehavior.Strict);
            temperature.Setup(t => t.Rair).Returns(40.63).Verifiable();
            temperature.Setup(t => t.AtmosphericPressure).Returns(1.01325).Verifiable();

            var canopy = new Mock<ICanopyParameters>(MockBehavior.Strict);

            var leafTemp = 27;
            var gbh = 0.127634;

            var rtw = 180;

            var expected = 0.1437732786549164;

            // Act
            var water = new WaterInteractionModel(temperature.Object, canopy.Object, leafTemp, gbh);
            var actual = water.CalcGt(rtw);

            // Assert
            Assert.AreEqual(expected, actual);
            temperature.Verify();
        }

        [Test]
        public void Temperature_WhenCalculated_ReturnsExpectedValue()
        {
            // Arrange
            var temperature = new Mock<ITemperature>(MockBehavior.Strict);
            temperature.Setup(t => t.AirTemperature).Returns(27.0).Verifiable();
            temperature.Setup(t => t.AbsoluteTemperature).Returns(273).Verifiable();
            temperature.Setup(t => t.MinTemperature).Returns(16.2).Verifiable();

            var canopy = new Mock<ICanopyParameters>(MockBehavior.Strict);
            canopy.Setup(c => c.Sigma).Returns(0.0000000567).Verifiable();
            canopy.Setup(c => c.Rcp).Returns(1200).Verifiable();
            canopy.Setup(c => c.PsychrometricConstant).Returns(0.066).Verifiable();

            var leafTemp = 27;
            var gbh = 0.127634;

            var rtw = 700;
            var rn = 320;

            var expected = 28.732384941224293;

            // Act
            var water = new WaterInteractionModel(temperature.Object, canopy.Object, leafTemp, gbh);
            var actual = water.CalcTemperature(rtw, rn);

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}
