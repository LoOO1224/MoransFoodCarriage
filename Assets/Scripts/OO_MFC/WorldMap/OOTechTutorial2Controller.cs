using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 1st_Road_to_Stage1의 HUD 소개와 초반 대화 큐시트를 담당합니다.
/// Road 감독은 이동만 맡고, 이 배우는 UI 가이드와 데이터 대사 순서만 맡습니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechTutorial2Controller : MonoBehaviour
{
    [Header("Run Rule")]
    [SerializeField] private string _firstRoadGroupName = "1st_Road_to_Stage1";

    [Header("UI Group")]
    [SerializeField] private string _dialogueGroupName = "DialogueGroup";
    [SerializeField] private string _tutorialGuideGroupName = "TutorialGuideGroup";

    [Header("Tutorial Data Id")]
    [SerializeField] private string[] _hudTutorialIdArray =
    {
        "narration_tutorial_04",
        "narration_tutorial_05",
        "narration_tutorial_06",
        "narration_tutorial_07",
        "narration_tutorial_08"
    };
    [SerializeField] private string _missionTutorialId = "narration_tutorial_09";

    [Header("Dialogue Data Id")]
    [SerializeField] private string _chunyangRiceDialogueId = "character_Chunyang_03";
    [SerializeField] private string _moranPumpkinDialogueId = "character_Moran_03";
    [SerializeField] private string _mrJaeikReadyDialogueId = "character_Mr.Jaeik_03";
    [SerializeField] private string _chunyangEastDialogueId = "character_Chunyang_04";
    [SerializeField] private string _roadMap1MrJaeikDialogueId = "character_Mr.Jaeik_04";
    [SerializeField] private string _roadMap1ChunyangDialogueId = "character_Chunyang_05";
    [SerializeField] private string _roadMap1MoranDialogueId = "character_Moran_04";
    [SerializeField] private string _roadMap1TutorialId = "narration_tutorial_10";

    [Header("Ingredient Data Id")]
    [SerializeField] private string _riceIngredientId = "Ing_Rice_01";
    [SerializeField] private string _vegetableIngredientId = "Ing_Pumpkin_01";
    [SerializeField] private int _riceIngredientCount = 12;
    [SerializeField] private int _vegetableIngredientCount = 12;

    private readonly OOTechRoadHUDButtonKind[] _hudButtonKindArray =
    {
        OOTechRoadHUDButtonKind.Inventory,
        OOTechRoadHUDButtonKind.Codex,
        OOTechRoadHUDButtonKind.Mission,
        OOTechRoadHUDButtonKind.Cooking,
        OOTechRoadHUDButtonKind.WorldMap
    };

    private readonly string[] _fallbackTitleArray =
    {
        "인벤토리",
        "도감",
        "임무",
        "요리하기",
        "월드맵"
    };

    private readonly string[] _fallbackDescriptionArray =
    {
        "새로 얻은 재료와 물건을 확인합니다.",
        "새로 알게 된 정보와 기록을 확인합니다.",
        "현재 해야 할 일을 확인합니다.",
        "재료를 사용해 음식을 만듭니다. 첫 번째 길을 지나면 열립니다.",
        "전체 이동 경로와 다음 목적지를 확인합니다."
    };

    private GameObject Group_Dialogue;
    private DialogueUI UI_Dialogue;
    private GameObject Group_TutorialGuide;
    private OOTechTutorialGuideUI UI_TutorialGuide;

    public bool IsTutorialRunning { get; private set; }

    /// <summary>
    /// 그룹이 꺼질 때 남은 대사창과 가이드창을 정리합니다.
    /// 이전 공연의 큐가 남아 다음 Road 재시작을 막지 않게 하는 안전장치입니다.
    /// </summary>
    private void OnDisable()
    {
        IsTutorialRunning = false;
        CloseDialogueGroup();
        CloseTutorialGuideGroup();
    }

    /// <summary>
    /// RoadMap1에 처음 도착했을 때 재익군, 춘양, 모란 대화와 요리 준비 안내를 재생합니다.
    /// </summary>
    public IEnumerator PlayRoadMap1ArrivalRoutine()
    {
        IsTutorialRunning = true;

        yield return ShowDialogueDataAndWait(_roadMap1MrJaeikDialogueId);
        yield return ShowDialogueDataAndWait(_roadMap1ChunyangDialogueId);
        yield return ShowDialogueDataAndWait(_roadMap1MoranDialogueId);
        CloseDialogueGroup();
        yield return OpenRoadMap1TutorialGuideAndWait();

        IsTutorialRunning = false;
    }

    /// <summary>
    /// 1st_Road_to_Stage1에 진입하자마자 HUD 소개, 초반 대사, 재료 지급, 임무 안내를 순서대로 진행합니다.
    /// </summary>
    public IEnumerator PlayOpeningTutorialRoutine(OOTechRoadHUDController hudController, string currentGroupName)
    {
        if (hudController == null || currentGroupName != _firstRoadGroupName)
        {
            if (hudController != null)
                hudController.SetCookingUnlocked(true);

            yield break;
        }

        IsTutorialRunning = true;
        hudController.SetCookingUnlocked(false);
        hudController.SetInventoryNewBadgeActive(false);
        hudController.SetCodexNewBadgeActive(false);
        hudController.SetMissionNewBadgeActive(false);

        yield return PlayHUDGuideRoutine(hudController);
        yield return PlayOpeningDialogueRoutine(hudController);
        yield return OpenMissionTutorialGuideAndWait();

        hudController.RequestSetCookingQuestActive();
        IsTutorialRunning = false;
    }

    /// <summary>
    /// RoadGroup이 꺼지거나 다시 시작될 때 진행 중인 튜토리얼 UI를 정리합니다.
    /// </summary>
    public void StopTutorial(OOTechRoadHUDController hudController)
    {
        IsTutorialRunning = false;

        if (hudController != null)
            hudController.CloseHUDGuide();

        CloseDialogueGroup();
        CloseTutorialGuideGroup();
    }

    /// <summary>
    /// 인벤토리, 도감, 임무, 요리하기, 월드맵 버튼을 왼쪽부터 차례로 포커싱합니다.
    /// </summary>
    private IEnumerator PlayHUDGuideRoutine(OOTechRoadHUDController hudController)
    {
        int guideCount = Mathf.Min(_hudButtonKindArray.Length, _hudTutorialIdArray.Length);

        for (int index = 0; index < guideCount; index++)
        {
            bool isDone = false;
            OO_Tutorial tutorialData = GetTutorialData(_hudTutorialIdArray[index]);
            string title = GetGuideTitle(index, tutorialData);
            string description = GetGuideDescription(index, tutorialData);

            hudController.ShowHUDGuideStep(_hudButtonKindArray[index], title, description, delegate
            {
                isDone = true;
            });

            yield return new WaitUntil(() => isDone);
        }
    }

    /// <summary>
    /// HUD 소개 뒤 캐릭터 대사를 보여주고 쌀/채소 재료를 인벤토리에 지급합니다.
    /// </summary>
    private IEnumerator PlayOpeningDialogueRoutine(OOTechRoadHUDController hudController)
    {
        yield return ShowDialogueDataAndWait(_chunyangRiceDialogueId);
        RequestGiveIngredient(hudController, _riceIngredientId, _riceIngredientCount);

        yield return ShowDialogueDataAndWait(_moranPumpkinDialogueId);
        RequestGiveIngredient(hudController, _vegetableIngredientId, _vegetableIngredientCount);

        yield return ShowDialogueDataAndWait(_mrJaeikReadyDialogueId);
        yield return ShowDialogueDataAndWait(_chunyangEastDialogueId);
        CloseDialogueGroup();
    }

    /// <summary>
    /// 대사 보상으로 들어온 재료를 모델에 추가하고 HUD에 NEW 배지를 띄웁니다.
    /// </summary>
    private void RequestGiveIngredient(OOTechRoadHUDController hudController, string ingredientId, int count)
    {
        if (OOTechGameManager.Inst == null || string.IsNullOrEmpty(ingredientId) || count <= 0)
            return;

        OOTechGameManager.Inst.AddItem(ingredientId, count);

        if (hudController != null)
        {
            hudController.RequestRefreshInventoryView();
            hudController.SetInventoryNewBadgeActive(true);
        }
    }

    /// <summary>
    /// DialogueGroup을 열고 데이터 ID에 해당하는 대사를 한 줄 재생합니다.
    /// </summary>
    private IEnumerator ShowDialogueDataAndWait(string dialogueId)
    {
        if (TryOpenDialogueGroup() == false || UI_Dialogue == null)
            yield break;

        bool isDone = false;
        OO_Dialogue dialogueData = GetDialogueData(dialogueId);
        UI_Dialogue.ShowDialogue(dialogueData, delegate
        {
            isDone = true;
        });

        yield return new WaitUntil(() => isDone);
    }

    /// <summary>
    /// 임무 안내 TutorialGuideGroup을 열고 확인 입력까지 기다립니다.
    /// </summary>
    private IEnumerator OpenMissionTutorialGuideAndWait()
    {
        if (TryOpenTutorialGuideGroup() == false || UI_TutorialGuide == null)
            yield break;

        bool isDone = false;
        OO_Tutorial tutorialData = GetTutorialData(_missionTutorialId);

        if (tutorialData == null)
            tutorialData = CreateFallbackMissionTutorialData();

        UI_TutorialGuide.ShowGuide(tutorialData, delegate
        {
            isDone = true;
        });

        yield return new WaitUntil(() => isDone);
    }

    /// <summary>
    /// RoadMap1 도착 후 요리하기 버튼을 눌러보라는 안내를 재생합니다.
    /// </summary>
    private IEnumerator OpenRoadMap1TutorialGuideAndWait()
    {
        if (TryOpenTutorialGuideGroup() == false || UI_TutorialGuide == null)
            yield break;

        bool isDone = false;
        OO_Tutorial tutorialData = GetTutorialData(_roadMap1TutorialId);

        if (tutorialData == null)
            tutorialData = CreateFallbackRoadMap1TutorialData();

        UI_TutorialGuide.ShowGuide(tutorialData, delegate
        {
            isDone = true;
        });

        yield return new WaitUntil(() => isDone);
    }

    /// <summary>
    /// 씬에 놓인 DialogueGroup을 찾아 켜고 DialogueUI 컴포넌트를 확보합니다.
    /// </summary>
    private bool TryOpenDialogueGroup()
    {
        if (Group_Dialogue == null)
            Group_Dialogue = FindSceneObjectByName(_dialogueGroupName);

        if (Group_Dialogue == null)
            return false;

        Group_Dialogue.SetActive(true);

        if (UI_Dialogue == null)
            UI_Dialogue = Group_Dialogue.GetComponentInChildren<DialogueUI>(true);

        if (UI_Dialogue != null)
            return true;

        CloseDialogueGroup();
        return false;
    }

    /// <summary>
    /// 씬에 놓인 TutorialGuideGroup을 찾아 켜고 가이드 UI 컴포넌트를 확보합니다.
    /// </summary>
    private bool TryOpenTutorialGuideGroup()
    {
        if (Group_TutorialGuide == null)
            Group_TutorialGuide = FindSceneObjectByName(_tutorialGuideGroupName);

        if (Group_TutorialGuide == null)
            return false;

        Group_TutorialGuide.SetActive(true);

        if (UI_TutorialGuide == null)
            UI_TutorialGuide = Group_TutorialGuide.GetComponentInChildren<OOTechTutorialGuideUI>(true);

        if (UI_TutorialGuide != null)
            return true;

        CloseTutorialGuideGroup();
        return false;
    }

    /// <summary>
    /// 대사 무대를 닫아 다음 연출이나 플레이 입력을 가리지 않게 합니다.
    /// </summary>
    private void CloseDialogueGroup()
    {
        if (Group_Dialogue != null)
            Group_Dialogue.SetActive(false);
    }

    /// <summary>
    /// 튜토리얼 가이드 무대를 닫아 다음 연출이나 플레이 입력을 가리지 않게 합니다.
    /// </summary>
    private void CloseTutorialGuideGroup()
    {
        if (Group_TutorialGuide != null)
            Group_TutorialGuide.SetActive(false);
    }

    /// <summary>
    /// OO_Dialogue.json에서 대사 데이터를 가져오고, 없으면 임시 대사를 만들어 진행이 끊기지 않게 합니다.
    /// </summary>
    private OO_Dialogue GetDialogueData(string dialogueId)
    {
        if (OOTechGameDataManager.Inst != null)
            return OOTechGameDataManager.Inst.GetDialogueData(dialogueId) ?? CreateFallbackDialogueData(dialogueId);

        return CreateFallbackDialogueData(dialogueId);
    }

    /// <summary>
    /// OO_Tutorial.json에서 가이드 데이터를 가져옵니다.
    /// </summary>
    private OO_Tutorial GetTutorialData(string tutorialId)
    {
        if (OOTechGameDataManager.Inst != null)
            return OOTechGameDataManager.Inst.GetTutorialData(tutorialId);

        return null;
    }

    /// <summary>
    /// 데이터가 비어 있을 때도 각 HUD 버튼의 이름이 자연스럽게 보이도록 예비 제목을 제공합니다.
    /// </summary>
    private string GetGuideTitle(int index, OO_Tutorial tutorialData)
    {
        if (tutorialData != null && string.IsNullOrEmpty(tutorialData.Title) == false)
            return tutorialData.Title;

        if (index >= 0 && index < _fallbackTitleArray.Length)
            return _fallbackTitleArray[index];

        return "안내";
    }

    /// <summary>
    /// 데이터가 비어 있을 때도 튜토리얼이 멈추지 않도록 예비 설명을 제공합니다.
    /// </summary>
    private string GetGuideDescription(int index, OO_Tutorial tutorialData)
    {
        if (tutorialData != null && string.IsNullOrEmpty(tutorialData.Description) == false)
            return tutorialData.Description;

        if (index >= 0 && index < _fallbackDescriptionArray.Length)
            return _fallbackDescriptionArray[index];

        return string.Empty;
    }

    /// <summary>
    /// 대사 데이터 누락 시 플레이가 멈추지 않도록 최소 대사를 만듭니다.
    /// </summary>
    private OO_Dialogue CreateFallbackDialogueData(string dialogueId)
    {
        switch (dialogueId)
        {
            case "character_Chunyang_03":
                return CreateDialogueData(dialogueId, "춘양", "내가 가져온 쌀이네.");
            case "character_Moran_03":
                return CreateDialogueData(dialogueId, "모란", "채소도 챙겨 두었어요.");
            case "character_Mr.Jaeik_03":
                return CreateDialogueData(dialogueId, "재익군", "이제 길을 나서면 되겠군.");
            case "character_Chunyang_04":
                return CreateDialogueData(dialogueId, "춘양", "먼저는 동쪽으로 가시지요.");
            case "character_Mr.Jaeik_04":
                return CreateDialogueData(dialogueId, "재익군", "길이 이어지는군.");
            case "character_Chunyang_05":
                return CreateDialogueData(dialogueId, "춘양", "요리를 준비해야겠소.");
            case "character_Moran_04":
                return CreateDialogueData(dialogueId, "모란", "따뜻한 음식이 필요해요.");
            default:
                return CreateDialogueData(dialogueId, "나레이션", dialogueId);
        }
    }

    /// <summary>
    /// DialogueUI가 요구하는 최소 필드를 채워 임시 대사 데이터를 만듭니다.
    /// </summary>
    private OO_Dialogue CreateDialogueData(string dialogueId, string speakerName, string text)
    {
        OO_Dialogue dialogueData = new OO_Dialogue();
        dialogueData.Id = dialogueId;
        dialogueData.SpeakerName = speakerName;
        dialogueData.Text = text;
        return dialogueData;
    }

    /// <summary>
    /// 임무 안내 데이터가 없을 때 보여줄 예비 Tutorial 데이터를 만듭니다.
    /// </summary>
    private OO_Tutorial CreateFallbackMissionTutorialData()
    {
        OO_Tutorial tutorialData = new OO_Tutorial();
        tutorialData.Id = _missionTutorialId;
        tutorialData.Title = "임무";
        tutorialData.Description = "배고픈 모란과 동료들을 위해 요리하세요.";
        return tutorialData;
    }

    /// <summary>
    /// RoadMap1 도착 안내 데이터가 없을 때 보여줄 예비 Tutorial 데이터를 만듭니다.
    /// </summary>
    private OO_Tutorial CreateFallbackRoadMap1TutorialData()
    {
        OO_Tutorial tutorialData = new OO_Tutorial();
        tutorialData.Id = _roadMap1TutorialId;
        tutorialData.Title = "요리하기";
        tutorialData.Description = "요리하기 버튼을 눌러 부엌으로 이동하세요.";
        return tutorialData;
    }

    /// <summary>
    /// 비활성화된 씬 오브젝트까지 포함해 이름으로 무대 오브젝트를 찾습니다.
    /// </summary>
    private GameObject FindSceneObjectByName(string objectName)
    {
        Scene activeScene = SceneManager.GetActiveScene();
        GameObject[] rootObjectArray = activeScene.GetRootGameObjects();

        foreach (GameObject rootObject in rootObjectArray)
        {
            if (rootObject.name == objectName)
                return rootObject;

            GameObject childObject = FindChildByName(rootObject.transform, objectName);

            if (childObject != null)
                return childObject;
        }

        return null;
    }

    /// <summary>
    /// 자식 무대 안쪽까지 재귀적으로 내려가 이름이 같은 오브젝트를 찾습니다.
    /// </summary>
    private GameObject FindChildByName(Transform rootTransform, string objectName)
    {
        foreach (Transform childTransform in rootTransform)
        {
            if (childTransform.name == objectName)
                return childTransform.gameObject;

            GameObject resultObject = FindChildByName(childTransform, objectName);

            if (resultObject != null)
                return resultObject;
        }

        return null;
    }
}
