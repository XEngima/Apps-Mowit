using System;

namespace MowControl
{
    public class LogItem
    {
        public LogItem(DateTime time, LogType type, string message)
        {
            Time = time;
            Message = message;
            Type = type;
        }

        public DateTime Time { get; private set; }

        public LogType Type { get; private set; }

        public string Message { get; private set; }

        public override string ToString()
        {
            return Time.ToString("yyyy-MM-dd HH:mm") + " - " + Message;
        }
    }
}
