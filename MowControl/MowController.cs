using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MowControl
{
    /// <summary>
    /// Klass som kontrollerar robotgräsklipparens schema och körning.
    /// </summary>
    public class MowController
    {
        private bool _mowerIsHome;

        public static string Version { get { return "1.27"; } }

        public MowController(
            IMowControlConfig config,
            IPowerSwitch powerSwitch,
            IWeatherForecast weatherForecast,
            ISystemTime systemTime,
            IHomeSensor homeSensor,
            IMowLogger logger,
            IRainSensor rainSensor)
        {
            Config = config;
            PowerSwitch = powerSwitch;
            WeatherForecast = weatherForecast;
            SystemTime = systemTime;
            HomeSensor = homeSensor;
            Logger = logger;
            RainSensor = rainSensor;

            _mowerIsHome = HomeSensor.IsHome;
        }

        private IMowControlConfig Config { get; set; }
        private IPowerSwitch PowerSwitch { get; set; }
        private IWeatherForecast WeatherForecast { get; set; }
        private ISystemTime SystemTime { get; set; }
        private IHomeSensor HomeSensor { get; set; }
        private IMowLogger Logger { get; set; }
        private IRainSensor RainSensor { get; set; }

        /// <summary>
        /// Hämtar nästa intervall. Om mitt i ett intervall hämtas intervallet efter det.
        /// </summary>
        public TimeInterval NextInterval
        {
            get
            {
                // Get next interval
                var nextInterval = Config.TimeIntervals
                    .OrderBy(i => i.StartHour)
                    .ThenBy(i => i.StartMin)
                    .FirstOrDefault(i =>
                        i.StartHour > IterationTime.Hour ||
                        (i.StartHour == IterationTime.Hour && i.StartMin >= IterationTime.Minute));

                // Om vi fick null betyder det att tiden passerat sista intervallet, och då blir nästa intervall 
                // istället det första på dagen.
                if (nextInterval == null)
                {
                    nextInterval = Config.TimeIntervals
                        .OrderBy(i => i.StartHour)
                        .ThenBy(i => i.StartMin)
                        .FirstOrDefault();
                }

                return nextInterval;
            }
        }

        /// <summary>
        /// Hämtar föregående intervall. Om mitt i ett intervall hämtas intervallet efter det.
        /// </summary>
        public TimeInterval PrevInterval
        {
            get
            {
                // Get prev interval
                var prevInterval = Config.TimeIntervals
                    .OrderByDescending(i => i.StartHour)
                    .ThenByDescending(i => i.StartMin)
                    .FirstOrDefault(i =>
                        i.StartHour < IterationTime.Hour ||
                        (i.StartHour == IterationTime.Hour && i.StartMin <= IterationTime.Minute));

                // Om vi fick null betyder det att tiden passerat sista intervallet, och då blir föregående intervall 
                // istället det sista på dagen.
                if (prevInterval == null)
                {
                    prevInterval = Config.TimeIntervals
                        .OrderByDescending(i => i.StartHour)
                        .ThenByDescending(i => i.StartMin)
                        .FirstOrDefault();
                }

                return prevInterval;
            }
        }

        /// <summary>
        /// Gets next interval. An interval currently active is returned.
        /// </summary>
        public TimeInterval NextOrCurrentInterval
        {
            get
            {
                // Kolla om vi är i ett intervall, och returnera det isf
                foreach (var interval in Config.TimeIntervals)
                {
                    //DateTime startTime = new DateTime(IterationTime.Year, IterationTime.Month, IterationTime.Day, interval.StartHour, interval.StartMin, 0);
                    //DateTime endTime = new DateTime(IterationTime.Year, IterationTime.Month, IterationTime.Day, interval.EndHour, interval.EndMin, 0);

                    if (interval.ContainsTime(IterationTime))
                    {
                        return interval;
                    }
                    //if (IterationTime >= startTime && IterationTime <= endTime)
                    //{
                    //    return interval;
                    //}
                }

                return NextInterval;
            }
        }

        /// <summary>
        /// Gets next interval's start time.
        /// </summary>
        public DateTime NextIntervalStartTime
        {
            get
            {
                DateTime nextIntervalStartTime = new DateTime(IterationTime.Year, IterationTime.Month, IterationTime.Day, NextInterval.StartHour, NextInterval.StartMin, 0);
                if (nextIntervalStartTime < IterationTime)
                {
                    nextIntervalStartTime = nextIntervalStartTime.AddDays(1);
                }

                return nextIntervalStartTime;
            }
        }

        public async Task StartAsync()
        {
            if (Config.UsingContactHomeSensor && HomeSensor is TimeBasedHomeSensor)
            {
                throw new InvalidOperationException("The time based home sensor cannot act as a contact home sensor. If the time based home sensor is used, please set option UseContactHomeSensor to false.");
            }

            Logger.Write(SystemTime.Now, LogType.MowControllerStarted, LogLevel.InfoMoreInteresting, $"Mow Controller {Version} started.");
            _mowerIsHome = HomeSensor.IsHome;

            try
            {
                while (true)
                {
                    await CheckAndActAsync();
                    Thread.Sleep(60000);
                }
            }
            catch (Exception ex)
            {
                Logger.Write(SystemTime.Now, LogType.Failure, LogLevel.Fatal, ex.ToString());
            }
        }

        /// <summary>
        /// Kontrollerar om klippning är nödvändigt, dvs. om antalet timmar per dag redan är uppfyllt sedan
        /// servicen startade.
        /// </summary>
        /// <returns>true om klippning är nödvändig, annars false.</returns>
        private bool MowingNecessary()
        {
            if (Config.UsingContactHomeSensor)
            {
                return true;
            }

            double workHours = 0d;
            double hoursOverSchedule = 0d;
            bool powerIsOn = false;
            double totalWorkHours = 0d;
            double totalHours = 0d;

            var mowStartedLogItem = Logger.LogItems
                .OrderByDescending(i => i.Time)
                .FirstOrDefault(i => i.Type == LogType.MowControllerStarted);

            DateTime now = IterationTime;
            DateTime floorNow = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            DateTime dateTime = mowStartedLogItem.Time;
            DateTime floorDate = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0);

            while (floorDate <= now)
            {
                // Räkna bara de senaste 7 dagarna.
                if (floorDate <= floorNow.AddDays(-7))
                {
                    hoursOverSchedule = 0;
                    totalWorkHours = 0;
                }

                workHours = 0;

                foreach (var interval in Config.TimeIntervals.OrderBy(ti => ti.StartHour).ThenBy(ti => ti.StartMin))
                {
                    DateTime intervalStartTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, interval.StartHour, interval.StartMin, 0);

                    if (intervalStartTime < now)
                    {
                        // Hämta senaste loggmeddelandet
                        var lastLogItem = Logger.LogItems
                            .Where(i => i.Time <= intervalStartTime && (i.Type == LogType.MowControllerStarted || i.Type == LogType.PowerOn || i.Type == LogType.PowerOff))
                            .OrderByDescending(i => i.Time)
                            .ThenByDescending(i => i.Type)
                            .FirstOrDefault();

                        if (lastLogItem != null)
                        {
                            powerIsOn = lastLogItem.Type == LogType.PowerOn;
                        }

                        if (powerIsOn)
                        {
                            workHours = interval.ToTimeSpan().TotalHours;
                            totalWorkHours += workHours;
                            //hoursOverSchedule = workHours - (Config.AverageWorkPerDayHours * totalWorkHours) / 24d;
                        }
                    }
                }

                //if (workHours > 0)
                //{
                //    hoursOverSchedule += (workHours - Config.AverageWorkPerDayHours);
                //}

                dateTime = dateTime.AddDays(1);
                floorDate = floorDate.AddDays(1);
            }

            bool mowingNecessary = true;

            if (totalWorkHours > 0)
            {
                DateTime nextIntervalStartTime = new DateTime(now.Year, now.Month, now.Day, NextOrCurrentInterval.StartHour, NextOrCurrentInterval.StartMin, 0);
                totalHours = (nextIntervalStartTime - mowStartedLogItem.Time).TotalHours;
                double nextIntervalHours = NextOrCurrentInterval.ToTimeSpan().TotalHours;
                mowingNecessary = ((totalWorkHours - nextIntervalHours) / totalHours) < (Config.AverageWorkPerDayHours / 24d);
            }

            // Gör en extra koll på vädret

            if (!WeatherForecast.CheckIfWeatherWillBeGood(48))
            {
                mowingNecessary = true;
            }

            return mowingNecessary;
        }

        ///// <summary>
        ///// Gets the time when the mower came. SystemTime.Now if the mower has not came back or if the time is unknown.
        ///// </summary>
        //private DateTime? MowerCameTime
        //{
        //    get
        //    {
        //        return HomeSensor.MowerCameTime;

        //        //if (HomeSensor.IsHome)
        //        //{
        //        //    var mowerLeftLogItem = Logger.LogItems
        //        //        .OrderByDescending(x => x.Time)
        //        //        .FirstOrDefault(x => x.Type == LogType.MowerLeft);

        //        //    var mowerCameLogItem = Logger.LogItems
        //        //        .OrderByDescending(x => x.Time)
        //        //        .FirstOrDefault(x => x.Type == LogType.MowerCame);

        //        //    if (mowerCameLogItem != null)
        //        //    {
        //        //        if (mowerLeftLogItem == null || mowerLeftLogItem.Time < mowerCameLogItem.Time)
        //        //        {
        //        //            return mowerCameLogItem.Time;
        //        //        }
        //        //    }
        //        //}

        //        //return SystemTime.Now;
        //    }
        //}

        /// <summary>
        /// Returns whether the current system time is between (or not in) time intervals.
        /// </summary>
        private bool BetweenIntervals
        {
            get
            {
                bool betweenIntervals = true;
                foreach (var interval in Config.TimeIntervals)
                {
                    // Om ett intervall håller på
                    if (interval.ContainsTime(IterationTime))
                    {
                        betweenIntervals = false;
                    }
                }

                return betweenIntervals;
            }
        }

        private DateTime IterationTime { get; set; }

        /// <summary>
        /// Get whether the time is right before an interval starts or not.
        /// </summary>
        private bool RightBeforeIntervalStarts
        {
            get
            {
                bool rightBeforeIntervalStart = false;

                rightBeforeIntervalStart = IterationTime >= NextIntervalStartTime.AddMinutes(-5);

                return rightBeforeIntervalStart;
            }
        }

        /// <summary>
        /// Get whether the time is right before an interval starts or not.
        /// </summary>
        private bool SafelyAfterIntervalEnd
        {
            get
            {
                bool safelyAfterIntervalEnd = false;

                if (BetweenIntervals)
                {
                    DateTime minutesFromEnd = (new DateTime(IterationTime.Year, IterationTime.Month, IterationTime.Day, PrevInterval.EndHour, PrevInterval.EndMin, 0).AddMinutes(5));

                    if (PrevInterval.StartHour > IterationTime.Hour || PrevInterval.StartHour == IterationTime.Hour && PrevInterval.StartMin > IterationTime.Minute)
                    {
                        minutesFromEnd = minutesFromEnd.AddDays(-1);
                    }

                    safelyAfterIntervalEnd = IterationTime >= minutesFromEnd;
                }

                return safelyAfterIntervalEnd;
            }
        }

        /// <summary>
        /// Kollar om strömmen ska slås på eller av och gör det i sådana fall.
        /// </summary>
        public async Task CheckAndActAsync()
        {
            await Task.Yield();
            CheckAndAct();
        }

        private bool _isActing = false;

        /// <summary>
        /// Kollar om strömmen ska slås på eller av och gör det i sådana fall.
        /// </summary>
        public void CheckAndAct()
        {
            if (_isActing)
            {
                Logger.Write(SystemTime.Now, LogType.Debug, LogLevel.Debug, "Last CheckAndAct was not finished.");
                return;
            }

            _isActing = true;

            //if (SystemTime.Now.ToString("HH:mm") == "00:04")
            //{
            //    int debug = 0;
            //}

            IterationTime = SystemTime.Now;

            if (Config.TimeIntervals == null)
            {
                throw new InvalidOperationException();
            }

            try
            {
                // Write the hourly report log item

                if (IterationTime.Minute == 0)
                {
                    var hourlyReportItem = Logger.LogItems.OrderByDescending(r => r.Time).FirstOrDefault(x => x.Type == LogType.HourlyReport);

                    if (hourlyReportItem == null || hourlyReportItem.Time.ToString("yyyy-MM-dd HH:mm") != IterationTime.ToString("yyyy-MM-dd HH:mm"))
                    {
                        int wetness = RainSensor is SmhiRainSensor ? ((SmhiRainSensor)RainSensor).Wetness : 0;
                        string weatherAheadDescription;
                        WeatherForecast.CheckIfWeatherWillBeGood(12, out weatherAheadDescription);
                        Logger.Write(IterationTime, LogType.HourlyReport, LogLevel.InfoLessInteresting, "Hourly report: " + weatherAheadDescription + " Current wetness: " + wetness);
                   }
                }

                // If a report was not made for yesterday, and if mowing started yesterday or before, create a report.

                var startLogItem = Logger.LogItems.FirstOrDefault(x => x.Type == LogType.MowControllerStarted);
                var todayStartTime = new DateTime(IterationTime.Year, IterationTime.Month, IterationTime.Day, 0, 0, 0);

                //if (IterationTime.Hour == 0)
                //{
                //    string sDebug = "startLogItem:" + startLogItem?.Time.ToString("yyyy-MM-dd HH:mm") + ";";
                //    Logger.Write(IterationTime, LogType.Debug, LogLevel.Debug, "Debug1: " + sDebug);
                //}

                if (startLogItem.Time < todayStartTime)
                {
                    var yesterdayStartTime = new DateTime(IterationTime.Year, IterationTime.Month, IterationTime.Day, 0, 0, 0).AddDays(-1);

                    var reportLogItem = Logger.LogItems.FirstOrDefault(x => x.Type == LogType.DailyReport && x.Time >= todayStartTime && x.Time < todayStartTime.AddDays(1));

                    //if (IterationTime.Hour == 0)
                    //{
                    //    string sDebug = "reportLogItem:" + reportLogItem?.Time.ToString("yyyy-MM-dd HH:mm") + ";";
                    //    Logger.Write(IterationTime, LogType.Debug, LogLevel.Debug, "Debug2: " + sDebug);
                    //}

                    if (reportLogItem == null)
                    {
                        var mowingLogItems = Logger.LogItems.Where(x => (x.Type == LogType.MowingStarted || x.Type == LogType.MowingEnded) && x.Time >= yesterdayStartTime && x.Time < todayStartTime);

                        var sb = new StringBuilder();
                        sb.AppendLine("Mowing Summary " + yesterdayStartTime.ToString("yyyy-MM-dd"));
                        sb.AppendLine();
                        DateTime startTime = DateTime.MinValue;
                        var mowingTime = new TimeSpan();

                        foreach (LogItem mowingLogItem in mowingLogItems)
                        {
                            sb.Append(mowingLogItem.Time.ToString("HH:mm"));
                            sb.Append(" ");
                            sb.AppendLine(mowingLogItem.Message);

                            if (mowingLogItem.Type == LogType.MowingStarted)
                            {
                                startTime = mowingLogItem.Time;
                            }
                            else
                            {
                                mowingTime += (mowingLogItem.Time - startTime);
                            }
                        }

                        sb.AppendLine();
                        sb.Append("Total mowed: ");
                        sb.Append(Math.Floor(mowingTime.TotalHours));
                        sb.Append(":");
                        sb.Append(mowingTime.Minutes);
                        sb.AppendLine(" hours.");

                        //if (IterationTime.Hour == 0)
                        //{
                        //    string sDebug = "mowingLogItems.Count:" + mowingLogItems.Count() + ";";
                        //    Logger.Write(IterationTime, LogType.Debug, LogLevel.Debug, "Debug: " + sDebug);
                        //}

                        Logger.Write(IterationTime, LogType.DailyReport, LogLevel.InfoMoreInteresting, sb.ToString());
                    }
                }

                // Check if there has ocurred an interval start, and in case it has, write a log message

                if (!BetweenIntervals && PowerSwitch.Status == PowerStatus.On && NextOrCurrentInterval.StartHour == IterationTime.Hour && NextOrCurrentInterval.StartMin == IterationTime.Minute)
                {
                    SetMowingStarted();
                }

                // Check if mower has entered or exited its home since last time

                if (_mowerIsHome != HomeSensor.IsHome)
                {
                    _mowerIsHome = HomeSensor.IsHome;

                    if (_mowerIsHome)
                    {
                        Logger.Write(IterationTime, LogType.MowerCame, LogLevel.Info, "Mower came.");
                        SetMowingStarted();
                    }
                    else
                    {
                        Logger.Write(IterationTime, LogType.MowerLeft, LogLevel.Info, "Mower left.");
                    }
                }

                // Check if mower is lost, but only if contact sensor is used.

                if (Config.UsingContactHomeSensor && !_mowerIsHome)
                {
                    var lastMowerLeftLogItem = Logger.LogItems
                        .OrderByDescending(x => x.Time)
                        .FirstOrDefault(x => x.Type == LogType.MowerLeft);

                    var lastLogItem = Logger.LogItems
                        .OrderByDescending(x => x.Time)
                        .FirstOrDefault(x => x.Type == LogType.MowerLost || x.Type == LogType.MowerCame);

                    if (lastMowerLeftLogItem?.Time.AddHours(Config.MaxMowingHoursWithoutCharge) <= IterationTime && lastLogItem?.Type != LogType.MowerLost)
                    {
                        Logger.Write(IterationTime, LogType.MowerLost, LogLevel.InfoMoreInteresting, $"Mower seems to be lost. It did not return home after {Config.MaxMowingHoursWithoutCharge} hours as expected.");

                        if (!BetweenIntervals)
                        {
                            SetMowingEnded();
                        }
                    }
                }

                // Turn on power

                if (PowerSwitch.Status != PowerStatus.On && !RainSensor.IsWet)
                {
                    foreach (var interval in Config.TimeIntervals)
                    {
                        // Om ett intervall håller på
                        if (interval.ContainsTime(IterationTime))
                        {
                            DateTime minutesFromEnd = (new DateTime(IterationTime.Year, IterationTime.Month, IterationTime.Day, interval.EndHour, interval.EndMin, 0).AddMinutes(-10));

                            // If the interval is not close to end
                            if (IterationTime < minutesFromEnd)
                            {
                                int forecastHours = interval.EndHour - IterationTime.Hour + 2;

                                if (Config.UsingContactHomeSensor)
                                {
                                    forecastHours = Config.MaxMowingHoursWithoutCharge + 1;
                                }

                                string weatherAheadDescription;

                                bool weatherWillBeGood = WeatherForecast.CheckIfWeatherWillBeGood(forecastHours, out weatherAheadDescription);
                                bool mowingNecessary = MowingNecessary();
                                if (weatherWillBeGood && mowingNecessary)
                                {
                                    PowerSwitch.TurnOn();
                                    Logger.Write(IterationTime, LogType.PowerOn, LogLevel.Info, "Power was turned on. " + weatherAheadDescription);
                                    SetMowingStarted();
                                }
                            }
                        }
                    }

                    if (BetweenIntervals && !RightBeforeIntervalStarts && SafelyAfterIntervalEnd)
                    {
                        PowerSwitch.TurnOn();
                        Logger.Write(IterationTime, LogType.PowerOn, LogLevel.Info, "Power was turned on. In between intervals.");
                    }
                }

                // Turn off power

                if (PowerSwitch.Status != PowerStatus.Off)
                {
                    if (HomeSensor.IsHome) // TODO: Fel i TimeBasedHomeSensor när kraften är Unknown...
                    {
                        DateTime nextIntervalExactStartTime = new DateTime(IterationTime.Year, IterationTime.Month, IterationTime.Day, NextInterval.StartHour, NextInterval.StartMin, 0);
                        if (nextIntervalExactStartTime < IterationTime)
                        {
                            nextIntervalExactStartTime = nextIntervalExactStartTime.AddDays(1);
                        }
                        double minutesLeftToIntervalStart = (nextIntervalExactStartTime - IterationTime).TotalMinutes;

                        // If there will be rain, turn off power
                        int forecastHours = NextInterval.ToTimeSpan().Hours + 2;

                        // If a contact home sensor is used, weather can be checked for much smaller time spans
                        if (Config.UsingContactHomeSensor)
                        {
                            forecastHours = Config.MaxMowingHoursWithoutCharge + 1;
                        }

                        if (minutesLeftToIntervalStart <= 5 || !BetweenIntervals && HomeSensor.IsHome && (IterationTime - HomeSensor.MowerCameTime).TotalMinutes >= 30 || PowerSwitch.Status == PowerStatus.Unknown)
                        {
                            string logMessage;
                            bool weatherWillBeGood = WeatherForecast.CheckIfWeatherWillBeGood(forecastHours, out logMessage);

                            if (weatherWillBeGood && RainSensor.IsWet)
                            {
                                logMessage = "Grass is wet.";
                            }

                            if (RainSensor.IsWet || !weatherWillBeGood)
                            {
                                PowerSwitch.TurnOff();
                                Logger.Write(IterationTime, LogType.PowerOff, LogLevel.Info, "Power was turned off. " + logMessage);

                                if (!BetweenIntervals)
                                {
                                    SetMowingEnded();
                                }
                            }

                            // If mowing not necessary, turn off power
                            if (!MowingNecessary())
                            {
                                PowerSwitch.TurnOff();
                                Logger.Write(IterationTime, LogType.PowerOff, LogLevel.Info, "Power was turned off. Mowing not necessary.");

                                if (!BetweenIntervals)
                                {
                                    SetMowingEnded();
                                }
                            }
                        }
                    }
                }

                // Check if we're at an interval end, and in case we are, write a log message

                if (IterationTime.Hour == 23 && IterationTime.Minute == 59)
                {
                    string sDebug = "BetweenIntervals:" + BetweenIntervals + ";";
                    sDebug += "PStatus:" + PowerSwitch.Status + ";";
                    sDebug += "IEndHour:" + NextOrCurrentInterval.EndHour + ";";
                    sDebug += "IEndMin:" + NextOrCurrentInterval.EndMin + ";";

                    Logger.Write(IterationTime, LogType.Debug, LogLevel.Debug, "Time is 23:59. " + sDebug);
                }

                if (!BetweenIntervals && PowerSwitch.Status == PowerStatus.On && NextOrCurrentInterval.EndHour == IterationTime.Hour && NextOrCurrentInterval.EndMin == IterationTime.Minute)
                {
                    SetMowingEnded();
                }

                _isActing = false;
            }
            catch (Exception ex)
            {
                string lastMsg = "";

                if (Logger.LogItems.Count > 0)
                {
                    lastMsg = Logger.LogItems[Logger.LogItems.Count - 1].Message;
                }

                if (ex.Message != lastMsg)
                {
                    Logger.Write(IterationTime, LogType.Failure, LogLevel.Error, ex.Message);
                }
            }
        }

        private void SetMowingStarted()
        {
            if (!BetweenIntervals && PowerSwitch.Status == PowerStatus.On)
            {
                // Check if last item as a MowinEnded item.
                var logItems = Logger.LogItems
                    .Where(x => x.Type == LogType.MowingStarted || x.Type == LogType.MowingEnded)
                    .OrderByDescending(x => x.Time)
                    .ToList();

                if (logItems.Count == 0 || logItems[0].Type == LogType.MowingEnded)
                {
                    Logger.Write(IterationTime, LogType.MowingStarted, LogLevel.InfoLessInteresting, "Mowing started.");
                }
            }
        }

        private void SetMowingEnded()
        {
            // Check if last item as a MowingStarted item.
            var logItems = Logger.LogItems
                .Where(x => x.Type == LogType.MowingStarted || x.Type == LogType.MowingEnded)
                .OrderByDescending(x => x.Time)
                .ToList();

            if (logItems.Count == 0 || logItems[0].Type == LogType.MowingStarted)
            {
                Logger.Write(IterationTime, LogType.MowingEnded, LogLevel.InfoLessInteresting, "Mowing ended.");
            }
        }
    }
}
