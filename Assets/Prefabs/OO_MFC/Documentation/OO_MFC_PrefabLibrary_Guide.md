# OO_MFC Prefab Library Guide

이 폴더는 OO_MFC 씬에서 반복해서 꺼내 쓸 소품 창고입니다.
영화 제작으로 보면, 매번 현장에서 종이 소품을 새로 만드는 대신 미리 정리된 실제 소품을 꺼내 쓰기 위한 보관함입니다.

## Managers

- `GameDataManager.prefab`
  - JSON 기반 Static Data를 읽는 데이터 매니저 소품입니다.
  - 씬에 전역 데이터 매니저가 필요할 때 사용합니다.

## UI/CommonButtons

- `CommonSkipButton.prefab`
  - Prologue, Tutorial, Scenario, Stage 등에서 다음 그룹으로 넘어갈 때 쓰는 공용 버튼입니다.
- `CommonBackButton.prefab`
  - Codex, WorldMap, Cooking 같은 오버레이에서 이전 그룹으로 돌아갈 때 쓰는 공용 버튼입니다.
- `ExitButton.prefab`
  - 종료 또는 닫기 계열 버튼으로 재사용할 수 있는 UI 소품입니다.

## UI/HUD

- Road/Stage에서 공유하는 하단 HUD 프리팹을 보관할 자리입니다.
- `HUDUIGroup`을 프리팹화하면 이 폴더에 넣어 두면 됩니다.

## UI/Dialogue

- `DialogueGroup`, `DialoguePanel`, SpeakerNameText 배경 같은 대화 UI 프리팹을 보관할 자리입니다.

## UI/Tutorial

- `TutorialGuideGroup`, HUD 포커스 가이드, 말풍선 가이드 같은 안내 UI 프리팹을 보관할 자리입니다.

## Characters

- Jaeik, Mr.Jaeik, Moran, Chunyang, JangYoungSim 같은 배우 프리팹을 보관할 자리입니다.
- 캐릭터는 가능하면 Animator, Rigidbody2D, Collider2D, 역할 컴포넌트를 함께 붙여서 저장합니다.

## Groups

- `RoadGroups`
  - `1st_Road_to_Stage1`, `2nd_Road_to_Stage2`처럼 반복되는 로드 무대 프리팹을 보관합니다.
  - `Tools/OO MFC/Build Road Stage Skeletons`도 이 폴더에 RoadGroup 프리팹을 저장합니다.
- `StageGroups`
  - `Stage1Group`, `Stage2Group`, `FinalStageGroup` 같은 스테이지 무대 프리팹을 보관합니다.
- `ScenarioGroups`
  - `Senario1Group`처럼 서사 진행이 큰 그룹 프리팹을 보관합니다.
- `CookingGroups`
  - `CookingGroup`, `CookingUIGroup`, `Cauldron` 관련 프리팹을 보관합니다.
- `WorldMapGroups`
  - `WorldMapGroup`과 월드맵 오버레이 프리팹을 보관합니다.

## Props

- 음식, 가마솥, 배경 위 상호작용 오브젝트처럼 캐릭터도 UI도 아닌 소품 프리팹을 보관합니다.

## 사용 기준

- 씬에서 직접 배치하고 눈으로 편집해야 하는 UI나 배우는 먼저 씬 오브젝트로 만든 뒤 프리팹화합니다.
- 코드가 매번 새 UI를 만드는 방식은 피하고, 프리팹 또는 씬 자식 오브젝트를 컨트롤러가 연결해서 쓰는 방식을 우선합니다.
- 반복 슬롯처럼 개수가 계속 바뀌는 소품은 템플릿 프리팹 또는 템플릿 자식 오브젝트를 복제해도 됩니다.
