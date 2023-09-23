namespace SPSL.CommandLine.Utils;

public class PermutationValue
{
    public string Name { get; }
    public string Value { get; }

    public PermutationValue(string option)
    {
        string[] parts = option.Split('=', 2);

        Name = parts[0].Trim();
        Value = parts[1].Trim();
    }

    public override string ToString()
    {
        return $"{Name}={Value}";
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name.GetHashCode(), Value.GetHashCode());
    }
}