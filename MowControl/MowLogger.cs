using MowControl;
using System;
using System.Collections.Generic;
using System.Text;

namespace MowControl
{
    public class MowLogger : IMowLogger
    {
        public MowLogger()
        {
            LogItems = new List<LogItem>();
        }

        public IList<LogItem> LogItems { get; private set; }

        public event MowLoggerEventHandler LogItemWritten;

        public void Write(DateTime time, LogType type, LogLevel level, string message)
        {
            var item = new LogItem(time, type, level, message);
            LogItems.Add(item);
            OnLogItemWritten(item);
        }

        private void OnLogItemWritten(LogItem item)
        {
            LogItemWritten?.Invoke(this, new MowLoggerEventArgs(item));
        }
    }
}
