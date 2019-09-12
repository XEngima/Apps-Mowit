using System;
using System.Collections.Generic;
using System.Text;

namespace DanielEiserman.Power
{
    /// <summary>
    /// Klass som hanterar på- och avslagning av stömmen till gräsklipparen.
    /// </summary>
    public interface IPowerSwitch : IPowerSwitchConsumer
    {
        event PowerChangedEventHandler PowerSwitchChanged;

        /// <summary>
        /// Sätter på strömmen till robotgräsklipparen.
        /// </summary>
        void TurnOn();

        /// <summary>
        /// Stänger av strömmen till robotgräsklipparen.
        /// </summary>
        void TurnOff();
    }
}
