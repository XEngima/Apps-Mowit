using System;

namespace MowControl
{
    public class LogItem
    {
        public LogItem(DateTime time, LogType type, LogLevel level, string message)
        {
            Time = time;
            Message = message;
            Type = type;
            Level = level;
        }

        public DateTime Time { get; private set; }

        public LogType Type { get; private set; }

        public LogLevel Level { get; private set; }

        public string Message { get; private set; }

        public override string ToString()
        {
            return Time.ToString("yyyy-MM-dd HH:mm") + " - " + Message;
        }
    }
}
