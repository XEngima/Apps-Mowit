using System;
using System.Linq;
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

        public static string Version { get { return "1.0"; } }

        public MowController(
            IMowControlConfig config, 
            IPowerSwitch powerSwitch, 
            IWeatherForecast weatherForecast, 
            ISystemTime systemTime,
            IHomeSensor homeSensor,
            IMowLogger logger)
        {
            Config = config;
            PowerSwitch = powerSwitch;
            WeatherForecast = weatherForecast;
            SystemTime = systemTime;
            HomeSensor = homeSensor;
            Logger = logger;

            _mowerIsHome = HomeSensor.IsHome;
        }

        private IMowControlConfig Config { get; set; }
        private IPowerSwitch PowerSwitch { get; set; }
        private IWeatherForecast WeatherForecast { get; set; }
        private ISystemTime SystemTime { get; set; }
        private IHomeSensor HomeSensor { get; set; }
        private IMowLogger Logger { get; set; }

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
                        i.StartHour > SystemTime.Now.Hour ||
                        (i.StartHour == SystemTime.Now.Hour && i.StartMin >= SystemTime.Now.Minute));

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
                        i.StartHour < SystemTime.Now.Hour ||
                        (i.StartHour == SystemTime.Now.Hour && i.StartMin <= SystemTime.Now.Minute));

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
                    DateTime startTime = new DateTime(SystemTime.Now.Year, SystemTime.Now.Month, SystemTime.Now.Day, interval.StartHour, interval.StartMin, 0);
                    DateTime endTime = new DateTime(SystemTime.Now.Year, SystemTime.Now.Month, SystemTime.Now.Day, interval.EndHour, interval.EndMin, 0);

                    if (SystemTime.Now >= startTime && SystemTime.Now < endTime)
                    {
                        return interval;
                    }
                }

                return NextInterval;
            }
        }

        /// <summary>
        /// Gets next interval's start time.
        /// </summary>
        public DateTime NextIntervalStartTime { get
            {
                DateTime nextIntervalStartTime = new DateTime(SystemTime.Now.Year, SystemTime.Now.Month, SystemTime.Now.Day, NextInterval.StartHour, NextInterval.StartMin, 0);
                if (nextIntervalStartTime < SystemTime.Now)
                {
                    nextIntervalStartTime = nextIntervalStartTime.AddDays(1);
                }

                return nextIntervalStartTime;
            }
        }

        public async Task StartAsync()
        {
            await Task.Yield();
            Run();
        }

        private void Run()
        {
            if (Config.UsingContactHomeSensor && HomeSensor is TimeBasedHomeSensor)
            {
                throw new InvalidOperationException("The time based home sensor cannot act as a contact home sensor. If the time based home sensor is used, please set option UseContactHomeSensor to false.");
            }

            Logger.Write(SystemTime.Now, LogType.MowControllerStarted, $"Mow Controller {Version} started.");
            _mowerIsHome = HomeSensor.IsHome;

            try
            {
                while (true)
                {
                    CheckAndAct();
                    Thread.Sleep(60000);
                }
            }
            catch (Exception ex)
            {
                Logger.Write(SystemTime.Now, LogType.Failure, ex.ToString());
            }
        }

        /// <summary>
        /// Kontrollerar om klippning är nödvändigt, dvs. om antalet timmar per dag redan är uppfyllt sedan
        /// servicen startade.
        /// </summary>
        /// <returns>true om klippning är nödvändig, annars false.</returns>
        private bool MowingNecessary()
        {
            double workHours = 0d;
            double hoursOverSchedule = 0d;
            bool powerIsOn = false;
            double totalWorkHours = 0d;
            double totalHours = 0d;

            var mowStartedLogItem = Logger.LogItems
                .OrderByDescending(i => i.Time)
                .FirstOrDefault(i => i.Type == LogType.MowControllerStarted);

            DateTime now = SystemTime.Now;
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

        /// <summary>
        /// Gets the time when the mower came. SystemTime.Now if the mower has not came back or if the time is unknown.
        /// </summary>
        private DateTime MowerCameTime
        {
            get
            {
                if (HomeSensor.IsHome)
                {
                    var mowerLeftLogItem = Logger.LogItems
                        .OrderByDescending(x => x.Time)
                        .FirstOrDefault(x => x.Type == LogType.MowerLeft);

                    var mowerCameLogItem = Logger.LogItems
                        .OrderByDescending(x => x.Time)
                        .FirstOrDefault(x => x.Type == LogType.MowerCame);

                    if (mowerCameLogItem != null)
                    {
                        if (mowerLeftLogItem == null || mowerLeftLogItem.Time < mowerCameLogItem.Time)
                        {
                            return mowerCameLogItem.Time;
                        }
                    }
                }

                return SystemTime.Now;
            }
        }

        /// <summary>
        /// Kollar om strömmen ska slås på eller av och gör det i sådana fall.
        /// </summary>
        public void CheckAndAct()
        {
            if (SystemTime.Now.ToString("HH:mm") == "00:04")
            {
                int debug = 0;
            }

            if (Config.TimeIntervals == null)
            {
                throw new InvalidOperationException();
            }

            bool betweenIntervals = true;
            foreach (var interval in Config.TimeIntervals)
            {
                // Om ett intervall håller på
                if (interval.ContainsTime(SystemTime.Now))
                {
                    betweenIntervals = false;
                }
            }

            bool beforeIntervalStart = false;
            bool safelyAfterIntervalEnd = false;

            if (betweenIntervals)
            {
                beforeIntervalStart = SystemTime.Now >= NextIntervalStartTime.AddMinutes(-5);

                DateTime minutesFromEnd = (new DateTime(SystemTime.Now.Year, SystemTime.Now.Month, SystemTime.Now.Day, PrevInterval.EndHour, PrevInterval.EndMin, 0).AddMinutes(5));

                if (PrevInterval.StartHour > SystemTime.Now.Hour || PrevInterval.StartHour == SystemTime.Now.Hour && PrevInterval.StartMin > SystemTime.Now.Minute)
                {
                    minutesFromEnd = minutesFromEnd.AddDays(-1);
                }

                safelyAfterIntervalEnd = SystemTime.Now >= minutesFromEnd;
            }

            try
            {
                // Check if mower has entered or exited its home since last time
                if (_mowerIsHome != HomeSensor.IsHome)
                {
                    _mowerIsHome = HomeSensor.IsHome;

                    if (_mowerIsHome)
                    {
                        Logger.Write(SystemTime.Now, LogType.MowerCame, "Mower came home.");
                    }
                    else
                    {
                        Logger.Write(SystemTime.Now, LogType.MowerLeft, "Mower left.");
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

                    if (lastMowerLeftLogItem?.Time.AddHours(Config.MaxMowingHoursWithoutCharge) < SystemTime.Now && lastLogItem?.Type != LogType.MowerLost)
                    {
                        Logger.Write(SystemTime.Now, LogType.MowerLost, "Mower seems to be lost. It did not return home as expected.");
                    }
                }

                // If power is turned off

                if (PowerSwitch.Status != PowerStatus.On)
                {
                    foreach (var interval in Config.TimeIntervals)
                    {
                        // Om ett intervall håller på
                        if (interval.ContainsTime(SystemTime.Now))
                        {
                            DateTime minutesFromEnd = (new DateTime(SystemTime.Now.Year, SystemTime.Now.Month, SystemTime.Now.Day, interval.EndHour, interval.EndMin, 0).AddMinutes(-10));

                            // If the interval is not close to end
                            if (SystemTime.Now < minutesFromEnd)
                            {
                                int forecastHours = interval.EndHour - SystemTime.Now.Hour + 2;
                                string weatherAheadDescription;

                                if (WeatherForecast.CheckIfWeatherWillBeGood(forecastHours, out weatherAheadDescription) && MowingNecessary())
                                {
                                    PowerSwitch.TurnOn();
                                    Logger.Write(SystemTime.Now, LogType.PowerOn, "Power was turned on. " + weatherAheadDescription);
                                }
                            }
                        }
                    }

                    if (betweenIntervals && HomeSensor.IsHome && !beforeIntervalStart && safelyAfterIntervalEnd)
                    {
                        PowerSwitch.TurnOn();
                        Logger.Write(SystemTime.Now, LogType.PowerOn, "Power was turned on. In between intervals.");
                    }
                }

                // If power is turned on

                if (PowerSwitch.Status != PowerStatus.Off)
                {
                    if (HomeSensor.IsHome) // TODO: Fel här när kraften är Unknown...
                    {
                        DateTime nextIntervalExactStartTime = new DateTime(SystemTime.Now.Year, SystemTime.Now.Month, SystemTime.Now.Day, NextInterval.StartHour, NextInterval.StartMin, 0);
                        if (nextIntervalExactStartTime < SystemTime.Now)
                        {
                            nextIntervalExactStartTime = nextIntervalExactStartTime.AddDays(1);
                        }
                        double minutesLeftToIntervalStart = (nextIntervalExactStartTime - SystemTime.Now).TotalMinutes;

                        // If there will be rain, turn off power
                        int forecastHours = NextInterval.ToTimeSpan().Hours + 2;

                        // If a contact home sensor is used, weather can be checked for much smaller time spans
                        if (Config.UsingContactHomeSensor)
                        {
                            forecastHours = Config.MaxMowingHoursWithoutCharge + 1;
                        }

                        if (minutesLeftToIntervalStart <= 5 || !betweenIntervals && HomeSensor.IsHome && (SystemTime.Now - MowerCameTime).TotalMinutes >= 30 || PowerSwitch.Status == PowerStatus.Unknown)
                        {
                            string weatherAheadDescription;

                            if (!WeatherForecast.CheckIfWeatherWillBeGood(forecastHours, out weatherAheadDescription))
                            {
                                PowerSwitch.TurnOff();
                                Logger.Write(SystemTime.Now, LogType.PowerOff, "Power was turned off. " + weatherAheadDescription);
                            }

                            // If mowing not necessary, turn off power
                            if (!MowingNecessary())
                            {
                                PowerSwitch.TurnOff();
                                Logger.Write(SystemTime.Now, LogType.PowerOff, "Power was turned off. Mowing not necessary.");
                            }
                        }
                    }
                }
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
                    Logger.Write(SystemTime.Now, LogType.Failure, ex.Message);
                }
            }
        }
    }
}
