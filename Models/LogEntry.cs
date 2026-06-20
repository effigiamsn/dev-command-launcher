using System;

namespace DevCommandLauncherApp.Models;

public sealed class LogEntry
{
    public DateTimeOffset Timestamp { get; init; }
    public bool IsError { get; init; }
    public string Source { get; init; } = "stdout";
    public string Message { get; init; } = string.Empty;

    public override string ToString()
    {
        var kind = IsError ? "[stderr]" : "[stdout]";
        return $"[{Timestamp:HH:mm:ss}] {kind} {Source}: {Message}";
    }
}
