// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OOTechStage2DialogueCue.cs
// - 역할: Stage2 연출, 대사, 보상, 카메라 큐를 분리해 처리합니다.
// - 유지보수: 큐시트 데이터와 씬 배치 오브젝트가 함께 맞아야 하므로 데이터 ID와 역할 오브젝트를 같이 확인합니다.
// =============================================================================
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Stage2???ㅼ씠?쇰줈洹?異쒕젰 梨낆엫??遺꾨━??而댄룷?뚰듃?낅땲??
/// Game View?먯꽌??湲곗〈 DialogueGroup???ъ궗?⑺븯怨? OO_Dialogue.json ?곗씠??ID濡???щ? 異쒕젰?⑸땲??
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
    /// 吏?뺥븳 ?ㅼ씠?쇰줈洹?ID瑜??닿퀬, ?뚮젅?댁뼱媛 ?댁뼱媛湲곕? ?꾨? ?뚭퉴吏 湲곕떎由쎈땲??
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
    /// 吏?뺥븳 ?ㅼ씠?쇰줈洹?ID瑜??닿퀬, ?뚮젅?댁뼱 ?낅젰 ?먮뒗 ?쒗븳 ?쒓컙???앸굹硫??レ뒿?덈떎.
    /// ?먭??ㅻ━??媛뺤젣 以묐떒 ??ъ쿂???곹솕????대컢???꾩슂??怨녹뿉 ?ъ슜?⑸땲??
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
    /// DialogueGroup???レ뒿?덈떎.
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
    /// ???UI 遺紐??ㅻ툕?앺듃瑜?李얠뒿?덈떎.
    /// ?곹솕濡?移섎㈃ DialoguePanel 諛곗슦媛 ?쒕뒗 臾대? ?먯껜瑜??ㅼ떆 耳??먮뒗 蹂댄뿕?낅땲??
    /// </summary>
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

