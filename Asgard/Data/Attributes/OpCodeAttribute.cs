using System;

namespace Asgard.Data
{
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Method, 
        AllowMultiple = false, 
        Inherited = true)]
	public partial class OpCodeAttribute : Attribute
	{
		/// <summary>
		/// Gets and sets the op-code.
		/// </summary>
		public string Code { get; set; }

		/// <summary>
		/// Gets and sets the description.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Gets and sets the group.
		/// </summary>
		public string Group { get; set; }

		/// <summary>
		/// Gets and sets the op-code name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets and sets the op-code number.
		/// </summary>
		public byte Number { get; set; }

		/// <summary>
		/// Gets and sets the op-code priority.
		/// </summary>
		public int Priority { get; set; }
	}
}
