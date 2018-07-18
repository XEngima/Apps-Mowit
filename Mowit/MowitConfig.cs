using MowControl;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Mowit
{
    [XmlRoot]
    public class MowitConfig
    {
        public MowPlannerConfig MowPlannerConfig { get; set; }

        public EmailConfig EmailConfig { get; set; }
    }
}
