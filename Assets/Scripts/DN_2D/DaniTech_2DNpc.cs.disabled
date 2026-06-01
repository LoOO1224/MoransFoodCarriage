using UnityEngine;

public class DaniTech_2DNpc : MonoBehaviour
{
    [Header("애니메이터")]
    [SerializeField] private DaniTech_2DAnimatorController AnimatorController_Entity;

    [Header("NPC의 정보")]
    [SerializeField] private int _instanceId;
    [SerializeField] private string _npcDataId;

    private bool _isFirstCollied;

    public void StartInteract()
    {
        ChangeNpcState(DaniTech_EntityAnimState.InteractionStart);
    }

    public void ChangeNpcState(DaniTech_EntityAnimState newState)
    {
        // 우선 애니메이션만 바꿔 봅시다
        AnimatorController_Entity.SetState(newState);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 충돌이 났을때, Trigger형이고, 플레이어라면! 내가 어떤 범위 내에 있는 것을 감지했다!
            OnPlayerCollision();
        }
    }

    private void OnPlayerCollision()
    {
        // 임시로 나중에 게임오브젝트 매니저를 통해서 NPC가 만들어질때까지는~ 임시번호 부여
        _instanceId = 777;


        if(_isFirstCollied == false) // 이게 결국 퀘스트 어디까지 깼는가 저장이 되므로, 이 로직은 나중에 게임 퀘스트 진행도에 따라 변경 된다
        {
            _isFirstCollied = true;
            DaniTechUIManager.Instance.OpenDialogueUI("dialogue_mainstream_1_1_200");
        }

        DaniTechUIManager.Instance.AddInteractionSlot(_instanceId, "G", "상점열기", this.gameObject.transform, OnInteractionButtonClicked);

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            OnInteractionButtonClicked("G");
        }
    }

    private void OnInteractionButtonClicked(string interectionKey)
    {
        if(interectionKey == "G")
        {
            DaniTechUIManager.Instance.OpenInventoryPopup();
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            DaniTechUIManager.Instance.RemoveInteractionSlot(_instanceId);
        }
    }

}
