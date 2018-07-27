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
        public MowControlConfig MowControlConfig { get; set; }

        public EmailConfig EmailConfig { get; set; }

        public static MowitConfig GetExampleConfig()
        {
            var mowControlConfig = new MowControlConfig()
            {
                PowerOnUrl = "http://example.com/on",
                PowerOffUrl = "http://example.com/off",
            };

            var emailConfig = new EmailConfig();

            return new MowitConfig
            {
                MowControlConfig = mowControlConfig,
                EmailConfig = emailConfig,
            };
        }
    }
}
