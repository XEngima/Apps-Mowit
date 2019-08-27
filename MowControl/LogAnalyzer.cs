using System;
using System.Collections.Generic;
using System.Text;
using MowControl.DateTimeExtensions;
using System.Linq;

namespace MowControl
{
    public class LogAnalyzer
    {
        private IMowLogger _logger;
        private List<TimePerDayItem> _mowingTimePerDayList;
        private List<TimePerDayItem> _actualMowingTimePerDayList;
        private List<TimePerDayItem> _mowerAwayList;

        public LogAnalyzer(IMowLogger logger, bool homeFromStart)
        {
            _logger = logger;
            _mowingTimePerDayList = new List<TimePerDayItem>();
            _actualMowingTimePerDayList = new List<TimePerDayItem>();
            _mowerAwayList = new List<TimePerDayItem>();

            IsLost = false;
            IsMowing = false;
            IsHome = homeFromStart;
            _isAway = !homeFromStart;
            IsStuck = false;

            if (!homeFromStart)
            {
                var startTime = _logger.LogItems.First(x => x.Type == LogType.MowControllerStarted).Time;
                LastMowerLeftTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, startTime.Minute, 0);
                LastActualMowingStartedTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, startTime.Minute, 0);
            }

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

        private TimePerDayItem GetLastMowerAwayTimePerDayItem()
        {
            if (_mowerAwayList.Count == 0)
            {
                _mowerAwayList.Add(new TimePerDayItem(CurrentLogItem.Time));
            }

            return _mowerAwayList[_mowerAwayList.Count - 1];
        }

        public double GetAverageMowingTime(DateTime iterationTime, out double mowingTimeToday)
        {
            double totalHours = 0;
            DateTime todayDate = new DateTime(iterationTime.Year, iterationTime.Month, iterationTime.Day);
            DateTime startDate = todayDate.AddDays(-5);
            DateTime date = startDate;
            int daysCount = 0;
            bool startedCounting = false;
            mowingTimeToday = 0;

            while (date <= todayDate)
            {
                var mowingTime = _mowingTimePerDayList.FirstOrDefault(mt => mt.Date.ToString("yyyy-MM-dd") == date.ToString("yyyy-MM-dd"));

                if (mowingTime != null)
                {
                    totalHours += mowingTime.SpentTime.TotalHours;
                    startedCounting = true;

                    if (date == todayDate)
                    {
                        mowingTimeToday += mowingTime.SpentTime.TotalHours;
                    }
                }

                if (startedCounting)
                {
                    daysCount++;
                }

                date = date.AddDays(1);
            }

            // If an interval has started today but not ended, then add the hours since start.
            var logItem = _logger.LogItems.OrderByDescending(x => x.Time).FirstOrDefault(x => (x.Type == LogType.MowingStarted || x.Type == LogType.MowingEnded) && x.Time.ToString("yyyy-MM-dd") == iterationTime.ToString("yyyy-MM-dd"));

            if (logItem != null && logItem.Type == LogType.MowingStarted)
            {
                totalHours += (iterationTime - logItem.Time).TotalHours;
                mowingTimeToday += (iterationTime - logItem.Time).TotalHours;
            }

            if (daysCount > 0)
            {
                return totalHours / daysCount;
            }

            return 0;
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

        public TimeSpan GetMowerAwayTimeForDay(DateTime date)
        {
            foreach (var mowerAwayTimePerDay in _mowerAwayList)
            {
                if (mowerAwayTimePerDay.Date.ToString("yyyy-MM-dd") == date.ToString("yyyy-MM-dd"))
                {
                    return mowerAwayTimePerDay.SpentTime;
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

        public void StartNewDayNowAtMidnight(DateTime date)
        {
            date = new DateTime(date.Year, date.Month, date.Day);

            if (IsAway)
            {
                var item = GetLastMowerAwayTimePerDayItem();

                if (item.Date.ToString("yyyy-MM-dd") != date.ToString("yyyy-MM-dd"))
                {
                    TimeSpan timeSinceMowerLeft = date - LastMowerLeftTime - (new TimeSpan(0, 1, 0));
                    item.AddSpentTime(timeSinceMowerLeft);
                    _mowerAwayList.Add(new TimePerDayItem(date));
                }
            }
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
                    _isActuallyMowing = value;

                    if (!_isActuallyMowing) // If just stopped mowing
                    {
                        var actualMowingTimePerDay = GetLastActuallyMowingTimePerDayItem();

                        if (actualMowingTimePerDay.Date.ToString("yyyy-MM-dd") != CurrentLogItem.Time.ToString("yyyy-MM-dd"))
                        {
                            actualMowingTimePerDay = new TimePerDayItem(CurrentLogItem.Time);
                            _actualMowingTimePerDayList.Add(actualMowingTimePerDay);
                        }

                        DateTime startTime = LastActualMowingStartedTime;
                        if (startTime.ToString("yyyy-MM-dd") != CurrentLogItem.Time.ToString("yyyy-MM-dd"))
                        {
                            startTime = new DateTime(CurrentLogItem.Time.Year, CurrentLogItem.Time.Month, CurrentLogItem.Time.Day);
                        }

                        actualMowingTimePerDay.AddSpentTime(CurrentLogItem.Time.FloorMinutes() - startTime.FloorMinutes());

                        LastActualMowingEndedTime = CurrentLogItem.Time;
                    }
                    else // If just started mowing
                    {
                        LastActualMowingStartedTime = CurrentLogItem.Time;
                    }
                }
            }
        }

        private bool _isAway;
        public bool IsAway
        {
            get
            {
                return _isAway;
            }
            private set
            {
                if (_isAway != value)
                {
                    _isAway = value;

                    if (!_isAway)
                    {
                        var mowerAwayTimePerDay = GetLastMowerAwayTimePerDayItem();

                        if (mowerAwayTimePerDay.Date.ToString("yyyy-MM-dd") != CurrentLogItem.Time.ToString("yyyy-MM-dd"))
                        {
                            mowerAwayTimePerDay = new TimePerDayItem(CurrentLogItem.Time);
                            _mowerAwayList.Add(mowerAwayTimePerDay);
                        }

                        DateTime startTime = LastMowerLeftTime;
                        if (startTime.ToString("yyyy-MM-dd") != CurrentLogItem.Time.ToString("yyyy-MM-dd"))
                        {
                            startTime = new DateTime(CurrentLogItem.Time.Year, CurrentLogItem.Time.Month, CurrentLogItem.Time.Day);
                        }

                        mowerAwayTimePerDay.AddSpentTime(CurrentLogItem.Time.FloorMinutes() - startTime.FloorMinutes());

                        LastMowerCameTime = CurrentLogItem.Time;
                    }
                    else
                    {
                        LastMowerLeftTime = CurrentLogItem.Time;
                    }
                }
            }
        }

        public bool IsHome { get; private set; }

        public bool IsStuck { get; private set; }

        private LogItem CurrentLogItem { get; set; }

        private DateTime LastMowingStartedTime { get; set; }

        private DateTime LastMowerLeftTime { get; set; }

        private DateTime LastMowerCameTime { get; set; }

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
                            IsAway = false;
                        }
                        break;
                    case LogType.MowerLeft:
                        {
                            IsHome = false;
                            IsStuck = false;
                            IsAway = true;

                            if (IsMowing)
                            {
                                IsActuallyMowing = true;
                            }
                        }
                        break;
                    case LogType.MowingStarted:
                        {
                            IsMowing = true;
                            IsStuck = false;

                            if (IsAway)
                            {
                                IsActuallyMowing = true;
                            }
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
                    case LogType.NewDay:
                        {
                            StartNewDayNowAtMidnight(CurrentLogItem.Time);
                            break;
                        }
                }
            }
        }
    }
}
