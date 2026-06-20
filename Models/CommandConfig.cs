using System;

namespace DevCommandLauncherApp.Models;

public sealed class CommandConfig
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string ButtonName { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public string Args { get; set; } = string.Empty;
    public string WorkingDirectory { get; set; } = string.Empty;
    public int? Port { get; set; }
    public string? Url { get; set; }
    public string? HealthUrl { get; set; }
}
