using System;
using System.Collections.Generic;
using System.Text;

namespace MowControl
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
        /// Kontrollerar om vädret framöver är tillräckligt bra för att klipparen ska kunna köra.
        /// </summary>
        /// <param name="hours">Antalet timmar framöver att kontrollera.</param>
        /// <param name="weatherAheadDescription">Description of the weather ahead.</param>
        /// <returns>true om vädret är tillräckligt bra för att köra, annars false.</returns>
        bool CheckIfWeatherWillBeGood(int hours, out string weatherAheadDescription);
    }
}
