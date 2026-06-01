using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class DaniTech_InteractionSlotUI : MonoBehaviour
{
    [SerializeField] private int SlotOffsetX;
    [SerializeField] private int SlotOffsetY;

    [SerializeField] private Text Text_InteractionTitle;
    [SerializeField] private Text Text_KeyName;
    [SerializeField] private DaniTechUIButton Button_OnClickInteraction;

    // 인터렉션 UI의 특징은 - 1개...
    private int _instanceId;

    // 참조형을 기록(캐싱)
    private Transform _targetTransform;
    private string _interactionCallbackMsg; // UI에서 눌러졌을때 응답해줄때 전달할 메세지 - 인터렉션의 ID가 될 수도 있고, Key가 될 수도 있고...자유

    private event Action<string> _onClickCallback;


    private void OnEnable()
    {
        //DaniTechCameraManager.Inst.BindMainCameraUpdatedEvent(OnCinemachineCameraUpdated);
        Button_OnClickInteraction.BindOnClickButtonEvent(OnClick_Interaction);
    }

    private void OnDisable()
    {
        //DaniTechCameraManager.Inst.UnBindMainCameraUpdatedEvent(OnCinemachineCameraUpdated);

        // 나중에 Unbind를 만들어서 수동으로 해제해줘도 무관!
        _onClickCallback = null;
    }

    public void OnClick_Interaction()
    {
        _onClickCallback?.Invoke(_interactionCallbackMsg);
    }

    public void InitSlot(int instanceId, string interactionKey, string interactionTitle, Transform targetTransform, Action<string> onClickCallback = null)
    {
        _instanceId = instanceId;
        _targetTransform = targetTransform;

        Text_KeyName.text = interactionKey;  // 키보드일수도 있고, 나중에 Sprite로 바꿔주셔도 무관
        Text_InteractionTitle.text = interactionTitle; // 이 키가 눌러질때 동작내용!

        _interactionCallbackMsg = interactionKey;
        _onClickCallback = onClickCallback;

        SlotOffsetX = 140;
        SlotOffsetY = -105;
    }

    private void OnCinemachineCameraUpdated(CinemachineBrain brain)
    {
        // 참조형을 캐싱할때는 꼭! 널체크를 사용부에서 신경써주자.
        if (_targetTransform != null)
        {
            // 위치를 동기화 하고 싶다면 이렇게 Transform을 대입하지 않도록 주의하자 - 애초에 컴파일 에러가 남
            // this.gameObject.transform = _targetTransform;
            // this.gameObject.transform.position = _targetTransform.position;


            // World- > 스크린 좌표로
            Vector2 screenPos = Camera.main.WorldToScreenPoint(_targetTransform.position);

            // UGUI에서 사용하려고
            var rectTransform = this.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Vector2 finalScreenPos = new Vector2(screenPos.x, screenPos.y);
                rectTransform.anchoredPosition = finalScreenPos;
            }
        }
    }

    private void Update()
    {
        // 참조형을 캐싱할때는 꼭! 널체크를 사용부에서 신경써주자.
        if (_targetTransform != null)
        {
            // 위치를 동기화 하고 싶다면 이렇게 Transform을 대입하지 않도록 주의하자 - 애초에 컴파일 에러가 남
            // this.gameObject.transform = _targetTransform;
            // this.gameObject.transform.position = _targetTransform.position;


            // World- > 스크린 좌표로
            Vector2 screenPos = Camera.main.WorldToScreenPoint(_targetTransform.position);

            // UGUI에서 사용하려고
            var rectTransform = this.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Vector2 finalScreenPos = new Vector2(screenPos.x + SlotOffsetX, screenPos.y - SlotOffsetY);
                rectTransform.anchoredPosition = finalScreenPos;
            }
        }
    }

}
