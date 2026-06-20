using System;
using System.Collections.Generic;

namespace DevCommandLauncherApp.Models;

public sealed class AppConfig
{
    public List<ProjectConfig> Projects { get; set; } = new();
}
