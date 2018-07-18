using MowControl;
using System;
using System.Collections.Generic;
using System.Text;

namespace MowerTests
{
    public class TestPowerSwitch : IPowerSwitch
    {
        public TestPowerSwitch()
        {
            TurnOns = 0;
            TurnOffs = 0;
        }

        public TestPowerSwitch(bool isActive)
            :this()
        {
            IsOn = isActive;
        }

        public bool IsOn { get; set; }

        public bool HasBeenTurnedOnOnce
        {
            get
            {
                return TurnOns == 1;
            }
        }

        public bool HasBeenTurnedOffOnce
        {
            get
            {
                return TurnOffs == 1;
            }
        }

        public int TurnOns { get; private set; }
        public int TurnOffs { get; private set; }

        public event PowerChangedEventHandler PowerSwitchChanged;

        public void TurnOff()
        {
            TurnOffs++;
            IsOn = false;
        }

        public void TurnOn()
        {
            IsOn = true;
            TurnOns++;
        }
    }
}
