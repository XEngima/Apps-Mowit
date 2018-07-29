using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MowControl;

namespace MowerTests
{
    [TestClass]
    public class TimeBasedHomeSensorTests
    {
        [TestMethod]
        public void IsHome_BeforeStart_IsHome()
        {
            // Arrange
            var timeIntervals = new List<TimeInterval>();
            timeIntervals.Add(new TimeInterval(6, 0, 12, 0));

            var config = new MowControlConfig()
            {
                TimeIntervals = timeIntervals,
                AverageWorkPerDayHours = 12,
                MaxHourlyThunderPercent = 0,
                MaxHourlyPrecipitaionMillimeter = 0
            };
            var systemTime = new TestSystemTime(new DateTime(2018, 6, 22, 5, 55, 0));
            var powerSwitch = new TestPowerSwitch(true);
            var systemStartTime = systemTime.Now.AddDays(-1);

            var homeSensor = new TimeBasedHomeSensor(systemStartTime, config, powerSwitch, systemTime);

            // Act

            // Assert
            Assert.IsTrue(homeSensor.IsHome);
        }

        [TestMethod]
        public void IsHome_TooCloseToYesterdaysInterval_IsNotHome()
        {
            // Arrange
            var timeIntervals = new List<TimeInterval>();
            timeIntervals.Add(new TimeInterval(18, 0, 23, 59));

            var config = new MowControlConfig()
            {
                TimeIntervals = timeIntervals,
                AverageWorkPerDayHours = 12,
                MaxHourlyThunderPercent = 0,
                MaxHourlyPrecipitaionMillimeter = 0
            };
            var systemTime = new TestSystemTime(new DateTime(2018, 6, 22, 00, 15, 0));
            var powerSwitch = new TestPowerSwitch(true);
            var systemStartTime = systemTime.Now.AddDays(-1);

            var homeSensor = new TimeBasedHomeSensor(systemStartTime, config, powerSwitch, systemTime);

            // Act

            // Assert
            Assert.IsFalse(homeSensor.IsHome);
        }

        [TestMethod]
        public void IsHome_EnoughTimeSinceYesterdaysInterval_IsHome()
        {
            // Arrange
            var timeIntervals = new List<TimeInterval>();
            timeIntervals.Add(new TimeInterval(18, 0, 23, 59));

            var config = new MowControlConfig()
            {
                TimeIntervals = timeIntervals,
                AverageWorkPerDayHours = 12,
                MaxHourlyThunderPercent = 0,
                MaxHourlyPrecipitaionMillimeter = 0
            };
            var systemTime = new TestSystemTime(new DateTime(2018, 6, 22, 1, 00, 0));
            var powerSwitch = new TestPowerSwitch(true);
            var systemStartTime = systemTime.Now.AddDays(-1);

            var homeSensor = new TimeBasedHomeSensor(systemStartTime, config, powerSwitch, systemTime);

            // Act

            // Assert
            Assert.IsTrue(homeSensor.IsHome);
        }

        [TestMethod]
        public void IsHome_WorkingInTimeInterval_IsNotHome()
        {
            // Arrange
            var timeIntervals = new List<TimeInterval>();
            timeIntervals.Add(new TimeInterval(6, 0, 12, 0));

            var config = new MowControlConfig()
            {
                TimeIntervals = timeIntervals,
                AverageWorkPerDayHours = 12,
                MaxHourlyThunderPercent = 0,
                MaxHourlyPrecipitaionMillimeter = 0
            };
            var systemTime = new TestSystemTime(new DateTime(2018, 6, 22, 11, 10, 0));
            var powerSwitch = new TestPowerSwitch(true);
            var systemStartTime = systemTime.Now.AddDays(-1);
            var homeSensor = new TimeBasedHomeSensor(systemStartTime, config, powerSwitch, systemTime);

            // Act

            // Assert
            Assert.IsFalse(homeSensor.IsHome);
        }

        [TestMethod]
        public void IsHome_JustStoppedMowingAndOnItsWayHome_IsNotHome()
        {
            // Arrange
            var timeIntervals = new List<TimeInterval>();
            timeIntervals.Add(new TimeInterval(6, 0, 12, 0));

            var config = new MowControlConfig()
            {
                TimeIntervals = timeIntervals,
                AverageWorkPerDayHours = 12,
                MaxHourlyThunderPercent = 0,
                MaxHourlyPrecipitaionMillimeter = 0
            };
            var systemTime = new TestSystemTime(new DateTime(2018, 6, 22, 12, 15, 0));
            var powerSwitch = new TestPowerSwitch(true);
            var systemStartTime = systemTime.Now.AddDays(-1);
            var homeSensor = new TimeBasedHomeSensor(systemStartTime, config, powerSwitch, systemTime);

            // Act
            bool isHome = homeSensor.IsHome; // First is always true

            // Assert
            Assert.IsFalse(isHome);
        }

        [TestMethod]
        public void IsHome_AfterAnIntervalWithPowerOff_IsStillHome()
        {
            // Arrange
            var timeIntervals = new List<TimeInterval>();
            timeIntervals.Add(new TimeInterval(6, 0, 12, 0));

            var config = new MowControlConfig()
            {
                TimeIntervals = timeIntervals,
                AverageWorkPerDayHours = 12,
                MaxHourlyThunderPercent = 0,
                MaxHourlyPrecipitaionMillimeter = 0
            };

            var systemTime = new TestSystemTime(new DateTime(2018, 6, 22, 11, 59, 0));
            var powerSwitch = new TestPowerSwitch(false);
            var systemStartTime = systemTime.Now.AddDays(-1);
            var homeSensor = new TimeBasedHomeSensor(systemStartTime, config, powerSwitch, systemTime);

            var isHome = homeSensor.IsHome;
            systemTime.TickMinutes(2);
            powerSwitch.TurnOn();

            // Act
            isHome = homeSensor.IsHome;

            // Assert
            Assert.IsTrue(isHome);
        }

        [TestMethod]
        public void IsHome_FirstCheck_IsHome()
        {
            // Arrange
            var timeIntervals = new List<TimeInterval>();
            timeIntervals.Add(new TimeInterval(6, 0, 12, 0));

            var config = new MowControlConfig()
            {
                TimeIntervals = timeIntervals,
                AverageWorkPerDayHours = 12,
                MaxHourlyThunderPercent = 0,
                MaxHourlyPrecipitaionMillimeter = 0
            };

            var systemTime = new TestSystemTime(new DateTime(2018, 6, 22, 6, 30, 0));
            var powerSwitch = new TestPowerSwitch(true);
            var systemStartTime = systemTime.Now;

            var homeSensor = new TimeBasedHomeSensor(systemStartTime, config, powerSwitch, systemTime);

            // Act
            bool isHome = homeSensor.IsHome;

            // Assert
            Assert.IsTrue(isHome);
        }
    }
}
