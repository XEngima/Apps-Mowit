using Microsoft.VisualStudio.TestTools.UnitTesting;
using MowControl;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MowerTests
{
    [TestClass]
    public class MowControllerTests
    {
        [TestMethod]
        public void CheckAndAct_ConfigOk_NoException()
        {
            // Arrange
            var timeIntervals = new List<TimeInterval>()
            {
                new TimeInterval(0, 0, 10, 0)
            };

            var config = new MowControlConfig()
            {
                TimeIntervals = timeIntervals,
                AverageWorkPerDayHours = 12,
                MaxHourlyThunderPercent = 0,
                MaxHourlyPrecipitaionMillimeter = 0
            };
            var powerSwitch = new TestPowerSwitch();
            var systemTime = new TestSystemTime(DateTime.Now);
            var homeSensor = new TestHomeSensor(true);
            var logger = new MowLogger();
            var weatherForecast = new TestWeatherForecast(true, systemTime);
            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);
            Exception receivedException = null;

            // Act
            try
            {
                mowController.CheckAndAct();
            }
            catch (Exception ex)
            {
                receivedException = ex;
            }

            // Assert
            Assert.IsNull(receivedException);
        }

        [TestMethod]
        public void CheckAndAct_PeriodNull_InvalidOperationException()
        {
            // Arrange
            var config = new MowControlConfig()
            {
                TimeIntervals = null
            };
            var powerSwitch = new TestPowerSwitch();
            var homeSensor = new TestHomeSensor(true);
            var systemTime = new SystemTime();
            var mowController = new MowController(config, powerSwitch, null, systemTime, homeSensor, null);
            Exception receivedException = null;

            // Act
            try
            {
                mowController.CheckAndAct();
            }
            catch (Exception ex)
            {
                receivedException = ex;
            }

            // Assert
            Assert.IsNotNull(receivedException);
            Assert.IsInstanceOfType(receivedException, typeof(InvalidOperationException));
        }

        [TestMethod]
        public void CheckAndAct_GoodWeatherAndCloseToStartMowing_CurrentIsTurnedOn()
        {
            // Arrange
            var config = TestFactory.NewConfig6To12();
            var powerSwitch = new TestPowerSwitch();
            var systemTime = new TestSystemTime(2018, 7, 24, 6, 0);
            var weatherForecast = TestFactory.NewWeatherForecastGood(systemTime);
            var homeSensor = new TestHomeSensor(true);
            var logger = TestFactory.NewMowLogger(systemTime.Now);
            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            mowController.CheckAndAct();

            // Assert
            Assert.AreEqual(powerSwitch.Status, PowerStatus.On);
            Assert.IsTrue(powerSwitch.HasBeenTurnedOnOnce);

            var logItems = logger.LogItems.Where(x => x.Type == LogType.PowerOn).ToList();

            Assert.AreEqual(1, logItems.Count);
            Assert.AreEqual(LogType.PowerOn, logItems[0].Type);
            string expectedLogDate = "2018-07-24 06:00";
            Assert.AreEqual(expectedLogDate, logItems[0].Time.ToString("yyyy-MM-dd HH:mm"));
            Assert.IsTrue(logItems[0].Message.StartsWith("Power was turned on."));
        }

        [TestMethod]
        public void CheckAndAct_AfterWorkingIntervalAndHome_CurrentStillTurnedOn()
        {
            // Arrange
            var config = TestFactory.NewConfig6To12();
            var powerSwitch = new TestPowerSwitch(isActive: true);
            var systemTime = new TestSystemTime(13, 0);
            var weatherForecast = new TestWeatherForecast(true, systemTime);
            var systemStartTime = systemTime.Now.AddDays(-1);
            var homeSensor = new TimeBasedHomeSensor(systemStartTime, config, powerSwitch, systemTime);
            var logger = new MowLogger();
            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            mowController.CheckAndAct();

            // Assert
            Assert.AreEqual(powerSwitch.Status, PowerStatus.On);
        }

        private static void RunOverTime(MowController mowController, TestSystemTime systemTime, int hours, int minutes)
        {
            minutes = hours * 60 + minutes;

            for (int i = 0; i < minutes; i++)
            {
                mowController.CheckAndAct();
                systemTime.TickMinutes(1);
            }
        }

        [TestMethod]
        public void CheckAndAct_AfterWorkingIntervalAndComingHomeRightBeforeNextBadWeatherInterval_CurrentTurnedOffOnce()
        {
            // Arrange
            var config = TestFactory.NewConfig6To12And13To19();
            var powerSwitch = new TestPowerSwitch(isActive: true);
            var systemTime = new TestSystemTime(2018, 7, 24, 12, 50);
            var weatherForecast = TestFactory.NewWeatherForecastBad(systemTime);
            var systemStartTime = systemTime.Now.AddDays(-1);
            var homeSensor = new TimeBasedHomeSensor(systemStartTime, config, powerSwitch, systemTime);
            var logger = TestFactory.NewMowLogger(systemTime.Now);
            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            RunOverTime(mowController, systemTime, 0, 10);

            // Assert
            Assert.AreEqual(powerSwitch.Status, PowerStatus.Off);
            Assert.IsTrue(powerSwitch.HasBeenTurnedOffOnce);

            var logItems = logger.LogItems.Where(x => x.Type == LogType.PowerOff).ToList();

            Assert.AreEqual(1, logItems.Count);
            Assert.AreEqual(LogType.PowerOff, logItems[0].Type);
            string expectedLogDate = "2018-07-24 12:55";
            Assert.AreEqual(expectedLogDate, logItems[0].Time.ToString("yyyy-MM-dd HH:mm"));
            Assert.IsTrue(logItems[0].Message.StartsWith("Power was turned off."));
        }

        [TestMethod]
        public void CheckAndAct_AfterWorkingIntervalAndComingHomeRightBeforeNextBadWeatherInterval2_CurrentTurnedOffOnce()
        {
            // Arrange
            var config = TestFactory.NewConfig6To12And13To19();
            var powerSwitch = new TestPowerSwitch(isActive: true);
            var systemTime = new TestSystemTime(2018, 7, 24, 12, 58);
            var weatherForecast = TestFactory.NewWeatherForecastBad(systemTime);
            var systemStartTime = systemTime.Now.AddDays(-1);
            var homeSensor = new TimeBasedHomeSensor(systemStartTime, config, powerSwitch, systemTime);
            var logger = TestFactory.NewMowLogger(systemTime.Now);
            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            RunOverTime(mowController, systemTime, 0, 10);

            // Assert
            Assert.AreEqual(powerSwitch.Status, PowerStatus.Off);
            Assert.IsTrue(powerSwitch.HasBeenTurnedOffOnce);

            var logItems = logger.LogItems.Where(x => x.Type == LogType.PowerOff).ToList();

            Assert.AreEqual(1, logItems.Count);
            Assert.AreEqual(LogType.PowerOff, logItems[0].Type);
            string expectedLogDate = "2018-07-24 12:58";
            Assert.AreEqual(expectedLogDate, logItems[0].Time.ToString("yyyy-MM-dd HH:mm"));
            Assert.IsTrue(logItems[0].Message.StartsWith("Power was turned off."));
        }

        [TestMethod]
        public void CheckAndAct_Running24HoursInGoodWeather_CurrentNeverChanged()
        {
            // Arrange
            var config = TestFactory.NewConfig6To12();
            var powerSwitch = new TestPowerSwitch(isActive: false);
            var systemTime = new TestSystemTime(2018, 7, 24, 3, 0);
            var weatherForecast = TestFactory.NewWeatherForecastGood(systemTime);
            var systemStartTime = systemTime.Now.AddDays(-1);
            var homeSensor = new TimeBasedHomeSensor(systemStartTime, config, powerSwitch, systemTime);
            var logger = TestFactory.NewMowLogger(systemTime.Now);
            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            RunOverTime(mowController, systemTime, 24, 0);

            // Assert
            Assert.AreEqual(powerSwitch.Status, PowerStatus.On);
            Assert.AreEqual(1, powerSwitch.TurnOns);
            Assert.AreEqual(0, powerSwitch.TurnOffs);
        }

        [TestMethod]
        public void CheckAndAct_Running24HoursInGoodWeatherGettingBad_CurrentTurnedOffOnce()
        {
            // Arrange
            var config = TestFactory.NewConfig0To6And12To18();
            var powerSwitch = new TestPowerSwitch(isActive: false);
            var systemTime = new TestSystemTime(2018, 7, 24, 11, 0);
            var weatherForecast = TestFactory.NewWeatherForecastGood(systemTime);
            var systemStartTime = systemTime.Now.AddDays(-1);
            var homeSensor = new TimeBasedHomeSensor(systemStartTime, config, powerSwitch, systemTime);
            var logger = TestFactory.NewMowLogger(systemTime.Now);
            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            RunOverTime(mowController, systemTime, 12, 0);
            weatherForecast.SetExpectation(false);
            RunOverTime(mowController, systemTime, 12, 0);

            // Assert
            Assert.AreEqual(powerSwitch.Status, PowerStatus.On);
            Assert.AreEqual(2, powerSwitch.TurnOns);
            Assert.AreEqual(1, powerSwitch.TurnOffs);

            var logItems = logger.LogItems.Where(x => x.Type == LogType.PowerOn || x.Type == LogType.PowerOff).ToList();
            Assert.AreEqual(3, logItems.Count);

            Assert.AreEqual(LogType.PowerOff, logItems[1].Type);
            string expectedLogDate = "2018-07-24 23:55";
            Assert.AreEqual(expectedLogDate, logItems[1].Time.ToString("yyyy-MM-dd HH:mm"));
            Assert.IsTrue(logItems[1].Message.StartsWith("Power was turned off."));
        }

        [TestMethod]
        public void CheckAndAct_Running24HoursInGoodWeatherGettingBadStartAtDay_CurrentTurnedOffOnce()
        {
            // Arrange
            var config = TestFactory.NewConfig6To12And18To2359();
            var powerSwitch = new TestPowerSwitch(isActive: false);
            var systemTime = new TestSystemTime(2018, 7, 24, 16, 0);
            var weatherForecast = TestFactory.NewWeatherForecastGood(systemTime);
            var systemStartTime = systemTime.Now.AddDays(-1);
            var homeSensor = new TimeBasedHomeSensor(systemStartTime, config, powerSwitch, systemTime);
            var logger = TestFactory.NewMowLogger(systemTime.Now);
            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            RunOverTime(mowController, systemTime, 12, 0);
            weatherForecast.SetExpectation(false);
            RunOverTime(mowController, systemTime, 12, 0);

            // Assert
            Assert.AreEqual(powerSwitch.Status, PowerStatus.On);
            Assert.AreEqual(2, powerSwitch.TurnOns);
            Assert.AreEqual(1, powerSwitch.TurnOffs);

            var logItems = logger.LogItems.Where(x => x.Type == LogType.PowerOn || x.Type == LogType.PowerOff).ToList();
            Assert.AreEqual(3, logItems.Count);

            Assert.AreEqual(LogType.PowerOff, logItems[1].Type);
            string expectedLogDate = "2018-07-25 05:55";
            Assert.AreEqual(expectedLogDate, logItems[1].Time.ToString("yyyy-MM-dd HH:mm"));
            Assert.IsTrue(logItems[1].Message.StartsWith("Power was turned off."));
        }

        [TestMethod]
        public void CheckAndAct_Running24HoursInBadWeatherGettingGood_CurrentTurnedOffOnce()
        {
            // Arrange
            var config = TestFactory.NewConfig6To12And18To2359();
            var powerSwitch = new TestPowerSwitch(isActive: true);
            var systemTime = new TestSystemTime(2018, 7, 24, 3, 0);
            var weatherForecast = TestFactory.NewWeatherForecastBad(systemTime);
            var systemStartTime = systemTime.Now.AddDays(-1);
            var homeSensor = new TimeBasedHomeSensor(systemStartTime, config, powerSwitch, systemTime);
            var logger = TestFactory.NewMowLogger(systemTime.Now);
            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            RunOverTime(mowController, systemTime, 12, 0);
            weatherForecast.SetExpectation(true);
            RunOverTime(mowController, systemTime, 12, 0);

            // Assert
            Assert.AreEqual(powerSwitch.Status, PowerStatus.On);
            Assert.AreEqual(1, powerSwitch.TurnOns);
            Assert.AreEqual(1, powerSwitch.TurnOffs);

            var logItems = logger.LogItems.Where(x => x.Type == LogType.PowerOn || x.Type == LogType.PowerOff).ToList();

            Assert.AreEqual(2, logItems.Count);
            Assert.AreEqual(LogType.PowerOff, logItems[0].Type);
            string expectedLogDate = "2018-07-24 05:55";
            Assert.AreEqual(expectedLogDate, logItems[0].Time.ToString("yyyy-MM-dd HH:mm"));
            Assert.IsTrue(logItems[0].Message.StartsWith("Power was turned off."));

            Assert.AreEqual(LogType.PowerOn, logItems[1].Type);
            expectedLogDate = "2018-07-24 12:05";
            Assert.AreEqual(expectedLogDate, logItems[1].Time.ToString("yyyy-MM-dd HH:mm"));
            Assert.IsTrue(logItems[1].Message.StartsWith("Power was turned on."));
        }

        [TestMethod]
        public void CheckAndAct_Running24HoursInBadWeatherGettingGoodStartAtDay_CurrentTurnedOffOnce()
        {
            // Arrange
            var config = TestFactory.NewConfig6To12And18To2359();
            var powerSwitch = new TestPowerSwitch(isActive: true);
            var systemTime = new TestSystemTime(2018, 7, 24, 12, 0);
            var weatherForecast = TestFactory.NewWeatherForecastBad(systemTime);
            var systemStartTime = systemTime.Now.AddDays(-1);
            var homeSensor = new TimeBasedHomeSensor(systemStartTime, config, powerSwitch, systemTime);
            var logger = TestFactory.NewMowLogger(systemTime.Now);
            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            RunOverTime(mowController, systemTime, 12, 0);
            weatherForecast.SetExpectation(true);
            RunOverTime(mowController, systemTime, 12, 0);

            // Assert
            Assert.AreEqual(powerSwitch.Status, PowerStatus.On);
            Assert.AreEqual(1, powerSwitch.TurnOns);
            Assert.AreEqual(1, powerSwitch.TurnOffs);

            var logItems = logger.LogItems.Where(x => x.Type == LogType.PowerOn || x.Type == LogType.PowerOff).ToList();
            Assert.AreEqual(2, logItems.Count);

            Assert.AreEqual(LogType.PowerOff, logItems[0].Type);
            string expectedLogDate = "2018-07-24 17:55";
            Assert.AreEqual(expectedLogDate, logItems[0].Time.ToString("yyyy-MM-dd HH:mm"));
            Assert.IsTrue(logItems[0].Message.StartsWith("Power was turned off."));

            Assert.AreEqual(LogType.PowerOn, logItems[1].Type);
            expectedLogDate = "2018-07-25 06:00";
            Assert.AreEqual(expectedLogDate, logItems[1].Time.ToString("yyyy-MM-dd HH:mm"));
            Assert.IsTrue(logItems[1].Message.StartsWith("Power was turned on."));
        }

        [TestMethod]
        public void CheckAndAct_HasWorkedEnoughGoodWeatherAhead_PowerOffNotNeeded()
        {
            // Arrange
            var config = TestFactory.NewConfig6To12And18To2359();
            var powerSwitch = new TestPowerSwitch(isActive: true);
            var systemTime = new TestSystemTime(2018, 6, 28, 17, 55);
            var weatherForecast = TestFactory.NewWeatherForecastGood(systemTime);
            var systemStartTime = systemTime.Now.AddDays(-1);
            var homeSensor = new TimeBasedHomeSensor(systemStartTime, config, powerSwitch, systemTime);

            var logger = new MowLogger();
            logger.Write(new DateTime(2018, 6, 26, 0, 0, 0), LogType.MowControllerStarted, "Mow controller started.");
            logger.Write(new DateTime(2018, 6, 26, 6, 0, 0), LogType.PowerOn, "Power was turned on.");

            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            mowController.CheckAndAct();

            // Assert
            Assert.AreEqual(powerSwitch.Status, PowerStatus.Off);
            Assert.AreEqual(0, powerSwitch.TurnOns);
            Assert.AreEqual(1, powerSwitch.TurnOffs);

            var logItems = logger.LogItems.Where(x => x.Type == LogType.PowerOff).ToList();

            Assert.AreEqual(1, logItems.Count);
            Assert.AreEqual(LogType.PowerOff, logItems[0].Type);
            string expectedLogDate = (new DateTime(2018, 6, 28, 17, 55, 0)).ToString("yyyy-MM-dd ") + "17:55";
            Assert.AreEqual(expectedLogDate, logItems[0].Time.ToString("yyyy-MM-dd HH:mm"));
            Assert.AreEqual("Power was turned off. Mowing not necessary.", logItems[0].Message);
        }

        [TestMethod]
        public void CheckAndAct_HasWorkedEnoughBadWeatherAhead_PowerOffNotNeeded()
        {
            // Arrange
            var config = TestFactory.NewConfig6To12And18To2359();
            var powerSwitch = new TestPowerSwitch(isActive: true);
            var systemTime = new TestSystemTime(2018, 6, 27, 5, 55);

            var weatherForecast = TestFactory.NewWeatherForecastGood(systemTime);
            weatherForecast.AddExpectation(false, new DateTime(2018, 6, 28, 12, 0, 0));
            var systemStartTime = systemTime.Now.AddDays(-1);

            var homeSensor = new TimeBasedHomeSensor(systemStartTime, config, powerSwitch, systemTime);

            var logger = new MowLogger();
            logger.Write(new DateTime(2018, 06, 25, 0, 0, 0), LogType.MowControllerStarted, "");
            logger.Write(new DateTime(2018, 06, 25, 0, 0, 0), LogType.PowerOn, "");

            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            mowController.CheckAndAct();

            // Assert
            Assert.AreEqual(powerSwitch.Status, PowerStatus.On);
            Assert.AreEqual(0, powerSwitch.TurnOns);
            Assert.AreEqual(0, powerSwitch.TurnOffs);
        }

        [TestMethod]
        public void CheckAndAct_HasWorkedEnoughGoodWeatherAhead2_PowerOffMowingNotNeeded()
        {
            // Arrange
            var config = TestFactory.NewConfig3To10And16To2300();
            var powerSwitch = new TestPowerSwitch(isActive: false);
            var systemTime = new TestSystemTime(2018, 06, 24, 3, 0);
            var weatherForecast = TestFactory.NewWeatherForecastGood(systemTime);
            var systemStartTime = systemTime.Now.AddDays(-1);
            var homeSensor = new TimeBasedHomeSensor(systemStartTime, config, powerSwitch, systemTime);

            var logger = TestFactory.NewMowLogger(new DateTime(2018, 6, 24, 3, 0, 0));

            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            RunOverTime(mowController, systemTime, 72, 0); // 3 dagar

            // Assert
            Assert.AreEqual(powerSwitch.Status, PowerStatus.On);
            Assert.AreEqual(2, powerSwitch.TurnOns);
            Assert.AreEqual(1, powerSwitch.TurnOffs);

            var logItems = logger.LogItems.Where(x => x.Type == LogType.PowerOn || x.Type == LogType.PowerOff).ToList();

            Assert.AreEqual(3, logItems.Count);
            Assert.AreEqual(LogType.PowerOff, logItems[1].Type);
            Assert.AreEqual("2018-06-26 02:55", logItems[1].Time.ToString("yyyy-MM-dd HH:mm"));
            Assert.AreEqual("Power was turned off. Mowing not necessary.", logItems[1].Message);

            Assert.AreEqual(LogType.PowerOn, logItems[2].Type);
            Assert.AreEqual("2018-06-26 10:05", logItems[2].Time.ToString("yyyy-MM-dd HH:mm"));
            Assert.IsTrue(logItems[2].Message.StartsWith("Power was turned on."));
        }

        [TestMethod]
        public void CheckAndAct_HasWorkedEnoughGoodWeatherAheadLongTime_PowerOffMowingNotNeeded()
        {
            // Arrange
            var config = TestFactory.NewConfig3To10And16To2300();
            var powerSwitch = new TestPowerSwitch(isActive: false);
            var systemTime = new TestSystemTime(2018, 06, 24, 3, 0);
            var weatherForecast = TestFactory.NewWeatherForecastGood(systemTime);
            var systemStartTime = systemTime.Now.AddDays(-1);
            var homeSensor = new TimeBasedHomeSensor(systemStartTime, config, powerSwitch, systemTime);

            var logger = TestFactory.NewMowLogger(new DateTime(2018, 6, 24, 3, 0, 0));
            //logger.Write(new DateTime(2018, ), LogType.MowControllerStarted, "");

            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            RunOverTime(mowController, systemTime, 240, 0); // 10 dagar

            // Assert
            var logItems = logger.LogItems.Where(x => x.Type == LogType.PowerOn || x.Type == LogType.PowerOff).ToList();

            Assert.AreEqual(powerSwitch.Status, PowerStatus.On);
            Assert.AreEqual(5, powerSwitch.TurnOns);
            Assert.AreEqual(4, powerSwitch.TurnOffs);
            Assert.AreEqual(9, logItems.Count);
        }

        [TestMethod]
        public void CheckAndAct_FailedToGetWeather_DontChangePower()
        {
            // Arrange
            var config = TestFactory.NewConfig3To10And16To2300();
            var powerSwitch = new TestPowerSwitch(isActive: false);
            var systemTime = new TestSystemTime(2018, 06, 24, 3, 0);

            var weatherForecast = TestFactory.NewWeatherForecastGood(systemTime);
            weatherForecast.SetFailureAndThrowException(true);
            var systemStartTime = systemTime.Now.AddDays(-1);

            var homeSensor = new TimeBasedHomeSensor(systemStartTime, config, powerSwitch, systemTime);

            var logger = TestFactory.NewMowLogger(new DateTime(2018, 6, 24, 0, 0, 0));

            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            mowController.CheckAndAct();

            // Assert
            Assert.AreEqual(powerSwitch.Status, PowerStatus.Off);
            Assert.AreEqual(0, powerSwitch.TurnOns);
            Assert.AreEqual(0, powerSwitch.TurnOffs);

            Assert.AreEqual(2, logger.LogItems.Count);
            Assert.AreEqual(LogType.Failure, logger.LogItems[1].Type);
            Assert.AreEqual("2018-06-24 03:00", logger.LogItems[1].Time.ToString("yyyy-MM-dd HH:mm"));
            Assert.AreEqual("Failed to contact weather service.", logger.LogItems[1].Message);
        }

        [TestMethod]
        public void CheckAndAct_FailedToGetWeather_NotRepeated()
        {
            // Arrange
            var config = TestFactory.NewConfig3To10And16To2300();
            var powerSwitch = new TestPowerSwitch(isActive: false);
            var systemTime = new TestSystemTime(2018, 06, 24, 3, 0);

            var weatherForecast = TestFactory.NewWeatherForecastGood(systemTime);
            weatherForecast.SetFailureAndThrowException(true);
            var systemStartTime = systemTime.Now.AddDays(-1);

            var homeSensor = new TimeBasedHomeSensor(systemStartTime, config, powerSwitch, systemTime);

            var logger = TestFactory.NewMowLogger(new DateTime(2018, 6, 24, 0, 0, 0));

            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            mowController.CheckAndAct();
            systemTime.TickMinutes(1);
            mowController.CheckAndAct();

            // Assert
            Assert.AreEqual(powerSwitch.Status, PowerStatus.Off);
            Assert.AreEqual(0, powerSwitch.TurnOns);
            Assert.AreEqual(0, powerSwitch.TurnOffs);

            Assert.AreEqual(2, logger.LogItems.Count);
            Assert.AreEqual(LogType.Failure, logger.LogItems[1].Type);
            Assert.AreEqual("2018-06-24 03:00", logger.LogItems[1].Time.ToString("yyyy-MM-dd HH:mm"));
            Assert.AreEqual("Failed to contact weather service.", logger.LogItems[1].Message);
        }

        [TestMethod]
        public void CheckAndAct_ComingHome_LogMessageSaved()
        {
            // Arrange
            var config = TestFactory.NewConfig3To10And16To2300();
            var powerSwitch = new TestPowerSwitch(isActive: true);
            var systemTime = new TestSystemTime(2018, 06, 24, 4, 30);

            var weatherForecast = TestFactory.NewWeatherForecastGood(systemTime);

            var homeSensor = new TestHomeSensor(false);

            var logger = TestFactory.NewMowLogger(new DateTime(2018, 6, 24, 0, 0, 0));

            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            mowController.CheckAndAct();
            systemTime.TickMinutes(1);
            homeSensor.SetIsHome(true);
            mowController.CheckAndAct();

            // Assert
            Assert.AreEqual(powerSwitch.Status, PowerStatus.On);
            Assert.AreEqual(0, powerSwitch.TurnOns);
            Assert.AreEqual(0, powerSwitch.TurnOffs);

            Assert.AreEqual(2, logger.LogItems.Count);
            Assert.AreEqual(LogType.MowerCame, logger.LogItems[1].Type);
            Assert.AreEqual("2018-06-24 04:31", logger.LogItems[1].Time.ToString("yyyy-MM-dd HH:mm"));
        }

        [TestMethod]
        public void CheckAndAct_NotComingHome_LogMessageTellingMowerIsLost()
        {
            // Arrange
            var config = TestFactory.NewConfig3To10And16To2300(usingContactHomeSensor: true);
            var powerSwitch = new TestPowerSwitch(isActive: true);
            var systemTime = new TestSystemTime(2018, 06, 24, 3, 30);
            var weatherForecast = TestFactory.NewWeatherForecastGood(systemTime);
            var homeSensor = new TestHomeSensor(true);
            var logger = TestFactory.NewMowLogger(new DateTime(2018, 6, 24, 0, 0, 0));

            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            homeSensor.SetIsHome(false); // Mower leaves
            mowController.CheckAndAct(); // Sets mower to away
            systemTime.TickMinutes(123); // Mote time 123 minutes ahead

            // Act
            mowController.CheckAndAct(); // Should see that mower has been lost

            // Assert
            var logItems = logger.LogItems.Where(x => x.Type == LogType.MowerLost).ToList();

            Assert.AreEqual(1, logItems.Count);
            Assert.AreEqual(LogType.MowerLost, logItems[0].Type);
            Assert.AreEqual("2018-06-24 05:33", logItems[0].Time.ToString("yyyy-MM-dd HH:mm"));
        }

        [TestMethod]
        public void CheckAndAct_NotLeavingHome_LogMessageTellingMowerNeverLeft()
        {
            // Arrange
            var config = TestFactory.NewConfig3To10And16To2300(usingContactHomeSensor: true);
            var powerSwitch = new TestPowerSwitch(isActive: true);
            var systemTime = new TestSystemTime(2018, 06, 24, 18, 10);
            var weatherForecast = TestFactory.NewWeatherForecastGood(systemTime);
            var homeSensor = new TestHomeSensor(true);
            var logger = TestFactory.NewMowLogger(new DateTime(2018, 6, 24, 0, 0, 0));

            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            mowController.CheckAndAct(); // Should see that mower seems to be stuck in home

            // Assert
            var logItems = logger.LogItems.Where(x => x.Type == LogType.MowerStuckInHome).ToList();

            Assert.AreEqual(1, logItems.Count);
            Assert.AreEqual(LogType.MowerLost, logItems[0].Type);
            Assert.AreEqual("2018-06-24 18:10", logItems[0].Time.ToString("yyyy-MM-dd HH:mm"));
        }

        [TestMethod]
        public void CheckAndAct_NotComingHome_LogMessageNotRepeated()
        {
            // Arrange
            var config = TestFactory.NewConfig3To10And16To2300(usingContactHomeSensor: true);
            var powerSwitch = new TestPowerSwitch(isActive: true);
            var systemTime = new TestSystemTime(2018, 06, 24, 3, 30);
            var weatherForecast = TestFactory.NewWeatherForecastGood(systemTime);
            var homeSensor = new TestHomeSensor(true);
            var logger = TestFactory.NewMowLogger(new DateTime(2018, 6, 24, 0, 0, 0));

            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            homeSensor.SetIsHome(false); // Mower leaves
            mowController.CheckAndAct(); // Sets mower to away
            systemTime.TickMinutes(123); // Mote time 123 minutes ahead
            mowController.CheckAndAct();
            systemTime.TickMinutes(1);

            // Act
            mowController.CheckAndAct(); // Should see that mower has been lost, but not set the log message again.

            // Assert
            var logItems = logger.LogItems.Where(x => x.Type == LogType.MowerLost).ToList();

            Assert.AreEqual(1, logItems.Count);
        }

        [TestMethod]
        public void CheckAndAct_NotComingHomeAndNotUsingContactSensor_LostLogMessageNotWritten()
        {
            // Arrange
            var config = TestFactory.NewConfig3To10And16To2300(usingContactHomeSensor: false);
            var powerSwitch = new TestPowerSwitch(isActive: true);
            var systemTime = new TestSystemTime(2018, 06, 24, 3, 30);
            var weatherForecast = TestFactory.NewWeatherForecastGood(systemTime);
            var homeSensor = new TestHomeSensor(true);
            var logger = TestFactory.NewMowLogger(new DateTime(2018, 6, 24, 0, 0, 0));

            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            homeSensor.SetIsHome(false); // Mower leaves
            mowController.CheckAndAct(); // Sets mower to away
            systemTime.TickMinutes(123); // Mote time 123 minutes ahead

            // Act
            mowController.CheckAndAct(); // Should see that mower has been lost

            // Assert
            var logItems = logger.LogItems.Where(x => x.Type == LogType.MowerLost).ToList();

            Assert.AreEqual(0, logItems.Count);
        }

        [TestMethod]
        public void CheckAndAct_BadWeatherBetweenTwoIntervals_CurrentTurnedOn()
        {
            // Arrange
            var config = TestFactory.NewConfig3To10And16To2300();
            var systemTime = new TestSystemTime(2018, 7, 24, 10, 5);
            var powerSwitch = new TestPowerSwitch(isActive: false);
            var weatherForecast = TestFactory.NewWeatherForecastBad(systemTime);
            var homeSensor = new TestHomeSensor(true);
            var logger = TestFactory.NewMowLogger(systemTime.Now);
            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            mowController.CheckAndAct();

            // Assert
            Assert.AreEqual(powerSwitch.Status, PowerStatus.On);
            Assert.IsFalse(powerSwitch.HasBeenTurnedOffOnce);
            Assert.IsTrue(powerSwitch.HasBeenTurnedOnOnce);

            var logItems = logger.LogItems.Where(x => x.Type == LogType.PowerOn).ToList();

            Assert.AreEqual(1, logItems.Count);
            string expectedLogDate = "2018-07-24 10:05";
            Assert.AreEqual(expectedLogDate, logItems[0].Time.ToString("yyyy-MM-dd HH:mm"));
        }

        [TestMethod]
        public void CheckAndAct_BadWeatherAtEndOfInterval_CurrentTurnedOffBeforeTnterval()
        {
            // Arrange
            var config = TestFactory.NewConfig3To10And16To2300(usingContactHomeSensor: false);
            var systemTime = new TestSystemTime(2018, 7, 24, 15, 0);
            var powerSwitch = new TestPowerSwitch(isActive: true);
            var weatherForecast = TestFactory.NewWeatherForecastGood(systemTime);
            weatherForecast.AddExpectation(false, new DateTime(2018, 7, 24, 21, 30, 0));
            var homeSensor = new TestHomeSensor(isHome: true);
            var logger = TestFactory.NewMowLogger(systemTime.Now);
            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            RunOverTime(mowController, systemTime, 10, 0);

            // Assert
            var logItems = logger.LogItems.Where(x => x.Type == LogType.PowerOff || x.Type == LogType.PowerOn).ToList();

            Assert.AreEqual(powerSwitch.Status, PowerStatus.On);
            Assert.AreEqual(2, logItems.Count);

            Assert.AreEqual(LogType.PowerOff, logItems[0].Type);
            Assert.AreEqual("2018-07-24 15:55", logItems[0].Time.ToString("yyyy-MM-dd HH:mm"));

            Assert.AreEqual(LogType.PowerOn, logItems[1].Type);
            Assert.AreEqual("2018-07-24 23:05", logItems[1].Time.ToString("yyyy-MM-dd HH:mm"));
        }

        [TestMethod]
        public void CheckAndAct_BadWeatherAtEndOfIntervalUsingContactSensor_CurrentTurnedOffInTnterval()
        {
            // Arrange
            var config = TestFactory.NewConfig3To10And16To2300(usingContactHomeSensor: true, maxMowingWithoutCharge: 2);
            var systemTime = new TestSystemTime(2018, 7, 24, 15, 0);
            var powerSwitch = new TestPowerSwitch(isActive: true);
            var weatherForecast = TestFactory.NewWeatherForecastGood(systemTime);
            weatherForecast.AddExpectation(false, new DateTime(2018, 7, 24, 21, 30, 0));
            var homeSensor = new TestHomeSensor(isHome: true);
            var logger = TestFactory.NewMowLogger(systemTime.Now);
            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            RunOverTime(mowController, systemTime, 1, 0);
            homeSensor.SetIsHome(false);
            RunOverTime(mowController, systemTime, 1, 30);
            homeSensor.SetIsHome(true);
            RunOverTime(mowController, systemTime, 1, 0);
            homeSensor.SetIsHome(false);
            RunOverTime(mowController, systemTime, 1, 30);
            homeSensor.SetIsHome(true);
            RunOverTime(mowController, systemTime, 5, 0);

            // Assert
            var logItems = logger.LogItems.Where(x => x.Type == LogType.PowerOff || x.Type == LogType.PowerOn).ToList();

            Assert.AreEqual(powerSwitch.Status, PowerStatus.On);
            Assert.AreEqual(2, logItems.Count);

            Assert.AreEqual(LogType.PowerOff, logItems[0].Type);
            Assert.AreEqual("2018-07-24 20:30", logItems[0].Time.ToString("yyyy-MM-dd HH:mm"));

            Assert.AreEqual(LogType.PowerOn, logItems[1].Type);
            Assert.AreEqual("2018-07-24 23:05", logItems[1].Time.ToString("yyyy-MM-dd HH:mm"));
        }

        [TestMethod]
        public void CheckAndAct_PowerStatusUnknownGoodWeatherAhead_LogMessageWritten()
        {
            // Arrange
            var config = TestFactory.NewConfig3To10And16To2300(usingContactHomeSensor: true, maxMowingWithoutCharge: 2);
            var systemTime = new TestSystemTime(2018, 7, 24, 6, 0);
            var powerSwitch = new TestPowerSwitch(PowerStatus.Unknown);
            var weatherForecast = TestFactory.NewWeatherForecastGood(systemTime);
            var homeSensor = new TestHomeSensor(isHome: true);
            var logger = TestFactory.NewMowLogger(systemTime.Now);
            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            mowController.CheckAndAct();

            // Assert
            var logItems = logger.LogItems.Where(x => x.Type == LogType.PowerOn || x.Type == LogType.PowerOff).ToList();

            Assert.AreEqual(powerSwitch.Status, PowerStatus.On);
            Assert.AreEqual(1, logItems.Count);

            Assert.AreEqual(LogType.PowerOn, logItems[0].Type);
            Assert.AreEqual("2018-07-24 06:00", logItems[0].Time.ToString("yyyy-MM-dd HH:mm"));
        }

        [TestMethod]
        public void CheckAndAct_PowerStatusUnknownBadWeatherAhead_LogMessageWritten()
        {
            // Arrange
            var config = TestFactory.NewConfig3To10And16To2300(
                usingContactHomeSensor: true, 
                maxMowingWithoutCharge: 2);
            var systemTime = new TestSystemTime(2018, 7, 24, 6, 0);
            var powerSwitch = new TestPowerSwitch(PowerStatus.Unknown);
            var weatherForecast = TestFactory.NewWeatherForecastBad(systemTime);
            var homeSensor = new TestHomeSensor(isHome: true);
            var logger = TestFactory.NewMowLogger(systemTime.Now);
            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            mowController.CheckAndAct();

            // Assert
            var logItems = logger.LogItems.Where(x => x.Type == LogType.PowerOn || x.Type == LogType.PowerOff).ToList();

            Assert.AreEqual(powerSwitch.Status, PowerStatus.Off);
            Assert.AreEqual(1, logItems.Count);

            Assert.AreEqual(LogType.PowerOff, logItems[0].Type);
            Assert.AreEqual("2018-07-24 06:00", logItems[0].Time.ToString("yyyy-MM-dd HH:mm"));
        }
    }
}
