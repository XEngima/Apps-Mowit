using System;
using System.Linq;
using System.Net;
using System.IO;
using Newtonsoft.Json;

/// <summary>
/// https://opendata.smhi.se/apidocs/metfcst/parameters.html
/// </summary>
namespace SmhiWeather
{
    public static class Smhi
    {
        private static TimeSpan _refreshInterval = new TimeSpan(1, 0, 0);
        private static Forecast _cachedForecast = null;
        private static DateTime _lastRequestUtcTime = DateTime.MinValue;
        private static decimal _coordLat = 0;
        private static decimal _coordLon = 0;

        public static void Init(decimal lat, decimal lon, TimeSpan refreshInterval)
        {
            _coordLat = lat;
            _coordLon = lon;
            _refreshInterval = refreshInterval;
        }

        public static Forecast GetForecast()
        {
            if (_coordLat == 0 || _coordLon == 0)
            {
                throw new InvalidOperationException("SMHI Weather Service not inizialised. Lat and Lon coordinates cannot be 0.");
            }

            if (_cachedForecast == null || _lastRequestUtcTime + _refreshInterval < DateTime.UtcNow)
            {
                string lat = _coordLat.ToString("0.00");
                string lon = _coordLon.ToString("0.00");
                string uri = $"http://opendata-download-metfcst.smhi.se/api/category/pmp3g/version/2/geotype/point/lon/{lon}/lat/{lat}/data.json";
                HttpWebRequest webRequest = WebRequest.CreateHttp(uri);

                using (var webResponse = (HttpWebResponse)webRequest.GetResponse())
                using (var reader = new StreamReader(webResponse.GetResponseStream()))
                {
                    //JavaScriptSerializer js = new JavaScriptSerializer();
                    string sJson = reader.ReadToEnd();
                    //var forecast = (Forecast)js.Deserialize(sJson, typeof(Forecast));

                    Forecast forecast = JsonConvert.DeserializeObject<Forecast>(sJson);

                    _lastRequestUtcTime = DateTime.UtcNow;
                    _cachedForecast = forecast;
                    return forecast;
                }
            }
            else
            {
                return _cachedForecast;
            }
        }

        public static ForecastTimeSerie GetCurrentWeather()
        {
            if (_coordLat == 0 || _coordLon == 0)
            {
                throw new InvalidOperationException("SMHI Weather Service not inizialised. Lat and Lon coordinates cannot be 0.");
            }

            DateTime utcNow = DateTime.UtcNow;
            Forecast forecast = GetForecast();

            foreach (var timeSerie in forecast.timeseries.OrderBy(ts => ts.validTime))
            {
                if (timeSerie.validTime.ToUniversalTime().AddMinutes(30) > utcNow)
                {
                    return timeSerie;
                }
            }

            return null;
        }
    }
}
