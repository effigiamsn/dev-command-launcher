# Configuration Module

## Purpose

Configuration module은 project/command 설정을 JSON file로 load/save하고, legacy shape를 현재 project-based config로 migration한다.

## Related Files

- `Services/LauncherConfigurationService.cs`
- `Models/LauncherConfiguration.cs`
- `Models/ProjectConfig.cs`
- `Models/CommandConfig.cs`
- `ViewModels/MainViewModel.cs`
- `Views/SettingsPage.xaml`
- `Views/SettingsPage.xaml.cs`

## Public APIs

- `LauncherConfigurationService.LoadOrCreate()`
- `LauncherConfigurationService.Save(AppConfig)`
- `LauncherConfigurationService.GetConfigPath()`
- `MainViewModel.SaveConfiguration()`
- `MainViewModel.AddProject()` / `DeleteSelectedProject()`
- `MainViewModel.AddCommand()` / `DeleteSelectedCommand()`

## Internal Flow

설정 파일은 `%APPDATA%/DevCommandLauncherApp/launcher.config.json`에 저장된다. 파일이 없으면 seed configuration을 생성한다. JSON 오류나 빈 project list는 fallback config로 복구하고 warning message를 반환한다.

legacy JSON에 `Items` property가 있으면 `LegacyLauncherConfiguration`으로 읽어서 project별 command 구조로 migration한다.

## State/Data Flow

`MainViewModel.ReloadConfiguration()`이 config를 load한 뒤 `RebuildCollections()`로 `ProjectViewModel`과 `CommandViewModel` collection을 다시 만든다. Settings page에서 변경한 값은 각 ViewModel property가 `Config` object에 직접 반영하고, `SaveConfiguration()`이 validate 후 JSON으로 저장한다.

## Important Constraints

- command id와 project id는 runtime state/log lookup key로 쓰이므로 임의 변경에 주의한다.
- URL/Health URL은 absolute URI로 validate한다.
- `PortText`는 UI string이지만 save 전에 numeric validation을 통과해야 한다.
- config normalize는 null/empty string을 정리하지만 business validation은 `MainViewModel.TryValidateConfiguration()`에서 한다.

## Known Problems

- seed configuration에 사용자 local path가 들어 있다. 배포나 새 사용자 환경에서는 기본값으로 적절하지 않을 수 있다.
- config schema version field가 없다.

## Regression Notes

- 설정 저장 후 selection 보존이 필요한 곳은 `RebuildCollections(preserveSelection: true)`를 사용한다.

## Rejected Approaches

- settings 변경을 별도 DTO에 복사한 뒤 save하는 방식은 현재 사용하지 않는다. ViewModel property가 `Config`를 직접 mutate한다.

## TODO

- config schema version 추가 여부 검토.
- seed configuration을 사용자 독립적인 예제로 바꿀지 검토.