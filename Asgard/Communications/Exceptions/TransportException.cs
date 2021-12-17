using System;

namespace Asgard.Communications
{
    public class TransportException : Exception
    {
        public TransportException(string message) : base(message) { }
        public TransportException(string message, Exception innerException) : base(message, innerException) { }
    }
}
