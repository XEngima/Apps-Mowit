using System;
using System.Runtime.Serialization;

namespace MowControl
{
    [Serializable]
    public class WeatherException : Exception
    {
        public WeatherException()
        {
        }

        public WeatherException(string message)
            : base(message)
        {
        }

        public WeatherException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected WeatherException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}