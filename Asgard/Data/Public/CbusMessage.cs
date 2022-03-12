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
    }
}
