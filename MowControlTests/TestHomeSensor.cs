using MowControl;
using System;
using System.Collections.Generic;
using System.Text;

namespace MowerTests
{
    public class TestHomeSensor : IHomeSensor
    {
        private ISystemTime _systemTime;

        public TestHomeSensor(
            ISystemTime systemTime,
            bool isHome = false,
            DateTime? mowerCameTime = null,
            DateTime? mowerLeftTime = null)
        {
            _systemTime = systemTime;
            IsHome = isHome;
            MowerCameTime = mowerCameTime.HasValue ? mowerCameTime.Value : DateTime.MinValue;
            MowerLeftTime = mowerLeftTime.HasValue ? mowerLeftTime.Value : DateTime.MinValue;
        }

        public void SetIsHome(bool isHome)
        {
            bool wasHome = IsHome;

            if (isHome && !wasHome)
            {
                MowerCameTime = _systemTime.Now;
            }
            else if (!isHome && wasHome)
            {
                MowerLeftTime = _systemTime.Now;
            }

            IsHome = isHome;
        }

        public bool IsHome { get; private set; }

        public DateTime MowerCameTime { get; private set; }

        public DateTime MowerLeftTime { get; private set; }
    }
}
