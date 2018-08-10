using MowControl;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace MowerTests
{
    public class TestWeatherForecast : IWeatherForecast
    {
        private IList<WeatherPeriod> _weatherPeriods = new List<WeatherPeriod>();
        private ISystemTime _systemTime;
        private bool _throwException;
        private DateTime _lastRainTime;

        /// <summary>
        /// Gets a text describing the weather ahead. Set when CheckIfWeatherWillBeGood is executed.
        /// </summary>
        public string WeatherAheadDescription { get; private set; }

        public TestWeatherForecast(bool expectingGoodWeather, ISystemTime systemTime)
        {
            _weatherPeriods.Add(new WeatherPeriod(new DateTime(1970, 1, 1), expectingGoodWeather));
            _systemTime = systemTime;
            _throwException = false;
            _lastRainTime = DateTime.MinValue;
        }

        public void SetFailureAndThrowException(bool throwException)
        {
            _throwException = throwException;
        }

        public void SetLastRainTime(DateTime lastRainTime)
        {
            _lastRainTime = lastRainTime;
        }

        /// <summary>
        /// Kollar om vädret de närmaste timmarna förväntas vara bra.
        /// </summary>
        /// <param name="hours">Antalet timmar fram att kontrollera.</param>
        /// <returns>true om vädret de närmaste timmarna förväntas bli bra, annars false.</returns>
        public bool CheckIfWeatherWillBeGood(int hours, out string weatherAheadDescription)
        {
            weatherAheadDescription = "";

            if (_throwException)
            {
                throw new WeatherException("Failed to contact weather service.");
            }

            var weatherPeriod = _weatherPeriods
                .Where(wp => wp.Time <= _systemTime.Now.AddHours(hours))
                .OrderByDescending(wp => wp.Time)
                .FirstOrDefault();

            return weatherPeriod.GoodWeather;
        }

        public bool CheckIfWeatherWillBeGood(int hours)
        {
            string weatherAheadDescription;
            return CheckIfWeatherWillBeGood(hours, out weatherAheadDescription);
        }

        public void SetExpectation(bool expectingGoodWeather)
        {
            _weatherPeriods = new List<WeatherPeriod>();
            _weatherPeriods.Add(new WeatherPeriod(new DateTime(1970, 1, 1), expectingGoodWeather));
        }

        public void AddExpectation(bool expectingGoodWeather, DateTime fromTime)
        {
            _weatherPeriods.Add(new WeatherPeriod(fromTime, expectingGoodWeather));
        }
    }

    public class WeatherPeriod
    {
        public WeatherPeriod(DateTime time, bool goodWeather)
        {
            Time = time;
            GoodWeather = goodWeather;
        }

        public DateTime Time { get; }

        public bool GoodWeather { get; }
    }
}
