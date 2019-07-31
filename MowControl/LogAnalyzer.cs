using System;
using System.Collections.Generic;
using System.Text;

namespace MowControl
{
    public class LogAnalyzer
    {
        private IMowLogger _logger;
        private List<TimePerDayItem> _mowingTimePerDayList;

        public LogAnalyzer(IMowLogger logger, bool homeFromStart)
        {
            _logger = logger;
            _mowingTimePerDayList = new List<TimePerDayItem>();

            IsLost = false;
            IsMowing = false;
            IsHome = homeFromStart;

            if (_logger != null)
            {
                PerformLogAnalyzis();
            }
        }

        private TimePerDayItem GetLastMowingTimePerDayItem()
        {
            if (_mowingTimePerDayList.Count == 0)
            {
                _mowingTimePerDayList.Add(new TimePerDayItem(CurrentLogItem.Time));
            }

            return _mowingTimePerDayList[_mowingTimePerDayList.Count - 1];
        }

        public TimeSpan GetMowingTimeForDay(DateTime date)
        {
            foreach (var mowingTimePerDay in _mowingTimePerDayList)
            {
                if (mowingTimePerDay.Date.ToString("yyyy-MM-dd") == date.ToString("yyyy-MM-dd"))
                {
                    return mowingTimePerDay.SpentTime;
                }
            }

            return new TimeSpan();
        }

        public bool IsLost { get; private set; }

        private bool _isMowing;
        public bool IsMowing
        {
            get
            {
                return _isMowing;
            }
            private set
            {
                if (_isMowing != value)
                {
                    _isMowing = value;

                    if (!_isMowing) // If just stopped mowing
                    {
                        var mowingTimePerDay = GetLastMowingTimePerDayItem();

                        if (mowingTimePerDay.Date.ToString("yyyy-MM-dd") == CurrentLogItem.Time.ToString("yyyy-MM-dd"))
                        {
                            mowingTimePerDay.AddSpentTime(CurrentLogItem.Time - LastMowingStartedTime);
                        }

                        LastMowingEndedTime = CurrentLogItem.Time;
                    }
                    else // If just started mowing
                    {
                        LastMowingStartedTime = CurrentLogItem.Time;
                    }
                }
            }
        }

        public bool IsHome { get; private set; }

        private LogItem CurrentLogItem { get; set; }

        private DateTime LastMowingStartedTime { get; set; }

        private DateTime LastMowingEndedTime { get; set; }

        private void PerformLogAnalyzis()
        {
            foreach (var logItem in _logger.LogItems)
            {
                CurrentLogItem = logItem;

                switch (logItem.Type)
                {
                    case LogType.MowerLost:
                        {
                            IsLost = true;
                        }
                        break;
                    case LogType.MowerCame:
                        {
                            IsHome = true;
                            IsLost = false;
                        }
                        break;
                    case LogType.MowerLeft:
                        {
                            IsHome = false;
                        }
                        break;
                    case LogType.MowingStarted:
                        {
                            IsMowing = true;
                        }
                        break;
                    case LogType.MowingEnded:
                        {
                            IsMowing = false;
                        }
                        break;
                }
            }
        }
    }
}
