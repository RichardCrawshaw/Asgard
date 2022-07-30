using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asgard.Data;

namespace Asgard.EngineControl
{
    internal class ServiceModeReadException:Exception
    {
        public ServiceModeReadException(SessionStatusEnum sessionStatus) => this.SessionStatus = sessionStatus;

        public SessionStatusEnum SessionStatus { get; }

        public override string ToString() => $"Error reading CV in service mode: {SessionStatus}";
    }
}
