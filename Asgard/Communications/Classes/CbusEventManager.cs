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

        private readonly ICbusMessenger cbusMessenger;

        private readonly ILogger logger;

        private readonly ConcurrentDictionary<CbusEventKey, bool?> cbusEventStates = new();
        private readonly ConcurrentDictionary<CbusEventKey, CbusEventCallback> cbusEventCallbacks = new();
        private readonly EmptyingConcurrentQueue<Exception> cbusEventExceptionQueue = new();

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
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                this.IsDisposed = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
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

        public bool? GetState(ushort nodeNumber, ushort eventNumber)
        {
            var key =
                this.cbusEventStates.Keys
                    .Where(n => !n.IsShort)
                    .Where(n => n.NodeNumber == nodeNumber)
                    .Where(n => n.EventNumber == eventNumber)
                    .FirstOrDefault();
            if (key is null) return null;
            return this.cbusEventStates[key];
        }

        public bool? GetState(ushort eventNumber)
        {
            var key =
                this.cbusEventStates.Keys
                    .Where(n => n.IsShort)
                    .Where(n => n.EventNumber == eventNumber)
                    .FirstOrDefault();
            if (key is null) return null;
            return this.cbusEventStates[key];
        }

        public void RegisterCbusEvent<T>(ushort nodeNumber, ushort eventNumber, CbusEventCallback callback)
            where T : class, ICbusAccessoryEvent
        {
            var key = CbusEventKey.Create<T>(nodeNumber, eventNumber);
            this.cbusEventCallbacks[key] = callback;
        }

        #endregion

        #region Support routines

        private CbusEventKey? FindKey(ICbusAccessoryEvent cbusAccessoryEvent)
        {
            var result =
                this.cbusEventCallbacks.Keys
                    .Where(n => cbusAccessoryEvent.IsShortEvent == n.IsShort)
                    .Where(n => n.IsShort || n.NodeNumber == (cbusAccessoryEvent as IHasNodeNumber)?.NodeNumber)
                    .Where(n => (cbusAccessoryEvent.IsLongEvent && n.EventNumber == (cbusAccessoryEvent as IHasEventNumber)?.EventNumber) ||
                                (cbusAccessoryEvent.IsShortEvent && n.EventNumber == (cbusAccessoryEvent as IHasDeviceNumber)?.DeviceNumber))
                    .FirstOrDefault();
            return result;
        }

        private CbusEventCallback? GetRegisteredCallback(ICbusAccessoryEvent cbusAccessoryEvent)
        {
            var key = FindKey(cbusAccessoryEvent);
            if (key is null) return null;
            return this.cbusEventCallbacks[key];
        }

        private void ProcessQueue()
        {
            while (!this.CancellationToken.IsCancellationRequested)
            {
                while (!this.cbusEventExceptionQueue.IsEmpty)
                {
                    this.cbusEventExceptionQueue.TryDequeue(out var result);
                }

                this.cbusEventExceptionQueue.Trigger.Reset();
                this.cbusEventExceptionQueue.Trigger.Wait(500, this.CancellationToken);
            }
        }

        private void RunCallback(CbusEventCallback callback, ICbusAccessoryEvent cbusAccessoryEvent)
        {
            try
            {
                Task.Run(() => callback(cbusAccessoryEvent, this), this.CancellationToken);
            }
            catch (Exception ex)
            {
                this.logger.LogWarning(ex, "Failed to run callback for {0} {1} event from {2}{3}.",
                    cbusAccessoryEvent.IsShortEvent ? "short" : "long",
                    cbusAccessoryEvent.IsOnEvent ? "on" : "off",
                    cbusAccessoryEvent.IsShortEvent ? ""
                                                    : ((cbusAccessoryEvent as IHasNodeNumber)?.NodeNumber.ToString("X2") + " "),
                    cbusAccessoryEvent.IsShortEvent ? (cbusAccessoryEvent as IHasDeviceNumber)?.DeviceNumber.ToString("X2")
                                                    : (cbusAccessoryEvent as IHasEventNumber)?.EventNumber.ToString("X2"));
                this.cbusEventExceptionQueue.Enqueue(ex);
            }
        }

        #endregion

        #region Event handler routines

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

        private class CbusEventKey
        {
            public bool IsShort { get; set; }
            public ushort NodeNumber { get; set; }
            public ushort EventNumber { get; set; }

            public static CbusEventKey Create<T>(ushort nodeNumber, ushort eventNumber)
            {
                return
                    typeof(T).GetInterface(nameof(ICbusAccessoryShortEvent)) is not null
                        ? new CbusEventKey
                        {
                            IsShort = true,
                            NodeNumber = nodeNumber,
                        }
                        : new CbusEventKey
                        {
                            IsShort = false,
                            NodeNumber = nodeNumber,
                            EventNumber = eventNumber
                        };
            }
        }

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
        }

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
