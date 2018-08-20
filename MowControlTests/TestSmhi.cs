using MowControl;
using SmhiWeather;
using System;
using System.Collections.Generic;
using System.Text;

namespace MowerTests
{
    public class TestSmhi : ISmhi
    {
        private List<ForecastTimeSerie> _forecastTimeSeries;

        public TestSmhi(ISystemTime systemTime, ForecastTimeSerie[] forecastTimeSeries)
        {
            SystemTime = systemTime;
            _forecastTimeSeries = new List<ForecastTimeSerie>();
            _forecastTimeSeries.AddRange(forecastTimeSeries);
        }

        private ISystemTime SystemTime { get; set; }

        public decimal CoordLat => throw new NotImplementedException();

        public decimal CoordLon => throw new NotImplementedException();

        /// <summary>
        /// Updates an existing time serie.
        /// </summary>
        /// <param name="timeSerie"></param>
        public void UpdateTimeSerie(ForecastTimeSerie timeSerie)
        {
            for (int i = 0; i < _forecastTimeSeries.Count; i++)
            {
                if (_forecastTimeSeries[i].validTime.ToString("yyyy-MM-dd HH:mm") == timeSerie.validTime.ToString("yyyy-MM-dd HH:mm"))
                {
                    _forecastTimeSeries[i] = timeSerie;
                    break;
                }
            }
        }

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
