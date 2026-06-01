using System;
using UnityEngine;
using UnityEngine.UI;

public class DaniTechUIButton : MonoBehaviour
{
    [SerializeField] private Button Button_Base;
    [SerializeField] private Text Text_Base;
    [SerializeField] private Image Image_Base;
    [SerializeField] private Image Image_Select;

    // 수동으로 끊어주겠습니다.
    private bool _isSlotMenualUnbindEvent; 

    private void Awake()
    {
        // 1-2) 이 오브젝트가 생성될 때, 한번 컴포넌트를 찾아서 캐싱하자
        InitUIButton();
        SetDefaultUI();
    }

    private void OnDisable()
    {
        if (_isSlotMenualUnbindEvent == false)
        {
            Button_Base.onClick.RemoveAllListeners();
        }
    }


    private void SetDefaultUI()
    {
        if(Image_Select != null)
        {
            Image_Select.gameObject.SetActive(false);
        }
    }

    private void InitUIButton()
    {
        if(Button_Base != null)
        {
            return;
        }

        // 1-1) 외부에서도 등록할 수 있고,
            // 누군가 누락했다면 등록안해도 알아서 찾아주도록 로직을 넣어 놨다
        var button = this.gameObject.GetComponentInChildren<Button>();
        if(button != null)
        {
            this.Button_Base = button;
        }
    }

    public void BindOnClickButtonEvent(Action onClickCallback, bool isMenualUnbineEvent = false)
    {
        if(Button_Base == null) return;

        Button_Base.onClick.AddListener(onClickCallback.Invoke);
        _isSlotMenualUnbindEvent = isMenualUnbineEvent;
    }

    public void UnBindAllOnClickButtonEvent()
    {
        if (Button_Base == null) return;

        Button_Base.onClick.RemoveAllListeners();
    }

    public void ChangeButtonText(string buttonStr)
    {
        // 혹시 이버튼을 동적으로, 코드에서 텍스트를 수정해야할 때 사용
        if (Text_Base == null) return;

        Text_Base.text = buttonStr;
    }
}
