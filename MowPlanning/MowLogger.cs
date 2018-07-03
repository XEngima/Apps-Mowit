using MowPlanning;
using System;
using System.Collections.Generic;
using System.Text;

namespace MowPlanning
{
    public class MowLogger : IMowLogger
    {
        public MowLogger()
        {
            LogItems = new List<LogItem>();
        }

        public IList<LogItem> LogItems { get; private set; }

        public event EventHandler LogItemWritten;

        public void Write(DateTime time, LogType type, string message)
        {
            LogItems.Add(new LogItem(time, type, message));
            OnLogItemWritten();
        }

        private void OnLogItemWritten()
        {
            LogItemWritten?.Invoke(this, new EventArgs());
        }
    }
}
