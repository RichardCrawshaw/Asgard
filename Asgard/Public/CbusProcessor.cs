using System;
using System.Linq;
using NLog;

namespace Asgard
{
    /// <summary>
    /// Abstract class to handle the processing of CBUS messages.
    /// </summary>
    public abstract class CbusProcessor :
        ICbusProcessor
    {
        #region Fields

        /// <summary>
        /// Logger for standard program logging.
        /// </summary>
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The settings object.
        /// </summary>
        private readonly ISettings settings;

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
        public CbusProcessor(ISettings settings, IGridConnectProcessor gridConnectProcessor)
        {
            logger.Trace(() => nameof(CbusProcessor));
            if (gridConnectProcessor is null)
                throw new ArgumentNullException(nameof(gridConnectProcessor));

            this.settings = settings;
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
            logger.Trace(() => nameof(Connect));

            this.gridConnectProcessor.Connect();
            this.gridConnectProcessor.MessageReceived += GridConnectProcessor_MessageReceived;

            logger.Debug("Connected.");
        }

        /// <summary>
        /// Disconnect the current instance.
        /// </summary>
        public void Disconnect()
        {
            logger.Trace(() => nameof(Disconnect));

            this.gridConnectProcessor.Disconnect();
            this.gridConnectProcessor.MessageReceived -= GridConnectProcessor_MessageReceived;

            logger.Debug("Disconnected.");
        }

        #endregion

        #region Support routines

        protected static (string OpCode, byte[] Data) GetMessage(byte[] payload)
        {
            logger.Trace(() => nameof(GetMessage));

            if (Enum.IsDefined(typeof(merg.cbus.OpCodes), (int)payload[0]))
            {
                var result = (OpCode: Enum.GetName(typeof(merg.cbus.OpCodes), payload[0]), Data: payload);
                return result;
            }

            return (OpCode: null, Data: payload);
        }

        protected static string InterpretMessage((string OpCode, byte[] Data) data)
        {
            logger.Trace(() => nameof(InterpretMessage));

            if (data.OpCode is null)
                return $"Unknown message: {string.Join(" ", data.Data.Select(d => $"0x{d:X2}"))}";

            var result =
                $"0x{data.Data[0]:X2} {data.OpCode} {string.Join(" ", data.Data.Skip(1).Select(d => $"0x{d:X2}"))}";
            return result;
        }

        /// <summary>
        /// Interpret the specified <paramref name="data"/> as a descriptive CBUS message.
        /// </summary>
        /// <param name="data">The data of the CBUS message.</param>
        /// <returns>A string containing the descriptive message.</returns>
        private static string InterpretMessage(byte[] data)
        {
            logger.Trace(() => nameof(InterpretMessage));

            if (Enum.IsDefined(typeof(merg.cbus.OpCodes), (int)data[0]))
            {
                var result = $"0X{data[0]:X2} {Enum.GetName(typeof(merg.cbus.OpCodes), data[0])} {string.Join(" ", data.Skip(1).Select(n => $"0x{n:X2}"))}";
                return result;
            }

            return $"Unknown message: {string.Join(" ", data.Select(n => $"0X{n:X2}"))}";
        }

        #endregion

        #region Event handler routines

        /// <summary>
        /// Event handler routine for the <see cref="GridConnectProcessor"/> <see cref="GridConnectProcessor.MessageReceived"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> for the event.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0022:Use expression body for methods",
            Justification = "Virtual method; want to have comment block within the braces.")]
        protected virtual void GridConnectProcessor_MessageReceived(object sender, GridConnectMessageEventArgs e)
        {
            logger.Trace(() => nameof(GridConnectProcessor_MessageReceived));

            // Add an override to provide the required behaviour.
        }

        #endregion
    }
}
