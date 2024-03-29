﻿namespace SPSL.Language.Parsing.AST;

/// <summary>
/// Represents any kind of data type.
/// </summary>
public interface IDataType : INode, ISemanticallyEquatable, IEquatable<IDataType>
{
    /// <summary>
    /// Define if this data type is an array type.
    /// </summary>
    bool IsArray { get; }

    /// <summary>
    /// Define the size of the array if <see cref="IsArray"/> is <c>true</c>.
    /// </summary>
    uint? ArraySize { get; }
}