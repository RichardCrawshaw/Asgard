namespace Asgard.Data
{
    public interface ICbusOpCode
    {
		/// <summary>
		/// Gets the code.
		/// </summary>
		string Code { get; }

		/// <summary>
		/// Gets the number of data-bytes.
		/// </summary>
		int DataLength { get; }

		/// <summary>
		/// Gets the description.
		/// </summary>
		string Description { get; }

		/// <summary>
		/// Gets the group.
		/// </summary>
		OpCodeGroup Group { get; }

		/// <summary>
		/// Gets the op-code number.
		/// </summary>
		byte Number { get; }

		/// <summary>
		/// Gets the name.
		/// </summary>
		string OpcodeName { get; }

		/// <summary>
		/// Gets the op-code priority.
		/// </summary>
		int Priority { get; }

		/// <summary>
		/// Gets the underlying CBUS message.
		/// </summary>
		ICbusMessage Message { get; }
	}
}
