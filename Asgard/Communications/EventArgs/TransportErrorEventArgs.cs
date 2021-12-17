using System;

namespace Asgard.Communications
{
    public class TransportErrorEventArgs:EventArgs
    {
        public Exception Exception { get; }

        public TransportErrorEventArgs(Exception exception)
        {
            this.Exception = exception;
        }
    }
}
