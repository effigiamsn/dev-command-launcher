using System.Collections.ObjectModel;
using DevCommandLauncherApp.Models;
using DevCommandLauncherApp.Services;
using Microsoft.UI.Dispatching;

namespace DevCommandLauncherApp.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly LauncherConfigurationService _configurationService = new();
    private readonly CommandProcessManager _processManager = new();
    private readonly DispatcherQueue _dispatcherQueue;
    private AppConfig _configuration = new();
    private ProjectViewModel? _selectedProject;
    private CommandViewModel? _selectedCommand;
    private LogCommandOption? _selectedLogCommand;
    private readonly DispatcherQueueTimer _logRefreshTimer;
    private string _statusMessage = "Ready.";
    private string _settingsMessage = string.Empty;
    private string _logText = "No logs yet. Start a command to see output.";
    private string _logFilter = string.Empty;

    public ObservableCollection<ProjectViewModel> Projects { get; } = new();
    public ObservableCollection<LogCommandOption> LogCommandOptions { get; } = new();

    public MainViewModel()
    {
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        _logRefreshTimer = _dispatcherQueue.CreateTimer();
        _logRefreshTimer.Interval = TimeSpan.FromMilliseconds(200);
        _logRefreshTimer.Tick += (_, _) =>
        {
            _logRefreshTimer.Stop();
            RefreshLogText();
        };
        _processManager.CommandStateChanged += OnCommandStateChanged;
        _processManager.CommandLogAdded += OnCommandLogAdded;
        ReloadConfiguration();
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public string SettingsMessage
    {
        get => _settingsMessage;
        private set
        {
            if (SetProperty(ref _settingsMessage, value))
            {
                OnPropertyChanged(nameof(HasSettingsMessage));
            }
        }
    }

    public bool HasSettingsMessage => !string.IsNullOrWhiteSpace(SettingsMessage);

    public string LogText
    {
        get => _logText;
        private set => SetProperty(ref _logText, value);
    }

    public string LogFilter
    {
        get => _logFilter;
        set
        {
            if (SetProperty(ref _logFilter, value))
            {
                RefreshLogText();
            }
        }
    }

    public ProjectViewModel? SelectedProject
    {
        get => _selectedProject;
        set
        {
            if (SetProperty(ref _selectedProject, value))
            {
                SelectedCommand = value?.Commands.FirstOrDefault();
            }
        }
    }

    public CommandViewModel? SelectedCommand
    {
        get => _selectedCommand;
        set => SetProperty(ref _selectedCommand, value);
    }

    public LogCommandOption? SelectedLogCommand
    {
        get => _selectedLogCommand;
        set
        {
            if (SetProperty(ref _selectedLogCommand, value))
            {
                RefreshLogText();
            }
        }
    }

    public void ReloadConfiguration()
    {
        var (config, warning) = _configurationService.LoadOrCreate();
        _configuration = config;
        RebuildCollections();
        StatusMessage = string.IsNullOrWhiteSpace(warning) ? "Config loaded." : warning;
        SettingsMessage = string.Empty;
        RefreshLogText();
    }

    public void SaveConfiguration()
    {
        if (!TryValidateConfiguration(out var error))
        {
            SettingsMessage = error;
            return;
        }

        _configurationService.Save(_configuration);
        RebuildCollections(preserveSelection: true);
        StatusMessage = "Settings saved.";
        SettingsMessage = "Settings saved.";
        RefreshLogText();
    }

    public void AddProject()
    {
        var project = new ProjectConfig
        {
            Id = CreateStableId("new-project", _configuration.Projects.Select(x => x.Id)),
            Name = "New Project"
        };
        _configuration.Projects.Add(project);
        RebuildCollections();
        SelectedProject = Projects.FirstOrDefault(x => x.Id == project.Id);
        SettingsMessage = "Project added.";
    }

    public void DeleteSelectedProject()
    {
        if (SelectedProject is null)
        {
            SettingsMessage = "Select a project first.";
            return;
        }

        _configuration.Projects.Remove(SelectedProject.Config);
        RebuildCollections();
        SettingsMessage = "Project deleted.";
    }

    public void AddCommand()
    {
        if (SelectedProject is null)
        {
            SettingsMessage = "Select a project first.";
            return;
        }

        var command = new CommandConfig
        {
            Id = CreateStableId("new-command", SelectedProject.Config.Commands.Select(x => x.Id)),
            ButtonName = "New Command",
            Command = "npm.cmd",
            Args = "run dev",
            WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        };

        SelectedProject.Config.Commands.Add(command);
        RebuildCollections(preserveSelection: true);
        SelectedProject = Projects.FirstOrDefault(x => x.Id == SelectedProject?.Id);
        SelectedCommand = SelectedProject?.Commands.FirstOrDefault(x => x.Id == command.Id);
        SettingsMessage = "Command added.";
    }

    public void DeleteSelectedCommand()
    {
        if (SelectedProject is null || SelectedCommand is null)
        {
            SettingsMessage = "Select a command first.";
            return;
        }

        SelectedProject.Config.Commands.Remove(SelectedCommand.Config);
        RebuildCollections(preserveSelection: true);
        SettingsMessage = "Command deleted.";
    }

    public async Task StartAsync(CommandViewModel? command)
    {
        if (command is null)
        {
            return;
        }

        StatusMessage = $"Starting {command.ButtonName}...";
        await _processManager.StartAsync(command.Config);
        command.Refresh(_processManager.GetState(command.Id));
    }

    public async Task DetectExistingServersAsync()
    {
        StatusMessage = "Checking configured server ports...";
        var commands = Projects.SelectMany(x => x.Commands).ToList();
        var detectionTasks = commands
            .Select(command => _processManager.DetectExistingServerAsync(command.Config))
            .ToList();

        await Task.WhenAll(detectionTasks);

        foreach (var command in commands)
        {
            command.Refresh(_processManager.GetState(command.Id));
        }

        StatusMessage = "Server port check complete.";
    }

    public void Stop(CommandViewModel? command)
    {
        if (command is null)
        {
            return;
        }

        _processManager.Stop(command.Id);
        command.Refresh(_processManager.GetState(command.Id));
        StatusMessage = $"Stopped {command.ButtonName}.";
    }

    public async Task RestartAsync(CommandViewModel? command)
    {
        if (command is null)
        {
            return;
        }

        StatusMessage = $"Restarting {command.ButtonName}...";
        await _processManager.RestartAsync(command.Config);
        command.Refresh(_processManager.GetState(command.Id));
    }

    public void SelectLogs(CommandViewModel? command)
    {
        if (command is null)
        {
            return;
        }

        SelectedLogCommand = LogCommandOptions.FirstOrDefault(x => x.CommandId == command.Id);
        RefreshLogText();
    }

    public void ClearSelectedLogs()
    {
        if (SelectedLogCommand is null || string.IsNullOrEmpty(SelectedLogCommand.CommandId))
        {
            foreach (var command in Projects.SelectMany(x => x.Commands))
            {
                _processManager.GetState(command.Id).Logs.Clear();
            }
        }
        else
        {
            _processManager.GetState(SelectedLogCommand.CommandId).Logs.Clear();
        }

        RefreshLogText();
    }

    public void RefreshRuntimeState()
    {
        foreach (var command in Projects.SelectMany(x => x.Commands))
        {
            command.Refresh(_processManager.GetState(command.Id));
        }
    }

    private void RebuildCollections(bool preserveSelection = false)
    {
        var selectedProjectId = preserveSelection ? SelectedProject?.Id : null;
        var selectedCommandId = preserveSelection ? SelectedCommand?.Id : null;
        var selectedLogCommandId = SelectedLogCommand?.CommandId;

        Projects.Clear();
        foreach (var project in _configuration.Projects.OrderBy(x => x.Name))
        {
            Projects.Add(new ProjectViewModel(project));
        }

        LogCommandOptions.Clear();
        LogCommandOptions.Add(new LogCommandOption { ProjectName = "All", ButtonName = "All Logs" });
        foreach (var project in Projects)
        {
            foreach (var command in project.Commands)
            {
                command.Refresh(_processManager.GetState(command.Id));
                LogCommandOptions.Add(new LogCommandOption
                {
                    ProjectName = project.Name,
                    ButtonName = command.ButtonName,
                    CommandId = command.Id
                });
            }
        }

        SelectedProject = Projects.FirstOrDefault(x => x.Id == selectedProjectId) ?? Projects.FirstOrDefault();
        SelectedCommand = SelectedProject?.Commands.FirstOrDefault(x => x.Id == selectedCommandId) ?? SelectedProject?.Commands.FirstOrDefault();
        SelectedLogCommand = LogCommandOptions.FirstOrDefault(x => x.CommandId == selectedLogCommandId) ?? LogCommandOptions.FirstOrDefault();
    }

    private bool TryValidateConfiguration(out string error)
    {
        foreach (var project in _configuration.Projects)
        {
            if (string.IsNullOrWhiteSpace(project.Name))
            {
                error = "Project name is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(project.Id))
            {
                project.Id = CreateStableId(project.Name, _configuration.Projects.Select(x => x.Id));
            }

            foreach (var command in project.Commands)
            {
                var commandViewModel = Projects.SelectMany(x => x.Commands).FirstOrDefault(x => x.Id == command.Id);
                if (commandViewModel is not null)
                {
                    if (!string.IsNullOrWhiteSpace(commandViewModel.PortText))
                    {
                        if (!int.TryParse(commandViewModel.PortText, out var parsedPort))
                        {
                            error = $"Port must be numeric: {command.ButtonName}";
                            return false;
                        }

                        command.Port = parsedPort;
                    }
                    else
                    {
                        command.Port = null;
                    }
                }

                if (string.IsNullOrWhiteSpace(command.ButtonName) ||
                    string.IsNullOrWhiteSpace(command.Command) ||
                    string.IsNullOrWhiteSpace(command.WorkingDirectory))
                {
                    error = "Button name, command, and working directory are required.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(command.Id))
                {
                    command.Id = CreateStableId(command.ButtonName, project.Commands.Select(x => x.Id));
                }

                if (!string.IsNullOrWhiteSpace(command.Url) &&
                    !Uri.TryCreate(command.Url, UriKind.Absolute, out _))
                {
                    error = $"Invalid URL: {command.ButtonName}";
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(command.HealthUrl) &&
                    !Uri.TryCreate(command.HealthUrl, UriKind.Absolute, out _))
                {
                    error = $"Invalid Health URL: {command.ButtonName}";
                    return false;
                }
            }
        }

        error = string.Empty;
        return true;
    }

    private void OnCommandStateChanged(string commandId, CommandRuntimeState state)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            var command = Projects.SelectMany(x => x.Commands).FirstOrDefault(x => x.Id == commandId);
            command?.Refresh(state);
            StatusMessage = state.StatusMessage ?? state.Status.ToString();
        });
    }

    private void OnCommandLogAdded(string commandId, LogEntry entry)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            var command = Projects.SelectMany(x => x.Commands).FirstOrDefault(x => x.Id == commandId);
            command?.Refresh(_processManager.GetState(commandId));
            ScheduleLogTextRefresh();
        });
    }

    private void ScheduleLogTextRefresh()
    {
        if (_logRefreshTimer.IsRunning)
        {
            return;
        }

        _logRefreshTimer.Start();
    }

    private void RefreshLogText()
    {
        var selectedId = SelectedLogCommand?.CommandId ?? string.Empty;
        var entries = new List<LogEntry>();

        if (string.IsNullOrEmpty(selectedId))
        {
            foreach (var command in Projects.SelectMany(x => x.Commands))
            {
                entries.AddRange(_processManager.GetState(command.Id).Logs.Snapshot());
            }
        }
        else
        {
            entries.AddRange(_processManager.GetState(selectedId).Logs.Snapshot());
        }

        var lines = entries
            .OrderBy(x => x.Timestamp)
            .Select(x => x.ToString());

        if (!string.IsNullOrWhiteSpace(LogFilter))
        {
            lines = lines.Where(x => x.Contains(LogFilter, StringComparison.OrdinalIgnoreCase));
        }

        var text = string.Join(Environment.NewLine, lines);
        LogText = string.IsNullOrWhiteSpace(text)
            ? "No logs yet. Start a command to see output."
            : text;
    }

    private static string CreateStableId(string name, IEnumerable<string> existingIds)
    {
        var normalized = new string(name.Trim().ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
            .ToArray());

        while (normalized.Contains("--", StringComparison.Ordinal))
        {
            normalized = normalized.Replace("--", "-", StringComparison.Ordinal);
        }

        normalized = normalized.Trim('-');
        if (string.IsNullOrWhiteSpace(normalized))
        {
            normalized = "item";
        }

        var existing = existingIds.Where(x => !string.IsNullOrWhiteSpace(x)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var candidate = normalized;
        var suffix = 2;
        while (existing.Contains(candidate))
        {
            candidate = $"{normalized}-{suffix++}";
        }

        return candidate;
    }
}
