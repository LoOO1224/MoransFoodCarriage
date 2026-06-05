// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechCookingResultPresenter.cs
// - 역할: 요리 완성 후 대사 출력과 결과 소비 타이밍을 담당합니다.
// - 영화 비유: 음식이 완성된 뒤 배우들의 "잘 먹었습니다" 장면을 진행하는 후반 연출 담당입니다.
// - 유지보수 포인트: 요리 성공 판정은 CookingManager, 인벤토리 실제 증감은 GameManager, 대사 출력만 이 컴포넌트가 맡습니다.
// =============================================================================
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 요리 완성 후 DialogueGroup 출력 흐름을 담당합니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechCookingResultPresenter : MonoBehaviour
{
    [Header("Data Id")]
    [SerializeField] private string _moranCookingCompleteDialogueId = "character_Moran_05";
    [SerializeField] private string _cookingCompleteDialogueGroupId = "dialogue_group_cooking_vegetable_porridge_complete_01";
    [SerializeField] private string _dialogueGroupName = "DialogueGroup";
    [SerializeField] private string _roadMapTutorialResultItemId = "OO_VegetableSoup_1";

    private GameObject Group_Dialogue;
    private DialogueUI UI_Dialogue;
    private bool _isCookingCompleteDialoguePlaying;

    /// <summary>
    /// 요리 완성 결과가 첫 튜토리얼 음식이면 대사 시퀀스를 시작합니다.
    /// </summary>
    public bool RequestPlayCookingCompleteDialogue(string resultItemId, Action<string> onConsumeResult, Action onComplete)
    {
        if (!RequestIsRoadMapCookingTutorialResult(resultItemId))
            return false;

        if (!gameObject.activeInHierarchy || _isCookingCompleteDialoguePlaying)
            return false;

        StartCoroutine(PlayCookingCompleteDialogueRoutine(resultItemId, onConsumeResult, onComplete));
        return true;
    }

    /// <summary>
    /// 열려 있는 DialogueGroup을 닫습니다.
    /// </summary>
    public void RequestCloseDialogueGroup()
    {
        if (UI_Dialogue != null)
            UI_Dialogue.CloseDialogue();

        if (OOTechUIManager.Inst != null && OOTechUIManager.Inst.CloseUI(_dialogueGroupName))
            return;

        if (Group_Dialogue != null)
            Group_Dialogue.SetActive(false);
    }

    private bool RequestIsRoadMapCookingTutorialResult(string resultItemId)
    {
        return resultItemId == _roadMapTutorialResultItemId;
    }

    private IEnumerator PlayCookingCompleteDialogueRoutine(string resultItemId, Action<string> onConsumeResult, Action onComplete)
    {
        _isCookingCompleteDialoguePlaying = true;

        if (!TryOpenDialogueGroup())
        {
            onConsumeResult?.Invoke(resultItemId);
            onComplete?.Invoke();
            _isCookingCompleteDialoguePlaying = false;
            yield break;
        }

        OO_Dialogue dialogueData = CreateCookingCompleteDialogueData();

        if (dialogueData == null || UI_Dialogue == null)
        {
            RequestCloseDialogueGroup();
            onConsumeResult?.Invoke(resultItemId);
            onComplete?.Invoke();
            _isCookingCompleteDialoguePlaying = false;
            yield break;
        }

        OO_Dialogue moranDialogueData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetDialogueData(_moranCookingCompleteDialogueId) : null;

        if (moranDialogueData != null)
            yield return ShowDialogueDataAndWait(moranDialogueData);

        yield return ShowDialogueDataAndWait(dialogueData);
        RequestCloseDialogueGroup();
        onConsumeResult?.Invoke(resultItemId);
        onComplete?.Invoke();
        _isCookingCompleteDialoguePlaying = false;
    }

    private IEnumerator ShowDialogueDataAndWait(OO_Dialogue dialogueData)
    {
        if (dialogueData == null || UI_Dialogue == null)
            yield break;

        bool isDone = false;
        UI_Dialogue.ShowDialogue(dialogueData, delegate
        {
            isDone = true;
        });

        yield return new WaitUntil(() => isDone);
    }

    private OO_Dialogue CreateCookingCompleteDialogueData()
    {
        OO_DialogueGroup dialogueGroupData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetDialogueGroupData(_cookingCompleteDialogueGroupId) : null;

        if (dialogueGroupData == null)
            return CreateDialogueData(_cookingCompleteDialogueGroupId, "춘양 + 재익군 + 모란", "잘 먹었습니다!");

        string speakerName = CreateDialogueGroupSpeakerName(dialogueGroupData);
        string text = CreateDialogueGroupText(dialogueGroupData);

        if (string.IsNullOrEmpty(text))
            text = "잘 먹었습니다!";

        if (string.IsNullOrEmpty(speakerName))
            speakerName = "춘양 + 재익군 + 모란";

        return CreateDialogueData(dialogueGroupData.Id, speakerName, text);
    }

    private string CreateDialogueGroupSpeakerName(OO_DialogueGroup dialogueGroupData)
    {
        List<string> speakerNameList = new List<string>();

        if (dialogueGroupData.SpeakerCharacterIdList != null)
        {
            foreach (string speakerCharacterId in dialogueGroupData.SpeakerCharacterIdList)
            {
                string speakerName = GetCharacterName(speakerCharacterId);

                if (!string.IsNullOrEmpty(speakerName) && !speakerNameList.Contains(speakerName))
                    speakerNameList.Add(speakerName);
            }
        }

        if (speakerNameList.Count == 0 && dialogueGroupData.SpeakerNameList != null)
        {
            foreach (string speakerName in dialogueGroupData.SpeakerNameList)
            {
                if (!string.IsNullOrEmpty(speakerName) && !speakerNameList.Contains(speakerName))
                    speakerNameList.Add(speakerName);
            }
        }

        if (speakerNameList.Count == 0 && dialogueGroupData.DialogueIdList != null && OOTechGameDataManager.Inst != null)
        {
            foreach (string dialogueId in dialogueGroupData.DialogueIdList)
            {
                OO_Dialogue dialogueData = OOTechGameDataManager.Inst.GetDialogueData(dialogueId);

                if (dialogueData != null && !string.IsNullOrEmpty(dialogueData.SpeakerName) && !speakerNameList.Contains(dialogueData.SpeakerName))
                    speakerNameList.Add(dialogueData.SpeakerName);
            }
        }

        return string.Join(" + ", speakerNameList);
    }

    private string CreateDialogueGroupText(OO_DialogueGroup dialogueGroupData)
    {
        if (!string.IsNullOrEmpty(dialogueGroupData.Text))
            return dialogueGroupData.Text;

        if (dialogueGroupData.DialogueIdList == null || OOTechGameDataManager.Inst == null)
            return string.Empty;

        foreach (string dialogueId in dialogueGroupData.DialogueIdList)
        {
            OO_Dialogue dialogueData = OOTechGameDataManager.Inst.GetDialogueData(dialogueId);

            if (dialogueData != null && !string.IsNullOrEmpty(dialogueData.Text))
                return dialogueData.Text;
        }

        return string.Empty;
    }

    private string GetCharacterName(string characterId)
    {
        if (OOTechGameDataManager.Inst == null || string.IsNullOrEmpty(characterId))
            return string.Empty;

        OO_Character characterData = OOTechGameDataManager.Inst.GetCharacterData(characterId);
        return characterData != null ? characterData.Name : string.Empty;
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

    private GameObject FindSceneObjectByName(string objectName)
    {
        Scene scene = SceneManager.GetActiveScene();

        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            GameObject foundObject = FindChildByName(rootObject.transform, objectName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }

    private GameObject FindChildByName(Transform rootTransform, string childName)
    {
        if (rootTransform == null || string.IsNullOrEmpty(childName))
            return null;

        if (rootTransform.name == childName)
            return rootTransform.gameObject;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            GameObject foundObject = FindChildByName(rootTransform.GetChild(index), childName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }
}
