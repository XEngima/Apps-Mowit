using Microsoft.VisualStudio.TestTools.UnitTesting;
using MowControl;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MowerTests
{
    [TestClass]
    public class TodoTests
    {
        private static void RunOverTime(MowController mowController, TestSystemTime systemTime, int hours, int minutes)
        {
            minutes = hours * 60 + minutes;

            for (int i = 0; i < minutes * 2; i++)
            {
                mowController.CheckAndAct();
                systemTime.TickSeconds(30);
            }
        }

        [TestMethod]
        public void ManualMowingBeforeIntervalStart_MowerLost_NotMowingEnded()
        {
            // Arrange
            var systemTime = new TestSystemTime(2019, 8, 11, 21, 35, 0);
            var config = TestFactory.NewConfig10To12And20To2359(
                usingContactHomeSensor: true);

            var logger = TestFactory.NewMowLogger(new DateTime(2019, 8, 11, 0, 0, 0));
            logger.LogItems.Add(new LogItem(new DateTime(2019, 8, 11, 19, 35, 0), LogType.MowerLeft, LogLevel.Debug, ""));

            var powerSwitch = new TestPowerSwitch(PowerStatus.On);
            var weatherForecast = TestFactory.NewWeatherForecastGood(systemTime);
            var homeSensor = new TestHomeSensor(systemTime, isHome: false);
            var rainSensor = new TestRainSensor(isWet: true);
            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger, rainSensor);

            // Act
            mowController.CheckAndAct();

            // Assert
            Assert.AreEqual(powerSwitch.Status, PowerStatus.On);

            var logItem = logger.LogItems.FirstOrDefault(x => x.Type == LogType.MowerLost);
            Assert.IsNotNull(logItem);

            logItem = logger.LogItems.FirstOrDefault(x => x.Type == LogType.MowingEnded);
            Assert.IsNull(logItem);
        }

        [TestMethod]
        public void ManualMowingBeforeIntervalStart_MowerReturnsDuringIntervalInWeb_NotMowingStarted()
        {
            // Arrange
            var systemTime = new TestSystemTime(2019, 8, 11, 22, 1, 0);
            var config = TestFactory.NewConfig10To12And20To2359(
                usingContactHomeSensor: true);

            var logger = TestFactory.NewMowLogger(new DateTime(2019, 8, 11, 0, 0, 0));
            logger.LogItems.Add(new LogItem(new DateTime(2019, 8, 11, 0, 0, 0), LogType.PowerOn, LogLevel.Debug, ""));
            logger.LogItems.Add(new LogItem(new DateTime(2019, 8, 11, 19, 35, 0), LogType.MowerLeft, LogLevel.Debug, ""));
            logger.LogItems.Add(new LogItem(new DateTime(2019, 8, 11, 21, 35, 0), LogType.MowerLost, LogLevel.Debug, ""));

            var powerSwitch = new TestPowerSwitch(PowerStatus.On);
            var weatherForecast = TestFactory.NewWeatherForecastGood(systemTime);
            var homeSensor = new TestHomeSensor(systemTime,
                isHome: true,
                mowerLeftTime: new DateTime(2019, 8, 11, 19, 35, 0),
                mowerCameTime: new DateTime(2019, 8, 11, 22, 1, 0));
            var rainSensor = new TestRainSensor(isWet: true);
            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger, rainSensor, mowerIsHome: false);

            // Act
            mowController.CheckAndAct();

            // Assert
            Assert.AreEqual(powerSwitch.Status, PowerStatus.On);

            var logItem = logger.LogItems.FirstOrDefault(x => x.Type == LogType.MowerCame);
            Assert.IsNotNull(logItem);

            logItem = logger.LogItems.FirstOrDefault(x => x.Type == LogType.MowingStarted);
            Assert.IsNull(logItem);
        }
    }
}
