using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace MowControl
{
    public class UrlPowerSwitch : IPowerSwitch
    {
        string _onUrl;
        string _offUrl;

        public UrlPowerSwitch(string onUrl, string offUrl)
        {
            _onUrl = onUrl;
            _offUrl = offUrl;
            Status = PowerStatus.Unknown;
        }

        public PowerStatus Status { get; private set; }

        public event PowerChangedEventHandler PowerSwitchChanged;

        public void TurnOff()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_offUrl);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();

            Status = PowerStatus.Off;
        }

        public void TurnOn()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_onUrl);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();

            Status = PowerStatus.On;
        }
    }
}
