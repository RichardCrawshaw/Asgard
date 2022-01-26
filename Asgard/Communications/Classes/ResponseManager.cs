using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Asgard.Data;

namespace Asgard.Communications
{
    /// <summary>
    /// Class to manage the automated response to incoming messages. It allows the registering of
    /// <see cref="ICbusOpCode"/> classes together with a callback method to be used when that type
    /// is received.
    /// </summary>
    public class ResponseManager
    {
        private readonly ICbusMessenger cbusMessenger;
        private readonly ConcurrentDictionary<Type, ResponseCallback> listeners = new();

        public ResponseManager(ICbusMessenger cbusMessenger)
        {
            this.cbusMessenger = cbusMessenger;
            this.cbusMessenger.MessageReceived += CbusMessenger_MessageReceived;
        }

        /// <summary>
        /// Register the specified <paramref name="callback"/> for the defined 
        /// <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="ICbusOpCode"/> that the <paramref name="callback"/> services.</typeparam>
        /// <param name="callback">The callback function to register.</param>
        public void Register<T>(Func<ICbusMessenger, ICbusMessage?, Task> callback)
            where T : class, ICbusOpCode => 
            this.listeners[typeof(T)] = new ResponseCallback<T>(callback);

        private async void CbusMessenger_MessageReceived(object? sender, CbusMessageEventArgs e)
        {
            var opCode = e.Message.GetOpCode();
            var type = opCode.GetType();
            if (this.listeners.ContainsKey(type))
                await this.listeners[type].Invoke(this.cbusMessenger, e.Message);
        }

        #region Nested classes

        /// <summary>
        /// Nested abstract class to hold basic details of the callback method.
        /// </summary>
        private abstract class ResponseCallback
        {
            /// <summary>
            /// The callback method.
            /// </summary>
            private readonly Func<ICbusMessenger, ICbusMessage?, Task> callback;

            /// <summary>
            /// Gets the type of <see cref="ICbusOpCode"/> that this instances services.
            /// </summary>
            public abstract Type OpCodeType { get; }
            
            protected ResponseCallback(Func<ICbusMessenger, ICbusMessage?, Task> callback)=>
                this.callback = callback;

            /// <summary>
            /// Invoke the callback method for the current instance using the specified
            /// <paramref name="cbusMessenger"/> for the specified <paramref name="cbusMessage"/>.
            /// </summary>
            /// <param name="cbusMessenger">The <see cref="ICbusMessenger"/> to use.</param>
            /// <param name="cbusMessage">The <see cref="ICbusMessage"/> that is being responded to.</param>
            /// <returns>A <see cref="Task"/>.</returns>
            public async Task Invoke(ICbusMessenger cbusMessenger, ICbusMessage? cbusMessage) =>
                await this.callback(cbusMessenger, cbusMessage);
            }

            /// <summary>
            /// Nested derrived class for the specific <typeparamref name="T"/>.
            /// </summary>
            /// <typeparam name="T">The <see cref="ICbusOpCode"/> that this instance services.</typeparam>
            private class ResponseCallback<T> : ResponseCallback
            where T : class, ICbusOpCode
        {
            public override Type OpCodeType => typeof(T);

            public ResponseCallback(Func<ICbusMessenger, ICbusMessage?, Task> callback)
                : base(callback) { }
        }

        #endregion
    }
}
