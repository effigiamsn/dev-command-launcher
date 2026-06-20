# Repository Rules

## 기본 원칙

- 코드 변경 전 관련 module 문서를 먼저 확인한다.
- UI 변경은 `dotnet build DevCommandLauncherApp.csproj`로 최소 검증한다.
- 작은 UI 조정이라도 실제 XAML binding 이름과 ViewModel property가 맞는지 확인한다.
- `bin/`, `obj/`, `*.user`, `.vs/`는 commit하지 않는다.
- 이 repository의 commit author는 `effigiamsn <295384108+effigiamsn@users.noreply.github.com>`를 사용한다.
- 메인 GitHub 계정 공동작업 표시는 commit message trailer에 `Co-authored-by: HexX <mchoi0427@gmail.com>`를 남긴다.

## 문서 규칙

- 구조, 제약, 회귀 이력은 한국어로 작성한다.
- file/class/property/API 이름은 English identifier 그대로 유지한다.
- module 변경 후 관련 `docs/modules/*.md`를 갱신한다.
- 반복 가능성이 있는 bug는 `docs/regression.md`에 원인, 증상, 예방책을 남긴다.

## UI 규칙

- 카드, sidebar, button처럼 고정된 UI 요소는 width/height/spacing을 명시해 layout shift를 줄인다.
- 좁은 카드에서는 button row가 넘치지 않도록 button width와 spacing을 함께 조정한다.
- section-level collapse는 section header control에서 실행하되, 카드 자체는 남기고 card details만 숨기는 compact mode를 기본으로 한다.
- card compact mode에서 유지할 요소는 title, status badge, action buttons이다.

## Git/GitHub 규칙

- `effigiamsn` GitHub 계정용 remote는 SSH alias `github-effigiamsn`를 사용한다.
- 새 PR branch는 이전에 merge된 branch를 재사용하지 않는다. GitHub UI가 PR button을 숨길 수 있다.
- 같은 branch를 이미 merged PR에 사용했다면 새 branch를 만들어 push한다.
- history rewrite가 필요한 경우 `git push --force-with-lease`만 사용한다.