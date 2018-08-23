using MowControl;
using System;
using System.Collections.Generic;
using System.Text;

namespace MowerTests
{
    public class TestSystemTime : ISystemTime
    {
        public TestSystemTime(DateTime dateTimeNow)
        {
            Now = dateTimeNow;
        }

        public TestSystemTime(int year, int month, int day, int hour, int minute, int second)
        {
            Now = new DateTime(year, month, day, hour, minute, second);
        }

        public TestSystemTime(int year, int month, int day, int hour, int minute)
            :this(year, month, day, hour, minute, 0)
        {
        }

        public DateTime Now { get; private set; }

        public void TickMinutes(int minutes)
        {
            Now = Now.AddMinutes(minutes);
        }

        public void TickSeconds(int seconds)
        {
            Now = Now.AddSeconds(seconds);
        }

        public static DateTime DaysAgo(int daysAgo, int hour, int minute)
        {
            DateTime date = DateTime.Now.AddDays(-daysAgo);
            return new DateTime(date.Year, date.Month, date.Day, hour, minute, 0);
        }
    }
}
