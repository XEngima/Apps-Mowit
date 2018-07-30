﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MowControl
{
    public interface IPowerSwitchConsumer
    {

        /// <summary>
        /// Hämtar huruvida strömmen till robotgräsklipparen är påslagen eller inte.
        /// </summary>
        PowerStatus Status { get; }
    }
}
