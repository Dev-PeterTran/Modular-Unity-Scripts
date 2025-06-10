using System;

namespace CustomAttributes {

	/// <summary>
	/// Forces any numerical value to be a positive numerical value.
	/// </sumary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class AbsoluteValueAttribute : ValidatorAttribute { }
}