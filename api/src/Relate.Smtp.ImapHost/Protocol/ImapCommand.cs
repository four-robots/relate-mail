namespace Relate.Smtp.ImapHost.Protocol;

/// <summary>
/// Represents a parsed IMAP command with tag and arguments.
/// IMAP commands have format: tag command [arguments]
/// Example: A001 LOGIN user@example.com password
/// </summary>
public record ImapCommand
{
    /// <summary>
    /// Command tag for correlating responses (e.g., "A001")
    /// </summary>
    public string Tag { get; init; } = string.Empty;

    /// <summary>
    /// Command name in uppercase (e.g., "LOGIN", "SELECT")
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Command arguments (may include quoted strings)
    /// </summary>
    public string[] Arguments { get; init; } = [];

    /// <summary>
    /// The raw argument string (useful for commands that need special parsing)
    /// </summary>
    public string RawArguments { get; init; } = string.Empty;

    /// <summary>
    /// Parse an IMAP command line into structured command.
    /// Handles quoted strings and literal+ syntax basics.
    /// </summary>
    public static ImapCommand Parse(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return new ImapCommand { Tag = "*", Name = "NOOP" };
        }

        var trimmed = line.Trim();

        // Find tag (first space-delimited token)
        var firstSpace = trimmed.IndexOf(' ');
        if (firstSpace == -1)
        {
            // Just a tag with no command
            return new ImapCommand { Tag = trimmed, Name = string.Empty };
        }

        var tag = trimmed[..firstSpace];
        var rest = trimmed[(firstSpace + 1)..].TrimStart();

        // Find command name
        var secondSpace = rest.IndexOf(' ');
        string commandName;
        string rawArgs;

        if (secondSpace == -1)
        {
            commandName = rest;
            rawArgs = string.Empty;
        }
        else
        {
            commandName = rest[..secondSpace];
            rawArgs = rest[(secondSpace + 1)..];
        }

        // Parse arguments respecting quotes
        var arguments = ParseArguments(rawArgs);

        return new ImapCommand
        {
            Tag = tag,
            Name = commandName.ToUpperInvariant(),
            Arguments = arguments,
            RawArguments = rawArgs
        };
    }

    /// <summary>
    /// Parse arguments respecting quoted strings.
    /// </summary>
    private static string[] ParseArguments(string args)
    {
        if (string.IsNullOrEmpty(args))
        {
            return [];
        }

        var result = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuote = false;
        var escaped = false;

        foreach (var c in args)
        {
            if (escaped)
            {
                current.Append(c);
                escaped = false;
                continue;
            }

            if (c == '\\')
            {
                escaped = true;
                continue;
            }

            if (c == '"')
            {
                inQuote = !inQuote;
                continue;
            }

            if (c == ' ' && !inQuote)
            {
                if (current.Length > 0)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                continue;
            }

            current.Append(c);
        }

        if (current.Length > 0)
        {
            result.Add(current.ToString());
        }

        return [.. result];
    }
}
