using System;
using System.Collections.Generic;
using System.Text;

namespace MowControl
{
    public class SystemTime : ISystemTime
    {
        public DateTime Now => DateTime.Now;
    }
}
