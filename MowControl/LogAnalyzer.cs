using System;
using System.Collections.Generic;
using System.Text;

namespace MowControl
{
    public class LogAnalyzer
    {
        private IMowLogger _logger;

        public LogAnalyzer(IMowLogger logger, bool homeFromStart)
        {
            _logger = logger;
            IsLost = false;
            IsMowing = false;
            IsHome = homeFromStart;

            if (_logger != null)
            {
                PerformLogAnalyzis();
            }
        }

        public bool IsLost { get; private set; }

        public bool IsMowing { get; private set; }

        public bool IsHome { get; private set; }

        private void PerformLogAnalyzis()
        {
            foreach (var logItem in _logger.LogItems)
            {
                switch (logItem.Type)
                {
                    case LogType.MowerLost:
                        {
                            IsLost = true;
                        }
                        break;
                    case LogType.MowerCame:
                        {
                            IsHome = true;
                            IsLost = false;
                        }
                        break;
                    case LogType.MowerLeft:
                        {
                            IsHome = false;
                        }
                        break;
                    case LogType.MowingStarted:
                        {
                            IsMowing = true;
                        }
                        break;
                    case LogType.MowingEnded:
                        {
                            IsMowing = false;
                        }
                        break;
                }
            }
        }
    }
}
