using System;
using System.Collections.Generic;
using System.Text;

namespace MowControl
{
    public interface IMowLogger
    {
        event MowLoggerEventHandler LogItemWritten;

        void Write(DateTime time, LogType type, LogLevel level, string message);

        IList<LogItem> LogItems { get; }
    }
}
