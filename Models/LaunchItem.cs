using System;

namespace DevCommandLauncherApp.Models;

public sealed class LaunchItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string ProjectName { get; set; } = "";
    public string ButtonName { get; set; } = "";
    public string WorkingDirectory { get; set; } = "";
    public string Command { get; set; } = "";
}
