using Asgard.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Asgard.Communications
{
    public class MessageManager
    {
        private readonly ICbusMessenger messenger;
        private readonly ILogger<MessageManager> logger;

        private static TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(2);

        public MessageManager(ICbusMessenger messenger, ILogger<MessageManager> logger = null)
        {
            this.messenger = messenger;
            this.logger = logger;
        }
        /// <summary>
        /// Sends a message and awaits a given reply, or times out.
        /// </summary>
        /// <typeparam name="T">The type of message expected in reply to the sent message.</typeparam>
        /// <param name="msg">The message to send.</param>
        /// <param name="filterResponse">An optional filter callback to further process the replies to ensure you get the right one.</param>
        /// <returns>The first message of the given type that passes the filterResponse.</returns>
        public Task<T> SendMessageWaitForReply<T>(ICbusOpCode msg, Func<T, bool> filterResponse = null) where T:ICbusOpCode => SendMessageWaitForReply(msg, DefaultTimeout, filterResponse);

        /// <summary>
        /// Sends a message and awaits a given reply, or times out.
        /// </summary>
        /// <typeparam name="T">The type of message expected in reply to the sent message.</typeparam>
        /// <param name="msg">The message to send.</param>
        /// <param name="timeout">The amount of time to wait for a response.</param>
        /// <param name="filterResponse">An optional filter callback to further process the replies to ensure you get the right one.</param>
        /// <returns>The first message of the given type that passes the filterResponse.</returns>
        public async Task<T> SendMessageWaitForReply<T>(ICbusOpCode msg, TimeSpan timeout, Func<T, bool> filterResponse = null) where T:ICbusOpCode => (await SendMessageWaitForReplies(msg, timeout, 1, filterResponse)).First();

        /// <summary>
        /// Sends a message and waits for replies, or times out.
        /// </summary>
        /// <typeparam name="T">The type of messages expected in reply to the sent message.</typeparam>
        /// <param name="msg">The message to send.</param>
        /// <param name="expected">The number of expected responses. Times out if this number is not received. Specify 0 for an unknown number of responses. Returns immediately when this number of responses have been received.</param>
        /// <param name="filterResponses">An optional filter callback to further process the replies to ensure you get the right ones.</param>
        /// <returns>The received responses.</returns>
        public Task<IEnumerable<T>> SendMessageWaitForReplies<T>(ICbusOpCode msg, int expected = 0, Func<T, bool> filterResponses = null) where T:ICbusOpCode => SendMessageWaitForReplies(msg, DefaultTimeout, expected, filterResponses);

        /// <summary>
        /// Sends a message and waits for replies, or times out.
        /// </summary>
        /// <typeparam name="T">The type of messages expected in reply to the sent message.</typeparam>
        /// <param name="msg">The message to send.</param>
        /// <param name="timeout">The amount of time to wait for the responses.</param>
        /// <param name="expected">The number of expected responses. Times out if this number is not received. Specify 0 for an unknown number of responses. Returns immediately when this number of responses have been received.</param>
        /// <param name="filterResponses">An optional filter callback to further process the replies to ensure you get the right ones.</param>
        /// <returns>The received responses.</returns>
        public async Task<IEnumerable<T>> SendMessageWaitForReplies<T>(ICbusOpCode msg, TimeSpan timeout, int expected = 0, Func<T, bool> filterResponses = null) where T:ICbusOpCode
        {
            logger?.LogTrace("Sending message of type {0}, awaiting {1} replies with a timeout of {2}", msg.GetType().Name, typeof(T).Name, expected);
            var tcs = new TaskCompletionSource<bool>();
            using var cts = new CancellationTokenSource(timeout);
            cts.Token.Register(() =>
            {
                if (expected == 0)
                {
                    tcs.TrySetCanceled();
                }
                else
                {
                    tcs.TrySetResult(false);
                }
            }, false);

            var responses = new List<T>();

            void AwaitResponse(object sender, CbusMessageEventArgs e)
            {
                if (e.Message.GetOpCode() is T response)
                {
                    if (filterResponses != null)
                    {
                        if (!filterResponses(response))
                        {
                            logger?.LogTrace("Message of correct type received, but did not pass filter");
                            return;
                        }
                    }

                    logger?.LogTrace("Message of correct type received, appended result");
                    responses.Add(response);
                    if (expected != 0 && responses.Count == expected)
                    {
                        logger?.LogTrace("All messages expected have been received");
                        tcs.TrySetResult(true);
                    }
                }
            }

            messenger.MessageReceived += AwaitResponse;
            await messenger.SendMessage(msg.Message);

            try
            {
                var all = await tcs.Task;
                if (!all)
                {
                    logger?.LogWarning("Not all expected messages received within the timeout time");
                    throw new TimeoutException();
                }
            }
            catch (TaskCanceledException)
            {
                if (responses.Count == 0)
                {
                    logger?.LogWarning("Not all expected messages received within the timeout time");
                    throw new TimeoutException();
                }
            }
            finally
            {
                messenger.MessageReceived -= AwaitResponse;
            }
            return responses;
        }
    }
}