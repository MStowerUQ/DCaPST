﻿using System;
using Moq;
using NUnit.Framework;
using DCAPST.Environment;
using DCAPST.Interfaces;

namespace ModelsTests.Environment.UnitTests
{
    [TestFixture]
    public class TemperatureTests
    {
        [TestCaseSource(typeof(TemperatureTestData), "ConstructorTestCases")]
        public void Constructor_IfInvalidArguments_ThrowsException(ISolarGeometry solar, double max, double min)
        {
            Assert.Throws<Exception>(() => new TemperatureModel(solar, max, min));
        }

        [TestCaseSource(typeof(TemperatureTestData), "InvalidTimeTestCases")]
        public void UpdateAirTemperature_IfInvalidTime_ThrowsException(double time)
        {
            // Arrange
            var mock = new Mock<ISolarGeometry>(MockBehavior.Strict);
            mock.Setup(s => s.Sunset).Returns(18.47939124591235).Verifiable();
            mock.Setup(s => s.DayLength).Returns(12.958782491824698).Verifiable();

            // Act
            var temp = new TemperatureModel(mock.Object, 28, 16);

            // Assert
            Assert.Throws<Exception>(() => temp.UpdateAirTemperature(time));
            mock.Verify();
        }

        [TestCaseSource(typeof(TemperatureTestData), "ValidTimeTestCases")]
        public void UpdateAirTemperature_IfValidTime_SetsCorrectTemperature(double time, double expected)
        {
            // Arrange
            var mock = new Mock<ISolarGeometry>(MockBehavior.Strict);
            mock.Setup(s => s.Sunset).Returns(18.47939124591235).Verifiable();
            mock.Setup(s => s.DayLength).Returns(12.958782491824698).Verifiable();

            // Act
            var temp = new TemperatureModel(mock.Object, 28, 16);
            temp.UpdateAirTemperature(time);
            var actual = temp.AirTemperature;

            // Assert
            Assert.AreEqual(expected, actual);
            mock.Verify();
        }        
    }
}
