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
            var timeIntervals = new List<TimeInterval>();

            var config = new MowPlannerConfig()
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
            var mowController = new MowController(config, powerSwitch, null, systemTime, homeSensor, logger);
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
            var config = new MowPlannerConfig()
            {
                TimeIntervals = null
            };
            var powerSwitch = new TestPowerSwitch();
            var homeSensor = new TestHomeSensor(true);
            var mowController = new MowController(config, powerSwitch, null, null, homeSensor, null);
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
            var systemTime = new TestSystemTime(6, 0);
            var weatherForecast = TestFactory.NewWeatherForecastGood(systemTime);
            var homeSensor = new TestHomeSensor(true);
            var logger = TestFactory.NewMowLogger();
            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            mowController.CheckAndAct();

            // Assert
            Assert.IsTrue(powerSwitch.IsOn);
            Assert.IsTrue(powerSwitch.HasBeenTurnedOnOnce);

            var logItems = logger.LogItems.Where(x => x.Type == LogType.PowerOn).ToList();

            Assert.AreEqual(1, logItems.Count);
            Assert.AreEqual(LogType.PowerOn, logItems[0].Type);
            string expectedLogDate = DateTime.Now.ToString("yyyy-MM-dd ") + "06:00";
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
            var homeSensor = new TimeBasedHomeSensor(config, systemTime);
            var logger = new MowLogger();
            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            mowController.CheckAndAct();

            // Assert
            Assert.IsTrue(powerSwitch.IsOn);
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
            var systemTime = new TestSystemTime(12, 50);
            var weatherForecast = TestFactory.NewWeatherForecastBad(systemTime);
            var homeSensor = new TimeBasedHomeSensor(config, systemTime);
            var logger = TestFactory.NewMowLogger();
            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            RunOverTime(mowController, systemTime, 0, 10);

            // Assert
            Assert.IsFalse(powerSwitch.IsOn);
            Assert.IsTrue(powerSwitch.HasBeenTurnedOffOnce);

            var logItems = logger.LogItems.Where(x => x.Type == LogType.PowerOff).ToList();

            Assert.AreEqual(1, logItems.Count);
            Assert.AreEqual(LogType.PowerOff, logItems[0].Type);
            string expectedLogDate = DateTime.Now.ToString("yyyy-MM-dd ") + "12:55";
            Assert.AreEqual(expectedLogDate, logItems[0].Time.ToString("yyyy-MM-dd HH:mm"));
            Assert.IsTrue(logItems[0].Message.StartsWith("Power was turned off."));
        }

        [TestMethod]
        public void CheckAndAct_AfterWorkingIntervalAndComingHomeRightBeforeNextBadWeatherInterval2_CurrentTurnedOffOnce()
        {
            // Arrange
            var config = TestFactory.NewConfig6To12And13To19();
            var powerSwitch = new TestPowerSwitch(isActive: true);
            var systemTime = new TestSystemTime(12, 58);
            var weatherForecast = TestFactory.NewWeatherForecastBad(systemTime);
            var homeSensor = new TimeBasedHomeSensor(config, systemTime);
            var logger = TestFactory.NewMowLogger();
            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            RunOverTime(mowController, systemTime, 0, 10);

            // Assert
            Assert.IsFalse(powerSwitch.IsOn);
            Assert.IsTrue(powerSwitch.HasBeenTurnedOffOnce);

            var logItems = logger.LogItems.Where(x => x.Type == LogType.PowerOff).ToList();

            Assert.AreEqual(1, logItems.Count);
            Assert.AreEqual(LogType.PowerOff, logItems[0].Type);
            string expectedLogDate = DateTime.Now.ToString("yyyy-MM-dd ") + "12:58";
            Assert.AreEqual(expectedLogDate, logItems[0].Time.ToString("yyyy-MM-dd HH:mm"));
            Assert.IsTrue(logItems[0].Message.StartsWith("Power was turned off."));
        }

        [TestMethod]
        public void CheckAndAct_Running24HoursInGoodWeather_CurrentNeverChanged()
        {
            // Arrange
            var config = TestFactory.NewConfig6To12();
            var powerSwitch = new TestPowerSwitch(isActive: false);
            var systemTime = new TestSystemTime(3, 0);
            var weatherForecast = TestFactory.NewWeatherForecastGood(systemTime);
            var homeSensor = new TimeBasedHomeSensor(config, systemTime);
            var logger = TestFactory.NewMowLogger();
            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            RunOverTime(mowController, systemTime, 24, 0);

            // Assert
            Assert.IsTrue(powerSwitch.IsOn);
            Assert.AreEqual(1, powerSwitch.TurnOns);
            Assert.AreEqual(0, powerSwitch.TurnOffs);
        }

        [TestMethod]
        public void CheckAndAct_Running24HoursInGoodWeatherGettingBad_CurrentTurnedOffOnce()
        {
            // Arrange
            var config = TestFactory.NewConfig0To6And12To18();
            var powerSwitch = new TestPowerSwitch(isActive: false);
            var systemTime = new TestSystemTime(11, 0);
            var weatherForecast = TestFactory.NewWeatherForecastGood(systemTime);
            var homeSensor = new TimeBasedHomeSensor(config, systemTime);
            var logger = TestFactory.NewMowLogger();
            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            RunOverTime(mowController, systemTime, 12, 0);
            weatherForecast.SetExpectation(false);
            RunOverTime(mowController, systemTime, 12, 0);

            // Assert
            Assert.IsFalse(powerSwitch.IsOn);
            Assert.AreEqual(1, powerSwitch.TurnOns);
            Assert.AreEqual(1, powerSwitch.TurnOffs);

            var logItems = logger.LogItems.Where(x => x.Type == LogType.PowerOn || x.Type == LogType.PowerOff).ToList();
            Assert.AreEqual(2, logItems.Count);

            Assert.AreEqual(LogType.PowerOff, logItems[1].Type);
            string expectedLogDate = DateTime.Now.ToString("yyyy-MM-dd ") + "23:55";
            Assert.AreEqual(expectedLogDate, logItems[1].Time.ToString("yyyy-MM-dd HH:mm"));
            Assert.IsTrue(logItems[1].Message.StartsWith("Power was turned off."));
        }

        [TestMethod]
        public void CheckAndAct_Running24HoursInGoodWeatherGettingBadStartAtDay_CurrentTurnedOffOnce()
        {
            // Arrange
            var config = TestFactory.NewConfig6To12And18To2359();
            var powerSwitch = new TestPowerSwitch(isActive: false);
            var systemTime = new TestSystemTime(16, 0);
            var weatherForecast = TestFactory.NewWeatherForecastGood(systemTime);
            var homeSensor = new TimeBasedHomeSensor(config, systemTime);
            var logger = TestFactory.NewMowLogger();
            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            RunOverTime(mowController, systemTime, 12, 0);
            weatherForecast.SetExpectation(false);
            RunOverTime(mowController, systemTime, 12, 0);

            // Assert
            Assert.IsFalse(powerSwitch.IsOn);
            Assert.AreEqual(1, powerSwitch.TurnOns);
            Assert.AreEqual(1, powerSwitch.TurnOffs);

            var logItems = logger.LogItems.Where(x => x.Type == LogType.PowerOn || x.Type == LogType.PowerOff).ToList();
            Assert.AreEqual(2, logItems.Count);

            Assert.AreEqual(LogType.PowerOff, logItems[1].Type);
            string expectedLogDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd ") + "05:55";
            Assert.AreEqual(expectedLogDate, logItems[1].Time.ToString("yyyy-MM-dd HH:mm"));
            Assert.IsTrue(logItems[1].Message.StartsWith("Power was turned off."));
        }

        [TestMethod]
        public void CheckAndAct_Running24HoursInBadWeatherGettingGood_CurrentTurnedOffOnce()
        {
            // Arrange
            var config = TestFactory.NewConfig6To12And18To2359();
            var powerSwitch = new TestPowerSwitch(isActive: true);
            var systemTime = new TestSystemTime(3, 0);
            var weatherForecast = TestFactory.NewWeatherForecastBad(systemTime);
            var homeSensor = new TimeBasedHomeSensor(config, systemTime);
            var logger = TestFactory.NewMowLogger();
            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            RunOverTime(mowController, systemTime, 12, 0);
            weatherForecast.SetExpectation(true);
            RunOverTime(mowController, systemTime, 12, 0);

            // Assert
            Assert.IsTrue(powerSwitch.IsOn);
            Assert.AreEqual(1, powerSwitch.TurnOns);
            Assert.AreEqual(1, powerSwitch.TurnOffs);

            var logItems = logger.LogItems.Where(x => x.Type == LogType.PowerOn || x.Type == LogType.PowerOff).ToList();

            Assert.AreEqual(2, logItems.Count);
            Assert.AreEqual(LogType.PowerOff, logItems[0].Type);
            string expectedLogDate = DateTime.Now.ToString("yyyy-MM-dd ") + "05:55";
            Assert.AreEqual(expectedLogDate, logItems[0].Time.ToString("yyyy-MM-dd HH:mm"));
            Assert.IsTrue(logItems[0].Message.StartsWith("Power was turned off."));

            Assert.AreEqual(LogType.PowerOn, logItems[1].Type);
            expectedLogDate = DateTime.Now.ToString("yyyy-MM-dd ") + "18:00";
            Assert.AreEqual(expectedLogDate, logItems[1].Time.ToString("yyyy-MM-dd HH:mm"));
            Assert.IsTrue(logItems[1].Message.StartsWith("Power was turned on."));
        }

        [TestMethod]
        public void CheckAndAct_Running24HoursInBadWeatherGettingGoodStartAtDay_CurrentTurnedOffOnce()
        {
            // Arrange
            var config = TestFactory.NewConfig6To12And18To2359();
            var powerSwitch = new TestPowerSwitch(isActive: true);
            var systemTime = new TestSystemTime(12, 0);
            var weatherForecast = TestFactory.NewWeatherForecastBad(systemTime);
            var homeSensor = new TimeBasedHomeSensor(config, systemTime);
            var logger = TestFactory.NewMowLogger();
            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            RunOverTime(mowController, systemTime, 12, 0);
            weatherForecast.SetExpectation(true);
            RunOverTime(mowController, systemTime, 12, 0);

            // Assert
            Assert.IsTrue(powerSwitch.IsOn);
            Assert.AreEqual(1, powerSwitch.TurnOns);
            Assert.AreEqual(1, powerSwitch.TurnOffs);

            var logItems = logger.LogItems.Where(x => x.Type == LogType.PowerOn || x.Type == LogType.PowerOff).ToList();
            Assert.AreEqual(2, logItems.Count);

            Assert.AreEqual(LogType.PowerOff, logItems[0].Type);
            string expectedLogDate = DateTime.Now.ToString("yyyy-MM-dd ") + "17:55";
            Assert.AreEqual(expectedLogDate, logItems[0].Time.ToString("yyyy-MM-dd HH:mm"));
            Assert.IsTrue(logItems[0].Message.StartsWith("Power was turned off."));

            Assert.AreEqual(LogType.PowerOn, logItems[1].Type);
            expectedLogDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd ") + "06:00";
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
            var homeSensor = new TimeBasedHomeSensor(config, systemTime);

            var logger = new MowLogger();
            logger.Write(new DateTime(2018, 6, 26, 0, 0, 0), LogType.MowControllerStarted, "Mow controller started.");
            logger.Write(new DateTime(2018, 6, 26, 6, 0, 0), LogType.PowerOn, "Power was turned on.");

            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            mowController.CheckAndAct();

            // Assert
            Assert.IsFalse(powerSwitch.IsOn);
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

            var homeSensor = new TimeBasedHomeSensor(config, systemTime);

            var logger = new MowLogger();
            logger.Write(new DateTime(2018, 06, 25, 0, 0, 0), LogType.MowControllerStarted, "");
            logger.Write(new DateTime(2018, 06, 25, 0, 0, 0), LogType.PowerOn, "");

            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            mowController.CheckAndAct();

            // Assert
            Assert.IsTrue(powerSwitch.IsOn);
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
            var homeSensor = new TimeBasedHomeSensor(config, systemTime);

            var logger = TestFactory.NewMowLogger(new DateTime(2018, 6, 24, 3, 0, 0));

            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            RunOverTime(mowController, systemTime, 72, 0); // 3 dagar

            // Assert
            Assert.IsTrue(powerSwitch.IsOn);
            Assert.AreEqual(2, powerSwitch.TurnOns);
            Assert.AreEqual(1, powerSwitch.TurnOffs);

            var logItems = logger.LogItems.Where(x => x.Type == LogType.PowerOn || x.Type == LogType.PowerOff).ToList();

            Assert.AreEqual(3, logItems.Count);
            Assert.AreEqual(LogType.PowerOff, logItems[1].Type);
            Assert.AreEqual("2018-06-26 02:55", logItems[1].Time.ToString("yyyy-MM-dd HH:mm"));
            Assert.AreEqual("Power was turned off. Mowing not necessary.", logItems[1].Message);

            Assert.AreEqual(LogType.PowerOn, logItems[2].Type);
            Assert.AreEqual("2018-06-26 16:00", logItems[2].Time.ToString("yyyy-MM-dd HH:mm"));
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
            var homeSensor = new TimeBasedHomeSensor(config, systemTime);

            var logger = TestFactory.NewMowLogger(new DateTime(2018, 6, 24, 3, 0, 0));
            //logger.Write(new DateTime(2018, ), LogType.MowControllerStarted, "");

            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            RunOverTime(mowController, systemTime, 240, 0); // 10 dagar

            // Assert
            var logItems = logger.LogItems.Where(x => x.Type == LogType.PowerOn || x.Type == LogType.PowerOff).ToList();

            Assert.IsTrue(powerSwitch.IsOn);
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

            var homeSensor = new TimeBasedHomeSensor(config, systemTime);

            var logger = TestFactory.NewMowLogger(new DateTime(2018, 6, 24, 0, 0, 0));

            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            mowController.CheckAndAct();

            // Assert
            Assert.IsFalse(powerSwitch.IsOn);
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

            var homeSensor = new TimeBasedHomeSensor(config, systemTime);

            var logger = TestFactory.NewMowLogger(new DateTime(2018, 6, 24, 0, 0, 0));

            var mowController = new MowController(config, powerSwitch, weatherForecast, systemTime, homeSensor, logger);

            // Act
            mowController.CheckAndAct();
            systemTime.TickMinutes(1);
            mowController.CheckAndAct();

            // Assert
            Assert.IsFalse(powerSwitch.IsOn);
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
            Assert.IsTrue(powerSwitch.IsOn);
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
    }
}
