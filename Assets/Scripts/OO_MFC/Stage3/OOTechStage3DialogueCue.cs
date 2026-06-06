// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechStage3DialogueCue.cs
// - 역할: Stage3/EncounterGroup에서 DialogueGroup과 ChoicePanel을 열고 결과를 기다리는 대사 큐 담당입니다.
// - 영화 비유: 무대감독이 대본을 직접 읽지 않고, 대사 조감독에게 "이 대사/선택지 진행"만 맡기는 구조입니다.
// - 유지보수 포인트: 장면 Controller는 데이터 ID만 넘기고, UI 탐색/열기/닫기는 이 컴포넌트 안에 둡니다.
// =============================================================================
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Stage3에서 데이터 기반 대사와 선택지를 출력합니다.
/// Game View에서는 기존 DialogueGroup을 재사용하므로 새 패널을 즉석 생성하지 않습니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechStage3DialogueCue : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private string _dialogueGroupName = "DialogueGroup";
    [SerializeField] private float _waitTimeoutSeconds = 30f;

    private DialogueUI UI_Dialogue;
    private GameObject Object_DialogueGroup;

    /// <summary>
    /// 지정한 Dialogue ID를 열고 플레이어가 넘길 때까지 기다립니다.
    /// </summary>
    public IEnumerator RequestShowDialogueAndWait(string dialogueId)
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
        dialogueUI.RequestRoadViewLayout();
        dialogueUI.ShowDialogue(dialogueData, delegate { isDone = true; });

        yield return WaitUntilDoneOrTimeout(isDoneFunc: () => isDone, $"Dialogue timeout: {dialogueId}");
        RequestCloseDialogue();
    }

    /// <summary>
    /// 지정한 Choice ID를 열고 예/아니오 선택 결과를 반환합니다.
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
        dialogueUI.RequestRoadViewLayout();
        dialogueUI.ShowChoice(choiceData, delegate (int index)
        {
            selectedIndex = index;
            isDone = true;
        });

        yield return WaitUntilDoneOrTimeout(isDoneFunc: () => isDone, $"Choice timeout: {choiceId}");
        RequestCloseDialogue();
        onChoiceSelected?.Invoke(selectedIndex);
    }

    /// <summary>
    /// 열려 있는 DialogueGroup을 닫습니다.
    /// </summary>
    public void RequestCloseDialogue()
    {
        if (UI_Dialogue != null)
            UI_Dialogue.CloseDialogue();

        if (OOTechUIManager.Inst != null)
            OOTechUIManager.Inst.CloseUI(_dialogueGroupName);
    }

    private IEnumerator WaitUntilDoneOrTimeout(Func<bool> isDoneFunc, string timeoutMessage)
    {
        float elapsedTime = 0f;

        while (!isDoneFunc() && elapsedTime < _waitTimeoutSeconds)
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

    private GameObject ResolveDialogueGroupObject()
    {
        if (Object_DialogueGroup != null)
            return Object_DialogueGroup;

        Object_DialogueGroup = OOTechUIManager.Inst != null ? OOTechUIManager.Inst.GetCreatedUI(_dialogueGroupName) : null;

        if (Object_DialogueGroup == null)
            Object_DialogueGroup = FindSceneObjectByName(_dialogueGroupName);

        return Object_DialogueGroup;
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

    private GameObject FindChildByName(Transform rootTransform, string objectName)
    {
        if (rootTransform == null || string.IsNullOrEmpty(objectName))
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
