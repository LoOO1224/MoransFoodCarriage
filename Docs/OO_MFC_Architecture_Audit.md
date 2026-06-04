# OO_MFC Architecture Audit

이 문서는 `OO_MFC` 씬과 스크립트를 앞으로 유지, 보수, 확장할 때 지킬 기준입니다.
기준은 첨부 PDF의 DaniTech 스타일, 현재 버그 원인, SOLID 원칙을 함께 반영합니다.

## 1. PDF 스타일 기준

- 멤버 변수는 `_aaa` 형태로 쓰고 소문자로 시작합니다.
- Unity 오브젝트 참조는 `[SerializeField] private`를 기본으로 하고, 이름은 `Text_AAA`, `Button_AAA`, `Prefab_AAA`, `Root_AAA`, `Object_AAA`처럼 대문자로 시작합니다.
- 외부 공개가 필요하면 public field보다 `{ get; private set; }` 또는 public Get/Set 메서드를 우선합니다.
- 클래스 내부에서만 쓰는 메서드는 `private`로 둡니다.
- 외부에서 호출해야 하는 메서드만 `public`으로 엽니다.
- 메서드는 동사로 시작합니다. 예: `RequestOpenInventory`, `SetHUDVisible`, `PlayStageNameRoutine`.
- Static Data는 `OO_Stage`, `OO_Ingredient`처럼 `GameDataBase`를 상속하는 Data 클래스로 둡니다.
- 플레이 중 변하고 저장되어야 하는 데이터는 `OOTechPlayerModel`, `OOTechItemModel`처럼 Model로 둡니다.
- UI 그룹 열기/닫기는 `OOTechUIManager`와 `UIManagerExtension`을 통해 통일합니다.
- 사운드는 `OOTechSoundManager` 또는 개별 BGM Player가 담당하되, 데이터 경로와 실제 AudioClip 참조 규칙을 섞지 않습니다.

## 2. 현재 위험 구역

### 거대 Controller

현재 줄 수 기준으로 위험도가 높은 파일입니다.

- `OOTechCookingGroupController.cs`: 약 1680줄
- `OOTechRoadHUDController.cs`: 약 1630줄
- `OOTechSenario1Controller.cs`: 약 1350줄
- `OOTechTutorial1Controller.cs`: 약 1040줄
- `OOTechRoadToStage1Controller.cs`: 약 920줄

이 파일들은 무대감독이 대사, 조명, 소품, 카메라, 배우 이동을 모두 직접 잡는 상태입니다.
앞으로는 장면 순서만 Controller에 남기고, 실제 역할은 작은 컴포넌트로 분리합니다.

### Runtime 생성 UI

아래 패턴은 버그를 만들기 쉽습니다.

- `new GameObject(...)`
- `Instantiate(...)`로 UI를 즉석 생성
- `Resources.FindObjectsOfTypeAll`
- `FindObjectsByType`를 매번 호출해서 다른 그룹 오브젝트 찾기

허용되는 경우:

- 인벤토리 슬롯처럼 실제로 동적으로 늘어나는 반복 요소
- 드래그 중 잠깐 보이는 `DragGhost`
- 이펙트처럼 1회성이고 씬 편집 대상이 아닌 오브젝트

금지에 가까운 경우:

- HUD 버튼, 튜토리얼 패널, 스킵 버튼, Stage 배경, Cooking 안내 UI
- 사용자가 Scene View에서 직접 배치하고 이미지 교체해야 하는 UI

## 3. Editor 스크립트 사용 규칙

Editor 스크립트는 런타임에서 직접 실행되지는 않지만, 씬 파일을 고치는 순간 사용자의 수동 배치를 덮어쓸 수 있습니다.
따라서 앞으로는 다음 원칙을 지킵니다.

- 자동 실행 금지. `MenuItem` 또는 명시적 배치모드 요청에서만 실행합니다.
- UI 크기와 위치를 강제로 고치지 않습니다.
- 사용자가 직접 배치한 오브젝트를 삭제하지 않습니다.
- 오브젝트를 만들더라도 이름, 역할, 컴포넌트가 명확해야 합니다.
- 수리 스크립트는 일회성 복구 도구이며, 게임 로직의 일부로 간주하지 않습니다.

## 4. 책임 분리 기준

### Manager

Manager는 공통 창구입니다.

- `OOTechGameManager`: 플레이 중 저장될 Model 보유
- `OOTechGameDataManager`: JSON Static Data 로드와 Get 메서드
- `OOTechUIManager`: 그룹 Open/Close
- `OOTechSoundManager`: 사운드 재생
- `OOTechItemCatalogManager`: 아이템 ID와 실제 아이콘/프리팹 연결

Manager가 직접 특정 장면의 배우를 이동시키거나 UI 배치를 고치기 시작하면 책임이 커집니다.

### Controller

Controller는 큐시트입니다.

예: `Stage1GroupController`

- Stage1-1 시작
- Moran 이동 허용
- 오른쪽 끝 도달 시 Stage1-2 전환
- StageName 연출 시작

Controller가 직접 버튼 이미지 생성, 아이템 이미지 경로 수리, 다른 그룹의 자식 검색까지 맡으면 SRP 위반입니다.

### Component

Component는 배우에게 붙는 역할표입니다.

예:

- `OOTechSceneObject`: 이 오브젝트의 RoleId
- `OOTechItemDefinitionObject`: 이 오브젝트가 어떤 아이템인지
- `OOTechCookingToolDropTarget`: 이 조리도구가 어떤 재료를 받는지
- `OOTechRoadHUDView`: HUD 안의 버튼과 텍스트 참조표

감독은 배우를 직접 붙잡지 않고, 배우에게 붙은 역할표를 읽어야 합니다.

### View

View는 화면 오브젝트 참조만 담당합니다.

- 버튼 이벤트 흐름 판단 금지
- 데이터 로딩 금지
- 씬 그룹 전환 금지

View는 `Text`, `Image`, `Button`, `RectTransform` 같은 무대 소품을 노출하고, Controller가 그 소품을 사용합니다.

## 5. SOLID 적용 기준

### S: 단일 책임 원칙

한 클래스가 바뀌는 이유는 하나여야 합니다.

- CookingGroupController가 튜토리얼, 카메라, 인벤토리, 레시피, 다이얼로그를 모두 가지면 위험합니다.
- 조리도구 판정은 `OOTechCookingToolDropTarget`으로 분리합니다.
- 인벤토리 표시 갱신은 HUD 또는 Inventory 전용 컴포넌트로 분리합니다.

### O: 개방 폐쇄 원칙

새 요리나 새 Stage가 생겨도 기존 Controller를 크게 수정하지 않아야 합니다.

- 새 재료는 `OO_Ingredient.json`과 `OOTechItemDefinitionObject`로 추가합니다.
- 새 요리는 `OO_Recipe.json`, `OO_Cook.json`으로 추가합니다.
- 새 Stage는 `OO_Stage.json`, `OO_StageQuest.json`, StageGroup 컴포넌트로 추가합니다.

### L: 리스코프 치환 원칙

같은 역할 컴포넌트는 같은 약속을 지켜야 합니다.

- 모든 아이템 오브젝트는 `ItemDataId`로 조회 가능해야 합니다.
- 모든 조리도구 DropTarget은 `CanAcceptIngredient` 규칙을 동일하게 따라야 합니다.

### I: 인터페이스 분리 원칙

큰 만능 컴포넌트보다 작은 역할 컴포넌트를 둡니다.

- HUD 표시
- HUD 버튼 입력
- 인벤토리 슬롯 표시
- 조리도구 드랍 판정
- Stage 이동

이 기능들을 하나의 Controller에 몰아넣지 않습니다.

### D: 의존 역전 원칙

구체 오브젝트 이름 검색보다 역할 컴포넌트와 데이터 ID에 의존합니다.

나쁜 방향:

```csharp
FindChildByName("Button_Cooking")
```

좋은 방향:

```csharp
[SerializeField] private OOTechRoadHUDView View_HUD;
```

또는:

```csharp
OOTechSceneObject.RoleId == "Button_Cooking"
```

## 6. 앞으로의 구현 체크리스트

새 기능을 추가하기 전에 반드시 확인합니다.

- 이 기능은 Data, Model, View, Component, Controller, Manager 중 어디 책임인가?
- 사용자가 직접 배치해야 하는 UI를 코드에서 만들고 있지 않은가?
- `FindObjectsOfTypeAll`이나 이름 검색 없이 연결할 수 있는가?
- 새 데이터는 Excel/JSON에서 드리븐되는가?
- 플레이 중 변하는 값은 Model로 저장되는가?
- 버튼과 이미지 참조는 View 컴포넌트에 모였는가?
- Controller는 큐시트만 들고 있는가?
- Editor 스크립트가 사용자의 수동 배치를 덮어쓰지 않는가?
- 변경 후 배치모드 컴파일 검증이 필요한가?

## 7. 우선 정리 순서

1. 현재 버그를 더 만들 수 있는 Editor 수리 도구를 사용 중지 목록으로 분류합니다.
2. HUD, Cooking, Stage1, Dialogue의 View 컴포넌트를 먼저 확정합니다.
3. 거대 Controller에서 UI 참조와 런타임 생성 코드를 View/Prefab 방식으로 분리합니다.
4. 이름 검색 기반 참조를 `SerializeField` 또는 `OOTechSceneObject.RoleId` 기반으로 바꿉니다.
5. 마지막에 배치모드로 컴파일 검증하고, 사용자가 Play Mode에서 조작 검증합니다.

## 8. Codex 작업 원칙

앞으로 Codex는 다음 원칙을 지킵니다.

- 코드 수정 전 배치모드 필요 여부를 먼저 말합니다.
- 기능을 만들기 전에 어떤 오브젝트/컴포넌트를 씬에 둬야 하는지 먼저 설명합니다.
- 런타임 UI 생성은 반복 슬롯과 임시 이펙트 외에는 피합니다.
- Editor 스크립트는 사용자가 요청한 경우에만 씬을 변경합니다.
- 수정 후 가장 오래 걸린 문제와 해결 방법을 설명합니다.
- PDF 스타일에 맞춰 핵심 메서드에는 짧은 한글 주석을 남깁니다.
