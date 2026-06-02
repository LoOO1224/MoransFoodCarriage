using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// WorldMapGroup의 드래그 안내 말풍선 UI를 들고 있는 View 컴포넌트입니다.
/// 월드맵 자체는 무대 배경이고, 이 View는 플레이어에게 조작법을 알려주는 안내 소품입니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechWorldMapOverlayView : MonoBehaviour
{
    [Header("Guide")]
    [SerializeField] private GameObject Root_GuideCanvas;
    [SerializeField] private GameObject Root_GuideBubble;
    [SerializeField] private TextMeshProUGUI Text_GuideTitle;
    [SerializeField] private TextMeshProUGUI Text_GuideBody;
    [SerializeField] private Button Button_GuideClose;

    public GameObject GuideCanvas => Root_GuideCanvas;
    public GameObject GuideBubble => Root_GuideBubble;
    public TextMeshProUGUI GuideTitleText => Text_GuideTitle;
    public TextMeshProUGUI GuideBodyText => Text_GuideBody;
    public Button GuideCloseButton => Button_GuideClose;

    /// <summary>
    /// 월드맵 안내 말풍선과 닫기 버튼을 자식 오브젝트에서 연결합니다.
    /// </summary>
    public void ResolveReferences()
    {
        Root_GuideCanvas = Root_GuideCanvas != null ? Root_GuideCanvas : gameObject;
        Root_GuideBubble = ResolveGameObject(Root_GuideBubble, "Panel_WorldMapDragGuide");
        Text_GuideTitle = ResolveText(Text_GuideTitle, "Text_GuideTitle");
        Text_GuideBody = ResolveText(Text_GuideBody, "Text_GuideBody");
        Button_GuideClose = ResolveButton(Button_GuideClose, "Button_GuideClose");
    }

    private GameObject ResolveGameObject(GameObject currentObject, string objectName)
    {
        if (currentObject != null)
            return currentObject;

        Transform targetTransform = FindChildByName(transform, objectName);
        return targetTransform != null ? targetTransform.gameObject : null;
    }

    private Button ResolveButton(Button currentButton, string objectName)
    {
        if (currentButton != null)
            return currentButton;

        Transform targetTransform = FindChildByName(transform, objectName);
        return targetTransform != null ? targetTransform.GetComponent<Button>() : null;
    }

    private TextMeshProUGUI ResolveText(TextMeshProUGUI currentText, string objectName)
    {
        if (currentText != null)
            return currentText;

        Transform targetTransform = FindChildByName(transform, objectName);
        return targetTransform != null ? targetTransform.GetComponent<TextMeshProUGUI>() : null;
    }

    private Transform FindChildByName(Transform rootTransform, string objectName)
    {
        if (rootTransform == null)
            return null;

        if (rootTransform.name == objectName)
            return rootTransform;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            Transform foundTransform = FindChildByName(rootTransform.GetChild(index), objectName);

            if (foundTransform != null)
                return foundTransform;
        }

        return null;
    }
}
