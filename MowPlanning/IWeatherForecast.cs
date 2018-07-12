using System;
using System.Collections.Generic;
using System.Text;

namespace MowPlanning
{
    public interface IWeatherForecast
    {
        /// <summary>
        /// Kontrollerar om vädret framöver är tillräckligt bra för att klipparen ska kunna köra.
        /// </summary>
        /// <param name="hours">Antalet timmar framöver att kontrollera.</param>
        /// <returns>true om vädret är tillräckligt bra för att köra, annars false.</returns>
        bool CheckIfWeatherWillBeGood(int hours);

        /// <summary>
        /// Gets a text describing the weather ahead. Set when CheckIfWeatherWillBeGood is executed.
        /// </summary>
        string WeatherAheadDescription { get; }
    }
}
