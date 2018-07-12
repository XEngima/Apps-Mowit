using SmhiWeather;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace MowPlanning
{
    public class WeatherForecast : IWeatherForecast
    {
        public WeatherForecast(int maxHourlyThunderPercent, double maxHourlyPrecipitationMillimeter)
        {
            MaxHourlyThunderPercent = maxHourlyThunderPercent;
            MaxHourlyPrecipitaionMillimeter = maxHourlyPrecipitationMillimeter;
        }

        /// <summary>
        /// Gets a text describing the weather ahead. Set when CheckIfWeatherWillBeGood is executed.
        /// </summary>
        public string WeatherAheadDescription { get; private set; }

        public bool CheckIfWeatherWillBeGood(int hours)
        {
            WeatherAheadDescription = "Weather will be fine.";
            Forecast forecast = Smhi.GetForecast();
            ForecastTimeSerie currentWeather = Smhi.GetCurrentWeather();

            int i = 0;
            foreach (ForecastTimeSerie timeSerie in forecast.timeseries
                                                    .Where(ts => ts.validTime >= currentWeather.validTime)
                                                    .OrderBy(ts => ts.validTime))
            {
                // Kolla om det kommer att regna för mycket
                ForecastParameter parameter = timeSerie.parameters.First(p => p.name == "pmax");
                if (parameter.values[0] > (decimal)MaxHourlyPrecipitaionMillimeter)
                {
                    WeatherAheadDescription = "Expecting rain as a maximum of " + parameter.values[0] + " mm/h at " + timeSerie.validTime.ToShortTimeString() + ".";
                    return false;
                }

                // Kolla om det är för hög sannolikhet för åska
                parameter = timeSerie.parameters.First(p => p.name == "tstm");
                if (parameter.values[0] > MaxHourlyThunderPercent)
                {
                    WeatherAheadDescription = "Thunder warning of " + parameter.values[0] + "% at " + timeSerie.validTime.ToShortTimeString() + ".";
                    return false;
                }

                i++;
                if (i > hours)
                {
                    break;
                }
            }

            return true;
        }

        private int MaxHourlyThunderPercent { get; set; }

        private double MaxHourlyPrecipitaionMillimeter { get; set; }
    }
}
