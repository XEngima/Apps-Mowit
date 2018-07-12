using MowPlanning;
using System;
using System.Collections.Generic;
using System.Text;

namespace MowPlanning
{
    public class MowPlannerConfig : IMowPlannerConfig
    {
        public List<TimeInterval> TimeIntervals { get; set; }

        public int AverageWorkPerDayHours { get; set; }

        public int MaxHourlyThunderPercent { get; set; }

        public float MaxHourlyPrecipitaionMillimeter { get; set; }

        public string PowerOnUrl { get; set; }

        public string PowerOffUrl { get; set; }

        public decimal CoordLat { get; }

        public decimal CoordLon { get; }
    }
}
