using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MowControl
{
    public class TimeInterval
    {
        public TimeInterval()
        {
        }

        public TimeInterval(int startHour, int startMin, int endHour, int endMin)
        {
            DateTime intervalStartTime = new DateTime(2018, 1, 1, StartHour, StartMin, 0);
            DateTime intervalEndTime = new DateTime(2018, 1, 1, EndHour, EndMin, 0);

            if (intervalStartTime > intervalEndTime)
            {
                throw new InvalidOperationException("Starttiden kan inte vara högre än sluttiden i ett intervall.");
            }

            StartHour = startHour;
            StartMin = startMin;
            EndHour = endHour;
            EndMin = endMin;
        }

        [XmlAttribute]
        public int StartHour { get; set; }

        [XmlAttribute]
        public int StartMin { get; set; }

        [XmlAttribute]
        public int EndHour { get; set; }

        [XmlAttribute]
        public int EndMin { get; set; }

        public TimeSpan ToTimeSpan() {
            DateTime intervalStartTime = new DateTime(2018, 1, 1, StartHour, StartMin, 0);
            DateTime intervalEndTime = new DateTime(2018, 1, 1, EndHour, EndMin, 0);

            return intervalEndTime - intervalStartTime;
        }

        /// <summary>
        /// Kontrollerar om ett klockslag är inom tidsintervallet.
        /// </summary>
        /// <param name="time">Tiden som ska kontrolleras.</param>
        /// <returns>true om tiden är inom intervallet, annars false.</returns>
        public bool ContainsTime(DateTime time)
        {
            DateTime intervalStartTime = new DateTime(time.Year, time.Month, time.Day, StartHour, StartMin, 0);
            DateTime afterIntervalEndTime = new DateTime(time.Year, time.Month, time.Day, EndHour, EndMin, 0).AddMinutes(1);

            return time >= intervalStartTime && time < afterIntervalEndTime;
        }

        public override string ToString()
        {
            return StartHour.ToString("00") + ":" + StartMin.ToString("00") + " - " + EndHour.ToString("00") + ":" + EndMin.ToString("00");
        }
    }
}
