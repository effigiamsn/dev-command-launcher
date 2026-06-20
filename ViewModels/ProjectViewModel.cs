using System.Collections.ObjectModel;
using DevCommandLauncherApp.Models;

namespace DevCommandLauncherApp.ViewModels;

public sealed class ProjectViewModel : ObservableObject
{
    public ProjectConfig Config { get; }
    public ObservableCollection<CommandViewModel> Commands { get; } = new();

    private bool _isCollapsed;

    public ProjectViewModel(ProjectConfig config)
    {
        Config = config;
        foreach (var command in config.Commands)
        {
            Commands.Add(new CommandViewModel(command));
        }
    }

    public string Id => Config.Id;
    public string CollapseGlyph => IsCollapsed ? "\uE70D" : "\uE70E";
    public string CollapseAutomationName => IsCollapsed ? "Expand project section" : "Collapse project section";

    public bool IsCollapsed
    {
        get => _isCollapsed;
        private set
        {
            if (SetProperty(ref _isCollapsed, value))
            {
                OnPropertyChanged(nameof(CollapseGlyph));
                OnPropertyChanged(nameof(CollapseAutomationName));
                foreach (var command in Commands)
                {
                    command.SetCollapsed(value);
                }
            }
        }
    }

    public void ToggleCollapsed()
    {
        IsCollapsed = !IsCollapsed;
    }

    public string Name
    {
        get => Config.Name;
        set
        {
            if (Config.Name == value)
            {
                return;
            }

            Config.Name = value;
            OnPropertyChanged();
        }
    }
}
