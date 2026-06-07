// =============================================================================
// OO_MFC ??븷 二쇱꽍
// - ?ㅽ겕由쏀듃: OOTechCookingResultPresenter.cs
// - ??븷: ?붾━ ?꾩꽦 ?????異쒕젰怨?寃곌낵 ?뚮퉬 ??대컢???대떦?⑸땲??
// - ?곹솕 鍮꾩쑀: ?뚯떇???꾩꽦????諛곗슦?ㅼ쓽 "??癒뱀뿀?듬땲?? ?λ㈃??吏꾪뻾?섎뒗 ?꾨컲 ?곗텧 ?대떦?낅땲??
// - ?좎?蹂댁닔 ?ъ씤?? ?붾━ ?깃났 ?먯젙? CookingManager, ?몃깽?좊━ ?ㅼ젣 利앷컧? GameManager, ???異쒕젰留???而댄룷?뚰듃媛 留≪뒿?덈떎.
// =============================================================================
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ?붾━ ?꾩꽦 ??DialogueGroup 異쒕젰 ?먮쫫???대떦?⑸땲??
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
    /// ?붾━ ?꾩꽦 寃곌낵媛 泥??쒗넗由ъ뼹 ?뚯떇?대㈃ ????쒗?ㅻ? ?쒖옉?⑸땲??
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
    /// ?대젮 ?덈뒗 DialogueGroup???レ뒿?덈떎.
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
            return CreateDialogueData(_cookingCompleteDialogueGroupId, "異섏뼇 + ?ъ씡援?+ 紐⑤?", "??癒뱀뿀?듬땲??");

        string speakerName = CreateDialogueGroupSpeakerName(dialogueGroupData);
        string text = CreateDialogueGroupText(dialogueGroupData);

        if (string.IsNullOrEmpty(text))
            text = "??癒뱀뿀?듬땲??";

        if (string.IsNullOrEmpty(speakerName))
            speakerName = "異섏뼇 + ?ъ씡援?+ 紐⑤?";

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
            Group_Dialogue = RequestSceneObjectByName(_dialogueGroupName);

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

    private GameObject RequestSceneObjectByName(string objectName)
    {
        Scene scene = SceneManager.GetActiveScene();

        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            GameObject foundObject = RequestChildObjectByName(rootObject.transform, objectName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }

    private GameObject RequestChildObjectByName(Transform rootTransform, string childName)
    {
        if (rootTransform == null || string.IsNullOrEmpty(childName))
            return null;

        if (rootTransform.name == childName)
            return rootTransform.gameObject;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            GameObject foundObject = RequestChildObjectByName(rootTransform.GetChild(index), childName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }
}

