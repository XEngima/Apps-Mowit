using System;
using System.Collections.Generic;
using System.Text;

namespace MowPlanning
{
    public interface ISystemTime
    {
        DateTime Now { get; }
    }
}
