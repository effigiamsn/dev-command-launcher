# Regression Notes

## NavigationView branch reuse can hide PR prompt

- Cause: 이미 merged된 PR의 head branch를 재사용하면 GitHub UI가 Pull Requests tab의 "Compare & pull request" prompt를 표시하지 않을 수 있다.
- Symptoms: branch는 `main`보다 ahead인데 PR open button이 보이지 않는다.
- Affected modules: GitHub workflow, branch management
- Prevention: merged PR branch를 재사용하지 말고 새 branch를 만든다.

## Commit author / co-author mismatch

- Cause: `effigiamsn`용 repository에서 SSH remote만 바꾸고 repo-local `user.name`, `user.email`을 바꾸지 않으면 global author `HexX`로 commit된다.
- Symptoms: `Co-authored-by: HexX <mchoi0427@gmail.com>`가 author와 같은 identity가 되어 Pair Extraordinaire badge 조건에 맞지 않는다.
- Affected modules: Git workflow
- Prevention: 이 repo의 local config는 `effigiamsn <295384108+effigiamsn@users.noreply.github.com>`로 유지한다.

## Card compact mode behavior confusion

- Cause: section collapse를 section list visibility에 직접 binding하면 카드 전체가 사라져 사용자가 기대한 compact card behavior와 달라진다.
- Symptoms: section을 접었을 때 카드 title/status/action buttons까지 사라진다.
- Affected modules: `Views/DashboardPage.xaml`, `ViewModels/ProjectViewModel.cs`, `ViewModels/CommandViewModel.cs`
- Prevention: section toggle은 각 command의 `DetailsVisibility`만 변경한다. `GridView`는 숨기지 않는다.

## Narrow card button overflow

- Cause: card width를 줄인 뒤 기존 action button width/spacing을 유지하면 row가 좁은 card 안에서 빡빡해진다.
- Symptoms: button row overflow, uneven layout, clipped controls
- Affected modules: `Views/DashboardPage.xaml`
- Prevention: card width 변경 시 action button width와 spacing을 함께 확인한다.