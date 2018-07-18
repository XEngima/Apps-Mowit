using MowPlanning;
using System;
using System.Collections.Generic;
using System.Text;

namespace MowPlanning
{
    public class MowPlannerConfig : IMowPlannerConfig
    {
        public MowPlannerConfig()
        {
            TimeIntervals = new List<TimeInterval>();
            AverageWorkPerDayHours = 24;
            MaxHourlyThunderPercent = 40;
            MaxHourlyPrecipitaionMillimeter = 0.1f;
            PowerOnUrl = "";
            PowerOffUrl = "";
            CoordLat = 0;
            CoordLon = 0;
            UsingContactHomeSensor = true;
            MaxMowingWithoutCharge = 2;
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

        public int MaxMowingWithoutCharge { get; set; }
    }
}
