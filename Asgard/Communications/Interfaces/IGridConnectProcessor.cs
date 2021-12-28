using System;
using System.Threading.Tasks;

namespace Asgard.Communications
{
    public interface IGridConnectProcessor
    {
        /// <summary>
        /// Fires when an error occurs at the transport layer. Incomplete messages are viewable here.
        /// </summary>
        event EventHandler<TransportErrorEventArgs> TransportError;

        /// <summary>
        /// Fires when a message has been received.
        /// </summary>
        /// <remarks>
        /// Only complete messages are raised via this event. 
        /// Incomplete messages are dealt with and removed internally.
        /// </remarks>
        event EventHandler<MessageReceivedEventArgs> GridConnectMessage;

        /// <summary>
        /// Used to send a new message on the transport layer.
        /// </summary>
        /// <param name="gridConnectMessage">The message to send, in GridConnect format.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        Task SendMessage(string gridConnectMessage);

        /// <summary>
        /// Opens and initialises the processor and the underlying transport connection.
        /// </summary>
        void Open();

        /// <summary>
        /// Closes the processor and the underlying transport connection.
        /// </summary>
        bool Close();
    }
}
