using SmhiWeather;
using System;
using System.Collections.Generic;
using System.Text;

namespace MowerTests
{
    public static class WeatherExtentions
    {
        public static ForecastTimeSerie Seed(this ForecastTimeSerie timeSerie, DateTime validTime, 
            decimal temperature = 0, 
            decimal relativeHumidity = 0, 
            decimal precipitationMin = 0, 
            decimal precipitationMax = 0)
        {
            timeSerie.validTime = validTime.ToUniversalTime();

            timeSerie.parameters = new ForecastParameter[]
            {
                new ForecastParameter() { name = "t", values = new [] { temperature } },
                new ForecastParameter() { name = "r", values = new [] { relativeHumidity } },
                new ForecastParameter() { name = "pmax", values = new [] { precipitationMax } },
                new ForecastParameter() { name = "pmin", values = new [] { precipitationMin } },
            };

            return timeSerie;
        }
    }
}
