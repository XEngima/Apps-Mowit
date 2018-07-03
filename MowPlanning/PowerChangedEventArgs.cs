namespace MowPlanning
{
    public class PowerChangedEventArgs
    {
        public PowerChangedEventArgs(bool powerTurnedOn, string reason)
        {
            PowerTurnedOn = powerTurnedOn;
            Reason = reason;
        }

        public bool PowerTurnedOn { get; private set; }

        public string Reason { get; private set; }
    }

    public delegate void PowerChangedEventHandler(object sender, PowerChangedEventArgs e);
}
