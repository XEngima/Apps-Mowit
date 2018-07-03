using System;
using System.Collections.Generic;
using System.Text;

namespace MowPlanning
{
    public class SystemTime : ISystemTime
    {
        public DateTime Now => DateTime.Now;
    }
}
