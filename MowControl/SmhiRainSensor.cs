using SmhiWeather;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace MowControl
{
    public class SmhiRainSensor : IRainSensor
    {
        private ISmhi _smhi;
        private List<ForecastTimeSerie> _weatherTimeSeries;
        private ISystemTime _systemTime;

        public SmhiRainSensor(ISystemTime systemTime, ISmhi smhi, params ForecastTimeSerie[] seededWeatherTimeSeries)
        {
            _systemTime = systemTime;
            _smhi = smhi;
            IsWet = false;
            _weatherTimeSeries = new List<ForecastTimeSerie>();
            Seed(seededWeatherTimeSeries);

            StartAsync();
        }

        /// <summary>
        /// Seeds the rain sensor with historic data to be used for taking decisions. If this is not used, the rain sensor will assume 
        /// dry surface and start building the weather history from the current time.
        /// </summary>
        /// <param name="historicWeatherTimeSeries"></param>
        public void Seed(params ForecastTimeSerie[] historicWeatherTimeSeries)
        {
            foreach (var timeSerie in historicWeatherTimeSeries)
            {
                var alreadyExistingTimeSerie = _weatherTimeSeries.FirstOrDefault(ts => ts.validTime == timeSerie.validTime);

                if (alreadyExistingTimeSerie != null)
                {
                    throw new ArgumentException("The list of historic weather time series will contain doubles.");
                }

                _weatherTimeSeries.Add(timeSerie);
            }
        }

        public bool IsWet
        {
            get; private set;
        }

        /// <summary>
        /// Gets or sets the wetness in percent. 0 is dry, 100 is as wet as it can be.
        /// </summary>
        public int Wetness { get; private set; }

        private async Task StartAsync()
        {
            while (true)
            {
                // First, add a new weather time serie to the list of historic weather.

                ForecastTimeSerie currentWeather = _smhi.GetCurrentWeather();

                var lastAddedTimeSerie = _weatherTimeSeries.OrderByDescending(ts => ts.validTime).FirstOrDefault(ts => ts.validTime == currentWeather.validTime);

                if (lastAddedTimeSerie == null)
                {
                    _weatherTimeSeries.Add(currentWeather);
                }

                decimal currentPrecipitation = Math.Max(currentWeather.PrecipitationMax, currentWeather.PrecipitationMin);

                // Calculate the wetness

                int wetness = Convert.ToInt32(currentPrecipitation * 100);

                foreach (var timeSerie in _weatherTimeSeries.Where(ts => ts.validTime > _systemTime.Now.ToUniversalTime().AddDays(-1)).OrderBy(ts => ts.validTime))
                {
                    decimal precipitation = Math.Max(timeSerie.PrecipitationMin, timeSerie.PrecipitationMax);

                    // If it's raining, increase the wetness
                    wetness += Convert.ToInt32(precipitation * 100);

                    if (wetness > 100)
                    {
                        wetness = 100;
                    }

                    // If it's not raining, decrease wetness in relation to relative humidity
                    if (precipitation == 0)
                    {
                        wetness -= timeSerie.RelativeHumidity / 2;
                    }
                }

                Wetness = wetness;
                IsWet = Wetness > 0;

                // Wait for 15 minutes
                await Task.Delay(15 * 60 * 1000);
            }
        }
    }
}
