namespace Relate.Smtp.Pop3Host.Protocol;

public record Pop3Command
{
    public string Name { get; init; } = string.Empty;
    public string[] Arguments { get; init; } = Array.Empty<string>();

    public static Pop3Command Parse(string line)
    {
        var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return new Pop3Command
        {
            Name = parts.Length > 0 ? parts[0].ToUpperInvariant() : string.Empty,
            Arguments = parts.Skip(1).ToArray()
        };
    }
}
