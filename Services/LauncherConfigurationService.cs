using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using DevCommandLauncherApp.Models;

namespace DevCommandLauncherApp.Services;

public sealed class LauncherConfigurationService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private const string ConfigFileName = "launcher.config.json";
    private readonly string _configPath;

    public LauncherConfigurationService()
    {
        var appData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DevCommandLauncherApp");

        Directory.CreateDirectory(appData);
        _configPath = Path.Combine(appData, ConfigFileName);
    }

    public (AppConfig config, string? warning) LoadOrCreate()
    {
        if (!File.Exists(_configPath))
        {
            var seeded = CreateSeedConfiguration();
            Save(seeded);
            return (seeded, null);
        }

        try
        {
            var json = File.ReadAllText(_configPath);
            var data = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions);

            if (data is not null && data.Projects.Count > 0)
            {
                NormalizeConfig(data);
                Save(data);
                return (data, null);
            }

            var hasLegacyShape = json.Contains("\"Items\"", StringComparison.OrdinalIgnoreCase);
            if (hasLegacyShape)
            {
                var legacy = JsonSerializer.Deserialize<LegacyLauncherConfiguration>(json, JsonOptions);
                if (legacy?.Items is not null && legacy.Items.Count > 0)
                {
                    var migrated = MigrateLegacyConfiguration(legacy);
                    NormalizeConfig(migrated);
                    Save(migrated);
                    return (migrated, "기존 설정을 새 형식으로 마이그레이션했습니다.");
                }
            }
        }
        catch (JsonException jsonEx)
        {
            var fallback = CreateSeedConfiguration();
            Save(fallback);
            return (fallback, $"설정 파일 JSON 오류: {jsonEx.Message} (기본값으로 복원)");
        }
        catch
        {
            var fallback = CreateSeedConfiguration();
            Save(fallback);
            return (fallback, "설정 파일을 읽는 중 오류가 발생해 기본값으로 복원했습니다.");
        }

        var defaulted = CreateSeedConfiguration();
        Save(defaulted);
        return (defaulted, "설정 파일에 프로젝트가 없어 기본값을 생성했습니다.");
    }

    public void Save(AppConfig configuration)
    {
        NormalizeConfig(configuration);
        File.WriteAllText(_configPath, JsonSerializer.Serialize(configuration, JsonOptions));
    }

    public string GetConfigPath() => _configPath;

    private static void NormalizeConfig(AppConfig config)
    {
        config.Projects ??= new();

        for (var p = 0; p < config.Projects.Count; p++)
        {
            var project = config.Projects[p];
            if (string.IsNullOrWhiteSpace(project.Id))
            {
                project.Id = Guid.NewGuid().ToString("N");
            }

            if (string.IsNullOrWhiteSpace(project.Name))
            {
                project.Name = "Unnamed Project";
            }

            project.Commands ??= new();
            for (var c = 0; c < project.Commands.Count; c++)
            {
                var command = project.Commands[c];
                if (string.IsNullOrWhiteSpace(command.Id))
                {
                    command.Id = Guid.NewGuid().ToString("N");
                }

                command.ButtonName = command.ButtonName?.Trim() ?? string.Empty;
                command.Command = command.Command?.Trim() ?? string.Empty;
                command.Args = command.Args?.Trim() ?? string.Empty;
                command.WorkingDirectory = command.WorkingDirectory?.Trim() ?? string.Empty;
                command.Url = string.IsNullOrWhiteSpace(command.Url) ? null : command.Url.Trim();
                command.HealthUrl = string.IsNullOrWhiteSpace(command.HealthUrl) ? null : command.HealthUrl.Trim();
            }
        }
    }

    private static AppConfig CreateSeedConfiguration()
    {
        return new AppConfig
        {
            Projects =
            {
                new()
                {
                    Id = "mastersurfer",
                    Name = "MasterSurfer",
                    Commands =
                    {
                        new()
                        {
                            Id = "devtool",
                            ButtonName = "DevTool",
                            Command = "npm.cmd",
                            Args = "run dev",
                            WorkingDirectory = @"E:\Business\MasterSurfer"
                        },
                        new()
                        {
                            Id = "frontend",
                            ButtonName = "FrontEnd",
                            Command = "npm.cmd",
                            Args = "run dev",
                            WorkingDirectory = @"E:\Business\MasterSurfer"
                        }
                    }
                },
                new()
                {
                    Id = "homepage",
                    Name = "홈페이지",
                    Commands =
                    {
                        new()
                        {
                            Id = "nmm-local",
                            ButtonName = "NMM Local",
                            Command = "npm.cmd",
                            Args = "run dev",
                            WorkingDirectory = @"E:\Business\NMM\website"
                        },
                        new()
                        {
                            Id = "designstudio",
                            ButtonName = "DesignStudio",
                            Command = "npm.cmd",
                            Args = "run dev",
                            WorkingDirectory = @"E:\Business\DesignSystemLab"
                        }
                    }
                }
            }
        };
    }

    private static AppConfig MigrateLegacyConfiguration(LegacyLauncherConfiguration legacy)
    {
        var config = new AppConfig();
        var grouped = legacy.Items.GroupBy(x => string.IsNullOrWhiteSpace(x.ProjectName) ? "Unnamed Project" : x.ProjectName);

        foreach (var projectGroup in grouped)
        {
            var project = new ProjectConfig
            {
                Name = projectGroup.Key
            };

            foreach (var item in projectGroup)
            {
                var (command, args) = SplitLegacyCommand(item.Command);
                project.Commands.Add(new CommandConfig
                {
                    ButtonName = string.IsNullOrWhiteSpace(item.ButtonName) ? "Command" : item.ButtonName,
                    Command = command,
                    Args = args,
                    WorkingDirectory = item.WorkingDirectory ?? string.Empty
                });
            }

            if (project.Commands.Count > 0)
            {
                config.Projects.Add(project);
            }
        }

        return config;
    }

    private static (string Command, string Args) SplitLegacyCommand(string? commandText)
    {
        if (string.IsNullOrWhiteSpace(commandText))
        {
            return ("npm.cmd", "run dev");
        }

        var trimmed = commandText.Trim();
        if (!trimmed.Contains(' '))
        {
            return (trimmed, string.Empty);
        }

        if (trimmed.StartsWith("\"", StringComparison.Ordinal))
        {
            var closingQuote = trimmed.IndexOf('"', 1);
            if (closingQuote > 0)
            {
                return (trimmed[1..closingQuote], trimmed[(closingQuote + 1)..].Trim());
            }
        }

        var index = trimmed.IndexOf(' ');
        return (trimmed[..index], trimmed[(index + 1)..]);
    }

    private sealed class LegacyLauncherConfiguration
    {
        public List<LegacyLaunchItem> Items { get; set; } = new();
    }

    private sealed class LegacyLaunchItem
    {
        public string? ProjectName { get; set; }
        public string? ButtonName { get; set; }
        public string? WorkingDirectory { get; set; }
        public string? Command { get; set; }
    }
}
