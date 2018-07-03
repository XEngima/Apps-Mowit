using System;
using System.Collections.Generic;
using System.Text;

namespace MowPlanning
{
    /// <summary>
    /// Sensor som kollar om robotgräsklipparen står i sitt bo eller inte.
    /// </summary>
    public interface IHomeSensor
    {
        /// <summary>
        /// Hämtar huruvida robotgräsklipparen står i boet eller inte.
        /// </summary>
        bool IsHome { get; }
    }
}
