using MowControl;
using System;
using System.Collections.Generic;
using System.Text;

namespace MowerTests
{
    public class TestHomeSensor : IHomeSensor
    {
        public TestHomeSensor(bool isHome)
        {
            IsHome = isHome;
        }

        public void SetIsHome(bool isHome)
        {
            IsHome = isHome;
        }

        public bool IsHome { get; private set; }
    }
}
