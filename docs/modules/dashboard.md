# Dashboard Module

## Purpose

Dashboard는 project section별 command card를 보여주고 start/stop/restart/log/open-url/copy-url action을 제공한다.

## Related Files

- `Views/DashboardPage.xaml`
- `Views/DashboardPage.xaml.cs`
- `ViewModels/ProjectViewModel.cs`
- `ViewModels/CommandViewModel.cs`
- `ViewModels/MainViewModel.cs`

## Public APIs

- `MainViewModel.StartAsync(CommandViewModel?)`
- `MainViewModel.Stop(CommandViewModel?)`
- `MainViewModel.RestartAsync(CommandViewModel?)`
- `MainViewModel.SelectLogs(CommandViewModel?)`
- `ProjectViewModel.ToggleCollapsed()`
- `CommandViewModel.SetCollapsed(bool)`

## Internal Flow

`DashboardPage`는 `App.ViewModel`을 `DataContext`로 사용한다. top command bar는 all commands에 대해 start/stop/reload를 호출한다. 각 card button은 `Tag`에 bound된 `CommandViewModel`을 code-behind handler로 전달한다.

section header의 collapse button은 `ProjectViewModel.ToggleCollapsed()`를 호출한다. 이 method는 section 안의 모든 `CommandViewModel.SetCollapsed(value)`를 호출해 card details 영역만 숨긴다.

## State/Data Flow

- `ProjectViewModel.Commands`는 card list의 source이다.
- `CommandViewModel.Status`는 status badge color/text와 action button enabled state를 결정한다.
- `CommandViewModel.DetailsVisibility`는 command summary, working directory, uptime, last log 영역의 visibility를 제어한다.
- `OpenUrlVisibility`는 URL이 있는 command에서만 open button을 보여준다.
- `ServerAddress`는 `Url`이 있으면 해당 값을 사용하고, 없으면 `Port` 기반 `http://localhost:<port>`를 표시한다.
- `ExternalRunning`은 앱이 직접 실행한 process는 아니지만 configured server endpoint가 응답하는 상태를 의미한다.

## Important Constraints

- section collapse는 `GridView` 자체를 숨기지 않는다.
- compact card 상태에서도 title, status badge, action buttons는 남아야 한다.
- card width는 현재 `252`이고 button row는 `38x36`, spacing `6` 기준으로 맞춰져 있다.
- card title은 `TextTrimming=CharacterEllipsis`를 사용한다.
- server address row는 click 가능한 `HyperlinkButton`과 copy icon button으로 구성한다. 긴 URL은 card width 안에서 layout shift가 생기지 않도록 row width를 card 안에 유지한다.

## Known Problems

- compact/expanded 상태가 config에 persist되지는 않는다.
- section collapse icon은 glyph code로 표시되며 theme/icon naming이 명확하지 않다.
- `Url`이 없고 `Port`만 있는 command는 `http://localhost:<port>`로 address를 추론한다.

## Regression Notes

- `docs/regression.md`의 "Card compact mode behavior confusion"과 "Narrow card button overflow"를 확인한다.

## Rejected Approaches

- `GridView.Visibility`를 section collapse에 직접 binding하는 방식은 rejected. 카드 전체가 사라져 요청된 compact behavior와 맞지 않는다.

## TODO

- 실제 화면에서 section collapse spacing과 scroll behavior를 확인한다.
