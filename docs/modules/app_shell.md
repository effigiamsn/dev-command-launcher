# App Shell

## Purpose

App shell은 WinUI `NavigationView` 기반의 sidebar navigation, page frame, 상단 title 영역, 하단 status bar를 제공한다.

## Related Files

- `MainWindow.xaml`
- `MainWindow.xaml.cs`
- `App.xaml`
- `App.xaml.cs`

## Public APIs

- `MainWindow.NavigateToLogs()`
- `MainWindow.OnNavigationSelectionChanged(...)`

## Internal Flow

`MainWindow`는 `NavigationView` menu selection에 따라 `ContentFrame`을 Dashboard, Settings, Logs page로 전환한다. Dashboard의 log button은 `MainWindow.NavigateToLogs()`를 호출해 Logs page로 이동한다.

## State/Data Flow

- `MainWindow`는 `App.ViewModel`을 참조해 status bar와 pages가 같은 global state를 사용하게 한다.
- `NavigationView.PaneTitle`은 `Dev Command Launcher`이다.
- `OpenPaneLength=220`으로 sidebar width를 좁게 유지한다.

## Important Constraints

- sidebar width를 줄일 때 `PaneTitle`이 잘리지 않는지 확인한다.
- NavigationView pane width는 앱 전체 content area에 영향을 준다.
- 이 app은 unpackaged WinUI desktop app이다.

## Known Problems

- DPI/font scaling이 큰 환경에서 `OpenPaneLength=220`이 충분한지 아직 실기 검증이 필요하다.

## Regression Notes

- Sidebar 변경 후 `dotnet build DevCommandLauncherApp.csproj`를 실행한다.

## Rejected Approaches

- sidebar title을 줄이거나 약어로 바꾸는 방식은 현재 rejected. 사용자 요청은 title이 잘리지 않는 선에서 pane만 좁히는 것이다.

## TODO

- 실제 UI에서 220px sidebar가 title/menu text를 안정적으로 표시하는지 확인한다.