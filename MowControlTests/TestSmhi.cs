using SmhiWeather;
using System;
using System.Collections.Generic;
using System.Text;

namespace MowerTests
{
    public class TestSmhi : ISmhi
    {
        public decimal CoordLat => throw new NotImplementedException();

        public decimal CoordLon => throw new NotImplementedException();

        public ForecastTimeSerie GetCurrentWeather()
        {
            throw new NotImplementedException();
        }

        public Forecast GetForecast()
        {
            throw new NotImplementedException();
        }
    }
}
