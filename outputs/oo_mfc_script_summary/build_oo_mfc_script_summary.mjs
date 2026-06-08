import fs from "node:fs/promises";
import path from "node:path";
import { SpreadsheetFile, Workbook } from "@oai/artifact-tool";

const outputDir = path.resolve("outputs/oo_mfc_script_summary");
const outputPath = path.join(outputDir, "OO_MFC_커스텀_스크립트_요약_중요도.xlsx");

const headers = [
  "중요도 순번",
  "중요도",
  "영역",
  "스크립트명",
  "경로",
  "씬 연결",
  "요약",
  "중요도 이유",
];

const rows = [
  [1, "최상", "Managers", "OOTechGameManager.cs", "Assets/Scripts/OO_MFC/Managers/OOTechGameManager.cs", "직접", "플레이어 모델, 인벤토리, 아이템 증감, 스테이지 클리어 상태를 관리하는 전체 진행 데이터 허브입니다.\nPDF 기준의 GameManager 역할과 가장 직접적으로 맞닿아 있습니다.", "아이템/진행 상태가 여러 스테이지와 UI에서 계속 참조되므로 전체 게임 흐름 영향이 가장 큽니다."],
  [2, "최상", "Managers", "OOTechGameDataManager.cs", "Assets/Scripts/OO_MFC/Managers/OOTechGameDataManager.cs", "직접", "Resources/JsonOutput의 정적 데이터를 읽고 OO_ 계열 GameData로 변환해 Dictionary 조회 API를 제공합니다.\n대사, 선택지, 요리, 스테이지, 튜토리얼 데이터 흐름의 중심입니다.", "기획 데이터 로드가 실패하면 대사/스테이지/요리 진행이 넓게 흔들립니다."],
  [3, "최상", "Managers", "OOTechUIManager.cs", "Assets/Scripts/OO_MFC/Managers/OOTechUIManager.cs", "직접", "씬 UI 그룹을 등록하고 Open/Close/Switch 상태를 중앙에서 관리합니다.\nPDF의 UIManager 구조를 OO_MFC 방식으로 변형한 핵심 UI 허브입니다.", "대부분의 화면 전환과 그룹 활성화가 이 매니저를 거치므로 사용자 진행 흐름에 직접 영향이 있습니다."],
  [4, "최상", "UI", "DialogueUI.cs", "Assets/Scripts/OO_MFC/UI/DialogueUI.cs", "직접", "대사, 내레이션, 선택지, 다음 버튼, 외부 클릭 차단 등을 처리하는 공통 대화 UI입니다.\n여러 스테이지 컨트롤러가 공유하는 이야기 출력 창입니다.", "스토리 게임의 핵심 UX이며, 대사 진행 오류는 대부분의 장면 진행을 막을 수 있습니다."],
  [5, "최상", "WorldMap", "OOTechRoadHUDController.cs", "Assets/Scripts/OO_MFC/WorldMap/OOTechRoadHUDController.cs", "직접", "로드/스테이지 화면의 HUD 버튼, 인벤토리, 미션, 월드맵, 요리 진입, NEW 배지를 관리합니다.\nGameManager 인벤토리와 UIManager 화면 전환을 이어주는 대형 컨트롤러입니다.", "플레이어가 자주 누르는 HUD와 진행 보조 UI를 담당해서 체감 영향이 큽니다."],
  [6, "최상", "Cooking", "OOTechCookingGroupController.cs", "Assets/Scripts/OO_MFC/Cooking/OOTechCookingGroupController.cs", "직접", "요리 화면의 재료 드래그, 조리 도구 판정, 인벤토리 반영, 튜토리얼 가이드, 스테이지별 예외 흐름을 담당합니다.\n현재 OO_MFC에서 가장 큰 기능 컨트롤러 중 하나입니다.", "요리 시스템과 스테이지 보상/퀘스트가 강하게 연결되어 있어 오류 시 진행 차단 위험이 큽니다."],
  [7, "상", "Cooking", "OOTechCookingManager.cs", "Assets/Scripts/OO_MFC/Cooking/OOTechCookingManager.cs", "직접", "선택된 재료 ID 목록을 레시피 데이터와 비교해 요리 성공/실패 결과를 반환합니다.\n데이터가 비어도 진행되도록 fallback 레시피도 가지고 있습니다.", "요리 결과 판정의 중심이라 GameManager 아이템 지급 이전 단계에서 매우 중요합니다."],
  [8, "상", "Managers", "OOTechSoundManager.cs", "Assets/Scripts/OO_MFC/Managers/OOTechSoundManager.cs", "직접", "BGM과 SFX AudioSource를 중앙에서 재생/정지하고 볼륨을 적용합니다.\n각 장면의 BGM 플레이어가 직접 AudioSource를 만지지 않도록 하는 공통 창구입니다.", "게임 진행 차단 가능성은 낮지만 전 장면 연출 품질에 넓게 영향을 줍니다."],
  [9, "상", "Managers", "OOTechItemCatalogManager.cs", "Assets/Scripts/OO_MFC/Managers/OOTechItemCatalogManager.cs", "직접", "씬/프리팹의 아이템 정의 오브젝트와 GameData를 연결해 아이템 이름과 아이콘 Sprite를 제공합니다.", "HUD 인벤토리와 요리 재료 표시가 이 매니저에 의존합니다."],
  [10, "상", "Components", "OOTechSceneContext.cs", "Assets/Scripts/OO_MFC/Components/OOTechSceneContext.cs", "직접", "씬 안의 역할 오브젝트를 캐싱하고 여러 컨트롤러가 공통으로 찾을 수 있게 돕는 문맥 컴포넌트입니다.", "스테이지 컨트롤러들이 배우/배경/포인트를 찾는 기반이라 중복 탐색을 줄입니다."],
  [11, "상", "Components", "OOTechSceneObject.cs", "Assets/Scripts/OO_MFC/Components/OOTechSceneObject.cs", "직접", "씬 오브젝트에 역할 ID를 붙여 GameData와 컨트롤러가 같은 대상을 찾게 하는 표식 컴포넌트입니다.", "역할 ID 기반 구조의 핵심 부품이라 씬 연결 안정성에 중요합니다."],
  [12, "상", "Characters", "OOTechJaeikController.cs", "Assets/Scripts/OO_MFC/Characters/OOTechJaeikController.cs", "직접", "재익 캐릭터의 이동, 상호작용 프롬프트, 먹기/변신/점프 애니메이션 등을 처리합니다.", "주요 캐릭터 조작과 상호작용이 엮여 있어 초반 진행과 연출에 영향이 큽니다."],
  [13, "상", "Stage", "OOTechStageActorMotion.cs", "Assets/Scripts/OO_MFC/Stage2/OOTechStageActorMotion.cs", "직접", "스테이지 배우의 이동, 물리 전환, 애니메이션 상태 재생, 렌더러 가시성 보정을 담당합니다.", "여러 스테이지 연출의 공통 배우 이동 도구처럼 쓰여 재사용 가치가 높습니다."],
  [14, "상", "UI", "OOTechRoadHUDView.cs", "Assets/Scripts/OO_MFC/UI/OOTechRoadHUDView.cs", "직접", "HUD에 배치된 버튼, 패널, 텍스트, 인벤토리 슬롯 루트 같은 UI 참조를 제공하는 View 스크립트입니다.", "RoadHUDController가 화면 요소를 안정적으로 찾는 데 필요한 UI 참조 묶음입니다."],
  [15, "상", "UI", "OOTechCookingGroupView.cs", "Assets/Scripts/OO_MFC/UI/OOTechCookingGroupView.cs", "직접", "요리 화면의 도구, 가이드, 버튼, 결과 출력 등 UI 참조를 정리해 CookingGroupController에 제공합니다.", "요리 컨트롤러의 거대한 UI 참조를 분리해주는 보조 구조라 유지보수 가치가 있습니다."],
  [16, "상", "WorldMap", "OOTechRoadToStage1Controller.cs", "Assets/Scripts/OO_MFC/WorldMap/OOTechRoadToStage1Controller.cs", "직접", "스테이지로 향하는 로드 구간의 이동, 대사, 미션, 보상, 다음 그룹 전환을 제어합니다.", "로드 진행과 스테이지 연결부를 담당해 플레이 흐름이 끊기지 않게 합니다."],
  [17, "상", "WorldMap", "OOTechStage1GroupController.cs", "Assets/Scripts/OO_MFC/WorldMap/OOTechStage1GroupController.cs", "직접", "Stage1의 입장 튜토리얼, 대사, 선택지, 보상, 클리어 UI, 다음 로드 이동을 관리합니다.", "첫 스테이지 진행과 보상 흐름을 잡는 대형 컨트롤러입니다."],
  [18, "상", "Stage2", "OOTechStage2GroupController.cs", "Assets/Scripts/OO_MFC/Stage2/OOTechStage2GroupController.cs", "직접", "Stage2의 캐릭터 배치, 대사 큐, 이동 연출, 클리어/다음 스테이지 전환을 담당합니다.", "Stage2 완료 조건과 다음 흐름을 직접 제어합니다."],
  [19, "상", "Stage3", "OOTechStage3EncounterController.cs", "Assets/Scripts/OO_MFC/Stage3/OOTechStage3EncounterController.cs", "직접", "Stage3 조우 장면의 선택, 사망/재시도, 요리 복귀, 클리어 버튼과 다음 단계 연결을 관리합니다.", "분기와 재시도 흐름이 많아 버그가 나면 플레이어가 막히기 쉽습니다."],
  [20, "상", "Stage4", "OOTechStage4GroupController.cs", "Assets/Scripts/OO_MFC/Stage4/OOTechStage4GroupController.cs", "직접", "Stage4 장면의 등장인물, 대사, 말풍선, BGM, 클리어 흐름을 제어합니다.", "후반 스테이지 진행과 엔딩 전 연결을 담당합니다."],
  [21, "상", "Final", "OOTechFinalStageController.cs", "Assets/Scripts/OO_MFC/Final/OOTechFinalStageController.cs", "직접", "최종 스테이지의 캐릭터 이동, 대사 큐, BGM, 엔딩 그룹 전환을 담당합니다.", "엔딩 직전 핵심 진행을 맡으므로 최종 완성도에 중요합니다."],
  [22, "상", "WorldMap", "OOTechWorldMapOverlayController.cs", "Assets/Scripts/OO_MFC/WorldMap/OOTechWorldMapOverlayController.cs", "직접", "월드맵 오버레이의 스테이지 표시, NEW 배지, 안내 닫기, 스테이지 선택 흐름을 관리합니다.", "진행 상태를 플레이어에게 보여주는 월드맵 기능의 중심입니다."],
  [23, "중상", "UI", "OOTechTutorialGuideUI.cs", "Assets/Scripts/OO_MFC/UI/OOTechTutorialGuideUI.cs", "직접", "튜토리얼 안내 패널, 제목/본문/다음 버튼, 스크롤 영역과 기본 UI 생성 보정을 담당합니다.", "초보 플레이어 안내와 스테이지 진행 설명에 중요합니다."],
  [24, "중상", "UI", "OOTechCodexGroupController.cs", "Assets/Scripts/OO_MFC/UI/OOTechCodexGroupController.cs", "직접", "도감 목록, 페이지 이동, 항목 선택, 이미지/텍스트 표시를 관리합니다.", "게임 진행 필수는 아니지만 메뉴 품질과 데이터 표시 일관성에 중요합니다."],
  [25, "중상", "UI", "OOTechInventorySlotView.cs", "Assets/Scripts/OO_MFC/UI/OOTechInventorySlotView.cs", "직접", "인벤토리 슬롯의 아이콘, 수량, 이름, 선택 상태를 표시하는 슬롯 View입니다.", "아이템 획득과 요리 재료 확인의 기본 표시 단위입니다."],
  [26, "중상", "Cooking", "OOTechCookingInventoryBridge.cs", "Assets/Scripts/OO_MFC/Cooking/OOTechCookingInventoryBridge.cs", "직접", "요리 UI와 HUD 인벤토리 갱신, NEW 배지, 퀘스트 완료 알림을 연결합니다.", "요리 결과가 HUD에 반영되는 연결부라 체감 오류를 줄이는 데 중요합니다."],
  [27, "중상", "Cooking", "OOTechCookingToolDropTarget.cs", "Assets/Scripts/OO_MFC/Cooking/OOTechCookingToolDropTarget.cs", "직접", "가마솥/도마/절구 같은 조리 도구의 드롭 영역, 허용 재료, 표시 상태를 관리합니다.", "드래그 앤 드롭 요리 판정의 실제 충돌/허용 규칙을 담당합니다."],
  [28, "중상", "Cooking", "OOTechCookingIngredientDragItem.cs", "Assets/Scripts/OO_MFC/Cooking/OOTechCookingIngredientDragItem.cs", "직접", "요리 재료 슬롯의 드래그 시작/이동/종료와 드래그 고스트 표시를 처리합니다.", "요리 시스템의 핵심 조작감에 직접 연결됩니다."],
  [29, "중상", "Interactions", "OOTechNPCInteractionActor.cs", "Assets/Scripts/OO_MFC/Interactions/OOTechNPCInteractionActor.cs", "직접", "NPC 상호작용 가능 여부, 일회성 처리, 상호작용 요청 이벤트를 관리합니다.", "스테이지별 NPC 대화/퀘스트 발동의 공통 상호작용 입구입니다."],
  [30, "중상", "WorldMap", "OOTechStageViewController.cs", "Assets/Scripts/OO_MFC/WorldMap/OOTechStageViewController.cs", "직접", "월드맵 또는 스테이지 표시용 View를 설정하고 상태에 맞게 갱신합니다.", "월드맵 스테이지 정보를 사용자에게 보여주는 표시 계층입니다."],
  [31, "중상", "Stage3", "OOTechStage3GroupController.cs", "Assets/Scripts/OO_MFC/Stage3/OOTechStage3GroupController.cs", "직접", "Stage3 진입 장면의 배치, 대사, 카메라, 다음 조우 장면 연결을 관리합니다.", "Stage3 본진입 전에 필요한 연출과 연결을 담당합니다."],
  [32, "중상", "Stage2", "OOTechStage2DialogueCue.cs", "Assets/Scripts/OO_MFC/Stage2/OOTechStage2DialogueCue.cs", "직접", "Stage2에서 특정 대사 ID를 순서대로 DialogueUI에 전달하는 큐 역할을 합니다.", "Stage2 스토리 출력 순서를 단순화해주는 보조 컨트롤러입니다."],
  [33, "중상", "Stage3", "OOTechStage3DialogueCue.cs", "Assets/Scripts/OO_MFC/Stage3/OOTechStage3DialogueCue.cs", "직접", "Stage3/후반 장면에서 대사 ID 목록을 순차 재생하는 공통 큐로 쓰입니다.", "Stage3뿐 아니라 후반 스테이지에서도 재사용되어 중요도가 올라갑니다."],
  [34, "중", "Stage2", "OOTechStage2CameraCue.cs", "Assets/Scripts/OO_MFC/Stage2/OOTechStage2CameraCue.cs", "직접", "Stage2 계열 장면의 카메라 이동과 줌 같은 연출 큐를 처리합니다.", "장면 연출 품질에 중요하지만 데이터/진행 허브보다는 영향 범위가 좁습니다."],
  [35, "중", "Stage2", "OOTechStage2ClearCue.cs", "Assets/Scripts/OO_MFC/Stage2/OOTechStage2ClearCue.cs", "직접", "Stage2 클리어 시점의 보상, UI, 다음 단계 신호를 돕는 클리어 큐입니다.", "스테이지 완료 흐름을 분리하지만 단독 영향 범위는 Stage2 중심입니다."],
  [36, "중", "Stage2", "OOTechStage2RewardService.cs", "Assets/Scripts/OO_MFC/Stage2/OOTechStage2RewardService.cs", "직접", "Stage2 보상 아이템 지급과 관련된 처리를 담당하는 서비스성 스크립트입니다.", "보상 로직을 컨트롤러에서 분리해 GameManager 아이템 흐름을 보조합니다."],
  [37, "중", "Main", "OOTechTutorial1Controller.cs", "Assets/Scripts/OO_MFC/OOTechTutorial1Controller.cs", "직접", "Tutorial1의 상호작용 안내, E 버튼, 이동/연출, 공통 스킵 버튼을 관리합니다.", "초반 튜토리얼 체험을 담당하지만 이후 공통 시스템보다는 범위가 제한됩니다."],
  [38, "중", "Main", "OOTechSenario1Controller.cs", "Assets/Scripts/OO_MFC/OOTechSenario1Controller.cs", "직접", "Scenario1의 캐릭터 참조, 대사/튜토리얼/연출, 변신 효과, 스킵 버튼을 관리합니다.", "초반 스토리 연출의 큰 컨트롤러지만 특정 장면 중심입니다."],
  [39, "중", "BGM", "PrologueController.cs", "Assets/Scripts/OO_MFC/BGM/PrologueController.cs", "직접", "프롤로그 컷씬 표시, 내레이션 진행, 대화 그룹 열기, 스킵/클릭 버튼 처리를 담당합니다.", "초반 프롤로그 진행의 핵심이지만 전체 시스템 공통도는 낮습니다."],
  [40, "중", "WorldMap", "OOTechTutorial2Controller.cs", "Assets/Scripts/OO_MFC/WorldMap/OOTechTutorial2Controller.cs", "직접", "두 번째 튜토리얼 또는 로드 구간의 아이템 지급, 대사, 안내 데이터 조회를 처리합니다.", "초반 로드 흐름을 이어주지만 특정 구간 전용입니다."],
  [41, "중", "WorldMap", "OOTechStageMoranFreeMoveController.cs", "Assets/Scripts/OO_MFC/WorldMap/OOTechStageMoranFreeMoveController.cs", "직접", "월드맵/스테이지에서 모란 캐릭터의 자유 이동과 입력 관련 처리를 담당합니다.", "플레이어 이동감에 영향을 주지만 적용 구간은 제한됩니다."],
  [42, "중", "Final", "OOTechCutSceneNarrationController.cs", "Assets/Scripts/OO_MFC/Final/OOTechCutSceneNarrationController.cs", "직접", "최종부 컷씬 내레이션을 데이터와 대사 큐에 맞춰 순차 출력합니다.", "후반 연출 흐름에 중요하지만 최종 스테이지 컨트롤러보다 보조적입니다."],
  [43, "중", "Final", "OOTechEndingCreditController.cs", "Assets/Scripts/OO_MFC/Final/OOTechEndingCreditController.cs", "직접", "엔딩 크레딧 스크롤, 로고 연출, 메인 메뉴 복귀 버튼을 관리합니다.", "엔딩 UX에 중요하지만 본편 진행 로직 영향은 낮습니다."],
  [44, "중", "Stage4", "OOTechStage4ClearPanelView.cs", "Assets/Scripts/OO_MFC/Stage4/OOTechStage4ClearPanelView.cs", "직접", "Stage4 클리어 패널의 제목/본문/다음 버튼 이벤트를 표시합니다.", "후반 클리어 UX를 담당하는 View라 컨트롤러보다는 보조적입니다."],
  [45, "중", "Stage4", "OOTechSpeechBubbleView.cs", "Assets/Scripts/OO_MFC/Stage4/OOTechSpeechBubbleView.cs", "직접", "말풍선 텍스트 표시, 표시 시간, 반복/속삭임 같은 연출 상태를 담당합니다.", "Stage4 연출 표현에 중요하지만 특정 UI 표현에 한정됩니다."],
  [46, "중", "WorldMap", "OOTechStagePlaceholderController.cs", "Assets/Scripts/OO_MFC/WorldMap/OOTechStagePlaceholderController.cs", "직접", "스테이지 임시 화면/플레이스홀더의 다음 버튼과 그룹 전환을 처리합니다.", "아직 완성되지 않은 구간을 이어주는 보조 진행 장치입니다."],
  [47, "중", "UI", "OOTechWorldMapOverlayView.cs", "Assets/Scripts/OO_MFC/UI/OOTechWorldMapOverlayView.cs", "직접", "월드맵 오버레이의 닫기 버튼과 기본 UI 참조를 제공하는 View입니다.", "OverlayController의 참조 안정성을 돕는 화면 부품입니다."],
  [48, "중", "UI", "OOTechCodexEntrySlotView.cs", "Assets/Scripts/OO_MFC/UI/OOTechCodexEntrySlotView.cs", "직접", "도감 항목 슬롯의 제목, 설명, 썸네일, 선택 이벤트를 표시합니다.", "도감 목록 표시의 반복 단위라 UX에는 중요하지만 진행 영향은 낮습니다."],
  [49, "중", "UI", "NextButtonController.cs", "Assets/Scripts/OO_MFC/UI/NextButtonController.cs", "직접", "공통 다음 버튼 입력을 받아 DialogueUI나 대상 흐름에 전달합니다.", "버튼 입력 연결부지만 기능 범위는 좁습니다."],
  [50, "중", "UI", "BackButtonController.cs", "Assets/Scripts/OO_MFC/UI/BackButtonController.cs", "프리팹", "공통 뒤로가기 버튼에서 이전 UI 그룹으로 돌아가는 요청을 보냅니다.", "프리팹 기반 공통 버튼이라 여러 화면에서 재사용될 수 있습니다."],
  [51, "중", "UI", "OOTechGroupSkipButtonController.cs", "Assets/Scripts/OO_MFC/UI/OOTechGroupSkipButtonController.cs", "직접", "공통 스킵 버튼 프리팹을 생성하고 현재 그룹에서 다음 그룹으로 넘기는 처리를 돕습니다.", "테스트/시연과 빠른 진행 보조에 유용하지만 본 시스템 핵심은 아닙니다."],
  [52, "중", "UI", "OOTechDialogueLayout.cs", "Assets/Scripts/OO_MFC/UI/OOTechDialogueLayout.cs", "직접", "DialogueUI의 레이아웃 크기, 위치, 스크롤/버튼 배치를 보정합니다.", "대화 UI의 시각 품질과 가독성을 보조합니다."],
  [53, "중", "UI", "OOTechDialogueSpeakerNameBackdrop.cs", "Assets/Scripts/OO_MFC/UI/OOTechDialogueSpeakerNameBackdrop.cs", "직접", "화자 이름 배경 영역의 크기와 표시 상태를 조정합니다.", "대사 UI의 세부 가독성 보정용 스크립트입니다."],
  [54, "중", "Interactions", "OOTechWorldInteractionMarker.cs", "Assets/Scripts/OO_MFC/Interactions/OOTechWorldInteractionMarker.cs", "프리팹", "월드 오브젝트 위 상호작용 마커를 카메라 기준으로 표시하고 위치를 맞춥니다.", "플레이어에게 상호작용 대상을 알려주는 프리팹 표시 부품입니다."],
  [55, "중", "Items", "OOTechItemDefinitionObject.cs", "Assets/Scripts/OO_MFC/Items/OOTechItemDefinitionObject.cs", "직접", "아이템 오브젝트에 데이터 ID, 표시명, 아이콘 Sprite/경로를 붙이는 정의 컴포넌트입니다.", "ItemCatalogManager가 아이템 표시 정보를 찾는 데 사용합니다."],
  [56, "하", "Components", "OOTechVisibleSpriteGuard.cs", "Assets/Scripts/OO_MFC/Components/OOTechVisibleSpriteGuard.cs", "직접", "SpriteRenderer가 보이지 않는 상황을 감지하거나 기본 가시성을 보정하는 안전장치 성격의 컴포넌트입니다.", "시각적 사고를 줄이지만 게임 흐름 핵심 로직은 아닙니다."],
  [57, "하", "Managers", "OOTechBuildViewSyncController.cs", "Assets/Scripts/OO_MFC/Managers/OOTechBuildViewSyncController.cs", "직접", "빌드/게임뷰에서 보이는 상태를 맞추기 위한 동기화성 보조 컨트롤러입니다.", "완성도 보정에는 도움이 되지만 핵심 시스템 영향은 낮습니다."],
  [58, "하", "Characters", "JangYoungSimController.cs", "Assets/Scripts/OO_MFC/Characters/JangYoungSimController.cs", "직접", "장영심 캐릭터의 이동 잠금과 놀람 애니메이션 같은 캐릭터별 동작을 처리합니다.", "특정 캐릭터 연출 전용이라 범위가 제한됩니다."],
  [59, "하", "Camera", "CameraFollowController.cs", "Assets/Scripts/OO_MFC/CameraFollowController.cs", "직접", "대상 Transform을 따라가는 기본 카메라 팔로우 동작을 처리합니다.", "플레이 감각에는 영향이 있지만 비교적 단순한 보조 기능입니다."],
  [60, "하", "Main", "MainMenuController.cs", "Assets/Scripts/OO_MFC/MainMenuController.cs", "직접", "메인 메뉴 버튼, 개발자 스킵 패널, 시작/도감/종료 요청을 처리합니다.", "진입 화면에는 중요하지만 본편 시스템보다는 영향 범위가 좁습니다."],
  [61, "하", "Cooking", "OOTechCookingGuideCue.cs", "Assets/Scripts/OO_MFC/Cooking/OOTechCookingGuideCue.cs", "직접", "요리 가이드에 필요한 튜토리얼 데이터를 조회하고 안내 문구를 제공하는 보조 큐입니다.", "요리 안내 품질을 높이지만 핵심 판정은 CookingManager/GroupController에 있습니다."],
  [62, "하", "Cooking", "OOTechCookingResultPresenter.cs", "Assets/Scripts/OO_MFC/Cooking/OOTechCookingResultPresenter.cs", "직접", "요리 성공 후 대사/결과 안내를 보여주는 표현 담당 스크립트입니다.", "결과 표시 UX에는 중요하지만 요리 판정 자체는 담당하지 않습니다."],
  [63, "하", "WorldMap", "OOTechRoadToStage1BGMPlayer.cs", "Assets/Scripts/OO_MFC/WorldMap/OOTechRoadToStage1BGMPlayer.cs", "직접", "로드 투 스테이지 구간의 BGM 재생을 SoundManager에 요청합니다.", "사운드 연출 전용이라 진행 로직 영향은 낮습니다."],
  [64, "하", "BGM", "Senario1_BGMPlayer.cs", "Assets/Scripts/OO_MFC/BGM/Senario1_BGMPlayer.cs", "직접", "Scenario1에서 상황별 BGM 클립을 SoundManager로 재생/전환합니다.", "특정 장면 음향 전용입니다."],
  [65, "하", "BGM", "MainMenuBGMPlayer.cs", "Assets/Scripts/OO_MFC/BGM/MainMenuBGMPlayer.cs", "직접", "메인 메뉴 BGM을 시작/정지할 때 SoundManager에 요청합니다.", "메뉴 분위기에는 중요하지만 기능 영향은 낮습니다."],
  [66, "하", "BGM", "Prologue1_Tutorial1_BGMPlayer.cs", "Assets/Scripts/OO_MFC/BGM/Prologue1_Tutorial1_BGMPlayer.cs", "직접", "Prologue1/Tutorial1 구간의 BGM 재생과 정지를 담당합니다.", "초반 음향 보조 스크립트입니다."],
  [67, "하", "BGM", "Prologue2_BGMPlayer.cs", "Assets/Scripts/OO_MFC/BGM/Prologue2_BGMPlayer.cs", "직접", "Prologue2 구간의 BGM 재생과 정지를 담당합니다.", "특정 컷씬 음향 보조 스크립트입니다."],
];

const workbook = Workbook.create();
const sheet = workbook.worksheets.add("OO_MFC Scripts");
sheet.showGridLines = false;

const allValues = [headers, ...rows];
sheet.getRangeByIndexes(0, 0, allValues.length, headers.length).values = allValues;

sheet.freezePanes.freezeRows(1);

const used = sheet.getRangeByIndexes(0, 0, allValues.length, headers.length);
used.format = {
  font: { name: "맑은 고딕", size: 10, color: "#111827" },
  wrapText: true,
  verticalAlignment: "top",
};
used.format.borders = { preset: "all", style: "thin", color: "#D9E2EC" };

const header = sheet.getRangeByIndexes(0, 0, 1, headers.length);
header.format = {
  fill: "#17365D",
  font: { name: "맑은 고딕", size: 10, bold: true, color: "#FFFFFF" },
  wrapText: true,
  horizontalAlignment: "center",
  verticalAlignment: "middle",
};
header.format.rowHeightPx = 34;

const priorityRange = sheet.getRangeByIndexes(1, 1, rows.length, 1);
priorityRange.format = {
  horizontalAlignment: "center",
  verticalAlignment: "middle",
  font: { name: "맑은 고딕", size: 10, bold: true },
};

sheet.getRangeByIndexes(1, 0, rows.length, 1).format = {
  horizontalAlignment: "center",
  verticalAlignment: "middle",
};

const categoryRange = sheet.getRangeByIndexes(1, 2, rows.length, 1);
categoryRange.format = { horizontalAlignment: "center", verticalAlignment: "middle" };

const connectionRange = sheet.getRangeByIndexes(1, 5, rows.length, 1);
connectionRange.format = { horizontalAlignment: "center", verticalAlignment: "middle" };

sheet.getRangeByIndexes(0, 0, allValues.length, 1).format.columnWidthPx = 72;
sheet.getRangeByIndexes(0, 1, allValues.length, 1).format.columnWidthPx = 70;
sheet.getRangeByIndexes(0, 2, allValues.length, 1).format.columnWidthPx = 105;
sheet.getRangeByIndexes(0, 3, allValues.length, 1).format.columnWidthPx = 245;
sheet.getRangeByIndexes(0, 4, allValues.length, 1).format.columnWidthPx = 360;
sheet.getRangeByIndexes(0, 5, allValues.length, 1).format.columnWidthPx = 75;
sheet.getRangeByIndexes(0, 6, allValues.length, 1).format.columnWidthPx = 520;
sheet.getRangeByIndexes(0, 7, allValues.length, 1).format.columnWidthPx = 420;

sheet.getRangeByIndexes(1, 0, rows.length, headers.length).format.rowHeightPx = 58;

for (let rowIndex = 1; rowIndex <= rows.length; rowIndex++) {
  const level = rows[rowIndex - 1][1];
  const rowRange = sheet.getRangeByIndexes(rowIndex, 0, 1, headers.length);
  if (level === "최상") {
    rowRange.format.fill = "#EAF3F8";
  } else if (level === "상") {
    rowRange.format.fill = "#F2F8F0";
  } else if (level === "중상") {
    rowRange.format.fill = "#FFF8E6";
  } else if (level === "중") {
    rowRange.format.fill = "#F8F7FC";
  } else {
    rowRange.format.fill = "#FFFFFF";
  }
}

const table = sheet.tables.add(`A1:H${rows.length + 1}`, true, "OOMFCScriptSummaryTable");
table.style = "TableStyleMedium2";
table.showFilterButton = true;

const noteRow = rows.length + 3;
sheet.getRangeByIndexes(noteRow - 1, 0, 2, headers.length).merge(true);
sheet.getRangeByIndexes(noteRow - 1, 0, 1, headers.length).values = [[
  "기준: 이전 대화에서 정리한 OO_MFC Scene 커스텀 스크립트 67개 기준입니다. 정적 helper(UIManagerExtension 등)처럼 씬 MonoBehaviour로 잡히지 않은 파일은 이 표에서 제외했습니다.",
]];
sheet.getRangeByIndexes(noteRow, 0, 1, headers.length).values = [[
  "중요도는 게임 전체 진행 영향도, 재사용 범위, 오류 발생 시 플레이 차단 가능성을 기준으로 주관적으로 정렬했습니다.",
]];
sheet.getRangeByIndexes(noteRow - 1, 0, 2, headers.length).format = {
  fill: "#F3F4F6",
  font: { name: "맑은 고딕", size: 9, italic: true, color: "#374151" },
  wrapText: true,
  verticalAlignment: "top",
};
sheet.getRangeByIndexes(noteRow - 1, 0, 2, headers.length).format.borders = { preset: "outside", style: "thin", color: "#CBD5E1" };
sheet.getRangeByIndexes(noteRow - 1, 0, 2, headers.length).format.rowHeightPx = 38;

await fs.mkdir(outputDir, { recursive: true });

const inspection = await workbook.inspect({
  kind: "table",
  range: "OO_MFC Scripts!A1:H12",
  include: "values",
  tableMaxRows: 12,
  tableMaxCols: 8,
  maxChars: 4000,
});
console.log(inspection.ndjson);

const preview = await workbook.render({
  sheetName: "OO_MFC Scripts",
  range: "A1:H25",
  scale: 1,
  format: "png",
});
await fs.writeFile(path.join(outputDir, "preview.png"), new Uint8Array(await preview.arrayBuffer()));

const xlsx = await SpreadsheetFile.exportXlsx(workbook);
await xlsx.save(outputPath);

console.log(`saved:${outputPath}`);
