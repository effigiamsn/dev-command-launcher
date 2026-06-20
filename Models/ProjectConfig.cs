using System;
using System.Collections.Generic;

namespace DevCommandLauncherApp.Models;

public sealed class ProjectConfig
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public List<CommandConfig> Commands { get; set; } = new();
}
