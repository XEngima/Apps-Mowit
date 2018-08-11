using Microsoft.VisualStudio.TestTools.UnitTesting;
using MowControl;
using MowerTests;
using System;
using System.Collections.Generic;
using System.Text;

namespace MowControllerTests
{
    [TestClass]
    public class SmhiRainSensorTests
    {
        [TestMethod]
        public void IsWet_RainAnHourAgo_Wet()
        {
            var smhi = new TestSmhi();

            var rainSensor = new SmhiRainSensor(smhi);

            // Act
            bool isWet = rainSensor.IsWet;

            Assert.IsTrue(isWet);
        }
    }
}
