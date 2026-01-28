namespace Relate.Smtp.Pop3Host.Protocol;

public static class Pop3Response
{
    public const string OK = "+OK";
    public const string ERR = "-ERR";

    public static string Success(string message = "") =>
        string.IsNullOrEmpty(message) ? OK : $"{OK} {message}";

    public static string Error(string message = "") =>
        string.IsNullOrEmpty(message) ? ERR : $"{ERR} {message}";

    public static string Greeting(string serverName) =>
        $"{OK} {serverName} POP3 server ready";
}
