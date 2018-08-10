using MowControl;
using System;
using System.Collections.Generic;
using System.Text;

namespace MowerTests
{
    public class TestRainSensor : IRainSensor
    {
        public TestRainSensor(bool isWet = false)
        {
            IsWet = isWet;
        }

        public bool IsWet { get; private set; }
    }
}
