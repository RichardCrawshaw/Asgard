namespace Asgard.Data
{
    /// <summary>
    /// Applied to OpCodes to indicate that they may be sent in response to specific request messages.
    /// </summary>
    /// <typeparam name="T">The type of request message that this may be a response to.</typeparam>
    public interface IReplyTo<T> :
        ICbusOpCode
        where T : ICbusOpCode
    {
        /// <summary>
        /// Called automatically to check if this particular response is for a specific request.
        /// </summary>
        /// <param name="request">The request that this response should be checked against.</param>
        /// <returns>Should return true if this is a matching response to the supplied, otherwise false."/></returns>
        bool IsReply(T request);
    }

    /// <summary>
    /// Applied to OpCodes to indicate that they may be sent as an error response to specific request messages.
    /// </summary>
    /// <typeparam name="T">The type of request message that this may be a response to.</typeparam>
    public interface IErrorReplyTo<T> :
        IReplyTo<T>
        where T : ICbusOpCode { }
}
