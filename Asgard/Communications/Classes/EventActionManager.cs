using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Asgard.Data;
using Asgard.Data.Interfaces;
using Microsoft.Extensions.Logging;

namespace Asgard.Communications
{
    public class EventActionManager :
        IDisposable
    {
        #region Fields

        private const int QUEUE_RETRY_TIME = 500;

        private readonly ICbusMessenger cbusMessenger;

        private readonly ILogger logger;

        private readonly ConcurrentDictionary<CbusEventStateKey, CbusEventCallback> cbusEventCallbacks = new();
        private readonly EmptyingConcurrentQueue<CbusEventError> cbusEventExceptionQueue = new();

        private readonly CancellationTokenSource cancellationTokenSource = new();

        #endregion

        #region Properties

        protected CancellationToken CancellationToken => this.cancellationTokenSource.Token;

        #endregion

        #region Constructors

        public EventActionManager(ICbusMessenger cbusMessenger, ILogger logger)
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
        /// Register for <typeparamref name="T"/> with the specified <paramref name="nodeNumber"/> 
        /// and <paramref name="eventNumber"/> the specified <paramref name="callback"/>.
        /// </summary>
        /// <typeparam name="T">The type of the CBUS event to register.</typeparam>
        /// <param name="nodeNumber">A <see cref="ushort"/> containing the Node Number.</param>
        /// <param name="eventNumber">A <see cref="ushort"/> containing the Event Number.</param>
        /// <param name="callback">The <see cref="CbusEventCallback"/> to register.</param>
        public void RegisterCbusEvent<T>(ushort nodeNumber, ushort eventNumber, CbusEventCallback callback)
            where T : class, ICbusAccessoryLongEvent =>
            RegisterCallback(CbusEventStateKey.Create<T>(nodeNumber, eventNumber), callback);

        /// <summary>
        /// Register for <typeparamref name="T"/> with the specified <paramref name="deviceNumber"/> 
        /// the specified <paramref name="callback"/>.
        /// </summary>
        /// <typeparam name="T">The type of the CBUS event to register.</typeparam>
        /// <param name="deviceNumber">A <see cref="ushort"/> containing the Device Number.</param>
        /// <param name="callback">The <see cref="CbusEventCallback"/> to register.</param>
        public void RegisterCbusEvent<T>(ushort deviceNumber, CbusEventCallback callback)
            where T : class, ICbusAccessoryShortEvent =>
            RegisterCallback(CbusEventStateKey.Create<T>(deviceNumber), callback);

        private void RegisterCallback(CbusEventStateKey key, CbusEventCallback callback) =>
            this.cbusEventCallbacks[key] = callback;

        #endregion

        #region Support routines

        /// <summary>
        /// Find the key to the <see cref="cbusEventCallbacks"/> that corresponds to the specified
        /// <paramref name="cbusAccessoryEvent"/>.
        /// </summary>
        /// <param name="cbusAccessoryEvent">An <see cref="ICbusAccessoryEvent"/> object.</param>
        /// <returns>The corresponding <see cref="CbusEventStateKey"/> or null if there is none that match.</returns>
        private CbusEventStateKey? FindKey(ICbusAccessoryEvent cbusAccessoryEvent)
        {
            var key = CbusEventStateKey.CreateFrom(cbusAccessoryEvent);
            return this.cbusEventCallbacks.ContainsKey(key) ? key : null;
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
            return key is null ? null : this.cbusEventCallbacks[key];
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
                        "{datetime} Failed to run callback for {length} {onOff} event from {nn}{en}.",
                            result.DateTime.ToString(@"yyyy\-MM\-dd HH\:mm\:ss.fff"),
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
                callback(cbusAccessoryEvent);
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
        private class CbusEventStateKey :
            IComparable<CbusEventStateKey>,
            IEquatable<CbusEventStateKey>
        {
            #region Properties

            public ushort EventNumber { get; set; }
            public bool IsShortEvent { get; set; }
            public ushort NodeNumber { get; set; }
            public bool IsOnEvent { get; set; }

            #endregion

            #region Static create methods

            /// <summary>
            /// Create a <see cref="CbusEventStateKey"/> for a Short Event from the specified
            /// <paramref name="deviceNumber"/> and <paramref name="state"/>.
            /// </summary>
            /// <param name="deviceNumber">A <see cref="ushort"/> containing the Device Number.</param>
            /// <param name="state">True if the state is ON; false if the state is OFF.</param>
            /// <returns>A <see cref="CbusEventStateKey"/> representing a Short Event.</returns>
            public static CbusEventStateKey Create(ushort deviceNumber, bool state)
            {
                var result =
                    new CbusEventStateKey
                    {
                        EventNumber = deviceNumber,
                        IsShortEvent = true,
                        IsOnEvent = state,
                    };
                return result;
            }

            /// <summary>
            /// Create a <see cref="CbusEventStateKey"/> for a Long Event from the specified
            /// <paramref name="nodeNumber"/>, <paramref name="eventNumber"/> and <paramref name="state"/>.
            /// </summary>
            /// <param name="nodeNumber">A <see cref="ushort"/> containing the Node Number.</param>
            /// <param name="eventNumber">A <see cref="ushort"/> containing the Event Number.</param>
            /// <param name="state">True if the state is ON; false if the state is OFF.</param>
            /// <returns>A <see cref="CbusEventStateKey"/> representing a Long Event.</returns>
            public static CbusEventStateKey Create(ushort nodeNumber, ushort eventNumber, bool state)
            {
                var result =
                    new CbusEventStateKey
                    {
                        NodeNumber = nodeNumber,
                        EventNumber = eventNumber,
                        IsShortEvent = false,
                        IsOnEvent = state,
                    };
                return result;
            }

            /// <summary>
            /// Create a <see cref="CbusEventStateKey"/> for a Short Event from <typeparamref name="T"/>
            /// and the specified <paramref name="deviceNumber"/>.
            /// </summary>
            /// <typeparam name="T">The type of CBUS event.</typeparam>
            /// <param name="deviceNumber">A <see cref="ushort"/> containing the Device Number.</param>
            /// <returns>A <see cref="CbusEventStateKey"/> representing a Short Event.</returns>
            /// <exception cref="InvalidOperationException">If it is not possible to determine whether the event is ON or OFF.</exception>
            public static CbusEventStateKey Create<T>(ushort deviceNumber)
                where T : class, ICbusAccessoryShortEvent
            {
                var type = typeof(T);
                var isOnEvent = type.GetInterface(nameof(ICbusAccessoryOnEvent)) is not null;
                var isOffEvent = type.GetInterface(nameof(ICbusAccessoryOffEvent)) is not null;
                if (isOnEvent == isOffEvent)
                    throw new InvalidOperationException
                        ($"Unable to determine whether {type.Name} is for an ON event or an OFF event.");
                return Create(deviceNumber, isOnEvent);
            }

            /// <summary>
            /// Create a <see cref="CbusEventStateKey"/> for a Long Event from <typeparamref name="T"/>
            /// and the specified <paramref name="nodeNumber"/> and <paramref name="eventNumber"/>.
            /// </summary>
            /// <typeparam name="T">The type of CBUS event.</typeparam>
            /// <param name="nodeNumber">A <see cref="ushort"/> containing the Node Number.</param>
            /// <param name="eventNumber">A <see cref="ushort"/> containing the Event Number.</param>
            /// <returns>A <see cref="CbusEventStateKey"/> representing a Long Event.</returns>
            /// <exception cref="InvalidOperationException">If it is not possible to determine whether the event is ON or OFF.</exception>
            public static CbusEventStateKey Create<T>(ushort nodeNumber, ushort eventNumber)
                where T : class, ICbusAccessoryLongEvent
            {
                var type = typeof(T);
                var isOnEvent = type.GetInterface(nameof(ICbusAccessoryOnEvent)) is not null;
                var isOffEvent = type.GetInterface(nameof(ICbusAccessoryOffEvent)) is not null;
                if (isOnEvent == isOffEvent)
                    throw new InvalidOperationException
                        ($"Unable to determine whether {type.Name} is for an ON event or an OFF event.");
                return Create(nodeNumber, eventNumber, isOnEvent);
            }

            public static CbusEventStateKey CreateFrom(ICbusAccessoryEvent cbusAccessoryEvent)
            {
                var nodeNumber = (cbusAccessoryEvent as IHasNodeNumber)?.NodeNumber;
                var eventNumber = (cbusAccessoryEvent as IHasEventNumber)?.EventNumber;
                var deviceNumber = (cbusAccessoryEvent as IHasDeviceNumber)?.DeviceNumber;
                var isOnEvent = cbusAccessoryEvent is ICbusAccessoryOnEvent;
                var isOffEvent = cbusAccessoryEvent is ICbusAccessoryOffEvent;

                if (isOnEvent == isOffEvent)
                    throw new InvalidOperationException(
                        $"Unable to determine whether {cbusAccessoryEvent.GetType().Name} is an ON event or an OFF event.");

                if (nodeNumber.HasValue && eventNumber.HasValue)
                    return Create(nodeNumber.Value, eventNumber.Value, isOnEvent);

                if (deviceNumber.HasValue)
                    return Create(deviceNumber.Value, isOnEvent);

                throw new InvalidOperationException(
                    $"Unable to determine whether {cbusAccessoryEvent.GetType().Name} is a Long Event or a Short Event.");
            }

            #endregion

            #region IComparable support

            public int CompareTo(CbusEventStateKey? other)
            {
                if (other is null) return 1;
                if (ReferenceEquals(this, other)) return 0;

                if (this.IsShortEvent && !other.IsShortEvent) return -1;
                if (!this.IsShortEvent && other.IsShortEvent) return 1;

                if (this.IsShortEvent)
                {
                    if (this.EventNumber == other.EventNumber)
                        return this.IsOnEvent.CompareTo(other.IsOnEvent);
                    return this.EventNumber.CompareTo(other.EventNumber);
                }

                if (this.NodeNumber == other.NodeNumber)
                {
                    if (this.EventNumber == other.EventNumber)
                        return this.IsOnEvent.CompareTo(other.IsOnEvent);
                    return this.EventNumber.CompareTo(other.EventNumber);
                }

                return this.NodeNumber.CompareTo(other.NodeNumber);
            }

            #endregion

            #region IEquatable support

            public bool Equals(CbusEventStateKey? other)
            {
                if (other is null) return false;
                if (ReferenceEquals(this, other)) return true;

                if (this.IsShortEvent != other.IsShortEvent) return false;
                if (this.EventNumber != other.EventNumber) return false;
                if (this.IsOnEvent != other.IsOnEvent) return false;

                if (this.IsShortEvent) return true;

                return this.NodeNumber == other.NodeNumber;
            }

            #endregion

            #region Overrides

            public override bool Equals(object? obj) => Equals(obj as CbusEventStateKey);

            public override int GetHashCode() => 
                HashCode.Combine(this.NodeNumber, this.EventNumber, this.IsShortEvent, this.IsOnEvent);

            public override string ToString() =>
                this.IsShortEvent
                    ? $"0x{this.EventNumber:X2} = {(this.IsOnEvent ? "ON " : "OFF")}"
                    : $"0x{this.NodeNumber:X2}:{this.EventNumber:X2} = {(this.IsOnEvent ? "ON " : "OFF")}";

            #endregion
        }

        /// <summary>
        /// A class to hold error information for when a callback throws an exception.
        /// </summary>
        private class CbusEventError
        {
            public Exception Exception { get; }

            public DateTime DateTime { get; }

            public CbusEventStateKey Key { get; }

            public CbusEventError(Exception exception, DateTime dateTime, CbusEventStateKey key)
            {
                this.Exception = exception;
                this.DateTime = dateTime;
                this.Key = key;
            }

            public CbusEventError(Exception exception, CbusEventStateKey key)
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
