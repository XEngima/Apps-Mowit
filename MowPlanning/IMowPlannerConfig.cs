using System;
using System.Collections.Generic;
using System.Text;

namespace MowPlanning
{
    public interface IMowPlannerConfig
    {
        /// <summary>
        /// Intervallen som ställts in på robotgräsklipparen.
        /// </summary>
        List<TimeInterval> TimeIntervals { get; }

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
    }
}
