using System.Runtime.Serialization;
using System.Text;
using SPSL.Language.AST;

namespace SPSL.Serialization;

[DataContract]
public class ShaderByteCode
{
    /// <summary>
    /// The stage of this shader.
    /// </summary>
    public ShaderStage Stage;

    /// <summary>
    /// Gets the shader data.
    /// </summary>
    /// <remarks>
    /// The data represents the shader source code 
    /// </remarks>
    /// <value>
    /// The shader data.
    /// </value>
    public byte[] Data { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShaderByteCode"/> class.
    /// </summary>
    public ShaderByteCode()
    {
        Data = Array.Empty<byte>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShaderByteCode"/> class.
    /// </summary>
    /// <param name="data">The data.</param>
    public ShaderByteCode(byte[] data)
    {
        Data = data;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShaderByteCode"/> class.
    /// </summary>
    /// <param name="source">The shader source code.</param>
    public ShaderByteCode(string source)
    {
        Data = Encoding.UTF8.GetBytes(source);
    }

    /// <summary>
    /// Shallow clones this instance.
    /// </summary>
    /// <returns>ShaderBytecode.</returns>
    public ShaderByteCode Clone()
    {
        return (ShaderByteCode)MemberwiseClone();
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="ShaderByteCode"/> to <see cref="System.Byte[]"/>.
    /// </summary>
    /// <param name="shaderByteCode">The shader bytecode.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator byte[](ShaderByteCode shaderByteCode)
    {
        return shaderByteCode.Data;
    }

    /// <summary>
    /// Gets the data as a string.
    /// </summary>
    /// <returns>System.String.</returns>
    public string GetDataAsString()
    {
        return Encoding.UTF8.GetString(Data, 0, Data.Length);
    }
}