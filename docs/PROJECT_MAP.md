# Project Map

## App Shell

- App entry / global ViewModel access -> `App.xaml`, `App.xaml.cs`
- Main window and NavigationView sidebar -> `MainWindow.xaml`, `MainWindow.xaml.cs`
- App icon / assets -> `Assets/`
- Manifest -> `app.manifest`
- Project file -> `DevCommandLauncherApp.csproj`

## Pages

- Dashboard page / command cards -> `Views/DashboardPage.xaml`, `Views/DashboardPage.xaml.cs`
- Settings page / project and command editing -> `Views/SettingsPage.xaml`, `Views/SettingsPage.xaml.cs`
- Logs page / command log viewer -> `Views/LogsPage.xaml`, `Views/LogsPage.xaml.cs`

## ViewModels

- Global app state, configuration orchestration, process manager bridge -> `ViewModels/MainViewModel.cs`
- Project section state and section-level compact toggle -> `ViewModels/ProjectViewModel.cs`
- Command card state, status badge, button enablement, compact details visibility -> `ViewModels/CommandViewModel.cs`
- Base `INotifyPropertyChanged` helper -> `ViewModels/ObservableObject.cs`
- Logs filter combo option -> `ViewModels/LogCommandOption.cs`

## Services

- JSON config load/save/migration/default seed -> `Services/LauncherConfigurationService.cs`
- Process start/stop/restart/log capture/status transitions -> `Services/CommandProcessManager.cs`
- Health endpoint polling -> `Services/HealthCheckService.cs`
- Port conflict check -> `Services/PortChecker.cs`

## Models

- App/project/command config -> `Models/LauncherConfiguration.cs`, `Models/ProjectConfig.cs`, `Models/CommandConfig.cs`
- Runtime command state/status -> `Models/CommandRuntimeState.cs`, `Models/CommandStatus.cs`
- Logs -> `Models/LogBuffer.cs`, `Models/LogEntry.cs`
- Legacy launch item -> `Models/LaunchItem.cs`

## Documentation

- Repository rules -> `docs/RULES.md`
- Module docs -> `docs/modules/*.md`
- Regression notes -> `docs/regression.md`
- TODO -> `docs/TODO.md`