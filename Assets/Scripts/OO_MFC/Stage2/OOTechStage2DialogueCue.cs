// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechStage2DialogueCue.cs
// - 역할: Stage2Group에서 데이터 기반 다이얼로그를 열고 기다리는 대사 큐 담당입니다.
// - 영화 비유: 무대감독이 대사 원고를 직접 들고 읽지 않고, 대사 조감독에게 "이 대사 진행"만 요청합니다.
// - 유지보수 포인트: Controller는 대사 순서만 알고, 실제 DialogueGroup 탐색/출력/닫기는 이 컴포넌트가 맡습니다.
// =============================================================================
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Stage2의 다이얼로그 출력 책임을 분리한 컴포넌트입니다.
/// Game View에서는 기존 DialogueGroup을 재사용하고, OO_Dialogue.json 데이터 ID로 대사를 출력합니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechStage2DialogueCue : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private string _dialogueGroupName = "DialogueGroup";
    [SerializeField] private float _dialogueWaitTimeoutSeconds = 18f;

    private DialogueUI UI_Dialogue;
    private GameObject Object_DialogueGroup;

    /// <summary>
    /// 지정한 다이얼로그 ID를 열고, 플레이어가 이어가기를 누를 때까지 기다립니다.
    /// </summary>
    public IEnumerator RequestShowDialogueAndWait(string dialogueId)
    {
        UI_Dialogue = ResolveDialogueUI();

        if (UI_Dialogue == null)
            yield break;

        OO_Dialogue dialogueData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetDialogueData(dialogueId) : null;

        if (dialogueData == null)
        {
            Debug.LogWarning($"[OOTechStage2DialogueCue] Dialogue data missing: {dialogueId}");
            yield break;
        }

        bool isDone = false;
        UI_Dialogue.RequestRoadViewLayout();
        UI_Dialogue.ShowDialogue(dialogueData, delegate { isDone = true; });

        Debug.Log($"[OOTechStage2DialogueCue] Dialogue opened: {dialogueId}");

        float elapsedTime = 0f;

        while (!isDone && elapsedTime < _dialogueWaitTimeoutSeconds)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (!isDone)
            Debug.LogWarning($"[OOTechStage2DialogueCue] Dialogue timeout, cue will continue: {dialogueId}");

        RequestCloseDialogue();
    }

    /// <summary>
    /// 지정한 다이얼로그 ID를 열고, 플레이어 입력 또는 제한 시간이 끝나면 닫습니다.
    /// 탐관오리의 강제 중단 대사처럼 영화적 타이밍이 필요한 곳에 사용합니다.
    /// </summary>
    public IEnumerator RequestShowDialogueForSeconds(string dialogueId, float seconds)
    {
        UI_Dialogue = ResolveDialogueUI();

        if (UI_Dialogue == null)
            yield break;

        OO_Dialogue dialogueData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetDialogueData(dialogueId) : null;

        if (dialogueData == null)
        {
            Debug.LogWarning($"[OOTechStage2DialogueCue] Timed dialogue data missing: {dialogueId}");
            yield break;
        }

        bool isDone = false;
        UI_Dialogue.RequestRoadViewLayout();
        UI_Dialogue.ShowDialogue(dialogueData, delegate { isDone = true; });

        float elapsedTime = 0f;

        while (!isDone && elapsedTime < seconds)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        RequestCloseDialogue();
    }

    /// <summary>
    /// DialogueGroup을 닫습니다.
    /// </summary>
    public void RequestCloseDialogue()
    {
        if (UI_Dialogue != null)
            UI_Dialogue.CloseDialogue();

        if (OOTechUIManager.Inst != null)
            OOTechUIManager.Inst.CloseUI(_dialogueGroupName);
    }

    private DialogueUI ResolveDialogueUI()
    {
        GameObject dialogueObject = ResolveDialogueGroupObject();

        if (dialogueObject == null)
            return null;

        if (OOTechUIManager.Inst != null)
            OOTechUIManager.Inst.OpenUI(_dialogueGroupName);

        dialogueObject.SetActive(true);

        if (UI_Dialogue == null)
            UI_Dialogue = dialogueObject.GetComponentInChildren<DialogueUI>(true);

        if (UI_Dialogue != null)
            UI_Dialogue.gameObject.SetActive(true);

        return UI_Dialogue;
    }

    /// <summary>
    /// 대사 UI 부모 오브젝트를 찾습니다.
    /// 영화로 치면 DialoguePanel 배우가 서는 무대 자체를 다시 켜 두는 보험입니다.
    /// </summary>
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
