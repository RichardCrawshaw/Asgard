using System;
using System.Linq;

namespace Asgard.Data
{
    public abstract class CbusMessage :
        ICbusMessage
    {
        #region Properties and indexers

        public byte[] Data { get; }

        public bool IsExtended { get; }

        public int Length => this.Data.Length;

        public byte this[int index]
        {
            get => this.Data[index];
            set => this.Data[index] = value;
        }

        #endregion

        #region Constructors

        protected CbusMessage(byte[] data, bool isExtended)
        {
            this.IsExtended = isExtended;

            var length =
                data.Length > 0 & !this.IsExtended
                    ? (data[0] >> 5) + 1
                    : data.Length;
            this.Data = new byte[length];
            data.CopyTo(this.Data, 0);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create and return an <see cref="ICbusMessage"/> object from the specified
        /// <paramref name="data"/> and <paramref name="isExtended"/> flag.
        /// </summary>
        /// <param name="data">A <see cref="byte[]"/>.</param>
        /// <param name="isExtended">A <see cref="bool"/> that indicates whether the message is from an extended frame or not.</param>
        /// <returns>An <see cref="ICbusMessage"/> object.</returns>
        /// <remarks>
        /// This method should only be used when it does not matter whether the returned
        /// type is an <see cref="ICbusStandardMessage"/> or <see cref="ICbusExtendedMessage"/>.
        /// It is generally preferable to use either <seealso cref="CbusStandardMessage.Create(byte[])"/>
        /// or <seealso cref="CbusExtendedMessage.Create(byte[])"/>.
        /// </remarks>
        public static ICbusMessage Create(byte[] data, bool isExtended = false) => 
            isExtended
                ? new CbusExtendedMessage(data)
                : new CbusStandardMessage(data);

        #endregion

        #region Overrides

        public override string ToString() =>
            $"L:{this.Length} {string.Join(" ", this.Data.Select(d => $"0x{d:X2}"))}";

        #endregion
    }

    public class CbusStandardMessage : CbusMessage,
        ICbusStandardMessage
    {
        #region Fields

        private readonly Lazy<ICbusOpCode> lazyOpCode;

        private bool hasOpcode;

        #endregion

        #region Constructors

        internal CbusStandardMessage(byte[] data)
            : base(data, false)
        {
            this.lazyOpCode = new Lazy<ICbusOpCode>(() => GetCbusOpCode(data.Length));
        }

        #endregion

        #region Methods

        public static ICbusStandardMessage Create(byte[] data) => new CbusStandardMessage(data);

        public bool TryGetOpCode(out ICbusOpCode opCode)
        {
            opCode = this.lazyOpCode.Value;
            return this.hasOpcode;
        } 

        #endregion

        #region Lazy initalisation routines

        private ICbusOpCode GetCbusOpCode(int length)
        {
            this.hasOpcode = length > 0 && OpCodeData.IsOpCode(this.Data[0]) & !this.IsExtended;
            return this.hasOpcode ? OpCodeData.Create(this) : new GeneralAcknowledgement();
        }

        #endregion
    }

    public class CbusExtendedMessage : CbusMessage,
        ICbusExtendedMessage
    {
        #region Constructors

        internal CbusExtendedMessage(byte[] data)
            : base(data, true) { }

        #endregion

        #region Methods

        public static ICbusExtendedMessage Create(byte[] data) => new CbusExtendedMessage(data);

        #endregion
    }
}
