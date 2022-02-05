using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asgard.Data;
using Asgard.Data.Interfaces;
using Microsoft.Extensions.Logging;

namespace Asgard.Communications
{
    public class CbusEventManager :
        ICbusEventManager,
        IDisposable
    {
        #region Fields

        private const int QUEUE_RETRY_TIME = 500;

        private readonly ICbusMessenger cbusMessenger;

        private readonly ILogger logger;

        private readonly ConcurrentDictionary<CbusEventKey, bool?> cbusEventStates = new();
        private readonly ConcurrentDictionary<CbusEventKey, CbusEventCallback> cbusEventCallbacks = new();
        private readonly EmptyingConcurrentQueue<CbusEventError> cbusEventExceptionQueue = new();

        private readonly CancellationTokenSource cancellationTokenSource = new();

        #endregion

        #region Properties

        protected CancellationToken CancellationToken => this.cancellationTokenSource.Token;

        #endregion

        #region Constructors

        public CbusEventManager(ICbusMessenger cbusMessenger, ILogger logger)
        {
            this.cbusMessenger = cbusMessenger;
            this.logger = logger;

            // Start the queue processing as soon as possible; if there is nothing to do then it
            // will just sit there idle.
            Task.Run(() => ProcessQueue());

            this.cbusMessenger.MessageReceived += CbusMessenger_MessageReceived;
        }

        #endregion

        #region IDisposable support

        public bool IsDisposed { get; private set; }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    this.cancellationTokenSource.Cancel();

                    // dispose managed state (managed objects)
                    this.cancellationTokenSource.Dispose();
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                this.IsDisposed = true;
            }
        }

        // // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~CbusEventManager()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get the CBUS long event state for the specified <paramref name="nodeNumber"/> and
        /// <paramref name="eventNumber"/>.
        /// </summary>
        /// <param name="nodeNumber">The Node Number.</param>
        /// <param name="eventNumber">The Event Number.</param>
        /// <returns>True if the CBUS event is in the ON state; false if it is in the OFF state; null if the state is unknown.</returns>
        public bool? GetState(ushort nodeNumber, ushort eventNumber)
        {
            var key =
                this.cbusEventStates.Keys
                    .Where(n => !n.IsShortEvent)
                    .Where(n => n.NodeNumber == nodeNumber)
                    .Where(n => n.EventNumber == eventNumber)
                    .FirstOrDefault();
            if (key is null) return null;
            return this.cbusEventStates[key];
        }

        /// <summary>
        /// Gets the CBUS short event state for the specified <paramref name="eventNumber"/>.
        /// </summary>
        /// <param name="eventNumber">The Event Number.</param>
        /// <returns>True if the CBUS event is in the ON state; false if it is in the OFF state; null if the state is unknown.</returns>
        public bool? GetState(ushort eventNumber)
        {
            var key =
                this.cbusEventStates.Keys
                    .Where(n => n.IsShortEvent)
                    .Where(n => n.EventNumber == eventNumber)
                    .FirstOrDefault();
            if (key is null) return null;
            return this.cbusEventStates[key];
        }

        /// <summary>
        /// Register for the specified <typeparamref name="T"/> with the specified
        /// <paramref name="nodeNumber"/> and <paramref name="eventNumber"/> the specified 
        /// <paramref name="callback"/>.
        /// </summary>
        /// <typeparam name="T">The type of the CBUS event to register.</typeparam>
        /// <param name="nodeNumber">The Node Number of the event (or zero for a short event).</param>
        /// <param name="eventNumber">The Event Number of the event.</param>
        /// <param name="callback">The <see cref="CbusEventCallback"/> to register.</param>
        public void RegisterCbusEvent<T>(ushort nodeNumber, ushort eventNumber, CbusEventCallback callback)
            where T : class, ICbusAccessoryEvent
        {
            var type = typeof(T);
            var isOnEvent = type.GetInterface(nameof(ICbusAccessoryOnEvent)) is not null;
            var key = CbusEventKey.Create<T>(nodeNumber, eventNumber, isOnEvent);
            this.cbusEventCallbacks[key] = callback;
        }

        #endregion

        #region Support routines

        /// <summary>
        /// Find the key to the <see cref="cbusEventCallbacks"/> that corresponds to the specified
        /// <paramref name="cbusAccessoryEvent"/>.
        /// </summary>
        /// <param name="cbusAccessoryEvent">An <see cref="ICbusAccessoryEvent"/> object.</param>
        /// <returns>The corresponding <see cref="CbusEventKey"/> or null if there is none that match.</returns>
        private CbusEventKey? FindKey(ICbusAccessoryEvent cbusAccessoryEvent)
        {
            var result =
                this.cbusEventCallbacks.Keys
                    .Where(n => cbusAccessoryEvent.IsShortEvent == n.IsShortEvent)
                    .Where(n => n.IsShortEvent || n.NodeNumber == (cbusAccessoryEvent as IHasNodeNumber)?.NodeNumber)
                    .Where(n => (cbusAccessoryEvent.IsLongEvent && n.EventNumber == (cbusAccessoryEvent as IHasEventNumber)?.EventNumber) ||
                                (cbusAccessoryEvent.IsShortEvent && n.EventNumber == (cbusAccessoryEvent as IHasDeviceNumber)?.DeviceNumber))
                    .FirstOrDefault();
            return result;
        }

        /// <summary>
        /// Gets the registered <see cref="CbusEventCallback"/> that matches the specified 
        /// <paramref name="cbusAccessoryEvent"/>.
        /// </summary>
        /// <param name="cbusAccessoryEvent">An <see cref="ICbusAccessoryEvent"/> object.</param>
        /// <returns>The corresponding <see cref="CbusEventCallback"/> or null if there is none that match.</returns>
        private CbusEventCallback? GetRegisteredCallback(ICbusAccessoryEvent cbusAccessoryEvent)
        {
            var key = FindKey(cbusAccessoryEvent);
            if (key is null) return null;
            return this.cbusEventCallbacks[key];
        }

        /// <summary>
        /// Processes the queue of errored callback messages.
        /// </summary>
        private void ProcessQueue()
        {
            while (!this.CancellationToken.IsCancellationRequested)
            {
                while (!this.cbusEventExceptionQueue.IsEmpty)
                {
                    this.cbusEventExceptionQueue.TryDequeue(out var result);
                    if (result is null) continue;

                    this.logger.LogWarning(result.Exception, 
                        "{0:yyyy-MM-dd HH:mm:ss.fff} Failed to run callback for {1} {2} event from {3}{4}.",
                            result.DateTime,
                            result.Key.IsShortEvent ? "short" : "long",
                            result.Key.IsOnEvent ? "on" : "off",
                            result.Key.IsShortEvent ? "" : (result.Key.NodeNumber.ToString("X2") + " "),
                            result.Key.EventNumber.ToString("X2"));

                    // Add any further error handling here.
                }

                this.cbusEventExceptionQueue.Trigger.Reset();
                this.cbusEventExceptionQueue.Trigger.Wait(QUEUE_RETRY_TIME, this.CancellationToken);
            }
        }

        /// <summary>
        /// Runs the specified <paramref name="callback"/> for the specified 
        /// <paramref name="cbusAccessoryEvent"/>.
        /// </summary>
        /// <param name="callback">The <see cref="CbusEventCallback"/> object that is to be run.</param>
        /// <param name="cbusAccessoryEvent">The <see cref="ICbusAccessoryEvent"/> object that has initiated the callback.</param>
        private void RunCallback(CbusEventCallback callback, ICbusAccessoryEvent cbusAccessoryEvent)
        {
            // Run the callback, catch any exceptions and queue them for additional handling at a
            // later point.

            try
            {
                callback(cbusAccessoryEvent, this);
            }
            catch (Exception ex)
            {
                var key = FindKey(cbusAccessoryEvent);
                if (key is null) return; // Should never be null.
                this.cbusEventExceptionQueue.Enqueue(new CbusEventError(ex, key));
            }
        }

        #endregion

        #region Event handler routines

        /// <summary>
        /// Occurs when a CBUS event message is received.
        /// </summary>
        private void CbusMessenger_MessageReceived(object? sender, CbusMessageEventArgs ea)
        {
            if (ea.Message is not ICbusAccessoryEvent cbusAccessoryEvent) return;
            var nodeNumber =
                cbusAccessoryEvent is IHasNodeNumber hasNodeNumber 
                    ? hasNodeNumber.NodeNumber 
                    : (ushort)0;
            var eventNumber=
                cbusAccessoryEvent is IHasEventNumber hasEventNumber ? hasEventNumber.EventNumber : 
                cbusAccessoryEvent is IHasDeviceNumber hasDeviceNumber ? hasDeviceNumber.DeviceNumber : 
                (ushort)0;

            // This isn't quite right. We're including the event state in the dictionary key.
            // So an ON event will exist separately to an OFF event. While we need that for 
            // registering the callbacks it isn't what we need for tracking the event states.
            var key = 
                CbusEventKey.Create(nodeNumber, eventNumber, 
                    cbusAccessoryEvent.IsOnEvent,
                    cbusAccessoryEvent.IsShortEvent);
            this.cbusEventStates[key] = cbusAccessoryEvent.IsOnEvent;

            var callback = GetRegisteredCallback(cbusAccessoryEvent);
            if (callback is null) return;

            Task.Run(() => RunCallback(callback, cbusAccessoryEvent))
                .AsgardFireAndForget();
        }

        #endregion

        #region Nested classes

        /// <summary>
        /// A class to hold event information: Node Number, Event (or Device) Number, whether it is
        /// long or short, and whether it is an ON or an OFF event.
        /// </summary>
        private class CbusEventKey
        {
            public ushort EventNumber { get; set; }
            public bool IsOnEvent { get; set; }
            public bool IsShortEvent { get; set; }
            public ushort NodeNumber { get; set; }

            public static CbusEventKey Create<T>(ushort nodeNumber, ushort eventNumber, bool isOnEvent)
            {
                return
                    typeof(T).GetInterface(nameof(ICbusAccessoryShortEvent)) is not null
                        ? new CbusEventKey
                        {
                            IsOnEvent = isOnEvent,
                            IsShortEvent = true,
                            NodeNumber = nodeNumber,
                        }
                        : new CbusEventKey
                        {
                            IsOnEvent = isOnEvent,
                            IsShortEvent = false,
                            NodeNumber = nodeNumber,
                            EventNumber = eventNumber,
                        };
            }

            public static CbusEventKey Create(ushort nodeNumber, ushort eventNumber, bool isOnEvent, bool isShortEvent)
            {
                return
                    isShortEvent
                        ? new CbusEventKey
                        {
                            IsOnEvent = isOnEvent,
                            IsShortEvent = true,
                            NodeNumber = nodeNumber,
                        }
                        : new CbusEventKey
                        {
                            IsOnEvent = isOnEvent,
                            IsShortEvent = false,
                            NodeNumber = nodeNumber,
                            EventNumber = eventNumber,
                        };
            }
        }

        /// <summary>
        /// A class to hold error information for when a callback throws an exception.
        /// </summary>
        private class CbusEventError
        {
            public Exception Exception { get; }

            public DateTime DateTime { get; }

            public CbusEventKey Key { get; }

            public CbusEventError(Exception exception, DateTime dateTime, CbusEventKey key)
            {
                this.Exception = exception;
                this.DateTime = dateTime;
                this.Key = key;
            }

            public CbusEventError(Exception exception, CbusEventKey key)
                : this(exception, DateTime.Now, key) { }
        }

        /// <summary>
        /// A class that sub-classes the <see cref="ConcurrentQueue{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of object that the <see cref="ConcurrentQueue{T}"/> holds.</typeparam>
        private class EmptyingConcurrentQueue<T> : ConcurrentQueue<T>
        {
            public ManualResetEventSlim Trigger { get; } = new ManualResetEventSlim();

            public new void Enqueue(T item)
            {
                base.Enqueue(item);
                this.Trigger.Set();
            }
        }

        #endregion
    }
}
