using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Communications
{
    public class TransportErrorEventArgs:EventArgs
    {
        public Exception Exception { get; }

        public TransportErrorEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
}
