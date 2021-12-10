using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Communications
{
    public interface ICbusCanFrameProcessor
    {
        CbusCanFrame ParseFrame(string transportString);
        string ConstructTransportString(CbusCanFrame frame);
    }
}
