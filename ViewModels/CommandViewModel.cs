using DevCommandLauncherApp.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace DevCommandLauncherApp.ViewModels;

public sealed class CommandViewModel : ObservableObject
{
    private static readonly SolidColorBrush RunningBrush = CreateBrush(255, 22, 163, 74);
    private static readonly SolidColorBrush StartingBrush = CreateBrush(255, 37, 99, 235);
    private static readonly SolidColorBrush StoppingBrush = CreateBrush(255, 100, 116, 139);
    private static readonly SolidColorBrush ExternalRunningBrush = CreateBrush(255, 8, 145, 178);
    private static readonly SolidColorBrush PortConflictBrush = CreateBrush(255, 245, 158, 11);
    private static readonly SolidColorBrush ErrorBrush = CreateBrush(255, 220, 38, 38);
    private static readonly SolidColorBrush StoppedBrush = CreateBrush(255, 55, 65, 81);
    private static readonly SolidColorBrush BlackBrush = new(Microsoft.UI.Colors.Black);
    private static readonly SolidColorBrush WhiteBrush = new(Microsoft.UI.Colors.White);

    public CommandConfig Config { get; }

    private CommandStatus _status = CommandStatus.Stopped;
    private string? _statusMessage;
    private string _lastLogLine = "-";
    private string _uptime = "-";
    private string _portText;
    private bool _isCollapsed;

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

            OnPropertyChanged(nameof(ServerAddress));
            OnPropertyChanged(nameof(ServerUri));
            OnPropertyChanged(nameof(HasServerAddress));
            OnPropertyChanged(nameof(OpenUrlVisibility));
            OnPropertyChanged(nameof(ServerAddressVisibility));
            OnPropertyChanged(nameof(IsCopyServerAddressEnabled));
        }
    }

    public string Url
    {
        get => Config.Url ?? string.Empty;
        set
        {
            var normalized = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
            if (string.Equals(Config.Url, normalized, StringComparison.Ordinal))
            {
                return;
            }

            Config.Url = normalized;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasUrl));
            OnPropertyChanged(nameof(HasServerAddress));
            OnPropertyChanged(nameof(OpenUrlVisibility));
            OnPropertyChanged(nameof(ServerAddress));
            OnPropertyChanged(nameof(ServerUri));
            OnPropertyChanged(nameof(ServerAddressVisibility));
            OnPropertyChanged(nameof(IsCopyServerAddressEnabled));
        }
    }

    public string HealthUrl
    {
        get => Config.HealthUrl ?? string.Empty;
        set
        {
            var normalized = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
            if (string.Equals(Config.HealthUrl, normalized, StringComparison.Ordinal))
            {
                return;
            }

            Config.HealthUrl = normalized;
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
                OnPropertyChanged(nameof(IsCopyServerAddressEnabled));
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
    public string ServerAddress => !string.IsNullOrWhiteSpace(Config.Url)
        ? Config.Url
        : Config.Port.HasValue && Config.Port.Value > 0
            ? $"http://localhost:{Config.Port.Value}"
            : string.Empty;
    public Uri? ServerUri => Uri.TryCreate(ServerAddress, UriKind.Absolute, out var uri) ? uri : null;
    public bool HasUrl => !string.IsNullOrWhiteSpace(Config.Url);
    public bool HasServerAddress => ServerUri is not null;
    public Visibility OpenUrlVisibility => HasServerAddress ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ServerAddressVisibility => HasServerAddress ? Visibility.Visible : Visibility.Collapsed;
    public Visibility DetailsVisibility => IsCollapsed ? Visibility.Collapsed : Visibility.Visible;
    public bool IsStartEnabled => Status is CommandStatus.Stopped or CommandStatus.Error or CommandStatus.Crashed or CommandStatus.PortConflict;
    public bool IsStopEnabled => Status is CommandStatus.Running or CommandStatus.Starting;
    public bool IsRestartEnabled => Status is not CommandStatus.Starting and not CommandStatus.Stopping and not CommandStatus.ExternalRunning;
    public bool IsCopyServerAddressEnabled => HasServerAddress;

    public bool IsCollapsed
    {
        get => _isCollapsed;
        private set
        {
            if (SetProperty(ref _isCollapsed, value))
            {
                OnPropertyChanged(nameof(DetailsVisibility));
            }
        }
    }

    public string StatusText => Status switch
    {
        CommandStatus.ExternalRunning => "External",
        CommandStatus.PortConflict => "Port Conflict",
        _ => Status.ToString()
    };

    public SolidColorBrush StatusBadgeBackground => Status switch
    {
        CommandStatus.Running => RunningBrush,
        CommandStatus.Starting => StartingBrush,
        CommandStatus.Stopping => StoppingBrush,
        CommandStatus.ExternalRunning => ExternalRunningBrush,
        CommandStatus.PortConflict => PortConflictBrush,
        CommandStatus.Error or CommandStatus.Crashed => ErrorBrush,
        _ => StoppedBrush
    };

    public SolidColorBrush StatusBadgeForeground => Status == CommandStatus.PortConflict
        ? BlackBrush
        : WhiteBrush;

    public void SetCollapsed(bool isCollapsed)
    {
        IsCollapsed = isCollapsed;
    }

    public void Refresh(CommandRuntimeState state)
    {
        Status = state.Status;
        StatusMessage = state.StatusMessage;
        LastLogLine = state.LastLogLine ?? "-";
        Uptime = state.StartTime is null ? "-" : (DateTimeOffset.UtcNow - state.StartTime.Value).ToString(@"hh\:mm\:ss");
        OnPropertyChanged(nameof(UptimeDisplay));
        OnPropertyChanged(nameof(LastLogDisplay));
    }

    private static SolidColorBrush CreateBrush(byte alpha, byte red, byte green, byte blue)
    {
        return new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(alpha, red, green, blue));
    }

}
