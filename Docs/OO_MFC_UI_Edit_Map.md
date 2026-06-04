# OO_MFC UI Edit Map

이 문서는 사용자가 Unity Hierarchy와 Inspector에서 직접 고치면 되는 UI 위치를 정리한 지도입니다.
앞으로 Codex는 사용자가 직접 편집해야 하는 UI를 런타임에서 즉석 생성하지 않는 방향을 우선합니다.

## 공통 원칙

- 버튼 이미지, 위치, 크기, 폰트, 색상은 하이어라키의 UI 오브젝트에서 직접 수정합니다.
- 스크립트는 버튼이 눌렸을 때 어떤 기능이 실행되는지만 연결합니다.
- `View` 스크립트는 UI 오브젝트 참조표입니다.
- `Controller` 스크립트는 장면 순서와 버튼 클릭 후 흐름만 담당해야 합니다.

## Road / Stage HUD

주로 수정할 곳:

- `HUDSystemGroup`
- `HUDUIGroup`
- `HUDUIGroup > Button_Inventory`
- `HUDUIGroup > Button_Codex`
- `HUDUIGroup > Button_Mission`
- `HUDUIGroup > Button_Cooking`
- `HUDUIGroup > Button_WorldMap`
- `HUDUIGroup > Button_MainMenu`
- `HUDUIGroup > Panel_Inventory`
- `HUDUIGroup > Panel_Mission`
- `HUDUIGroup > NewBadge_Inventory`
- `HUDUIGroup > NewBadge_Codex`
- `HUDUIGroup > NewBadge_Mission`

관련 스크립트:

- `OOTechRoadHUDView`: HUD 버튼/패널/텍스트 참조표
- `OOTechRoadHUDController`: 버튼 클릭 후 인벤토리, 임무, 요리, 월드맵 흐름 처리

수정 기준:

- 이미지나 위치가 마음에 안 들면 `HUDUIGroup` 자식 오브젝트를 수정합니다.
- 버튼 기능이 안 먹으면 `OOTechRoadHUDView`에 참조가 빠졌는지 확인합니다.
- 코드에서 HUD 오브젝트를 새로 만들지 않습니다.

## CookingGroup UI

주로 수정할 곳:

- `CookingGroup`
- `CookingGroup > CookingUIGroup`
- `CookingUIGroup > Panel_CauldronGuide`
- `CookingUIGroup > Panel_CuttingboardGuide`
- `CookingUIGroup > Text_CauldronGuideArrow`
- `CookingUIGroup > Text_CuttingboardGuideArrow`
- `CookingUIGroup > Button_GuideConfirm`
- `CookingUIGroup > Panel_Inventory`
- `CookingGroup > Kitchen`
- `CookingGroup > Cauldron`
- `CookingGroup > Cuttingboard`

관련 스크립트:

- `OOTechCookingGroupView`: Cooking UI 참조표
- `OOTechCookingGroupController`: 조리 튜토리얼, 드래그 앤 드롭, 레시피 완료 흐름
- `OOTechCookingToolDropTarget`: 가마솥/도마가 받을 수 있는 재료 규칙

수정 기준:

- 가마솥/도마 위치와 이미지는 하이어라키 오브젝트에서 직접 수정합니다.
- 어떤 재료를 받을지는 `OOTechCookingToolDropTarget` 또는 데이터 규칙으로 관리합니다.
- 가마솥에는 쌀/고기/끓이는 재료, 도마에는 채소/과일/손질 재료를 넣는 방향으로 확장합니다.

## Dialogue UI

주로 수정할 곳:

- `DialogueGroup`
- `DialogueGroup > DialoguePanel`
- `DialoguePanel > SpeakerNameText`
- `DialoguePanel > DialogueText`
- `DialoguePanel > Dialogue_NextButton`

관련 스크립트:

- `DialogueUI`: 데이터 드리븐 대사 출력과 다음 대사 진행
- `OOTechDialogueLayout`: RoadView에서 대화창 위치 보정
- `OOTechDialogueSpeakerNameBackdrop`: 화자 이름 뒤 배경 박스 보정

수정 기준:

- 화자 이름 배경, 버튼 위치, 대화창 크기는 `DialoguePanel` 안에서 직접 수정합니다.
- 화자/대사 내용은 `OO_Character.json`, `OO_Dialogue.json`, `OO_DialogueGroup.json`에서 데이터 드리븐합니다.

## TutorialGuideGroup

주로 수정할 곳:

- `TutorialGuideGroup`
- `TutorialGuideGroup > TutorialGuidePanel`
- `TutorialGuidePanel > Text_Title`
- `TutorialGuidePanel > Text_Description`
- `TutorialGuidePanel > Button_GuideConfirm`

관련 스크립트:

- `OOTechTutorialGuideUI`: 튜토리얼 문구와 확인 입력
- `OOTechTutorial2Controller`: Road HUD 소개 흐름

수정 기준:

- 확인 버튼 이미지는 하이어라키에서 직접 수정합니다.
- 튜토리얼 문구는 `OO_Tutorial.json`에서 데이터 드리븐합니다.
- `Button_GuideNext`를 다시 만들지 않습니다. 필요하면 `Button_GuideConfirm`만 씁니다.

## WorldMapGroup

주로 수정할 곳:

- `WorldMapGroup`
- `WorldMapGroup > WorldMapBackground`
- `WorldMapGroup > Button_Back`
- `WorldMapGroup > StagePortrait` 계열 오브젝트

관련 스크립트:

- `OOTechWorldMapOverlayView`: 월드맵 UI 참조표
- `OOTechWorldMapOverlayController`: 월드맵 열기/돌아가기/카메라 표시

수정 기준:

- 월드맵 이미지는 전체가 GameView에 보이도록 SpriteRenderer 또는 UI Image를 직접 조정합니다.
- HUD는 WorldMapGroup이 열릴 때 꺼지고, 돌아가기 시 원래 Road/Stage로 돌아옵니다.

## Stage1Group

주로 수정할 곳:

- `Stage1Group`
- `Stage1Group > Stage1-1`
- `Stage1Group > Stage1-2`
- `Stage1Group > Moran`
- `Stage1Group > StageName`

관련 스크립트:

- `OOTechStage1GroupController`: Stage1-1/Stage1-2 이동, Moran 조작, StageName 연출

수정 기준:

- `Stage1-1`, `Stage1-2` 배경 이미지는 각 오브젝트의 SpriteRenderer에서 직접 교체합니다.
- Moran 시작 위치는 `Stage1Group > Moran` 배치를 기준으로 조정하되, 진입 시 Controller가 맵 왼쪽 입구로 보정합니다.
- StageName 텍스트 내용은 `OO_Stage.json`의 `Name`에서 데이터 드리븐합니다.

## 앞으로 Codex에게 요청할 때 좋은 형식

기능 요청 전에 아래처럼 말하면 구조가 덜 깨집니다.

```text
이 UI는 내가 직접 배치할 예정이니 코드로 생성하지 말고,
필요한 컴포넌트와 연결해야 할 참조만 알려줘.
```

```text
이 기능은 데이터 드리븐으로 만들고,
엑셀/JSON 스키마와 연결되는 컴포넌트 이름을 먼저 제안해줘.
```

```text
새로운 오브젝트가 필요하면 런타임 생성하지 말고,
Hierarchy에 만들 오브젝트 이름과 붙일 컴포넌트를 알려줘.
```
