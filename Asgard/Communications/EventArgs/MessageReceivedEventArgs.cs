using System;

namespace Asgard.Communications
{
    public class MessageReceivedEventArgs:EventArgs
    {
        public string Message { get; }

        public MessageReceivedEventArgs(string message)
        {
            this.Message = message;
        }
    }
}
