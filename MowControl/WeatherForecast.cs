using SmhiWeather;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace MowControl
{
    public class WeatherForecast : IWeatherForecast
    {
        public WeatherForecast(Smhi smhi, int maxHourlyThunderPercent, double maxHourlyPrecipitationMillimeter)
        {
            Smhi = smhi;
            MaxHourlyThunderPercent = maxHourlyThunderPercent;
            MaxHourlyPrecipitaionMillimeter = maxHourlyPrecipitationMillimeter;
        }

        private Smhi Smhi { get; set; }

        public bool CheckIfWeatherWillBeGood(int hours, out string weatherAheadDescription)
        {
            weatherAheadDescription = "Weather will be fine.";
            Forecast forecast = Smhi.GetForecast();
            ForecastTimeSerie currentWeather = Smhi.GetCurrentWeather();

            int i = 0;
            foreach (ForecastTimeSerie timeSerie in forecast.timeseries
                                                    .Where(ts => ts.validTime >= currentWeather.validTime)
                                                    .OrderBy(ts => ts.validTime))
            {
                // Kolla om det kommer att regna för mycket
                if (timeSerie.PrecipitationMax > (decimal)MaxHourlyPrecipitaionMillimeter)
                {
                    weatherAheadDescription = "Expecting rain as a maximum of " + timeSerie.PrecipitationMax + " mm/h at " + timeSerie.validTime.ToLocalTime().ToShortTimeString() + ".";
                    return false;
                }

                // Kolla om det är för hög sannolikhet för åska
                if (timeSerie.ThunderProbability > MaxHourlyThunderPercent)
                {
                    weatherAheadDescription = "Thunder warning of " + timeSerie.ThunderProbability + "% at " + timeSerie.validTime.ToLocalTime().ToShortTimeString() + ".";
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

        public bool CheckIfWeatherWillBeGood(int hours)
        {
            string weatherAheadDescription;
            return CheckIfWeatherWillBeGood(hours, out weatherAheadDescription);
        }

        private int MaxHourlyThunderPercent { get; set; }

        private double MaxHourlyPrecipitaionMillimeter { get; set; }
    }
}
