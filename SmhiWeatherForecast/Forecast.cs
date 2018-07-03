using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmhiWeather
{
    public class Forecast
    {
        //public decimal lat { get; set; }
        //public decimal lon { get; set; }
        public DateTime referenceTime { get; set; }
        public ForecastTimeSerie[] timeseries { get; set; }
    }
}
