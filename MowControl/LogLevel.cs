using System;
using System.Collections.Generic;
using System.Text;

namespace MowControl
{
    public enum LogLevel
    {
        Debug = 10,
        InfoLessInteresting = 20,
        Info = 20,
        InfoMoreInteresting = 40,
        Warning = 50,
        Error = 60,
        Fatal = 70,
    }
}
