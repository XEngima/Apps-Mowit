using MowControl;
using System;
using System.Collections.Generic;
using System.Text;
using DanielEiserman.Power;

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
            Status = isActive ? PowerStatus.On : PowerStatus.Off;
        }

        public TestPowerSwitch(PowerStatus status)
            : this()
        {
            Status = status;
        }

        public PowerStatus Status { get; set; }

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
            Status = PowerStatus.Off;
        }

        public void TurnOn()
        {
            Status = PowerStatus.On;
            TurnOns++;
        }
    }
}
