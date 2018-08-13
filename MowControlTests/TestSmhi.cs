using MowControl;
using SmhiWeather;
using System;
using System.Collections.Generic;
using System.Text;

namespace MowerTests
{
    public class TestSmhi : ISmhi
    {
        private ForecastTimeSerie[] _forecastTimeSeries;

        public TestSmhi(ISystemTime systemTime, ForecastTimeSerie[] forecastTimeSeries)
        {
            SystemTime = systemTime;
            _forecastTimeSeries = forecastTimeSeries;
        }

        private ISystemTime SystemTime { get; set; }

        public decimal CoordLat => throw new NotImplementedException();

        public decimal CoordLon => throw new NotImplementedException();

        public ForecastTimeSerie GetCurrentWeather()
        {
            foreach (var timeSerie in _forecastTimeSeries)
            {
                if (SystemTime.Now >= timeSerie.ValidTimeLocal && SystemTime.Now < timeSerie.ValidTimeLocal.AddHours(1))
                {
                    return timeSerie;
                }
            }

            return null;
        }

        public Forecast GetForecast()
        {
            throw new NotImplementedException();
        }
    }
}
