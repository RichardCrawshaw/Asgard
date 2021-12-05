using System;

namespace Asgard.Data
{

    public partial class OpCodeData
    {
        #region Abstract properties

        /// <summary>
        /// Gets the code.
        /// </summary>
        public abstract string Code { get; }

        /// <summary>
        /// Gets the number of data-bytes.
        /// </summary>
        public abstract int DataLength { get; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Gets the group.
        /// </summary>
        public abstract OpCodeGroup Group { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the op-code number.
        /// </summary>
        public abstract byte Number { get; }

        /// <summary>
        /// Gets the op-code priority.
        /// </summary>
        public abstract int Priority { get; }

        /// <summary>
        /// Gets the underlying CBUS message.
        /// </summary>
        public ICbusMessage Message { get; }

        #endregion

        #region Constructors

        protected OpCodeData(ICbusMessage message) => this.Message = message;

        #endregion

        #region Convert methods

        protected void ConvertFromBool(int byteIndex, int bitIndex, bool value)
        {
            if (value)
            {
                var mask = (byte)(bitIndex switch
                {
                    0 => 0b_0000_0001,
                    1 => 0b_0000_0010,
                    2 => 0b_0000_0100,
                    3 => 0b_0000_1000,
                    4 => 0b_0001_0000,
                    5 => 0b_0010_0000,
                    6 => 0b_0100_0000,
                    7 => 0b_1000_0000,
                    _ => 0b_0000_0000,
                });
                this.Message[byteIndex] = (byte)(this.Message[byteIndex] | mask);
            }
            else
            {
                var mask = (byte)(bitIndex switch
                {
                    0 => 0b_1111_1110,
                    1 => 0b_1111_1101,
                    2 => 0b_1111_1011,
                    3 => 0b_1111_0111,
                    4 => 0b_1110_1111,
                    5 => 0b_1101_1111,
                    6 => 0b_1011_1111,
                    7 => 0b_0111_1111,
                    _ => 0b_1111_1111,
                });
                this.Message[byteIndex] = (byte)(this.Message[byteIndex] & mask);
            }
        }

        protected void ConvertFromByte(int byteIndex, byte value)
        {
            this.Message[byteIndex] = value;
        }

        protected void ConvertFromChar(int byteIndex, char value)
        {
            this.Message[byteIndex] = (byte)value;
        }

        protected void ConvertFromInt(int[] byteIndexes, int value)
        {
            var index1 = byteIndexes[0];
            var index2 = byteIndexes[1];
            var index3 = byteIndexes[2];
            var index4 = byteIndexes[3];

            this.Message[index1] = (byte)(value >> 00);
            this.Message[index2] = (byte)(value >> 08);
            this.Message[index3] = (byte)(value >> 16);
            this.Message[index4] = (byte)(value >> 24);
        }

        protected void ConvertFromShort(int[] byteIndexes, short value)
        {
            var index1 = byteIndexes[0];
            var index2 = byteIndexes[1];

            this.Message[index1] = (byte)(value >> 00);
            this.Message[index2] = (byte)(value >> 08);
        }

        protected void ConvertFromEnum<TEnum>(int byteIndex, TEnum value)
            where TEnum : struct, Enum
        {
            var type = Enum.GetUnderlyingType(typeof(TEnum));
            if (type == typeof(int) ||
                type == typeof(short) ||
                type == typeof(byte))
            {
                ConvertFromEnum(byteIndex, (value as int?).Value);
            }
        }

        protected void ConvertFromEnum<TEnum>(int byteIndex, int bitIndex, TEnum value)
    where TEnum : struct, Enum
        {
            var type = Enum.GetUnderlyingType(typeof(TEnum));
            if (type == typeof(int) ||
                type == typeof(short) ||
                type == typeof(byte))
            {
                ConvertFromEnum(byteIndex, bitIndex, (value as int?).Value);
            }
        }

        protected void ConvertFromEnum<TEnum>(int byteIndex, int[] bitIndexes, TEnum value)
            where TEnum : struct, Enum
        {
            var type = Enum.GetUnderlyingType(typeof(TEnum));
            if (type == typeof(int) ||
                type == typeof(short) ||
                type == typeof(byte))
            {
                ConvertFromEnum(byteIndex, bitIndexes, (value as int?).Value);
            }
        }

        protected void ConvertFromEnum(int byteIndex, int value)
        {
            this.Message[byteIndex] = (byte)value;
        }

        protected void ConvertFromEnum(int byteIndex, int bitIndex, int value)
        {
            var byteValue = this.Message[byteIndex];

  
            var enumMask = GetSetMask(0);

            if ((value & enumMask) == enumMask)
                byteValue |= GetSetMask(bitIndex);
            else
                byteValue &= GetClearMask(bitIndex);


            this.Message[byteIndex] = byteValue;
        }

        protected void ConvertFromEnum(int byteIndex, int[] bitIndexes, int value)
        {
            var byteValue = this.Message[byteIndex];

            for (var i = 0; i < bitIndexes.Length; i++)
            {
                var bitIndex = bitIndexes[i];
                var enumMask = GetSetMask(i);

                if ((value & enumMask) == enumMask)
                    byteValue |= GetSetMask(bitIndex);
                else
                    byteValue &= GetClearMask(bitIndex);
            }

            this.Message[byteIndex] = byteValue;
        }

        protected bool ConvertToBool(int byteIndex, int bitIndex)
        {
            var byteValue = this.Message[byteIndex];
            return bitIndex switch
            {
                0 => (byteValue & 0b_0000_0001) == 0b_0000_0001,
                1 => (byteValue & 0b_0000_0010) == 0b_0000_0010,
                2 => (byteValue & 0b_0000_0100) == 0b_0000_0100,
                3 => (byteValue & 0b_0000_1000) == 0b_0000_1000,
                4 => (byteValue & 0b_0001_0000) == 0b_0001_0000,
                5 => (byteValue & 0b_0010_0000) == 0b_0010_0000,
                6 => (byteValue & 0b_0100_0000) == 0b_0100_0000,
                7 => (byteValue & 0b_1000_0000) == 0b_1000_0000,
                _ => false,
            };
        }

        protected byte ConvertToByte(int byteIndex)
        {
            var byteValue = this.Message[byteIndex];
            return byteValue;
        }

        protected char ConvertToChar(int byteIndex)
        {
            var charValue = (char)this.Message[byteIndex];
            return charValue;
        }

        protected int ConvertToInt(int[] byteIndexes)
        {
            var index1 = byteIndexes[0];
            var index2 = byteIndexes[1];
            var index3 = byteIndexes[2];
            var index4 = byteIndexes[3];

            var value =
                this.Message[index4] << 24 +
                this.Message[index3] << 16 +
                this.Message[index2] << 08 +
                this.Message[index1];
            return value;
        }

        protected short ConvertToShort(int[] byteIndexes)
        {
            var index1 = byteIndexes[0];
            var index2 = byteIndexes[1];

            var value = (short)(
                this.Message[index2] << 8 +
                this.Message[index1]);
            return value;
        }

        protected TEnum ConvertToEnum<TEnum>(int byteIndex)
            where TEnum : struct, Enum
        {
            var type = Enum.GetUnderlyingType(typeof(TEnum));
            if (type == typeof(int) ||
                type == typeof(short) ||
                type == typeof(byte))
            {
                var result = ConvertToEnum(byteIndex) as TEnum?;
                if (result is not null)
                    return result.Value;
            }
            return default;
        }

        protected TEnum ConvertToEnum<TEnum>(int byteIndex, int bitIndex) 
            where TEnum : struct, Enum
        {
            var type = Enum.GetUnderlyingType(typeof(TEnum));
            if (type == typeof(int) ||
                type == typeof(short) ||
                type == typeof(byte))
            {
                var result = ConvertToEnum(byteIndex, bitIndex) as TEnum?;
                if (result is not null)
                    return result.Value;
            }
            return default;
        }
        protected TEnum ConvertToEnum<TEnum>(int byteIndex, int[] bitIndexes)
            where TEnum : struct, Enum
        {
            var type = Enum.GetUnderlyingType(typeof(TEnum));
            if (type == typeof(int) ||
                type == typeof(short) ||
                type == typeof(byte))
            {
                var result = ConvertToEnum(byteIndex, bitIndexes) as TEnum?;
                if (result is not null)
                    return result.Value;
            }
            return default;
        }

        protected int ConvertToEnum(int byteIndex)
        {
            return this.Message[byteIndex];
        }

        protected int ConvertToEnum(int byteIndex, int[] bitIndexes)
        {
            var value = ConvertToEnum(byteIndex);

            for(var i=0;i< bitIndexes.Length; i++)
            {
                var bitIndex = bitIndexes[i];
                var bitMask = GetSetMask(bitIndex);

                if ((value & bitMask) == bitMask)
                    value |= GetSetMask(i);
                else
                    value &= GetClearMask(i);
            }

            return value;
        }

        protected int ConvertToEnum(int byteIndex, int bitIndex)
        {
            var value = ConvertToEnum(byteIndex);

            var bitMask = GetSetMask(bitIndex);

            if ((value & bitMask) == bitMask)
                value |= GetSetMask(0);
            else
                value &= GetClearMask(0);


            return value;
        }

        #endregion

        #region Support routines

        private static byte GetSetMask(int bit)
        {
            return bit switch
            {
                0 => 0b_0000_0001,
                1 => 0b_0000_0010,
                2 => 0b_0000_0100,
                3 => 0b_0000_1000,
                4 => 0b_0001_0000,
                5 => 0b_0010_0000,
                6 => 0b_0100_0000,
                7 => 0b_1000_0000,
                _ => 0b_0000_0000,
            };
        }

        private static byte GetClearMask(int bit)
        {
            return bit switch
            {
                0 => 0b_1111_1110,
                1 => 0b_1111_1101,
                2 => 0b_1111_1011,
                3 => 0b_1111_0111,
                4 => 0b_1110_1111,
                5 => 0b_1101_1111,
                6 => 0b_1011_1111,
                7 => 0b_0111_1111,
                _ => 0b_1111_1111,
            };
        }

        #endregion
    }

}
