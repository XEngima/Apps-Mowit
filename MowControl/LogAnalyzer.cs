using System;
using System.Collections.Generic;
using System.Text;

namespace MowControl
{
    public class LogAnalyzer
    {
        private IMowLogger _logger;
        private List<TimePerDayItem> _mowingTimePerDayList;
        private List<TimePerDayItem> _actualMowingTimePerDayList;

        public LogAnalyzer(IMowLogger logger, bool homeFromStart)
        {
            _logger = logger;
            _mowingTimePerDayList = new List<TimePerDayItem>();
            _actualMowingTimePerDayList = new List<TimePerDayItem>();

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

        private TimePerDayItem GetLastActuallyMowingTimePerDayItem()
        {
            if (_actualMowingTimePerDayList.Count == 0)
            {
                _actualMowingTimePerDayList.Add(new TimePerDayItem(CurrentLogItem.Time));
            }

            return _actualMowingTimePerDayList[_actualMowingTimePerDayList.Count - 1];
        }

        public TimeSpan GetActuallyMowingTimeForDay(DateTime date)
        {
            foreach (var actualMowingTimePerDay in _actualMowingTimePerDayList)
            {
                if (actualMowingTimePerDay.Date.ToString("yyyy-MM-dd") == date.ToString("yyyy-MM-dd"))
                {
                    return actualMowingTimePerDay.SpentTime;
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


        private bool _isActuallyMowing;
        public bool IsActuallyMowing
        {
            get
            {
                return _isActuallyMowing;
            }
            private set
            {
                if (_isActuallyMowing != value)
                {
                    _isActuallyMowing = value;

                    if (!_isActuallyMowing) // If just stopped mowing
                    {
                        var actualMowingTimePerDay = GetLastActuallyMowingTimePerDayItem();

                        if (actualMowingTimePerDay.Date.ToString("yyyy-MM-dd") == CurrentLogItem.Time.ToString("yyyy-MM-dd"))
                        {
                            actualMowingTimePerDay.AddSpentTime(CurrentLogItem.Time - LastActualMowingStartedTime);
                        }

                        LastActualMowingEndedTime = CurrentLogItem.Time;
                    }
                    else // If just started mowing
                    {
                        LastActualMowingStartedTime = CurrentLogItem.Time;
                    }
                }
            }
        }
        public bool IsHome { get; private set; }

        private LogItem CurrentLogItem { get; set; }

        private DateTime LastMowingStartedTime { get; set; }

        private DateTime LastMowingEndedTime { get; set; }

        private DateTime LastActualMowingStartedTime { get; set; }

        private DateTime LastActualMowingEndedTime { get; set; }

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
                            IsActuallyMowing = false;
                        }
                        break;
                    case LogType.MowerCame:
                        {
                            IsHome = true;
                            IsLost = false;
                            IsActuallyMowing = false;
                        }
                        break;
                    case LogType.MowerLeft:
                        {
                            IsHome = false;
                            IsActuallyMowing = true;
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
                            IsActuallyMowing = false;
                        }
                        break;
                }
            }
        }
    }
}
