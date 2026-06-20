# Process Management Module

## Purpose

Process management module은 command process lifecycle, port conflict check, health check, stdout/stderr log capture, status transition을 담당한다.

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
- `CommandProcessManager.GetState(string)`
- Events: `CommandStateChanged`, `CommandLogAdded`

## Internal Flow

`StartAsync`는 working directory validation, optional port conflict check, command parsing을 거친 뒤 direct process launch를 시도한다. direct launch가 executable not found로 실패하면 shell fallback을 사용한다.

stdout/stderr는 async read로 capture되고 ANSI escape sequence는 제거된다. process exit event는 status를 `Stopped` 또는 `Crashed`로 갱신하고 system log를 추가한다.

## State/Data Flow

`CommandRuntimeState`는 command id별 dictionary에 보관된다. `MainViewModel`은 `CommandStateChanged`와 `CommandLogAdded` event를 `DispatcherQueue`로 UI thread에 marshal한 뒤 command card와 log text를 refresh한다.

## Important Constraints

- UI thread 업데이트는 `DispatcherQueue.TryEnqueue`로 수행한다.
- process kill은 `Kill(entireProcessTree: true)`를 사용한다.
- environment에는 UTF-8과 no-color 관련 변수들이 설정된다.
- `LogBuffer`는 UI log display의 source이므로 clear/filter behavior를 변경할 때 Logs page도 확인한다.

## Known Problems

- shell fallback command quoting은 복잡한 quote/escape case에서 취약할 수 있다.
- `Stop`은 `WaitForExit(3000)` 이후 process tree 종료 상태를 기준으로 status를 갱신한다.
- health check timeout/detail policy는 문서화가 더 필요하다.

## Regression Notes

- ANSI escape cleanup, UTF-8 environment, stdout/stderr redirect는 로그 표시 안정성에 중요하다.

## Rejected Approaches

- command 실행을 항상 shell로 보내는 방식은 rejected. direct launch를 우선하고 executable lookup 실패 시에만 shell fallback한다.

## TODO

- shell quoting test case 추가.
- health check failure UX 문구 정리.