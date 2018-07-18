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

            var config = new MowPlannerConfig()
            {
                TimeIntervals = timeIntervals,
                AverageWorkPerDayHours = 12,
                MaxHourlyThunderPercent = 0,
                MaxHourlyPrecipitaionMillimeter = 0
            };
            var systemTime = new TestSystemTime(new DateTime(2018, 6, 22, 5, 55, 0));

            var homeSensor = new TimeBasedHomeSensor(config, systemTime);

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

            var config = new MowPlannerConfig()
            {
                TimeIntervals = timeIntervals,
                AverageWorkPerDayHours = 12,
                MaxHourlyThunderPercent = 0,
                MaxHourlyPrecipitaionMillimeter = 0
            };
            var systemTime = new TestSystemTime(new DateTime(2018, 6, 22, 00, 15, 0));

            var homeSensor = new TimeBasedHomeSensor(config, systemTime);

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

            var config = new MowPlannerConfig()
            {
                TimeIntervals = timeIntervals,
                AverageWorkPerDayHours = 12,
                MaxHourlyThunderPercent = 0,
                MaxHourlyPrecipitaionMillimeter = 0
            };
            var systemTime = new TestSystemTime(new DateTime(2018, 6, 22, 1, 00, 0));

            var homeSensor = new TimeBasedHomeSensor(config, systemTime);

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

            var config = new MowPlannerConfig()
            {
                TimeIntervals = timeIntervals,
                AverageWorkPerDayHours = 12,
                MaxHourlyThunderPercent = 0,
                MaxHourlyPrecipitaionMillimeter = 0
            };
            var systemTime = new TestSystemTime(new DateTime(2018, 6, 22, 11, 10, 0));

            var homeSensor = new TimeBasedHomeSensor(config, systemTime);

            // Act

            // Assert
            Assert.IsFalse(homeSensor.IsHome);
        }

        [TestMethod]
        public void IsHome_JustStoppedWorkingAndOnItsWayHome_IsNotHome()
        {
            // Arrange
            var timeIntervals = new List<TimeInterval>();
            timeIntervals.Add(new TimeInterval(6, 0, 12, 0));

            var config = new MowPlannerConfig()
            {
                TimeIntervals = timeIntervals,
                AverageWorkPerDayHours = 12,
                MaxHourlyThunderPercent = 0,
                MaxHourlyPrecipitaionMillimeter = 0
            };
            var systemTime = new TestSystemTime(new DateTime(2018, 6, 22, 12, 15, 0));

            var homeSensor = new TimeBasedHomeSensor(config, systemTime);

            // Act

            // Assert
            Assert.IsFalse(homeSensor.IsHome);
        }
    }
}
