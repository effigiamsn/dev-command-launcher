namespace DevCommandLauncherApp.ViewModels;

public sealed class LogCommandOption
{
    public string ProjectName { get; init; } = string.Empty;
    public string ButtonName { get; init; } = string.Empty;
    public string CommandId { get; init; } = string.Empty;
    public string DisplayName => string.IsNullOrEmpty(CommandId) ? "All Logs" : $"{ProjectName} / {ButtonName}";

    public override string ToString()
    {
        return DisplayName;
    }
}
