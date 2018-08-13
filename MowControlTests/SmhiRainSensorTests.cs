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
        [TestMethod]
        public void IsWet_RainAnHourAgo_Wet()
        {
            var systemTime = new TestSystemTime(2018, 8, 13, 8, 0);
            var smhi = new TestSmhi(systemTime, new ForecastTimeSerie[] {
                new ForecastTimeSerie() { validTime = new DateTime(2018, 8, 13, 8, 0, 0) }
            });

            var rainSensor = new SmhiRainSensor(smhi);

            // Act
            bool isWet = rainSensor.IsWet;

            Assert.IsFalse(isWet);
        }
    }
}
