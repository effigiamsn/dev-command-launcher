# DevCommandLauncher Docs

이 폴더는 DevCommandLauncher의 장기 유지보수 문서입니다. 채팅 기록에만 남기면 잊히기 쉬운 구조, 제약, 회귀 이력을 이곳에 보존합니다.

## 읽는 순서

1. `docs/RULES.md` - 이 repository에서 지켜야 할 작업 규칙
2. `docs/PROJECT_MAP.md` - 기능 이름과 실제 파일 위치 매핑
3. `docs/modules/*.md` - 주요 module별 목적, 흐름, 제약
4. `docs/regression.md` - 반복되기 쉬운 bug와 예방 메모
5. `docs/TODO.md` - 아직 남은 후속 작업

## 프로젝트 요약

DevCommandLauncher는 Windows desktop용 WinUI command launcher입니다. 프로젝트별 command를 카드로 보여주고, command start/stop/restart, log 조회, 설정 편집을 제공합니다.

현재 앱은 `net10.0-windows10.0.19041.0` + WinUI + Windows App SDK 기반이며, packaged app이 아닌 unpackaged desktop app으로 빌드됩니다.