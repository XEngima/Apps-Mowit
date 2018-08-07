using System;
using System.Collections.Generic;
using System.Text;

namespace MowControl
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

        /// <summary>
        /// Gets the time when the mower last came. null if the mower has not yet came.
        /// </summary>
        DateTime? MowerCameTime { get; }

        /// <summary>
        /// Gets the time when the mower last left. null if the mower has not yet left.
        /// </summary>
        DateTime? MowerLeftTime { get; }
    }
}
