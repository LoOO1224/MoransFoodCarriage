using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// CookingGroup 안에 배치된 부엌 UI 소품을 모아 두는 View 컴포넌트입니다.
/// 가마솥 드롭 영역, 말풍선 가이드, 드래그 잔상 템플릿을 씬 오브젝트로 유지합니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechCookingGroupView : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private RectTransform Rect_Root;

    [Header("Cauldron")]
    [SerializeField] private RectTransform Rect_CauldronDropArea;
    [SerializeField] private TextMeshProUGUI Text_PotContent;

    [Header("Guide")]
    [SerializeField] private GameObject Root_GuideBubble;
    [SerializeField] private TextMeshProUGUI Text_GuideTitle;
    [SerializeField] private TextMeshProUGUI Text_GuideBody;
    [SerializeField] private Button Button_GuideConfirm;
    [SerializeField] private GameObject Root_InventoryGuideArrow;
    [SerializeField] private GameObject Root_CauldronGuideArrow;

    [Header("Status")]
    [SerializeField] private TextMeshProUGUI Text_Status;

    [Header("Drag")]
    [SerializeField] private RectTransform Rect_DragGhostTemplate;

    public RectTransform RootRect => Rect_Root;
    public RectTransform CauldronDropAreaRect => Rect_CauldronDropArea;
    public TextMeshProUGUI PotContentText => Text_PotContent;
    public GameObject GuideBubble => Root_GuideBubble;
    public TextMeshProUGUI GuideTitleText => Text_GuideTitle;
    public TextMeshProUGUI GuideBodyText => Text_GuideBody;
    public Button GuideConfirmButton => Button_GuideConfirm;
    public GameObject InventoryGuideArrow => Root_InventoryGuideArrow;
    public GameObject CauldronGuideArrow => Root_CauldronGuideArrow;
    public TextMeshProUGUI StatusText => Text_Status;
    public RectTransform DragGhostTemplateRect => Rect_DragGhostTemplate;

    /// <summary>
    /// 인스펙터 참조가 비어 있을 때 자식 이름으로 부엌 UI 소품을 다시 연결합니다.
    /// </summary>
    public void ResolveReferences()
    {
        if (Rect_Root == null)
            Rect_Root = transform as RectTransform;

        Rect_CauldronDropArea = ResolveRect(Rect_CauldronDropArea, "Rect_CauldronDropArea");
        Text_PotContent = ResolveText(Text_PotContent, "Text_PotContent");
        Root_GuideBubble = ResolveGameObject(Root_GuideBubble, "Panel_CauldronGuide");
        Text_GuideTitle = ResolveText(Text_GuideTitle, "Text_GuideTitle");
        Text_GuideBody = ResolveText(Text_GuideBody, "Text_GuideBody");
        Button_GuideConfirm = ResolveButton(Button_GuideConfirm, "Button_GuideConfirm");
        Root_InventoryGuideArrow = ResolveGameObject(Root_InventoryGuideArrow, "Text_InventoryGuideArrow");
        Root_CauldronGuideArrow = ResolveGameObject(Root_CauldronGuideArrow, "Text_CauldronGuideArrow");
        Text_Status = ResolveText(Text_Status, "Text_Status");
        Rect_DragGhostTemplate = ResolveRect(Rect_DragGhostTemplate, "Slot_DragGhostTemplate");
    }

    private GameObject ResolveGameObject(GameObject currentObject, string objectName)
    {
        if (currentObject != null)
            return currentObject;

        Transform targetTransform = FindChildByName(transform, objectName);
        return targetTransform != null ? targetTransform.gameObject : null;
    }

    private RectTransform ResolveRect(RectTransform currentRect, string objectName)
    {
        if (currentRect != null)
            return currentRect;

        Transform targetTransform = FindChildByName(transform, objectName);
        return targetTransform as RectTransform;
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
