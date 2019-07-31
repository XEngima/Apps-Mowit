using System;
using System.Collections.Generic;
using System.Text;

namespace MowControl.DateTimeExtensions
{
    public static class DateTimeExtensions
    {
        public static DateTime FloorMinutes(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0);
        }
    }
}
