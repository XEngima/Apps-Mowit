using System;
using System.Collections.Generic;
using System.Text;

namespace MowControl
{
    /// <summary>
    /// Sensor som kollar om robotgräsklipparen står i sitt bo eller inte.
    /// </summary>
    public class TimeBasedHomeSensor : IHomeSensor
    {
        private const int cIntervalOverSafeTimeMin = 55;

        private IMowPlannerConfig _config;
        private ISystemTime _systemTime;
        private IPowerSwitch _powerSwitch;
        private bool _wasHomeDuringLastInterval;
        bool _firstCheck;

        public TimeBasedHomeSensor(IMowPlannerConfig config, IPowerSwitch powerSwitch, ISystemTime systemTime)
        {
            _config = config;
            _systemTime = systemTime;
            _powerSwitch = powerSwitch;
            _wasHomeDuringLastInterval = true;
            _firstCheck = true;
        }

        /// <summary>
        /// Hämtar huruvida robotgräsklipparen står i boet eller inte.
        /// </summary>
        public bool IsHome {
            get
            {
                if (_config.TimeIntervals?.Count == 0)
                {
                    return true;
                }

                bool isHome = true;
                bool inAnyInterval = false;

                // If power is off, the mower is always at home.
                foreach (var timeInterval in _config.TimeIntervals)
                {
                    if (timeInterval.ContainsTime(_systemTime.Now))
                    {
                        inAnyInterval = true;
                        isHome = false;
                        break;
                    }

                    var timeIntervalEndTimeToday = new DateTime(_systemTime.Now.Year, _systemTime.Now.Month, _systemTime.Now.Day, timeInterval.EndHour, timeInterval.EndMin, 0);
                    var timeIntervalEndTimeYesterday = timeIntervalEndTimeToday.AddDays(-1);
                    var offsetTimeSpan = new TimeSpan(0, cIntervalOverSafeTimeMin, 0);

                    if (_systemTime.Now > timeIntervalEndTimeToday)
                    {
                        if (_systemTime.Now < timeIntervalEndTimeToday + offsetTimeSpan)
                        {
                            isHome = false;
                            break;
                        }
                    }
                    else
                    {
                        if (_systemTime.Now < timeIntervalEndTimeYesterday + offsetTimeSpan)
                        {
                            isHome = false;
                            break;
                        }
                    }
                }

                if (!_powerSwitch.IsOn)
                {
                    isHome = true;
                }

                if (inAnyInterval)
                {
                    _wasHomeDuringLastInterval = isHome;
                }

                if (!_firstCheck && !inAnyInterval && _wasHomeDuringLastInterval)
                {
                    return true;
                }

                _firstCheck = false;
                return isHome;
            }
        }
    }
}
