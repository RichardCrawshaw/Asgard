using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Data
{
    public partial class ResponseToRequestForIndividualNodeParameter : IReplyTo<RequestReadOfANodeParameterByIndex>
    {
        //public bool IsReply(RequestReadOfANodeParameterByIndex request) =>
        //    request.NodeNumber == this.NodeNumber && request.ParamIndex == this.ParamIndex;
    }
}
