using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Asgard.Data;

namespace Asgard.Communications
{
    //TODO: Potentially remove the messenger and message parameters here? Someone registering for this
    //is likely only interested in the actual opCode received?
    public delegate Task MessageCallback<T>(ICbusMessenger messenger, ICbusMessage message, T opCode);

    /// <summary>
    /// Class to manage the automated response to incoming messages. It allows the registering of
    /// <see cref="ICbusOpCode"/> classes together with a callback method to be used when that type
    /// is received.
    /// </summary>
    public class ResponseManager
    {
        #region Fields

        private readonly ICbusMessenger cbusMessenger;
        private readonly ConcurrentDictionary<Type, ResponseCallback> listeners = new();

        #endregion

        #region Constructors

        public ResponseManager(ICbusMessenger cbusMessenger) =>
            this.cbusMessenger = cbusMessenger;

        #endregion

        #region Methods

        /// <summary>
        /// Remove the registration for the defined <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="ICbusOpCode"/> to deregister.</typeparam>
        public void Deregister<T>(MessageCallback<T> callback)
            where T : class, ICbusOpCode
        {
            if (this.listeners.TryGetValue(typeof(T), out var listener))
            {
                if(listener is not ResponseCallback<T> l)
                {
                    //TODO: throw? really shouldn't happen
                    return;
                }
                l.RemoveCallback(callback);
                if (listener.CallbackCount == 0)
                {
                    this.listeners.TryRemove(typeof(T), out _);
                }
            }
            
            if (!this.listeners.Any())
                this.cbusMessenger.MessageReceived -= CbusMessenger_MessageReceived;
        }

        /// <summary>
        /// Remove the registration for the defined <typeparamref name="T"/>.  This will remove all callbacks for the specified type.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="ICbusOpCode"/> to deregister.</typeparam>
        public void Deregister<T>()
            where T : class, ICbusOpCode
        {
            this.listeners.TryRemove(typeof(T), out _);
            if (!this.listeners.Any())
                this.cbusMessenger.MessageReceived -= CbusMessenger_MessageReceived;
        }

        /// <summary>
        /// Register the specified <paramref name="callback"/> for the defined 
        /// <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="ICbusOpCode"/> that the <paramref name="callback"/> services.</typeparam>
        /// <param name="callback">The callback function to register.</param>
        public void Register<T>(MessageCallback<T> callback, Predicate<T>? filter = null)
            where T : class, ICbusOpCode
        {
            // If there were no callbacks registered, but now there are, the event handler routine
            // must be registered.
            var flag = this.listeners.Any();

            if (this.listeners.GetOrAdd(typeof(T), (t) => new ResponseCallback<T>()) is not ResponseCallback<T> listener)
            {
                //TODO: throw? really shouldn't happen
                return;
            }
            listener.AddCallback(callback, filter);
            if (!flag && this.listeners.Any())
                this.cbusMessenger.MessageReceived += CbusMessenger_MessageReceived;
        }

        #endregion

        #region Support routines

        private async void CbusMessenger_MessageReceived(object? sender, CbusMessageEventArgs e)
        {
            //TODO: error handling to prevent exceptions leaving async void

            if (e.Message is null) return;

            if (e.Message.TryGetOpCode(out var opCode))
            {
                var type = opCode.GetType();
                if (this.listeners.ContainsKey(type))
                    await this.listeners[type].Invoke(this.cbusMessenger, e.Message);
            }
        }

        #endregion

        #region Nested classes

        /// <summary>
        /// Nested abstract class to hold basic details of the callback method.
        /// </summary>
        private abstract class ResponseCallback
        {
            /// <summary>
            /// Gets the type of <see cref="ICbusOpCode"/> that this instances services.
            /// </summary>
            public abstract Type OpCodeType { get; }

            /// <summary>
            /// Returns the number of callbacks currently registered.
            /// </summary>
            public abstract int CallbackCount { get; }

            /// <summary>
            /// Invoke the callback method for the current instance using the specified
            /// <paramref name="cbusMessenger"/> for the specified <paramref name="cbusMessage"/>.
            /// </summary>
            /// <param name="cbusMessenger">The <see cref="ICbusMessenger"/> to use.</param>
            /// <param name="cbusMessage">The <see cref="ICbusMessage"/> that is being responded to.</param>
            /// <returns>A <see cref="Task"/>.</returns>
            public abstract Task Invoke(ICbusMessenger cbusMessenger, ICbusMessage cbusMessage);
        }

        /// <summary>
        /// Nested derrived class for the specific <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="ICbusOpCode"/> that this instance services.</typeparam>
        private class ResponseCallback<T> : ResponseCallback
            where T : class, ICbusOpCode
        {

            /// <summary>
            /// Multicast delegate holding the registered callbacks.
            /// </summary>
            private MessageCallback<T>? callback;
            private ConcurrentDictionary<MessageCallback<T>, Predicate<T>> filters = new();

            private readonly object lockObj = new();

            /// <inheritdoc/>
            public override Type OpCodeType => typeof(T);

            /// <inheritdoc/>
            public override int CallbackCount => this.callback?.GetInvocationList().Length ?? 0;

            /// <inheritdoc/>
            public override async Task Invoke(ICbusMessenger cbusMessenger, ICbusMessage cbusMessage)
            {
                if (!cbusMessage.TryGetOpCode(out var opCode) || opCode is not T opc)
                {
                    //TODO: throw? really shouldn't have happened
                    return;
                }

                var callbacks = this.callback?.GetInvocationList().Cast<MessageCallback<T>>();

                if (callbacks == null)
                    return;

                foreach(var callback in callbacks) {
                    if (filters.TryGetValue(callback, out var filter) && !filter(opc))
                    {
                        continue;
                    }
                    await callback.Invoke(cbusMessenger, cbusMessage, opc);
                }
            }

            /// <summary>
            /// Add a callback to be notified.
            /// </summary>
            /// <param name="callback">The callback to add.</param>
            public void AddCallback(MessageCallback<T> callback, Predicate<T>? filter = null)
            {
                lock (lockObj)
                {
                    if (this.callback == null)
                    {
                        this.callback = callback;
                    }
                    else
                    {
                        this.callback += callback;
                    }
                    if (filter != null)
                    {
                        filters.TryAdd(callback, filter);
                    }
                }
            }
            /// <summary>
            /// Removes a callback from further notifications.
            /// </summary>
            /// <param name="callback">The callback to remove.</param>
            public void RemoveCallback(MessageCallback<T> callback)
            {
                lock (lockObj)
                {
                    this.callback -= callback;
                    filters.TryRemove(callback, out _);
                }
            }
        }

        #endregion
    }
}
