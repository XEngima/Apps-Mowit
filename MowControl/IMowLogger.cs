using System;
using System.Collections.Generic;
using System.Text;

namespace MowControl
{
    public interface IMowLogger
    {
        event EventHandler LogItemWritten;

        void Write(DateTime time, LogType type, string message);

        IList<LogItem> LogItems { get; }
    }
}
