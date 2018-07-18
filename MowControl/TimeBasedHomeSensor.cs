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

        public TimeBasedHomeSensor(IMowPlannerConfig config, ISystemTime systemTime)
        {
            _config = config;
            _systemTime = systemTime;
        }

        /// <summary>
        /// Hämtar huruvida robotgräsklipparen står i boet eller inte.
        /// </summary>
        public bool IsHome {
            get
            {
                bool isHome = true;

                foreach (var timeInterval in _config.TimeIntervals)
                {
                    if (timeInterval.ContainsTime(_systemTime.Now))
                    {
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

                return isHome;
            }
        }
    }
}
