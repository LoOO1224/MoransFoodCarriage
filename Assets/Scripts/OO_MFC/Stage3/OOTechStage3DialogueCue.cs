// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OOTechStage3DialogueCue.cs
// - 역할: Stage3 산군/Encounter/Cooking 복귀 흐름을 처리합니다.
// - 유지보수: 인벤토리, 임무 UI, 대사 패널이 겹치는 구간이라 자동 닫힘과 복귀 타이밍을 조심합니다.
// =============================================================================
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Stage3?먯꽌 ?곗씠??湲곕컲 ??ъ? ?좏깮吏瑜?異쒕젰?⑸땲??
/// Game View?먯꽌??湲곗〈 DialogueGroup???ъ궗?⑺븯誘濡????⑤꼸??利됱꽍 ?앹꽦?섏? ?딆뒿?덈떎.
/// </summary>
[DisallowMultipleComponent]
public class OOTechStage3DialogueCue : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private string _dialogueGroupName = "DialogueGroup";
    [SerializeField] private float _waitTimeoutSeconds = 30f;

    private DialogueUI UI_Dialogue;
    private GameObject Object_DialogueGroup;
    private bool _isUseCutSceneBottomLayout;

    public void SetCutSceneBottomLayoutEnabled(bool isEnabled)
    {
        _isUseCutSceneBottomLayout = isEnabled;
    }

    /// <summary>
    /// 吏?뺥븳 Dialogue ID瑜??닿퀬 ?뚮젅?댁뼱媛 ?섍만 ?뚭퉴吏 湲곕떎由쎈땲??
    /// </summary>
    public IEnumerator RequestShowDialogueAndWait(string dialogueId)
    {
        yield return RequestShowDialogueAndWait(dialogueId, _waitTimeoutSeconds);
    }

    public IEnumerator RequestShowDialogueAndWait(string dialogueId, float waitTimeoutSeconds)
    {
        DialogueUI dialogueUI = ResolveDialogueUI();

        if (dialogueUI == null)
            yield break;

        OO_Dialogue dialogueData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetDialogueData(dialogueId) : null;

        if (dialogueData == null)
        {
            Debug.LogWarning($"[OOTechStage3DialogueCue] Dialogue data missing: {dialogueId}");
            yield break;
        }

        bool isDone = false;
        RequestApplyDialogueLayout(dialogueUI);
        dialogueUI.ShowDialogue(dialogueData, delegate { isDone = true; });

        yield return WaitUntilDoneOrTimeout(isDoneFunc: () => isDone, $"Dialogue timeout: {dialogueId}", waitTimeoutSeconds);
        RequestCloseDialogue();
    }

    /// <summary>
    /// 吏?뺥븳 Narration ID瑜?湲곗〈 DialogueGroup???꾩슦怨??뚮젅?댁뼱媛 ?섍만 ?뚭퉴吏 湲곕떎由쎈땲??
    /// ?곹솕濡?移섎㈃ ???ㅽ겕由곗쓣 留뚮뱾吏 ?딄퀬 媛숈? ?먮쭑?먯뿉 ?대젅?댁뀡 ?蹂몃쭔 ?쇱썙 ?ｋ뒗 諛⑹떇?낅땲??
    /// </summary>
    public IEnumerator RequestShowNarrationAndWait(string narrationId, string fallbackText = "")
    {
        DialogueUI dialogueUI = ResolveDialogueUI();

        if (dialogueUI == null)
            yield break;

        OO_Narration narrationData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetNarrationData(narrationId) : null;

        if (narrationData == null)
        {
            if (string.IsNullOrEmpty(fallbackText))
            {
                Debug.LogWarning($"[OOTechStage3DialogueCue] Narration data missing: {narrationId}");
                yield break;
            }

            narrationData = new OO_Narration
            {
                Id = narrationId,
                Title = "나레이션",
                NarrationTexts = new System.Collections.Generic.List<string> { fallbackText }
            };
        }

        bool isDone = false;
        RequestApplyDialogueLayout(dialogueUI);
        dialogueUI.ShowNarration(narrationData, delegate { isDone = true; });

        yield return WaitUntilDoneOrTimeout(isDoneFunc: () => isDone, $"Narration timeout: {narrationId}", _waitTimeoutSeconds);
        RequestCloseDialogue();
    }

    /// <summary>
    /// 吏?뺥븳 Choice ID瑜??닿퀬 ???꾨땲???좏깮 寃곌낵瑜?諛섑솚?⑸땲??
    /// </summary>
    public IEnumerator RequestShowChoiceAndWait(string choiceId, Action<int> onChoiceSelected)
    {
        DialogueUI dialogueUI = ResolveDialogueUI();

        if (dialogueUI == null)
            yield break;

        OO_Choice choiceData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetChoiceData(choiceId) : null;

        if (choiceData == null)
        {
            Debug.LogWarning($"[OOTechStage3DialogueCue] Choice data missing: {choiceId}");
            yield break;
        }

        bool isDone = false;
        int selectedIndex = -1;
        RequestApplyDialogueLayout(dialogueUI);
        dialogueUI.ShowChoice(choiceData, delegate (int index)
        {
            selectedIndex = index;
            isDone = true;
        });

        yield return WaitUntilDoneOrTimeout(isDoneFunc: () => isDone, $"Choice timeout: {choiceId}", _waitTimeoutSeconds);
        RequestCloseDialogue();
        onChoiceSelected?.Invoke(selectedIndex);
    }

    /// <summary>
    /// ?대젮 ?덈뒗 DialogueGroup???レ뒿?덈떎.
    /// </summary>
    public void RequestCloseDialogue()
    {
        if (UI_Dialogue != null)
            UI_Dialogue.CloseDialogue();

        if (OOTechUIManager.Inst != null)
            OOTechUIManager.Inst.CloseUI(_dialogueGroupName);
    }

    private IEnumerator WaitUntilDoneOrTimeout(Func<bool> isDoneFunc, string timeoutMessage, float waitTimeoutSeconds)
    {
        float elapsedTime = 0f;
        float safeTimeoutSeconds = Mathf.Max(0.01f, waitTimeoutSeconds);

        while (!isDoneFunc() && elapsedTime < safeTimeoutSeconds)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (!isDoneFunc())
            Debug.LogWarning($"[OOTechStage3DialogueCue] {timeoutMessage}");
    }

    private DialogueUI ResolveDialogueUI()
    {
        GameObject dialogueObject = ResolveDialogueGroupObject();

        if (dialogueObject == null)
            return null;

        if (OOTechUIManager.Inst != null)
        {
            OOTechUIManager.Inst.RegisterUI(_dialogueGroupName, dialogueObject);
            OOTechUIManager.Inst.OpenUI(_dialogueGroupName);
        }

        dialogueObject.SetActive(true);

        if (UI_Dialogue == null)
            UI_Dialogue = dialogueObject.GetComponentInChildren<DialogueUI>(true);

        if (UI_Dialogue != null)
            UI_Dialogue.gameObject.SetActive(true);

        return UI_Dialogue;
    }

    private void RequestApplyDialogueLayout(DialogueUI dialogueUI)
    {
        if (dialogueUI == null)
            return;

        if (_isUseCutSceneBottomLayout)
            dialogueUI.RequestCutSceneBottomLayout();
        else
            dialogueUI.RequestRoadViewLayout();
    }

    private GameObject ResolveDialogueGroupObject()
    {
        if (Object_DialogueGroup != null)
            return Object_DialogueGroup;

        Object_DialogueGroup = OOTechUIManager.Inst != null ? OOTechUIManager.Inst.GetCreatedUI(_dialogueGroupName) : null;

        if (Object_DialogueGroup == null)
            Object_DialogueGroup = RequestSceneObjectByName(_dialogueGroupName);

        return Object_DialogueGroup;
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

    private GameObject RequestChildObjectByName(Transform rootTransform, string objectName)
    {
        if (rootTransform == null || string.IsNullOrEmpty(objectName))
            return null;

        if (rootTransform.name == objectName)
            return rootTransform.gameObject;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            GameObject foundObject = RequestChildObjectByName(rootTransform.GetChild(index), objectName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }
}

