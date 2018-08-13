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

        public SmhiRainSensor(ISmhi smhi)
        {
            _smhi = smhi;
            IsWet = false;
            _weatherTimeSeries = new List<ForecastTimeSerie>();

            StartAsync();
        }

        public bool IsWet
        {
            get; private set;
        }

        private async Task StartAsync()
        {
            await Task.Yield();
            Run();
        }

        private void Run()
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
                int currentHumidity = currentWeather.RelativeHumidity;

                // Let it be dry if it has now rained for two hours.

                int i = 0;
                bool isDry = true;
                foreach (var timeSerie in _weatherTimeSeries.OrderByDescending(ts => ts.validTime))
                {
                    decimal precipitation = Math.Max(timeSerie.PrecipitationMin, timeSerie.PrecipitationMax);

                    if (precipitation > 0)
                    {
                        isDry = false;
                    }

                    if (i >= 3)
                    {
                        break;
                    }
                }

                IsWet = !isDry;

                // Wait for 15 minutes
                Thread.Sleep(15 * 60 * 1000);
            }
        }
    }
}
