using DevCommandLauncherApp.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace DevCommandLauncherApp.ViewModels;

public sealed class CommandViewModel : ObservableObject
{
    public CommandConfig Config { get; }

    private CommandStatus _status = CommandStatus.Stopped;
    private string? _statusMessage;
    private string _lastLogLine = "-";
    private string _uptime = "-";
    private string _portText;

    public CommandViewModel(CommandConfig config)
    {
        Config = config;
        _portText = config.Port?.ToString() ?? string.Empty;
    }

    public string Id => Config.Id;

    public string ButtonName
    {
        get => Config.ButtonName;
        set
        {
            if (Config.ButtonName == value)
            {
                return;
            }

            Config.ButtonName = value;
            OnPropertyChanged();
        }
    }

    public string Command
    {
        get => Config.Command;
        set
        {
            if (Config.Command == value)
            {
                return;
            }

            Config.Command = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CommandSummary));
        }
    }

    public string Args
    {
        get => Config.Args;
        set
        {
            if (Config.Args == value)
            {
                return;
            }

            Config.Args = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CommandSummary));
        }
    }

    public string WorkingDirectory
    {
        get => Config.WorkingDirectory;
        set
        {
            if (Config.WorkingDirectory == value)
            {
                return;
            }

            Config.WorkingDirectory = value;
            OnPropertyChanged();
        }
    }

    public string PortText
    {
        get => _portText;
        set
        {
            if (!SetProperty(ref _portText, value))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                Config.Port = null;
            }
            else if (int.TryParse(value, out var port))
            {
                Config.Port = port;
            }
        }
    }

    public string Url
    {
        get => Config.Url ?? string.Empty;
        set
        {
            Config.Url = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasUrl));
            OnPropertyChanged(nameof(OpenUrlVisibility));
        }
    }

    public string HealthUrl
    {
        get => Config.HealthUrl ?? string.Empty;
        set
        {
            Config.HealthUrl = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
            OnPropertyChanged();
        }
    }

    public CommandStatus Status
    {
        get => _status;
        private set
        {
            if (SetProperty(ref _status, value))
            {
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(StatusBadgeBackground));
                OnPropertyChanged(nameof(StatusBadgeForeground));
                OnPropertyChanged(nameof(IsStartEnabled));
                OnPropertyChanged(nameof(IsStopEnabled));
                OnPropertyChanged(nameof(IsRestartEnabled));
            }
        }
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public string LastLogLine
    {
        get => _lastLogLine;
        private set => SetProperty(ref _lastLogLine, value);
    }

    public string Uptime
    {
        get => _uptime;
        private set => SetProperty(ref _uptime, value);
    }

    public string CommandSummary => $"{Config.Command} {Config.Args}".Trim();
    public string UptimeDisplay => $"Uptime: {Uptime}";
    public string LastLogDisplay => $"Last log: {LastLogLine}";
    public bool HasUrl => !string.IsNullOrWhiteSpace(Config.Url);
    public Visibility OpenUrlVisibility => HasUrl ? Visibility.Visible : Visibility.Collapsed;
    public bool IsStartEnabled => Status is CommandStatus.Stopped or CommandStatus.Error or CommandStatus.Crashed or CommandStatus.PortConflict;
    public bool IsStopEnabled => Status is CommandStatus.Running or CommandStatus.Starting;
    public bool IsRestartEnabled => Status is not CommandStatus.Starting and not CommandStatus.Stopping;

    public string StatusText => Status switch
    {
        CommandStatus.PortConflict => "Port Conflict",
        _ => Status.ToString()
    };

    public SolidColorBrush StatusBadgeBackground => Status switch
    {
        CommandStatus.Running => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 22, 163, 74)),
        CommandStatus.Starting => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 37, 99, 235)),
        CommandStatus.Stopping => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 100, 116, 139)),
        CommandStatus.PortConflict => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 245, 158, 11)),
        CommandStatus.Error or CommandStatus.Crashed => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 220, 38, 38)),
        _ => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 55, 65, 81))
    };

    public SolidColorBrush StatusBadgeForeground => Status == CommandStatus.PortConflict
        ? new SolidColorBrush(Microsoft.UI.Colors.Black)
        : new SolidColorBrush(Microsoft.UI.Colors.White);

    public void Refresh(CommandRuntimeState state)
    {
        Status = state.Status;
        StatusMessage = state.StatusMessage;
        LastLogLine = state.LastLogLine ?? "-";
        Uptime = state.StartTime is null ? "-" : (DateTimeOffset.UtcNow - state.StartTime.Value).ToString(@"hh\:mm\:ss");
        OnPropertyChanged(nameof(UptimeDisplay));
        OnPropertyChanged(nameof(LastLogDisplay));
    }
}
