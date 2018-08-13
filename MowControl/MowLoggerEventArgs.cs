namespace MowControl
{
    public class MowLoggerEventArgs
    {
        public MowLoggerEventArgs(LogItem item)
        {
            Item = item;
        }

        public LogItem Item { get; private set; }
    }

    public delegate void MowLoggerEventHandler(object sender, MowLoggerEventArgs e);
}
