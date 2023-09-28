using System.Collections;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace SPSL.Serialization.Reflection;

/// <summary>
/// Binding to a state.
/// </summary>
[DataContract]
[DebuggerDisplay("{Mode} State ({StateName})")]
public class MaterialStateBinding
{
    /// <summary>
    /// Specifies what kind of value the state is storing.
    /// </summary>
    public enum ValueMode : byte
    {
        /// <summary>
        /// The state is storing a map of key-value pairs.
        /// </summary>
        Map = 1,

        /// <summary>
        /// The state is single string value.
        /// </summary>
        String = 2,
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialStateBinding"/> class.
    /// </summary>
    /// <param name="stateName">Name of the state.</param>
    /// <param name="description">The description.</param>
    public MaterialStateBinding(string stateName, Hashtable description)
    {
        StateName = stateName;
        Value = string.Empty;
        Description = description;
        Mode = ValueMode.Map;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialStateBinding"/> class.
    /// </summary>
    /// <param name="stateName">Name of the state.</param>
    /// <param name="value">Value of the state.</param>
    public MaterialStateBinding(string stateName, string value)
    {
        StateName = stateName;
        Value = value;
        Description = new();
        Mode = ValueMode.String;
    }

    /// <summary>
    /// The state name.
    /// </summary>
    [DataMember(Order = 0)] public string StateName;

    /// <summary>
    /// The description of this state.
    /// </summary>
    [DataMember(Order = 1)] public Hashtable Description;

    /// <summary>
    /// The value of this state. This is empty when the 
    /// </summary>
    [DataMember(Order = 2)] public string Value;

    /// <summary>
    /// Defines which type of value is used for this state.
    /// </summary>
    [DataMember(Order = 3)] public ValueMode Mode;
}