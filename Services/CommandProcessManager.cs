using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DevCommandLauncherApp.Models;

namespace DevCommandLauncherApp.Services;

public sealed class CommandProcessManager
{
    private static readonly Regex AnsiEscapeRegex = new(
        @"\u001B(?:\[[0-?]*[ -/]*[@-~]|\][^\u0007]*(?:\u0007|\u001B\\)|[PX^_].*?\u001B\\)",
        RegexOptions.Compiled);

    private readonly Dictionary<string, CommandRuntimeState> _states = new();
    private readonly object _stateLock = new();

    public event Action<string, CommandRuntimeState>? CommandStateChanged;
    public event Action<string, LogEntry>? CommandLogAdded;

    public CommandRuntimeState GetState(string commandId)
    {
        lock (_stateLock)
        {
            return _states.TryGetValue(commandId, out var existing)
                ? existing
                : CreateStateLocked(commandId);
        }
    }

    public async Task StartAsync(CommandConfig command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Id))
        {
            return;
        }

        var state = GetState(command.Id);
        if (state.Status is CommandStatus.Running or CommandStatus.Starting)
        {
            return;
        }

        state.Logs.Clear();
        state.SetStatus(CommandStatus.Starting, "Preparing");
        RaiseStateChanged(command.Id, state);

        if (string.IsNullOrWhiteSpace(command.WorkingDirectory) || !Directory.Exists(command.WorkingDirectory))
        {
            state.SetStatus(CommandStatus.Error, "Working directory is invalid.");
            AppendLog(state, true, "system", $"Working directory not found: {command.WorkingDirectory}");
            RaiseStateChanged(command.Id, state);
            return;
        }

        if (command.Port.HasValue && command.Port.Value > 0)
        {
            var portConflict = await PortChecker.IsPortInUseAsync(command.Port.Value, cancellationToken)
                .ConfigureAwait(false);
            if (portConflict)
            {
                state.SetStatus(CommandStatus.PortConflict, $"Port {command.Port} is already in use.");
                AppendLog(state, true, "system", $"Port in use: {command.Port}");
                RaiseStateChanged(command.Id, state);
                return;
            }
        }

        var parsedCommand = ParseCommand(command.Command, command.Args);
        if (string.IsNullOrWhiteSpace(parsedCommand.commandFile))
        {
            state.SetStatus(CommandStatus.Error, "Command is empty.");
            AppendLog(state, true, "system", "Executable is empty.");
            RaiseStateChanged(command.Id, state);
            return;
        }

        var commandLine = FormatCommandLine(parsedCommand.commandFile, parsedCommand.arguments);
        AppendLog(state, true, "system", $"Launching: {commandLine}");

        Process? process = null;

        try
        {
            process = CreateDirectProcess(command.WorkingDirectory, parsedCommand.commandFile, parsedCommand.arguments);
            AttachProcessEvents(process, command.Id, state);
            if (!process.Start())
            {
                state.SetStatus(CommandStatus.Error, "Failed to start.");
                AppendLog(state, true, "system", "Process.Start returned false.");
                process.Dispose();
                process = null;
                RaiseStateChanged(command.Id, state);
                return;
            }
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 2)
        {
            AppendLog(state, true, "system", $"Direct launch failed ({ex.Message}), fallback to shell.");
            if (process is not null)
            {
                process.Dispose();
                process = null;
            }

            try
            {
                process = CreateShellProcess(command.WorkingDirectory, commandLine);
                AttachProcessEvents(process, command.Id, state);
                if (!process.Start())
                {
                    state.SetStatus(CommandStatus.Error, "Failed to start with shell.");
                    AppendLog(state, true, "system", "Process.Start returned false.");
                    process.Dispose();
                    process = null;
                    RaiseStateChanged(command.Id, state);
                    return;
                }
            }
            catch (Exception shellEx)
            {
                state.SetStatus(CommandStatus.Error, shellEx.Message);
                AppendLog(state, true, "system", $"Fallback launch failed: {shellEx.Message}");
                if (process is not null)
                {
                    process.Dispose();
                }

                RaiseStateChanged(command.Id, state);
                return;
            }
        }
        catch (Exception ex)
        {
            state.SetStatus(CommandStatus.Error, ex.Message);
            AppendLog(state, true, "system", $"Failed to start: {ex.Message}");
            if (process is not null)
            {
                process.Dispose();
            }

            RaiseStateChanged(command.Id, state);
            return;
        }

        if (process is null)
        {
            state.SetStatus(CommandStatus.Error, "Process is not ready.");
            RaiseStateChanged(command.Id, state);
            return;
        }

        state.SetProcess(process);
        state.MarkStarted(DateTimeOffset.UtcNow);
        AppendLog(state, false, "system", $"PID {process.Id} started.");
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        if (!string.IsNullOrWhiteSpace(command.HealthUrl))
        {
            try
            {
                state.SetStatus(CommandStatus.Starting, "Waiting for health endpoint");
                var healthy = await HealthCheckService.WaitUntilHealthyAsync(command.HealthUrl, cancellationToken)
                    .ConfigureAwait(false);
                if (healthy)
                {
                    state.SetStatus(CommandStatus.Running, "Running");
                }
                else
                {
                    state.SetStatus(CommandStatus.Error, "Health check failed");
                    AppendLog(state, true, "system", $"Health check failed: {command.HealthUrl}");
                }
            }
            catch (OperationCanceledException)
            {
                state.SetStatus(CommandStatus.Error, "Start canceled");
            }
        }
        else
        {
            await Task.Delay(600, cancellationToken).ConfigureAwait(false);

            if (state.Status == CommandStatus.Starting && process.HasExited)
            {
                state.SetStatus(
                    process.ExitCode == 0 ? CommandStatus.Stopped : CommandStatus.Crashed,
                    process.ExitCode == 0 ? "Process exited immediately." : $"Process exit code: {process.ExitCode}");
            }
            else if (state.Status == CommandStatus.Starting)
            {
                state.SetStatus(CommandStatus.Running, "Running");
            }
        }

        RaiseStateChanged(command.Id, state);
    }

    public async Task RestartAsync(CommandConfig command, CancellationToken cancellationToken = default)
    {
        Stop(command.Id);
        await Task.Delay(250, cancellationToken).ConfigureAwait(false);
        await StartAsync(command, cancellationToken).ConfigureAwait(false);
    }

    public void Stop(string commandId)
    {
        var state = GetState(commandId);
        var process = state.Process;

        if (process is null || process.HasExited)
        {
            state.SetStatus(CommandStatus.Stopped, "Stopped");
            state.SetProcess(null);
            RaiseStateChanged(commandId, state);
            return;
        }

        state.SetStatus(CommandStatus.Stopping, "Stopping");
        RaiseStateChanged(commandId, state);

        try
        {
            process.Kill(entireProcessTree: true);
            process.WaitForExit(3000);
        }
        catch (Exception ex)
        {
            state.SetStatus(CommandStatus.Error, ex.Message);
            AppendLog(state, true, "system", $"Failed to stop: {ex.Message}");
            RaiseStateChanged(commandId, state);
        }
        finally
        {
            if (process.HasExited)
            {
                state.SetStatus(CommandStatus.Stopped, "Stopped");
            }

            state.SetProcess(null);
            RaiseStateChanged(commandId, state);
        }
    }

    private static (string commandFile, string arguments) ParseCommand(string command, string args)
    {
        var trimmedCommand = command?.Trim() ?? string.Empty;
        var trimmedArgs = args?.Trim() ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(trimmedArgs))
        {
            return (trimmedCommand, trimmedArgs);
        }

        if (string.IsNullOrWhiteSpace(trimmedCommand))
        {
            return (string.Empty, string.Empty);
        }

        if (trimmedCommand.StartsWith('"'))
        {
            var closing = trimmedCommand.IndexOf('"', 1);
            if (closing > 0)
            {
                var commandFile = trimmedCommand[1..closing];
                var argString = trimmedCommand[(closing + 1)..].Trim();
                return (commandFile, argString);
            }
        }

        var firstSpace = trimmedCommand.IndexOf(' ');
        if (firstSpace <= 0)
        {
            return (trimmedCommand, string.Empty);
        }

        return (trimmedCommand[..firstSpace], trimmedCommand[(firstSpace + 1)..]);
    }

    private static string FormatCommandLine(string commandFile, string arguments)
    {
        return string.IsNullOrWhiteSpace(arguments)
            ? commandFile
            : $"{commandFile} {arguments}";
    }

    private static Process CreateDirectProcess(string workingDirectory, string commandFile, string arguments)
    {
        return new Process
        {
            StartInfo = ConfigureProcessStartInfo(new ProcessStartInfo(commandFile, arguments)
            {
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            }),
            EnableRaisingEvents = true
        };
    }

    private static Process CreateShellProcess(string workingDirectory, string commandLine)
    {
        return new Process
        {
            StartInfo = ConfigureProcessStartInfo(new ProcessStartInfo
            {
                FileName = Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe",
                Arguments = $"/c \"{commandLine.Replace("\"", "\\\"")}\"",
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            }),
            EnableRaisingEvents = true
        };
    }

    private static ProcessStartInfo ConfigureProcessStartInfo(ProcessStartInfo startInfo)
    {
        startInfo.Environment["LANG"] = "ko_KR.UTF-8";
        startInfo.Environment["LC_ALL"] = "ko_KR.UTF-8";
        startInfo.Environment["PYTHONUTF8"] = "1";
        startInfo.Environment["PYTHONIOENCODING"] = "utf-8";
        startInfo.Environment["DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION"] = "0";
        startInfo.Environment["NO_COLOR"] = "1";
        startInfo.Environment["FORCE_COLOR"] = "0";
        return startInfo;
    }

    private void AttachProcessEvents(Process process, string commandId, CommandRuntimeState state)
    {
        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is null)
            {
                return;
            }

            AppendLog(state, false, "stdout", e.Data);
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is null)
            {
                return;
            }

            AppendLog(state, true, "stderr", e.Data);
        };

        process.Exited += (_, _) =>
        {
            if (state.Status == CommandStatus.Stopping)
            {
                state.SetStatus(CommandStatus.Stopped, "Stopped");
            }
            else
            {
                state.SetStatus(process.ExitCode == 0 ? CommandStatus.Stopped : CommandStatus.Crashed,
                    process.ExitCode == 0 ? "Process exited" : $"Process exit code: {process.ExitCode}");
            }

            state.SetProcess(null);
            AppendLog(state, true, "system", $"Process exited ({process.ExitCode}).");
            RaiseStateChanged(commandId, state);
        };
    }

    private CommandRuntimeState CreateStateLocked(string commandId)
    {
        var state = new CommandRuntimeState(commandId);
        state.Logs.EntryAdded += entry => CommandLogAdded?.Invoke(commandId, entry);
        _states[commandId] = state;
        return state;
    }

    private void RaiseStateChanged(string commandId, CommandRuntimeState state)
    {
        CommandStateChanged?.Invoke(commandId, state);
    }

    private void AppendLog(CommandRuntimeState state, bool isError, string source, string message)
    {
        var cleanMessage = CleanProcessOutput(message);
        state.Logs.Append(new LogEntry
        {
            Timestamp = DateTimeOffset.UtcNow,
            IsError = isError,
            Source = source,
            Message = cleanMessage
        });
    }

    private static string CleanProcessOutput(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return string.Empty;
        }

        return AnsiEscapeRegex.Replace(message, string.Empty);
    }
}
