using System;

namespace Asgard.Communications
{
    public class SendFailureException : Exception
    {
        public SendFailureException(string message) : base(message) { }

        public SendFailureException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
