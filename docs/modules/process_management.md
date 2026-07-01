# Process Management Module

## Purpose

Process management module은 command process lifecycle, existing server detection, port conflict check, health check, stdout/stderr log capture, status transition을 담당한다.

## Related Files

- `Services/CommandProcessManager.cs`
- `Services/HealthCheckService.cs`
- `Services/PortChecker.cs`
- `Models/CommandRuntimeState.cs`
- `Models/CommandStatus.cs`
- `Models/LogBuffer.cs`
- `Models/LogEntry.cs`
- `ViewModels/MainViewModel.cs`

## Public APIs

- `CommandProcessManager.StartAsync(CommandConfig, CancellationToken)`
- `CommandProcessManager.Stop(string)`
- `CommandProcessManager.RestartAsync(CommandConfig, CancellationToken)`
- `CommandProcessManager.DetectExistingServerAsync(CommandConfig, CancellationToken)`
- `CommandProcessManager.GetState(string)`
- Events: `CommandStateChanged`, `CommandLogAdded`

## Internal Flow

`StartAsync`는 working directory validation, optional port conflict/existing server check, command parsing을 거친 뒤 direct process launch를 시도한다. direct launch가 executable not found로 실패하면 shell fallback을 사용한다.

`DetectExistingServerAsync`는 앱 시작 또는 config reload 이후 configured `Port`를 검사한다. Port가 열려 있고 `HealthUrl`, `Url`, 또는 `http://localhost:<port>` 중 하나가 HTTP success로 응답하면 `ExternalRunning`으로 표시한다. Port는 열려 있지만 endpoint가 응답하지 않으면 `PortConflict`로 표시한다.

stdout/stderr는 async read로 capture되고 ANSI escape sequence는 제거된다. process exit event는 status를 `Stopped` 또는 `Crashed`로 갱신하고 system log를 추가한다.

`Stop`은 `Kill(entireProcessTree: true)` 이후 `WaitForExit(3000)`을 호출한다. timeout 이후에도 process가 살아 있으면 `Process` reference를 유지하고 `Error` 상태로 표시해 사용자가 다시 stop을 시도할 수 있게 한다.

## State/Data Flow

`CommandRuntimeState`는 command id별 dictionary에 보관된다. `MainViewModel`은 `CommandStateChanged`와 `CommandLogAdded` event를 `DispatcherQueue`로 UI thread에 marshal한 뒤 command card와 log text를 refresh한다.

앱이 직접 시작하지 않은 server는 `Process`가 없으므로 stop/restart 대상이 아니다. Dashboard는 이 상태를 `External` badge로 표시하고 open/copy URL action만 제공한다.

## Important Constraints

- UI thread 업데이트는 `DispatcherQueue.TryEnqueue`로 수행한다.
- process kill은 `Kill(entireProcessTree: true)`를 사용한다.
- environment에는 UTF-8과 no-color 관련 변수들이 설정된다.
- `LogBuffer`는 UI log display의 source이므로 clear/filter behavior를 변경할 때 Logs page도 확인한다.
- `ExternalRunning`과 `PortConflict` 구분은 HTTP endpoint 응답 여부에 의존한다. HTTP가 아닌 server이거나 URL이 잘못 설정된 server는 port 점유로 표시될 수 있다.
- shell fallback은 `&`, `|`, `^`, `<`, `>`, backtick 같은 shell metacharacter가 포함된 command line을 거부한다. 복잡한 shell syntax가 필요한 command는 executable/args를 direct launch 가능한 형태로 설정해야 한다.

## Known Problems

- shell fallback command quoting은 복잡한 quote/escape case에서 취약할 수 있어 metacharacter 포함 command는 실행하지 않는다.
- `Stop`은 `WaitForExit(3000)` 이후 process tree 종료 상태를 기준으로 status를 갱신한다.
- health check timeout/detail policy는 문서화가 더 필요하다.
- existing server detection은 짧은 HTTP timeout을 사용하므로 느린 endpoint는 일시적으로 `PortConflict`로 보일 수 있다.

## Regression Notes

- ANSI escape cleanup, UTF-8 environment, stdout/stderr redirect는 로그 표시 안정성에 중요하다.

## Rejected Approaches

- command 실행을 항상 shell로 보내는 방식은 rejected. direct launch를 우선하고 executable lookup 실패 시에만 shell fallback한다.

## TODO

- shell quoting test case 추가.
- health check failure UX 문구 정리.
