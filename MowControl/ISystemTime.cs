using System;
using System.Collections.Generic;
using System.Text;

namespace MowControl
{
    public interface ISystemTime
    {
        DateTime Now { get; }
    }
}
