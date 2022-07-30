using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asgard.Data;
using Microsoft.Extensions.Logging;

namespace Asgard.Communications
{
    public class MessageManager
    {
        private readonly ICbusMessenger messenger;
        private readonly ILogger<MessageManager>? logger;

        private static TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(2);

        public MessageManager(ICbusMessenger messenger,
                              ILogger<MessageManager>? logger = null)
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
        public Task<T> SendMessageWaitForReply<T>(ICbusOpCode msg, Func<T, bool>? filterResponse = null)
            where T : ICbusOpCode => 
            SendMessageWaitForReply(msg, DefaultTimeout, filterResponse);

        /// <summary>
        /// Sends a message and awaits a given reply, or times out.
        /// </summary>
        /// <typeparam name="T">The type of message expected in reply to the sent message.</typeparam>
        /// <param name="msg">The message to send.</param>
        /// <param name="timeout">The amount of time to wait for a response.</param>
        /// <param name="filterResponse">An optional filter callback to further process the replies to ensure you get the right one.</param>
        /// <returns>The first message of the given type that passes the filterResponse.</returns>
        public async Task<T> SendMessageWaitForReply<T>(ICbusOpCode msg, TimeSpan timeout, Func<T, bool>? filterResponse = null)
            where T : ICbusOpCode => 
            (await SendMessageWaitForReplies(msg, timeout, 1, filterResponse)).First();

        /// <summary>
        /// Sends a message and waits for replies, or times out.
        /// </summary>
        /// <typeparam name="T">The type of messages expected in reply to the sent message.</typeparam>
        /// <param name="msg">The message to send.</param>
        /// <param name="expected">The number of expected responses. Times out if this number is not received. Specify 0 for an unknown number of responses. Returns immediately when this number of responses have been received.</param>
        /// <param name="filterResponses">An optional filter callback to further process the replies to ensure you get the right ones.</param>
        /// <returns>The received responses.</returns>
        public Task<IEnumerable<T>> SendMessageWaitForReplies<T>(ICbusOpCode msg, int expected = 0, Func<T, bool>? filterResponses = null)
            where T : ICbusOpCode => 
            SendMessageWaitForReplies(msg, DefaultTimeout, expected, filterResponses);

        /// <summary>
        /// Sends a message and awaits a given reply, or times out.
        /// </summary>
        /// <typeparam name="T">The type of message to send, and use Asgard to filter and return known reply messages for.</typeparam>
        /// <param name="msg">The message to send.</param>
        public async Task<IReplyTo<T>> SendMessageWaitForReply<T>(T msg)
            where T : ICbusOpCode => 
            (await SendMessageWaitForReplies<T>(msg, DefaultTimeout, 1)).First();

        /// <summary>
        /// Sends a message and awaits a given reply, or times out.
        /// </summary>
        /// <typeparam name="T">The type of message to send, and use Asgard to filter and return known reply messages for.</typeparam>
        /// <param name="msg">The message to send.</param>
        /// <param name="timeout">The amount of time to wait for a response.</param>
        public async Task<IReplyTo<T>> SendMessageWaitForReply<T>(T msg, TimeSpan timeout)
            where T : ICbusOpCode => 
            (await SendMessageWaitForReplies<T>(msg, timeout, 1)).First();

        /// <summary>
        /// Send a message and waits for replies, or times out.
        /// </summary>
        /// <typeparam name="T">The type of message to send, and use Asgard to filter and return known reply messages for.</typeparam>
        /// <param name="message">The message to send.</param>
        /// <param name="expected">The number of expected responses. Times out if this number is not received. Specify 0 for an unknown number of responses. Returns immediately when this number of responses have been received.</param>
        /// <returns>The received responses.</returns>
        public Task<IEnumerable<IReplyTo<T>>> SendMessageWaitForReplies<T>(T message, int expected = 0)
            where T : ICbusOpCode =>
            SendMessageWaitForReplies(message, DefaultTimeout, expected);

        /// <summary>
        /// Send a message and waits for replies, or times out.
        /// </summary>
        /// <typeparam name="T">The type of message to send, and use Asgard to filter and return known reply messages for.</typeparam>
        /// <param name="message">The message to send.</param>
        /// <param name="timeout">The amount of time to wait for a response.</param>
        /// <param name="expected">The number of expected responses. Times out if this number is not received. Specify 0 for an unknown number of responses. Returns immediately when this number of responses have been received.</param>
        /// <returns>The received responses.</returns>
        public Task<IEnumerable<IReplyTo<T>>> SendMessageWaitForReplies<T>(T message, TimeSpan timeout, int expected = 0)
            where T : ICbusOpCode =>
            SendMessageWaitForReplies<IReplyTo<T>>(message, timeout, expected, (r) => r.IsReply(message));

        /// <summary>
        /// Sends a message and waits for replies, or times out.
        /// </summary>
        /// <typeparam name="T">The type of messages expected in reply to the sent message.</typeparam>
        /// <param name="msg">The message to send.</param>
        /// <param name="timeout">The amount of time to wait for the responses.</param>
        /// <param name="expected">The number of expected responses. Times out if this number is not received. Specify 0 for an unknown number of responses. Returns immediately when this number of responses have been received.</param>
        /// <param name="filterResponses">An optional filter callback to further process the replies to ensure you get the right ones.</param>
        /// <returns>The received responses.</returns>
        public async Task<IEnumerable<T>> SendMessageWaitForReplies<T>(ICbusOpCode msg,
                                                                       TimeSpan timeout,
                                                                       int expected = 0,
                                                                       Func<T, bool>? filterResponses = null)
            where T : ICbusOpCode
        {
            this.logger?.LogTrace(
                $"Sending message of type {0}, awaiting {1} replies with a timeout of {2}",
                msg.GetType().Name, typeof(T).Name, expected);

            using var cts = new CancellationTokenSource(timeout);
            var tcs = CreateSendWaitCompletionSource(cts, timeout, expected);

            var responses = new List<T>();

            void AwaitResponse(object? sender, CbusStandardMessageEventArgs e)
            {
                AwaitResponse<T>(e.Message, filterResponses, responses, expected, tcs);
            }

            try
            {
                this.messenger.StandardMessageReceived += AwaitResponse;
                await SendMessageWaitForReplies<T>(msg, tcs);
            }
            catch (TaskCanceledException)
            {
                if (responses.Count == 0)
                {
                    this.logger?.LogWarning(
                        "Not all expected messages received within the timeout time");
                    throw new TimeoutException();
                }
            }
            finally
            {
                this.messenger.StandardMessageReceived -= AwaitResponse;
            }

            return responses;
        }

        private bool AwaitResponse<T>(ICbusStandardMessage message,
                                      Func<T, bool>? filterResponses,
                                      List<T> responses,
                                      int expected,
                                      TaskCompletionSource<bool> tcs)
            where T : ICbusOpCode
        {
            if (!message.TryGetOpCode(out var opCode) || opCode is not T response)
                return false;

            if (filterResponses is not null && !filterResponses(response))
            {
                this.logger?.LogTrace("Message of expected type received, but did not pass filter.");
                return false;
            }

            responses.Add(response);

            if (responses.Count == expected)
            {
                if (expected != 0)
                    this.logger?.LogTrace($"All {expected} messages expected have been received.");
                tcs.TrySetResult(true);
                return true;
            }

            return false;
        }

        private static TaskCompletionSource<bool> CreateSendWaitCompletionSource(CancellationTokenSource cts, TimeSpan timeout, int expected)
        {
            var tcs = new TaskCompletionSource<bool>();
            cts.Token.Register(() =>
            {
                if (expected == 0)
                    tcs.TrySetCanceled();
                else
                    tcs.TrySetResult(false);
            }, useSynchronizationContext: false);

            return tcs;
        }

        private async Task SendMessageWaitForReplies<T>(ICbusOpCode msg, TaskCompletionSource<bool> tcs)
            where T : ICbusOpCode
        {
            var sent = await this.messenger.SendMessage(msg.Message);

            if (!sent)
            {
                this.logger?.LogWarning("The requested message was not sent: {0}", msg);
                throw new SendFailureException($"The requested message was not sent: {msg}");
            }

            var all = await tcs.Task;
            if (!all)
            {
                this.logger?.LogWarning("Not all expected messages received within the timeout time");
                throw new TimeoutException($"Timeout waiting for replies to {msg}");
            }
        }
    }
}