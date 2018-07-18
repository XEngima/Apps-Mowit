using System;
using System.Collections.Generic;
using System.Text;

namespace MowControl
{
    public enum LogType
    {
        MowControllerStarted = 1,
        PowerOn = 2,
        PowerOff = 3,
        Failure = 4,
        MowerEnteredHome = 5,
        MowerExitedHome = 6,
        MowerLost = 7,
    }
}
