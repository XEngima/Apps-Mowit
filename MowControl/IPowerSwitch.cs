using System;
using System.Collections.Generic;
using System.Text;

namespace MowControl
{
    /// <summary>
    /// Klass som hanterar på- och avslagning av stömmen till gräsklipparen.
    /// </summary>
    public interface IPowerSwitch
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

        /// <summary>
        /// Hämtar huruvida strömmen till robotgräsklipparen är påslagen eller inte.
        /// </summary>
        bool IsOn { get; }
    }
}
