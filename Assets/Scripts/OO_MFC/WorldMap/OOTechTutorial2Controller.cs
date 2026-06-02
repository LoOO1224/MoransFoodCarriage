using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 1st_Road_to_Stage1의 HUD 소개와 시작 대화 큐시트를 담당합니다.
/// 이동과 카메라는 Road 컨트롤러가 맡고, 이 컴포넌트는 UI 가이드/데이터 대화/재료 지급 순서만 맡습니다.
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
    [SerializeField] private int _riceIngredientCount = 2;
    [SerializeField] private int _vegetableIngredientCount = 2;

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
        "새로 알게 된 정보를 확인합니다.",
        "현재 해야 할 일을 확인합니다.",
        "요리 재료를 사용해 음식을 만듭니다. 첫 번째 길을 지나면 열립니다.",
        "전체 이동 경로와 다음 목적지를 확인합니다."
    };

    private GameObject Group_Dialogue;
    private DialogueUI UI_Dialogue;
    private GameObject Group_TutorialGuide;
    private OOTechTutorialGuideUI UI_TutorialGuide;

    public bool IsTutorialRunning { get; private set; }

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
    /// 1st_Road_to_Stage1에 진입하자마자 HUD 기능 소개와 시작 대화를 순서대로 보여줍니다.
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
    /// RoadGroup이 꺼지거나 재시작될 때 진행 중인 튜토리얼 UI를 정리합니다.
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
    /// 인벤토리, 도감, 임무, 요리하기, 월드맵 버튼을 왼쪽부터 차례로 화살표 포커싱합니다.
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
    /// HUD 소개 뒤에 캐릭터 대화를 보여주고 쌀/채소 재료를 인벤토리에 지급합니다.
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
    /// GameManager 인벤토리에 재료를 넣고 HUD NEW 배지를 켭니다.
    /// </summary>
    private void RequestGiveIngredient(OOTechRoadHUDController hudController, string ingredientId, int count)
    {
        if (OOTechGameManager.Inst != null)
            OOTechGameManager.Inst.AddItem(ingredientId, Mathf.Max(1, count));

        if (hudController == null)
            return;

        hudController.RequestRefreshInventoryView();
        hudController.SetInventoryNewBadgeActive(true);
    }

    /// <summary>
    /// Dialogue 데이터를 찾아 DialogueGroup에 표시하고, 플레이어가 넘길 때까지 기다립니다.
    /// </summary>
    private IEnumerator ShowDialogueDataAndWait(string dialogueId)
    {
        if (!TryOpenDialogueGroup())
            yield break;

        OO_Dialogue dialogueData = GetDialogueData(dialogueId);

        if (dialogueData == null)
            dialogueData = CreateFallbackDialogueData(dialogueId);

        if (dialogueData == null || UI_Dialogue == null)
            yield break;

        bool isDone = false;
        UI_Dialogue.ShowDialogue(dialogueData, delegate
        {
            isDone = true;
        });

        yield return new WaitUntil(() => isDone);
    }

    /// <summary>
    /// 시작 대화가 끝난 뒤 임무 갱신 튜토리얼 가이드를 보여줍니다.
    /// </summary>
    private IEnumerator OpenMissionTutorialGuideAndWait()
    {
        if (!TryOpenTutorialGuideGroup())
            yield break;

        bool isDone = false;
        OO_Tutorial tutorialData = GetTutorialData(_missionTutorialId);

        if (tutorialData == null)
            tutorialData = CreateFallbackMissionTutorialData();

        UI_TutorialGuide.SetTitleEmphasisActive(false);
        UI_TutorialGuide.ShowGuide(tutorialData, delegate
        {
            isDone = true;
        });

        yield return new WaitUntil(() => isDone);
        CloseTutorialGuideGroup();
    }

    /// <summary>
    /// RoadMap1 도착 후 요리하기 안내 튜토리얼 가이드를 보여줍니다.
    /// </summary>
    private IEnumerator OpenRoadMap1TutorialGuideAndWait()
    {
        if (!TryOpenTutorialGuideGroup())
            yield break;

        bool isDone = false;
        OO_Tutorial tutorialData = GetTutorialData(_roadMap1TutorialId);

        if (tutorialData == null)
            tutorialData = CreateFallbackRoadMap1TutorialData();

        UI_TutorialGuide.SetTitleEmphasisActive(false);
        UI_TutorialGuide.ShowGuide(tutorialData, delegate
        {
            isDone = true;
        });

        yield return new WaitUntil(() => isDone);
        CloseTutorialGuideGroup();
    }

    /// <summary>
    /// DialogueGroup을 열고 DialogueUI를 Road View 배치로 준비합니다.
    /// </summary>
    private bool TryOpenDialogueGroup()
    {
        if (Group_Dialogue == null)
            Group_Dialogue = FindSceneObjectByName(_dialogueGroupName);

        if (Group_Dialogue == null)
            return false;

        if (UI_Dialogue == null)
            UI_Dialogue = Group_Dialogue.GetComponentInChildren<DialogueUI>(true);

        if (UI_Dialogue == null)
            return false;

        UI_Dialogue.RequestRoadViewLayout();

        if (OOTechUIManager.Inst != null)
        {
            OOTechUIManager.Inst.RegisterUI(_dialogueGroupName, Group_Dialogue);

            if (OOTechUIManager.Inst.OpenUI(_dialogueGroupName))
            {
                UI_Dialogue.RequestRoadViewLayout();
                return true;
            }
        }

        Group_Dialogue.SetActive(true);
        UI_Dialogue.RequestRoadViewLayout();
        return true;
    }

    /// <summary>
    /// TutorialGuideGroup을 열어 일반 튜토리얼 패널을 사용할 수 있게 합니다.
    /// </summary>
    private bool TryOpenTutorialGuideGroup()
    {
        if (Group_TutorialGuide == null)
            Group_TutorialGuide = FindSceneObjectByName(_tutorialGuideGroupName);

        if (Group_TutorialGuide == null)
            return false;

        if (UI_TutorialGuide == null)
            UI_TutorialGuide = Group_TutorialGuide.GetComponentInChildren<OOTechTutorialGuideUI>(true);

        if (UI_TutorialGuide == null)
            return false;

        if (OOTechUIManager.Inst != null)
        {
            OOTechUIManager.Inst.RegisterUI(_tutorialGuideGroupName, Group_TutorialGuide);

            if (OOTechUIManager.Inst.OpenUI(_tutorialGuideGroupName))
                return true;
        }

        Group_TutorialGuide.SetActive(true);
        return true;
    }

    private void CloseDialogueGroup()
    {
        if (UI_Dialogue != null)
            UI_Dialogue.CloseDialogue();

        if (OOTechUIManager.Inst != null && OOTechUIManager.Inst.CloseUI(_dialogueGroupName))
            return;

        if (Group_Dialogue != null)
            Group_Dialogue.SetActive(false);
    }

    private void CloseTutorialGuideGroup()
    {
        if (UI_TutorialGuide != null)
        {
            UI_TutorialGuide.SetTitleEmphasisActive(false);
            UI_TutorialGuide.CloseGuide();
        }

        if (OOTechUIManager.Inst != null && OOTechUIManager.Inst.CloseUI(_tutorialGuideGroupName))
            return;

        if (Group_TutorialGuide != null)
            Group_TutorialGuide.SetActive(false);
    }

    private OO_Dialogue GetDialogueData(string dialogueId)
    {
        if (OOTechGameDataManager.Inst == null)
            return null;

        return OOTechGameDataManager.Inst.GetDialogueData(dialogueId);
    }

    private OO_Tutorial GetTutorialData(string tutorialId)
    {
        if (OOTechGameDataManager.Inst == null)
            return null;

        return OOTechGameDataManager.Inst.GetTutorialData(tutorialId);
    }

    private string GetGuideTitle(int index, OO_Tutorial tutorialData)
    {
        if (tutorialData != null && !string.IsNullOrEmpty(tutorialData.Name))
            return tutorialData.Name;

        if (index >= 0 && index < _fallbackTitleArray.Length)
            return _fallbackTitleArray[index];

        return "튜토리얼";
    }

    private string GetGuideDescription(int index, OO_Tutorial tutorialData)
    {
        if (tutorialData != null && !string.IsNullOrEmpty(tutorialData.Description))
            return tutorialData.Description;

        if (index >= 0 && index < _fallbackDescriptionArray.Length)
            return _fallbackDescriptionArray[index];

        return "이 기능은 나중에 데이터로 교체됩니다.";
    }

    /// <summary>
    /// 데이터가 아직 준비되지 않았을 때도 플레이 흐름이 끊기지 않도록 임시 대화를 만듭니다.
    /// </summary>
    private OO_Dialogue CreateFallbackDialogueData(string dialogueId)
    {
        if (dialogueId == _chunyangRiceDialogueId)
            return CreateDialogueData(dialogueId, "춘양", "모란님, 길을 나서기 전에 쌀을 챙기시지요.");

        if (dialogueId == _moranPumpkinDialogueId)
            return CreateDialogueData(dialogueId, "모란", "좋아요. 호박도 함께 챙겨 둘게요.");

        if (dialogueId == _mrJaeikReadyDialogueId)
            return CreateDialogueData(dialogueId, "재익군", "아씨, 저도 준비됐구멍요. 길만 열리면 힘껏 나아가겠멍요!");

        if (dialogueId == _chunyangEastDialogueId)
            return CreateDialogueData(dialogueId, "춘양", "모란님, 이 미련한 놈의 허물은 내 대신 사죄하겠소. 밥값을 하려면 부지런히 길을 나서야 할 터…… 먼저는 동쪽으로 가시지요. 그곳에 탐관오리의 수탈로 배 굶주린 자들이 유독 많다 들었습니다.");

        if (dialogueId == _roadMap1MrJaeikDialogueId)
            return CreateDialogueData(dialogueId, "재익군", "슬슬 배가 고프지 않습니까요? 모란 님의 따뜻한 요리가 있으면 힘이 나겠구먼유.");

        if (dialogueId == _roadMap1ChunyangDialogueId)
            return CreateDialogueData(dialogueId, "춘양", "마침 다들 지쳤을 터이니 따뜻한 식사를 부탁드려도 되겠소?");

        if (dialogueId == _roadMap1MoranDialogueId)
            return CreateDialogueData(dialogueId, "모란", "당연히 해드려야죠! 가마솥을 뜨겁게 달궈볼게요.");

        return null;
    }

    private OO_Dialogue CreateDialogueData(string dialogueId, string speakerName, string text)
    {
        return new OO_Dialogue
        {
            Id = dialogueId,
            SpeakerName = speakerName,
            Text = text
        };
    }

    /// <summary>
    /// 임무 갱신 데이터가 없을 때 쓰는 임시 튜토리얼 문구입니다.
    /// </summary>
    private OO_Tutorial CreateFallbackMissionTutorialData()
    {
        return new OO_Tutorial
        {
            Id = _missionTutorialId,
            Name = "임무 갱신",
            Title = "임무 갱신",
            Description = "임무가 갱신되었습니다. 임무 UI를 확인한 뒤 동쪽으로 이동하세요."
        };
    }

    /// <summary>
    /// RoadMap1 요리 준비 데이터가 없을 때 쓰는 임시 튜토리얼 문구입니다.
    /// </summary>
    private OO_Tutorial CreateFallbackRoadMap1TutorialData()
    {
        return new OO_Tutorial
        {
            Id = _roadMap1TutorialId,
            Name = "요리 준비",
            Title = "요리 준비",
            Description = "요리하기 버튼을 눌러 부엌으로 들어가세요. 가마솥에 알맞은 재료를 넣으면 음식이 완성됩니다."
        };
    }

    private GameObject FindSceneObjectByName(string objectName)
    {
        Scene scene = SceneManager.GetActiveScene();

        if (!scene.IsValid())
            return null;

        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            GameObject foundObject = FindChildByName(rootObject.transform, objectName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }

    private GameObject FindChildByName(Transform rootTransform, string objectName)
    {
        if (rootTransform == null)
            return null;

        if (rootTransform.name == objectName)
            return rootTransform.gameObject;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            GameObject foundObject = FindChildByName(rootTransform.GetChild(index), objectName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }
}
