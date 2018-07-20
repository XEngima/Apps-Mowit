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

        public TestSystemTime(int year, int month, int day, int hour, int minute)
        {
            Now = new DateTime(year, month, day, hour, minute, 0);
        }

        public TestSystemTime(int hour, int minute)
        {
            Now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, minute, 0);
        }

        public TestSystemTime(int hour)
        {
            Now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, 0, 0);
        }

        public DateTime Now { get; private set; }

        public void TickMinutes(int minutes)
        {
            Now = Now.AddMinutes(minutes);
        }

        public static DateTime DaysAgo(int daysAgo, int hour, int minute)
        {
            DateTime date = DateTime.Now.AddDays(-daysAgo);
            return new DateTime(date.Year, date.Month, date.Day, hour, minute, 0);
        }
    }
}
