using CommandLine;

namespace SPSL.CommandLine;

[Verb("material", HelpText = "Compiles SPSL material into binary code for use into Spark Engine.")]
public class MaterialOptions : ShadingOptions
{
}