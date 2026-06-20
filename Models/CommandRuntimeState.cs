namespace DevCommandLauncherApp.Models;

public sealed class CommandRuntimeState
{
    public string CommandId { get; }
    public LogBuffer Logs { get; } = new(2000);
    public CommandStatus Status { get; private set; } = CommandStatus.Stopped;
    public string? StatusMessage { get; private set; }
    public System.Diagnostics.Process? Process { get; private set; }
    public DateTimeOffset? StartTime { get; private set; }

    public CommandRuntimeState(string commandId)
    {
        CommandId = commandId;
    }

    public void SetProcess(System.Diagnostics.Process? process)
    {
        Process = process;
        if (process is null)
        {
            StartTime = null;
        }
    }

    public void SetStatus(CommandStatus status, string? statusMessage = null)
    {
        Status = status;
        StatusMessage = statusMessage;
    }

    public void MarkStarted(DateTimeOffset startedAt)
    {
        StartTime = startedAt;
    }

    public string? LastLogLine
    {
        get
        {
            var entries = Logs.Snapshot();
            return entries.Count == 0 ? null : entries[^1].Message;
        }
    }
}
