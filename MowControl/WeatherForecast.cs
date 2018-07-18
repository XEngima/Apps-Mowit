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
                ForecastParameter parameter = timeSerie.parameters.First(p => p.name == "pmax");
                if (parameter.values[0] > (decimal)MaxHourlyPrecipitaionMillimeter)
                {
                    weatherAheadDescription = "Expecting rain as a maximum of " + parameter.values[0] + " mm/h at " + timeSerie.validTime.ToShortTimeString() + ".";
                    return false;
                }

                // Kolla om det är för hög sannolikhet för åska
                parameter = timeSerie.parameters.First(p => p.name == "tstm");
                if (parameter.values[0] > MaxHourlyThunderPercent)
                {
                    weatherAheadDescription = "Thunder warning of " + parameter.values[0] + "% at " + timeSerie.validTime.ToShortTimeString() + ".";
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
