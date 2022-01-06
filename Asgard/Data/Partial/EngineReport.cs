namespace Asgard.Data
{
    public partial class EngineReport : IReplyTo<GetEngineSession>
    {
        public bool IsReply(GetEngineSession request) => this.Address == request.Address;
    }
}
