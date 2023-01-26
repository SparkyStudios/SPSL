using Antlr4.Runtime;
using SPSL.Language;
using SPSL.Language.AST;
using SPSL.Language.AST.Visitors;

var spsl = """
namespace SparkyStudios\Tests\Files;

// Can import other namespaces
use Another\Shaders\Library;

// Shader/Shader fragments interfaces
interface DiffuseSetup
{
  color4 diffuse_color();
}

// Shader fragment - Contains a collection of reusable and scoped functions
fragment Common
{
  void swap(inout int left, inout int right)
  {
    int k = left;
    left = right;
    right = k;
  }
}

// A shader fragment can extend another one and implement interfaces
fragment MaterialStage extends Common implements DiffuseSetup
{
  color4 diffuse_color()
  {
    float r = 1, g = 1, b = 1, a = 1;
    return color4(r, g, b, a);
  }

  void Process()
  {
    int[4] _out;

    float test = ((3.2f * 4) + (5 / 2)) ^ 2u;
    test += (float)3;

    _out[0] = test;
    bool b = true && false;
    _out[1] = b ? test : test * 2;

    if (b)
    {
      this.swap(0, 1);
    }
    else if (!b)
    {
      this.swap(1, 2);
    }
    elif (b and b)
    {
      this.swap(2, 3);
    }
    else
    {
      this.swap((int)_out[0], (int)_out[1]);
    }
  }
}

// Another shader fragment example
fragment LightningStage implements DiffuseSetup
{
  color4 diffuse_color()
  {
    return color4(0.0f);
  }

  vector4f light_first_pass()
  {
    return vector4f(0);
  }
}

// Enumeration - Enumeration values are always numbers
type LightType as enum
{
  // The first value is always 0 if not defined
  Ambient,
  // An enumeration value can be defined
  Directional = 3,
  // Undefined values are incremented from the last defined value
  Point, // = 4
  Spot, // = 5
}

// Structure - A structure can hold data from any type...
type LightState as struct
{
  // ...as wel as custom types
  LightType lightType;
  float intensity;
  color4 color;
  vector3f orientation;
}

// Another structure example
type DefaultVertexShaderInput as struct
{
  // Structure fields can have annotations
  @location(0) vector3f position;
  @location(1) color4 color;
  @location(2) vector3f normal;
  @location(3) vector2f uv_coordinates;
  @location(4) vector3f tangent;
  @location(5) vector3f bi_tangent;
}

type SimpleVertexShaderOutput as struct
{
  vector3f normal;
  vector2f uv;
  vector3f positionRelativeToWorld;
  vector4f positionRelativeToCamera;
  float shadowDistance;
  vector4f fragPosLightSpace;
  matrix3f tbn;
}

// An abstract shader - Abstract shaders are not emitted until extended
abstract shader ShaderBase
{
  SimpleVertexShaderOutput FillData(DefaultVertexShaderInput data)
  {
    SimpleVertexShaderOutput result;
    result.normal = normalize(data.normal);

    // Fill more data...

    return result;
  }

  int GetInt()
  {
    return 0;
  }
}

// Another abstract shader - This one extends the first abstract shader (ShaderBase)
// but since it is abstract, it does not emit any code.
abstract shader VertexShaderBase extends ShaderBase
{
  // A shader function can override its base class
  override int GetInt()
  {
    return 1;
  }
}

// A concrete vertex shader - Concrete shaders are emitted recursively (with base shaders)
vertex shader SimpleVertexShader extends VertexShaderBase
{
  // A shader (concrete or abstract) can import shader fragments
  use MaterialStage;
  use LightningStage;

  // Overridden function
  override int GetInt()
  {
    // An overridden function can call his base
    return base.GetInt() + 1;
  }

  // Functions can have annotations - The entry annotation marks the entry point of the shader.
  // This will be used mostly by the compiler. The translator doesn't care about this.
  @entryPoint
  void SimpleVertexShader()
  {
    // Conflicting names must be used through their respective scopes (namespace or shader fragment)
    // otherwise the emitted code can be invalid.
    color4 light_diffuse = LightningStage\diffuse_color();
    color4 mat_diffuse = MaterialStage\diffuse_color();

    int[] _out;

    float test = ((3.2f * 4) + (5 / 2)) ^ 2;
    test += (float)3;

    _out[0] = test;
    bool b = true && false;
    _out[1] = b ? test : test * 2;

    // Functions from imported shader fragments can be used
    swap((int)_out[0], (int)_out[1]);
  }
}
""";

// ---- Build AST

SPSLLexer lexer = new(new AntlrInputStream(spsl));

lexer.RemoveErrorListeners();

SPSLParser parser = new(new CommonTokenStream(lexer));
parser.RemoveErrorListeners();

ASTVisitor shaderVisitor = new();

AST ast = shaderVisitor.Visit(parser.file());

// ---- Translate to HLSL

SPSL.Translation.HLSL.Translator hlsl = new();

string code = hlsl.Translate(ast);

using var stream = new System.IO.StreamWriter("spsl.hlsl");
stream.Write(code);
