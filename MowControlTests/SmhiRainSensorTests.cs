using Microsoft.VisualStudio.TestTools.UnitTesting;
using MowControl;
using MowerTests;
using System;
using System.Collections.Generic;
using System.Text;
using SmhiWeather;

namespace MowerTests
{
    [TestClass]
    public class SmhiRainSensorTests
    {
        private ForecastTimeSerie NewTimeSerie(DateTime validTime,
            decimal temperature = 0,
            decimal relativeHumidity = 0,
            decimal precipitationMin = 0,
            decimal precipitationMax = 0)
        {
            return new ForecastTimeSerie
            {
                validTime = validTime.ToUniversalTime(),
                parameters = new ForecastParameter[] {
                    new ForecastParameter() { name = "t", values = new [] { temperature } },
                    new ForecastParameter() { name = "r", values = new [] { relativeHumidity } },
                    new ForecastParameter() { name = "pmax", values = new [] { precipitationMax } },
                    new ForecastParameter() { name = "pmin", values = new [] { precipitationMin } },
                }
            };
        }

        [TestMethod]
        public void IsWet_CurrentlyRaining_Wet()
        {
            // Assign
            var systemTime = new TestSystemTime(2018, 8, 13, 8, 0);

            // Forecasted weather
            var smhi = new TestSmhi(systemTime, new ForecastTimeSerie[]
            {
                NewTimeSerie(new DateTime(2018, 8, 13, 8, 0, 0), precipitationMax: 0.2m, relativeHumidity: 65),
            });

            // Past weather
            var rainSensor = new SmhiRainSensor(systemTime, smhi, new ForecastTimeSerie[] {});

            // Act
            bool isWet = rainSensor.IsWet;

            // Assert
            Assert.IsTrue(isWet);
        }

        [TestMethod]
        public void IsWet_HeavyRainAnHourAgo_StillWet()
        {
            // Assign
            var systemTime = new TestSystemTime(2018, 8, 13, 8, 0);

            // Forecasted weather
            var smhi = new TestSmhi(systemTime, new ForecastTimeSerie[]
            {
                new ForecastTimeSerie().Seed(new DateTime(2018, 8, 13, 8, 0, 0), precipitationMax: 0m, relativeHumidity: 69),
            });

            // Past weather
            var rainSensor = new SmhiRainSensor(systemTime, smhi, new ForecastTimeSerie[] {
                new ForecastTimeSerie().Seed(new DateTime(2018, 8, 13, 7, 0, 0), precipitationMax: 1.5m, relativeHumidity: 75),
            });

            // Act
            bool isWet = rainSensor.IsWet;

            // Assert
            Assert.IsTrue(isWet);
        }

        [TestMethod]
        public void IsWet_LightRainSixHoursAgo_Dry()
        {
            // Assign
            var systemTime = new TestSystemTime(2018, 8, 13, 18, 0);

            // Forecasted weather
            var smhi = new TestSmhi(systemTime, new ForecastTimeSerie[]
            {
                new ForecastTimeSerie().Seed(new DateTime(2018, 8, 13, 18, 0, 0), precipitationMax: 0m, relativeHumidity: 45),
            });

            // Past weather
            var rainSensor = new SmhiRainSensor(systemTime, smhi, new ForecastTimeSerie[] {
                new ForecastTimeSerie().Seed(new DateTime(2018, 8, 13, 12, 0, 0), precipitationMax: 0.1m, relativeHumidity: 40),
                new ForecastTimeSerie().Seed(new DateTime(2018, 8, 13, 13, 0, 0), precipitationMax: 0m, relativeHumidity: 40),
                new ForecastTimeSerie().Seed(new DateTime(2018, 8, 13, 14, 0, 0), precipitationMax: 0m, relativeHumidity: 40),
                new ForecastTimeSerie().Seed(new DateTime(2018, 8, 13, 15, 0, 0), precipitationMax: 0m, relativeHumidity: 40),
                new ForecastTimeSerie().Seed(new DateTime(2018, 8, 13, 16, 0, 0), precipitationMax: 0m, relativeHumidity: 40),
                new ForecastTimeSerie().Seed(new DateTime(2018, 8, 13, 17, 0, 0), precipitationMax: 0m, relativeHumidity: 40),
            });

            // Act
            bool isWet = rainSensor.IsWet;

            // Assert
            Assert.IsFalse(isWet);
        }

        [TestMethod]
        public void IsWet_RainInEveningThenALongNight_Wet()
        {
            // Assign
            var systemTime = new TestSystemTime(2018, 8, 14, 5, 0);

            // Forecasted weather
            var smhi = new TestSmhi(systemTime, new ForecastTimeSerie[]
            {
                new ForecastTimeSerie().Seed(new DateTime(2018, 8, 14, 5, 0, 0), precipitationMax: 0m, relativeHumidity: 45),
            });

            // Past weather
            var rainSensor = new SmhiRainSensor(systemTime, smhi, new ForecastTimeSerie[] {
                new ForecastTimeSerie().Seed(new DateTime(2018, 8, 13, 12, 22, 0), precipitationMax: 1m, relativeHumidity: 95),
                new ForecastTimeSerie().Seed(new DateTime(2018, 8, 13, 13, 23, 0), precipitationMax: 0, relativeHumidity: 95),
                new ForecastTimeSerie().Seed(new DateTime(2018, 8, 14, 14, 0, 0), precipitationMax: 0, relativeHumidity: 95),
                new ForecastTimeSerie().Seed(new DateTime(2018, 8, 14, 15, 1, 0), precipitationMax: 0, relativeHumidity: 95),
                new ForecastTimeSerie().Seed(new DateTime(2018, 8, 14, 16, 2, 0), precipitationMax: 0, relativeHumidity: 95),
                new ForecastTimeSerie().Seed(new DateTime(2018, 8, 14, 17, 3, 0), precipitationMax: 0, relativeHumidity: 95),
                new ForecastTimeSerie().Seed(new DateTime(2018, 8, 14, 17, 4, 0), precipitationMax: 0m, relativeHumidity: 95),
            });

            // Act
            bool isWet = rainSensor.IsWet;

            // Assert
            Assert.IsTrue(isWet);
        }
    }
}
