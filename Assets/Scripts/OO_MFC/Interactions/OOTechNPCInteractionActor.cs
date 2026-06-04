// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechNPCInteractionActor.cs
// - 역할: NPC 배우에게 붙이는 E 상호작용 역할표입니다.
// - 감독 관점: 이 컴포넌트는 "관객이 배우에게 가까이 왔는지"와 "E 큐가 들어왔는지"만 확인합니다.
// - 유지보수 포인트: 대사 UI, 퀘스트 갱신, 그룹 전환은 Controller/Manager가 맡고, NPC 배우는 신호만 보냅니다.
// =============================================================================
using System;
using UnityEngine;

/// <summary>
/// NPC 근처에서 E 상호작용 프롬프트를 보여주고, 입력이 들어오면 Controller에게 요청을 전달합니다.
/// Game View에서는 Moran이 촌장 근처에 오면 E 버튼 소품이 켜지고, E 입력 시 다이얼로그 큐가 시작됩니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechNPCInteractionActor : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Transform Transform_Player;
    [SerializeField] private GameObject Object_InteractionPrompt;

    [Header("Data")]
    [SerializeField] private string _dialogueDataId = "character_VillageChief_01";
    [SerializeField] private string _nextStageQuestDataId = "Stage1_Quest_02";

    [Header("Interaction")]
    [SerializeField] private KeyCode _interactionKey = KeyCode.E;
    [SerializeField] private float _interactionDistance = 2.4f;
    [SerializeField] private Vector3 _promptWorldOffset = new Vector3(0f, 1.45f, 0f);
    [SerializeField] private bool _isOneShot = true;

    private bool _isInteractable = true;
    private bool _isInteractionRequested;

    public event Action<OOTechNPCInteractionActor> InteractionRequested;

    public string DialogueDataId { get { return _dialogueDataId; } }
    public string NextStageQuestDataId { get { return _nextStageQuestDataId; } }

    /// <summary>
    /// Stage 감독이 플레이어와 E 버튼 소품을 연결해 줍니다.
    /// 씬에서 직접 연결해 둔 경우에도 같은 API로 다시 정리할 수 있습니다.
    /// </summary>
    public void RequestSetup(Transform playerTransform, GameObject interactionPrompt)
    {
        Transform_Player = playerTransform;
        Object_InteractionPrompt = interactionPrompt;
        SetPromptActive(false);
    }

    /// <summary>
    /// NPC마다 다른 대사 ID와 다음 퀘스트 ID를 지정합니다.
    /// 같은 배우 역할표를 촌장, 상인, 주민에게 재사용하기 위한 진입점입니다.
    /// </summary>
    public void RequestSetInteractionData(string dialogueDataId, string nextStageQuestDataId)
    {
        _dialogueDataId = dialogueDataId;
        _nextStageQuestDataId = nextStageQuestDataId;
    }

    /// <summary>
    /// E 상호작용 거리와 프롬프트 위치를 씬 배치에 맞게 조정합니다.
    /// </summary>
    public void RequestSetInteractionRule(float interactionDistance, Vector3 promptWorldOffset, KeyCode interactionKey)
    {
        _interactionDistance = Mathf.Max(0.1f, interactionDistance);
        _promptWorldOffset = promptWorldOffset;
        _interactionKey = interactionKey;
    }

    /// <summary>
    /// 튜토리얼이나 다이얼로그 중에는 NPC가 입력을 받지 않도록 잠급니다.
    /// </summary>
    public void RequestSetInteractable(bool isInteractable)
    {
        _isInteractable = isInteractable;

        if (!_isInteractable)
            SetPromptActive(false);
    }

    /// <summary>
    /// 한 번 말한 NPC를 다시 말하게 할 필요가 있을 때 Controller가 호출합니다.
    /// </summary>
    public void RequestResetInteraction()
    {
        _isInteractionRequested = false;
    }

    private void OnDisable()
    {
        SetPromptActive(false);
    }

    private void Update()
    {
        UpdateInteractionCue();
    }

    /// <summary>
    /// 매 프레임 배우와 플레이어의 거리, E 입력 큐를 확인합니다.
    /// </summary>
    private void UpdateInteractionCue()
    {
        bool isPromptVisible = _isInteractable && !_isInteractionRequested && IsPlayerNear();
        SetPromptActive(isPromptVisible);

        if (!isPromptVisible)
            return;

        PlacePrompt();

        if (Input.GetKeyDown(_interactionKey))
            RequestInteraction();
    }

    private bool IsPlayerNear()
    {
        if (Transform_Player == null)
            return false;

        float distance = Vector2.Distance(transform.position, Transform_Player.position);
        return distance <= _interactionDistance;
    }

    private void RequestInteraction()
    {
        if (_isOneShot)
            _isInteractionRequested = true;

        SetPromptActive(false);
        InteractionRequested?.Invoke(this);
    }

    private void PlacePrompt()
    {
        if (Object_InteractionPrompt == null)
            return;

        Object_InteractionPrompt.transform.position = transform.position + _promptWorldOffset;
    }

    private void SetPromptActive(bool isActive)
    {
        if (Object_InteractionPrompt == null)
            return;

        if (Object_InteractionPrompt.activeSelf == isActive)
            return;

        Object_InteractionPrompt.SetActive(isActive);
    }
}
