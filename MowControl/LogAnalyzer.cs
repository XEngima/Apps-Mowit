using System;
using System.Collections.Generic;
using System.Text;
using MowControl.DateTimeExtensions;

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
            IsStuck = false;

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

                    if (CurrentLogItem.Time.Day == 9)
                    {
                        int debug = 0;
                    }

                    if (!_isMowing) // If just stopped mowing
                    {
                        var mowingTimePerDay = GetLastMowingTimePerDayItem();

                        if (mowingTimePerDay.Date.ToString("yyyy-MM-dd") != CurrentLogItem.Time.ToString("yyyy-MM-dd"))
                        {
                            mowingTimePerDay = new TimePerDayItem(CurrentLogItem.Time);
                            _mowingTimePerDayList.Add(mowingTimePerDay);
                        }

                        mowingTimePerDay.AddSpentTime(CurrentLogItem.Time.FloorMinutes() - LastMowingStartedTime.FloorMinutes());

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
                    if (CurrentLogItem.Time.ToString("yyyy-MM-dd HH:mm") == "2018-07-24 23:59")
                    {
                        int debug = 0;
                    }

                    _isActuallyMowing = value;

                    if (!_isActuallyMowing) // If just stopped mowing
                    {
                        var actualMowingTimePerDay = GetLastActuallyMowingTimePerDayItem();

                        if (actualMowingTimePerDay.Date.ToString("yyyy-MM-dd") != CurrentLogItem.Time.ToString("yyyy-MM-dd"))
                        {
                            actualMowingTimePerDay = new TimePerDayItem(CurrentLogItem.Time);
                            _actualMowingTimePerDayList.Add(actualMowingTimePerDay);
                        }

                        actualMowingTimePerDay.AddSpentTime(CurrentLogItem.Time.FloorMinutes() - LastActualMowingStartedTime.FloorMinutes());

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

        public bool IsStuck { get; private set; }

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
                            IsStuck = false;
                        }
                        break;
                    case LogType.MowingStarted:
                        {
                            IsMowing = true;
                            IsStuck = false;
                        }
                        break;
                    case LogType.MowingEnded:
                        {
                            IsMowing = false;
                            IsActuallyMowing = false;
                        }
                        break;
                    case LogType.MowerStuckInHome:
                        {
                            IsStuck = true;
                        }
                        break;
                }
            }
        }
    }
}
