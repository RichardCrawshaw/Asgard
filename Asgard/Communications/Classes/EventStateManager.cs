using System;
using System.Collections.Concurrent;
using System.Threading;
using Asgard.Data;
using Asgard.Data.Interfaces;
using Microsoft.Extensions.Logging;

namespace Asgard.Communications
{
    /// <summary>
    /// Class to track the state of CBUS events. Use manager[NodeNumber, EventNumber] to retrieve
    /// the state of Long Events, and manager[DeviceNumber] to retrieve the state of Short Events.
    /// </summary>
    public class EventStateManager
    {
        #region Fields

        private const int QUEUE_RETRY_TIME = 500;

        private readonly ICbusMessenger cbusMessenger;

        private readonly ILogger logger;

        private readonly ConcurrentDictionary<CbusEventKey, bool?> cbusEventStates = new();

        private readonly CancellationTokenSource cancellationTokenSource = new();

        #endregion

        #region Properties

        protected CancellationToken CancellationToken => this.cancellationTokenSource.Token;

        /// <summary>
        /// Gets the state of the CBUS Long Event with the specified <paramref name="nodeNumber"/>
        /// and <paramref name="eventNumber"/>.
        /// </summary>
        /// <param name="nodeNumber">A <see cref="ushort"/> that contains the Node Number.</param>
        /// <param name="eventNumber">A <see cref="ushort"/> that contains the Event Number.</param>
        /// <returns>True if the event is ON; false if the event is OFF; null if the state is unknown.</returns>
        public bool? this[ushort nodeNumber, ushort eventNumber]
        {
            get
            {
                var key = CbusEventKey.Create(nodeNumber, eventNumber);
                if (this.cbusEventStates.ContainsKey(key))
                    return this.cbusEventStates[key];
                return null;
            }
        }

        /// <summary>
        /// Gets the state of the CBUS Short event with the specified <paramref name="deviceNumber"/>.
        /// </summary>
        /// <param name="deviceNumber">A <see cref="ushort"/> that contains the Device Number.</param>
        /// <returns>True if the event is ON; false if the event is OFF; null if the state is unknown.</returns>
        public bool? this[ushort deviceNumber]
        {
            get
            {
                var key=CbusEventKey.Create(deviceNumber);
                if (this.cbusEventStates.ContainsKey(key))
                    return this.cbusEventStates[key];
                return null;
            }
        }

        #endregion

        #region Constructors

        public EventStateManager(ICbusMessenger cbusMessenger, ILogger logger)
        {
            this.cbusMessenger = cbusMessenger;
            this.logger = logger;

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
        // ~EventStateManager()
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

        #region Support routines

        /// <summary>
        /// Update the state of the event that corresponds to the specified 
        /// <paramref name="cbusAccessoryEvent"/>.
        /// </summary>
        /// <param name="cbusAccessoryEvent">An <see cref="ICbusAccessoryEvent"/> object.</param>
        private void UpdateEventState(ICbusAccessoryEvent cbusAccessoryEvent)
        {
            var nodeNumber =
                cbusAccessoryEvent is IHasNodeNumber hasNodeNumber
                    ? hasNodeNumber.NodeNumber
                    : (ushort?)null;
            var eventNumber =
                cbusAccessoryEvent is IHasEventNumber hasEventNumber
                    ? hasEventNumber.EventNumber
                    : (ushort?)null;
            var deviceNumber =
                cbusAccessoryEvent is IHasDeviceNumber hasDeviceNumber
                    ? hasDeviceNumber.DeviceNumber :
                    (ushort?)null;

            if (nodeNumber.HasValue && eventNumber.HasValue)
            {
                var key = CbusEventKey.Create(nodeNumber.Value, eventNumber.Value);
                this.cbusEventStates[key] = cbusAccessoryEvent.IsOnEvent;
            }
            else if (deviceNumber.HasValue)
            {
                var key = CbusEventKey.Create(deviceNumber.Value);
                this.cbusEventStates[key] = cbusAccessoryEvent.IsOnEvent;
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

            UpdateEventState(cbusAccessoryEvent);
        }

        #endregion

        #region Nested classes

        /// <summary>
        /// A class to hold event information: Node Number, Event (or Device) Number, whether it is
        /// long or short. It DOES NOT hold event state.
        /// </summary>
        private class CbusEventKey :
            IComparable<CbusEventKey>,
            IEquatable<CbusEventKey>
        {
            #region Properties

            public ushort EventNumber { get; set; }
            public bool IsShortEvent { get; set; }
            public ushort NodeNumber { get; set; }

            #endregion

            #region Static create methods

            /// <summary>
            /// Create a <see cref="CbusEventKey"/> for a Short Event from the specified 
            /// <paramref name="deviceNumber"/>.
            /// </summary>
            /// <param name="deviceNumber">A <see cref="ushort"/> containing the Device Number.</param>
            /// <returns>A <see cref="CbusEventKey"/> representing a Short Event.</returns>
            public static CbusEventKey Create(ushort deviceNumber)
            {
                return
                    new CbusEventKey
                    {
                        EventNumber = deviceNumber,
                        IsShortEvent = true,
                        NodeNumber = 0,
                    };
            }

            /// <summary>
            /// Create a <see cref="CbusEventKey"/> for a Long Event from the specified 
            /// <paramref name="nodeNumber"/> and <paramref name="eventNumber"/>.
            /// </summary>
            /// <param name="nodeNumber">A <see cref="ushort"/> containing the Node Number.</param>
            /// <param name="eventNumber">A <see cref="ushort"/> containing the Event Number.</param>
            /// <returns>A <see cref="CbusEventKey"/> representing a Long Event.</returns>
            public static CbusEventKey Create(ushort nodeNumber, ushort eventNumber)
            {
                return
                    new CbusEventKey
                    {
                        EventNumber = eventNumber,
                        IsShortEvent = false,
                        NodeNumber = nodeNumber,
                    };
            }

            #endregion

            #region IComparable support

            public int CompareTo(CbusEventKey? other)
            {
                if (other is null) return 1;
                if (ReferenceEquals(this, other)) return 0;

                if (this.IsShortEvent && !other.IsShortEvent) return -1;
                if (!this.IsShortEvent && other.IsShortEvent) return 1;

                if (this.IsShortEvent)
                    return this.EventNumber.CompareTo(other.EventNumber);

                if (this.NodeNumber == other.NodeNumber)
                    return this.EventNumber.CompareTo(other.EventNumber);
                return this.NodeNumber.CompareTo(other.NodeNumber);
            }

            #endregion

            #region IEquatable support

            public bool Equals(CbusEventKey? other)
            {
                if (other is null) return false;
                if (ReferenceEquals(this, other)) return true;

                if (this.IsShortEvent != other.IsShortEvent) return false;
                if (this.EventNumber != other.EventNumber) return false;
                if (!this.IsShortEvent && this.NodeNumber != other.NodeNumber)
                    return false;
                return true;
            }

            #endregion

            #region Overrides

            public override bool Equals(object? obj) => Equals(obj as CbusEventKey);

            public override int GetHashCode() =>
                HashCode.Combine(this.NodeNumber, this.EventNumber, this.IsShortEvent);

            public override string ToString() =>
                this.IsShortEvent
                    ? $"0x{this.EventNumber:X2}"
                    : $"0x{this.NodeNumber:X2}:{this.EventNumber:X2}";

            #endregion
        }

        #endregion
    }
}
