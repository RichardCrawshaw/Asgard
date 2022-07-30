using System;

namespace Asgard.Data
{
    // This file contains the implementation of IReplyTo<T> on the OpCode classes.
    // The interface is applied to the classes in the code generation, which is driven by the
    // Bifrost NuGet of the CBUS OpCode specification.

    public partial class SetNodeNumber
    {
        public bool IsReply(RequestNodeNumber request) => throw new NotImplementedException();
    }

    public partial class NodeNumberAcknowledge
    {
        public bool IsReply(SetNodeNumber request) => throw new NotImplementedException();
    }
    
    public partial class CommandStationErrorReport
    {
        public bool IsReply(GetEngineSession request)
        {
            var address = (ushort)(
                (this.Data1 << 08) +
                this.Data2);

            return address == request.Address;
        }
        public bool IsReply(QueryEngine request) => throw new NotImplementedException();

        public bool IsReply(RequestEngineSession request)
        {
            var address = (ushort)(
                (this.Data1 << 08) +
                this.Data2);

            return address == request.Address;
        }
        public bool IsReply(QueryConsist request) => throw new NotImplementedException();
    }
    
    public partial class EventSpaceLeftReplyFromNode
    {
        public bool IsReply(ReadNumberOfEventsAvailableInANode request) => throw new NotImplementedException();
    }
    
    public partial class NumberOfEventsStoredInNode
    {
        public bool IsReply(RequestToReadNumberOfStoredEvents request) => throw new NotImplementedException();
    }
    
    public partial class ReportCv
    {
        public bool IsReply(ReadCv request) => this.Session == request.Session && this.CV == request.CV;
    }
    
    public partial class ServiceModeStatus
    {
        public bool IsReply(ReadCv request) => this.Session == request.Session;
    }

    public partial class AccessoryOnResponseEvent
    {
        public bool IsReply(AccessoryRequestEvent request) => throw new NotImplementedException();
    }
    
    public partial class AccessoryOffResponseEvent
    {
        public bool IsReply(AccessoryRequestEvent request) => throw new NotImplementedException();
    }
    
    public partial class ResponseToARequestForANodeVariableValue
    {
        public bool IsReply(RequestReadOfANodeVariable request) => throw new NotImplementedException();
    }
    
    public partial class ResponseToRequestForIndividualNodeParameter
    {
        public bool IsReply(RequestReadOfANodeParameterByIndex request) =>
            request.NodeNumber == this.NodeNumber && request.ParamIndex == this.ParamIndex;
    }
    
    public partial class AccessoryShortResponseOn
    {
        public bool IsReply(AccessoryShortRequestEvent request) => throw new NotImplementedException();
    }
    
    public partial class AccessoryShortResponseOff
    {
        public bool IsReply(AccessoryShortRequestEvent request) => throw new NotImplementedException();
    }
    
    public partial class AccessoryOnResponseEvent1
    {
        public bool IsReply(AccessoryRequestEvent request) => throw new NotImplementedException();
    }
    
    public partial class AccessoryOffResponseEvent1
    {
        public bool IsReply(AccessoryRequestEvent request) => throw new NotImplementedException();
    }
    
    public partial class ResponseToRequestForReadOfEvValue
    {
        public bool IsReply(RequestForReadOfAnEventVariable request) => throw new NotImplementedException();
    }
    
    public partial class ResponseToQueryNode
    {
        public bool IsReply(QueryNodeNumber request) => true;
    }
    
    public partial class AccessoryShortResponseOn1
    {
        public bool IsReply(AccessoryShortRequestEvent request) => throw new NotImplementedException();
    }
    
    public partial class AccessoryShortResponseOff1
    {
        public bool IsReply(AccessoryShortRequestEvent request) => throw new NotImplementedException();
    }
    
    public partial class ResponseToARequestForAnEvValueInANodeInLearnMode
    {
        public bool IsReply(ReadEventVariableInLearnMode request) => throw new NotImplementedException();
    }
    
    public partial class AccessoryOnResponseEvent2
    {
        public bool IsReply(AccessoryRequestEvent request) => throw new NotImplementedException();
    }
    
    public partial class AccessoryOffResponseEvent2
    {
        public bool IsReply(AccessoryRequestEvent request) => throw new NotImplementedException();
    }
    
    public partial class AccessoryShortResponseOn2
    {
        public bool IsReply(AccessoryShortRequestEvent request) => throw new NotImplementedException();
    }
    
    public partial class AccessoryShortResponseOff2
    {
        public bool IsReply(AccessoryShortRequestEvent request) => throw new NotImplementedException();
    }
    
    public partial class EngineReport
    {
        public bool IsReply(GetEngineSession request) => this.Address == request.Address;
        public bool IsReply(QueryEngine request) => throw new NotImplementedException();
        public bool IsReply(RequestEngineSession request) => this.Address == request.Address;
        public bool IsReply(QueryConsist request) => throw new NotImplementedException();
    }
    
    public partial class ResponseToRequestForNodeNameString
    {
        public bool IsReply(RequestModuleName request) => throw new NotImplementedException();
    }
    
    public partial class CommandStationStatusReport
    {
        public bool IsReply(RequestCommandStationStatus request) => throw new NotImplementedException();
    }
    
    public partial class ResponseToRequestForNodeParameters
    {
        public bool IsReply(RequestNodeParameters request) => throw new NotImplementedException();
    }
    
    public partial class ResponseToRequestToReadNodeEvents
    {
        public bool IsReply(ReadBackAllStoredEventsInANode request) => throw new NotImplementedException();
        public bool IsReply(RequestReadOfStoredEventsByEventIndex request) => throw new NotImplementedException();
    }
    
    public partial class AccessoryOnResponseEvent3
    {
        public bool IsReply(AccessoryRequestEvent request) => throw new NotImplementedException();
    }
    
    public partial class AccessoryOffResponseEvent3
    {
        public bool IsReply(AccessoryRequestEvent request) => throw new NotImplementedException();
    }
    
    public partial class AccessoryNodeDataResponse
    {
        public bool IsReply(RequestNodeDataEvent request) => throw new NotImplementedException();
    }
    
    public partial class DeviceDataResponseShortMode
    {
        public bool IsReply(RequestDeviceDataShortMode request) => throw new NotImplementedException();
    }
    
    public partial class AccessoryShortResponseOn3
    {
        public bool IsReply(AccessoryShortRequestEvent request) => throw new NotImplementedException();
    }
    
    public partial class AccessoryShortResponseOff3
    {
        public bool IsReply(AccessoryShortRequestEvent request) => throw new NotImplementedException();
    }
}
