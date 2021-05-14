
using System;
/// <summary>
/// Represents a variable that can easily be modified, and reverted back to a previous state
/// </summary>
public interface INitroVariable
{
	/// <summary>
	/// The base value 
	/// </summary>
	object BaseValue { get; set; }
	/// <summary>
	/// The current value of the variable
	/// </summary>
	object Value { get;}
	/// <summary>
	/// How many variable modifiers are currently applied
	/// </summary>
	int ModifiersApplied { get; }
	/// <summary>
	/// The type of the value
	/// </summary>
	Type ValueType { get; }
}
