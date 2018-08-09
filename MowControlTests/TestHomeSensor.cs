using MowControl;
using System;
using System.Collections.Generic;
using System.Text;

namespace MowerTests
{
    public class TestHomeSensor : IHomeSensor
    {
        private ISystemTime _systemTime;
        private bool _isHome;
        DateTime _startTime;
        //bool _simulateComingAndLeaving;

        public TestHomeSensor(
            ISystemTime systemTime,
            bool isHome = false,
            DateTime? mowerCameTime = null,
            DateTime? mowerLeftTime = null)
        {
            _systemTime = systemTime;
            _startTime = systemTime.Now;
            //_simulateComingAndLeaving = simulateComingAndLeaving;
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

        public bool IsHome {
            get
            {
                //if (_)
                //if (_startTime.ToString("yyyy-MM-dd") != _systemTime.Now.Hour _systemTime.Now.Minute == 0)
                //{
                //    _isHome = !_isHome;
                //}

                return _isHome;
            }
            set
            {
                _isHome = value;
            }
        }

        public DateTime MowerCameTime { get; private set; }

        public DateTime MowerLeftTime { get; private set; }
    }
}
