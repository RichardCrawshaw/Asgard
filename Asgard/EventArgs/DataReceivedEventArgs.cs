using System;

namespace Asgard.Comms
{
    public class DataReceivedEventArgs : EventArgs
    {
        public byte[] Data { get; }

        private DataReceivedEventArgs(int length) : base() => this.Data = new byte[length];

        public DataReceivedEventArgs(byte[] data) : this(data.Length) => data.CopyTo(this.Data, 0);
    }
}
