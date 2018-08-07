using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace MowControl
{
    public class SimulatedContactHomeSensor : IHomeSensor
    {
        private const decimal _cTimeFactor = 1m;
        private TimeInterval[] _timeIntervals;
        private SystemTime _systemTime;
        private IPowerSwitchConsumer _powerSwitch;

        public SimulatedContactHomeSensor(SystemTime systemTime, TimeInterval[] timeIntervals, IPowerSwitchConsumer powerSwitch)
        {
            IsHome = true;

            _systemTime = systemTime;
            _timeIntervals = timeIntervals;
            _powerSwitch = powerSwitch;

            MowerCameTime = null;
            MowerLeftTime = null;

            StartAsync();
        }

        public async Task StartAsync()
        {
            await Task.Yield();
            Run();
        }

        private TimeInterval GetCurrentInterval()
        {
            foreach (var interval in _timeIntervals)
            {
                // Om ett intervall håller på
                if (interval.ContainsTime(_systemTime.Now))
                {
                    return interval;
                }
            }

            return null;
        }

        private bool InAnyInterval
        {
            get
            {
                return GetCurrentInterval() != null;
            }
        }

        private int GetMinutesLeftInCurrentInterval()
        {
            var interval = GetCurrentInterval();

            if (interval != null)
            {
                DateTime now = new DateTime(1970, 1, 1, _systemTime.Now.Hour, _systemTime.Now.Minute, _systemTime.Now.Second);
                DateTime endTime = new DateTime(1970, 1, 1, interval.EndHour, interval.EndMin, 0);

                return Convert.ToInt32((endTime - now).TotalMinutes);
            }

            return 0;
        }

        private void Run()
        {
            Random random = new Random((int)(DateTime.Now.Ticks));

            // Wait between 1 and 5 minutes until first getaway
            int oneMinuteTicks = Convert.ToInt32((1 * 60 * 1000) * _cTimeFactor);
            int fiveMinuteTicks = Convert.ToInt32((5 * 60 * 1000) * _cTimeFactor);
            int fiftyMinuteTicks = Convert.ToInt32((50 * 60 * 1000) * _cTimeFactor);
            int sixtyMinuteTicks = Convert.ToInt32((60 * 60 * 1000) * _cTimeFactor);
            int seventyMinuteTicks = Convert.ToInt32((70 * 60 * 1000) * _cTimeFactor);
            int hunderedMinuteTicks = Convert.ToInt32((100 * 60 * 1000) * _cTimeFactor);

            System.Threading.Thread.Sleep(random.Next(oneMinuteTicks, fiveMinuteTicks));

            while (true)
            {
                // Wait while power is off or not in any interval
                while (_powerSwitch.Status != PowerStatus.On || !InAnyInterval)
                {
                    System.Threading.Thread.Sleep(random.Next(oneMinuteTicks, fiveMinuteTicks));
                }
                
                // Mower leaving
                IsHome = false;
                MowerLeftTime = _systemTime.Now;

                // Calculate the time left, but take into account the interval's end time and add some for the mower to make it back.

                // Wait between 1 hour and 1:40 hours before coming back
                int outMowingMinutes = random.Next(60, 100);

                if (outMowingMinutes > GetMinutesLeftInCurrentInterval())
                {
                    outMowingMinutes = GetMinutesLeftInCurrentInterval(); // Time to interval end
                    outMowingMinutes += random.Next(1, 20);               // Time to make it back home
                }

                int outMowingTicks = Convert.ToInt32((outMowingMinutes * 60 * 1000) * _cTimeFactor);
                System.Threading.Thread.Sleep(outMowingTicks);

                // Mower coming
                IsHome = true;
                MowerCameTime = _systemTime.Now;

                // Charge for 50 to 70 minutes
                int chargingTicks = random.Next(fiftyMinuteTicks, seventyMinuteTicks);
                System.Threading.Thread.Sleep(chargingTicks);
            }
        }

        public bool IsHome { get; private set; }

        public DateTime? MowerCameTime { get; private set; }

        public DateTime? MowerLeftTime { get; private set; }
    }
}
