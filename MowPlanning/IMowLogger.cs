using System;
using System.Collections.Generic;
using System.Text;

namespace MowPlanning
{
    public interface IMowLogger
    {
        event EventHandler LogItemWritten;

        void Write(DateTime time, LogType type, string message);

        IList<LogItem> LogItems { get; }
    }
}
