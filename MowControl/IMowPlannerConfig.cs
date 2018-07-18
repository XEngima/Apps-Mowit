using System;
using System.Collections.Generic;
using System.Text;

namespace MowControl
{
    public interface IMowPlannerConfig
    {
        /// <summary>
        /// Intervallen som ställts in på robotgräsklipparen.
        /// </summary>
        List<TimeInterval> TimeIntervals { get; }

        /// <summary>
        /// Hämtar Lat-delen av koordinaten där gräsmattan ligger (där vädret ska hämtas)
        /// </summary>
        decimal CoordLat { get; }

        /// <summary>
        /// Hämtar Lon-delen av koordinaten där gräsmattan ligger (där vädret ska hämtas)
        /// </summary>
        decimal CoordLon { get; }

        /// <summary>
        /// Hämtar antalet arbetstimmar per dag.
        /// </summary>
        int AverageWorkPerDayHours { get; }

        /// <summary>
        /// Hämtar max risk för åska i procent.
        /// </summary>
        int MaxHourlyThunderPercent { get; }

        /// <summary>
        /// Hämtar max nederbörd i millimeter per timme.
        /// </summary>
        float MaxHourlyPrecipitaionMillimeter { get; }

        /// <summary>
        /// Hämtar URL:en för att slå på strömmen.
        /// </summary>
        string PowerOnUrl { get; }

        /// <summary>
        /// Hämtar URL:en för att slå av strömmen.
        /// </summary>
        string PowerOffUrl { get; }

        /// <summary>
        /// Gets whether a real contact sensor is used or not. If it is, then there can be more logic around
        /// micro intervals when the mower is coming and leaving. Otherwise the system can only trust the home
        /// sensor in between whole intervals. For example, if a contact home sensor is used, the mow 
        /// controller can see when the mower gets stuck and do not return to base.
        /// </summary>
        bool UsingContactHomeSensor { get; }

        // Gets the number of hours that the mower can be expected to be on the field without charging.
        int MaxMowingWithoutCharge { get; }
    }
}
