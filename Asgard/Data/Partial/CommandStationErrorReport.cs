using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Data
{
    public partial class CommandStationErrorReport : IErrorReplyTo<GetEngineSession>
    {
        public bool IsReply(GetEngineSession request)
        {
            var address = (ushort)(
                (this.Data1 << 08) +
                this.Data2);

            return address == request.Address;
        }
    }
}
