namespace DevCommandLauncherApp.Models;

public enum CommandStatus
{
    Stopped,
    Starting,
    Running,
    Stopping,
    Error,
    Crashed,
    ExternalRunning,
    PortConflict
}
