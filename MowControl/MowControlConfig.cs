using MowControl;
using System;
using System.Collections.Generic;
using System.Text;

namespace MowControl
{
    public sealed class MowControlConfig : IMowControlConfig
    {
        public MowControlConfig()
        {
            TimeIntervals = new List<TimeInterval>();
            AverageWorkPerDayHours = 24;
            MaxHourlyThunderPercent = 40;
            MaxHourlyPrecipitaionMillimeter = 0.1f;
            PowerOnUrl = "";
            PowerOffUrl = "";
            CoordLat = 0;
            CoordLon = 0;
            UsingContactHomeSensor = false;
            MaxMowingHoursWithoutCharge = 2;
            MaxChargingHours = 2;
            MaxRelativeHumidityPercent = 100;
        }

        public List<TimeInterval> TimeIntervals { get; set; }

        public int AverageWorkPerDayHours { get; set; }

        public int MaxHourlyThunderPercent { get; set; }

        public float MaxHourlyPrecipitaionMillimeter { get; set; }

        public string PowerOnUrl { get; set; }

        public string PowerOffUrl { get; set; }

        public decimal CoordLat { get; set; }

        public decimal CoordLon { get; set; }

        public bool UsingContactHomeSensor { get; set; }

        public int MaxMowingHoursWithoutCharge { get; set; }

        public int MaxChargingHours { get; set; }

        public int MaxRelativeHumidityPercent { get; set; }
    }
}
