using System.Collections.ObjectModel;
using DevCommandLauncherApp.Models;

namespace DevCommandLauncherApp.ViewModels;

public sealed class ProjectViewModel : ObservableObject
{
    public ProjectConfig Config { get; }
    public ObservableCollection<CommandViewModel> Commands { get; } = new();

    public ProjectViewModel(ProjectConfig config)
    {
        Config = config;
        foreach (var command in config.Commands)
        {
            Commands.Add(new CommandViewModel(command));
        }
    }

    public string Id => Config.Id;

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
