using System;
using System.Linq;

namespace Asgard.Comms
{
    /// <summary>
    /// Abstract class to handle the processing of CBUS messages.
    /// </summary>
    public abstract class CbusProcessor :
        ICbusProcessor
    {
        #region Fields

        /// <summary>
        /// The grid connect processor.
        /// </summary>
        private readonly IGridConnectProcessor gridConnectProcessor;

        #endregion

        #region Properties

        /// <summary>
        /// Gets whether the current instance is connected.
        /// </summary>
        public bool IsConnected => this.gridConnectProcessor.IsConnected;

        /// <summary>
        /// Gets whether the <see cref="CbusProcessor"/> has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        public int PortNumber { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="CbusProcessor"/> using the specified
        /// <paramref name="gridConnectProcessor"/>.
        /// </summary>
        /// <param name="gridConnectProcessor">An <see cref="IGridConnectProcessor"/> object.</param>
        public CbusProcessor(IGridConnectProcessor gridConnectProcessor)
        {
            if (gridConnectProcessor is null)
                throw new ArgumentNullException(nameof(gridConnectProcessor));

            this.gridConnectProcessor = gridConnectProcessor;
        }

        #endregion

        #region IDisposable support

        protected virtual void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                    if (this.gridConnectProcessor?.IsConnected ?? false)
                        this.gridConnectProcessor?.Disconnect();
                    this.gridConnectProcessor?.Dispose();
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                this.IsDisposed = true;
            }
        }

        // // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~CbusProcessor()
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
        /// Connect the curent instance.
        /// </summary>
        public void Connect()
        {
            this.gridConnectProcessor.Connect();
            this.gridConnectProcessor.MessageReceived += GridConnectProcessor_MessageReceived;
        }

        /// <summary>
        /// Disconnect the current instance.
        /// </summary>
        public void Disconnect()
        {
            this.gridConnectProcessor.Disconnect();
            this.gridConnectProcessor.MessageReceived -= GridConnectProcessor_MessageReceived;
        }

        #endregion

        #region Support routines

        protected static (string OpCode, byte[] Data) GetMessage(byte[] payload)
        {
            if (Enum.IsDefined(typeof(merg.cbus.OpCodes), (int)payload[0]))
            {
                var result = (OpCode: Enum.GetName(typeof(merg.cbus.OpCodes), payload[0]), Data: payload);
                return result;
            }

            return (OpCode: null, Data: payload);
        }

        protected static string InterpretMessage((string OpCode, byte[] Data) data)
        {
            if (data.OpCode is null)
                return $"Unknown message: {string.Join(" ", data.Data.Select(d => $"0x{d:X2}"))}";

            var result =
                $"0x{data.Data[0]:X2} {data.OpCode} {string.Join(" ", data.Data.Skip(1).Select(d => $"0x{d:X2}"))}";
            return result;
        }

        #endregion

        #region Event handler routines

        /// <summary>
        /// Event handler routine for the <see cref="GridConnectProcessor"/> <see cref="GridConnectProcessor.MessageReceived"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> for the event.</param>
        protected virtual void GridConnectProcessor_MessageReceived(object sender, GridConnectMessageEventArgs e)
        {
            // Add an override to provide the required behaviour.
        }

        #endregion
    }
}
