using System;
using System.Collections.Generic;
using System.Text;

namespace MowControl
{
    public class TimePerDayItem
    {
        public TimePerDayItem(DateTime date)
        {
            Date = date;
            SpentTime = new TimeSpan();
        }

        public DateTime Date { get; private set; }

        public TimeSpan SpentTime { get; private set; }

        public void AddSpentTime(TimeSpan timeToAdd)
        {
            SpentTime = SpentTime + timeToAdd;
        }

        public override string ToString()
        {
            return Date.ToString("yyyy-MM-dd") + " - " + SpentTime.ToString();
        }
    }
}
