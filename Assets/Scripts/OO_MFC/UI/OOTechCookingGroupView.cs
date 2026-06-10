// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OOTechCookingGroupView.cs
// - 역할: UI 오브젝트 참조, 표시 갱신, 버튼 입력 연결을 담당합니다.
// - 유지보수: 씬 Hierarchy 이름으로 런타임 참조를 복구하는 코드가 많아 오브젝트 이름 변경에 주의합니다.
// =============================================================================
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// CookingGroup ?덉뿉 諛곗튂??遺??UI ?뚰뭹??紐⑥븘 ?먮뒗 View 而댄룷?뚰듃?낅땲??
/// 媛留덉넡 ?쒕∼ ?곸뿭, 留먰뭾??媛?대뱶, ?쒕옒洹??붿긽 ?쒗뵆由우쓣 ???ㅻ툕?앺듃濡??좎??⑸땲??
/// </summary>
[DisallowMultipleComponent]
public class OOTechCookingGroupView : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private RectTransform Rect_Root;

    [Header("Cauldron")]
    [SerializeField] private RectTransform Rect_CauldronDropArea;
    [SerializeField] private TextMeshProUGUI Text_PotContent;

    [Header("Cuttingboard")]
    [SerializeField] private RectTransform Rect_CuttingboardDropArea;

    [Header("Guide")]
    [SerializeField] private GameObject Root_GuideBubble;
    [SerializeField] private TextMeshProUGUI Text_GuideTitle;
    [SerializeField] private TextMeshProUGUI Text_GuideBody;
    [SerializeField] private Button Button_GuideConfirm;
    [SerializeField] private GameObject Root_CuttingboardGuideBubble;
    [SerializeField] private TextMeshProUGUI Text_CuttingboardGuideTitle;
    [SerializeField] private TextMeshProUGUI Text_CuttingboardGuideBody;
    [SerializeField] private Button Button_CuttingboardGuideConfirm;
    [SerializeField] private GameObject Root_InventoryGuideArrow;
    [SerializeField] private GameObject Root_CauldronGuideArrow;
    [SerializeField] private GameObject Root_CuttingboardGuideArrow;

    [Header("Status")]
    [SerializeField] private TextMeshProUGUI Text_Status;
    [SerializeField] private TextMeshProUGUI Text_InventoryNewBadge;

    [Header("Drag")]
    [SerializeField] private RectTransform Rect_DragGhostTemplate;

    public RectTransform RootRect => Rect_Root;
    public RectTransform CauldronDropAreaRect => Rect_CauldronDropArea;
    public TextMeshProUGUI PotContentText => Text_PotContent;
    public RectTransform CuttingboardDropAreaRect => Rect_CuttingboardDropArea;
    public GameObject GuideBubble => Root_GuideBubble;
    public TextMeshProUGUI GuideTitleText => Text_GuideTitle;
    public TextMeshProUGUI GuideBodyText => Text_GuideBody;
    public Button GuideConfirmButton => Button_GuideConfirm;
    public GameObject CuttingboardGuideBubble => Root_CuttingboardGuideBubble;
    public TextMeshProUGUI CuttingboardGuideTitleText => Text_CuttingboardGuideTitle;
    public TextMeshProUGUI CuttingboardGuideBodyText => Text_CuttingboardGuideBody;
    public Button CuttingboardGuideConfirmButton => Button_CuttingboardGuideConfirm;
    public GameObject InventoryGuideArrow => Root_InventoryGuideArrow;
    public GameObject CauldronGuideArrow => Root_CauldronGuideArrow;
    public GameObject CuttingboardGuideArrow => Root_CuttingboardGuideArrow;
    public TextMeshProUGUI StatusText => Text_Status;
    public TextMeshProUGUI InventoryNewBadgeText => Text_InventoryNewBadge;
    public RectTransform DragGhostTemplateRect => Rect_DragGhostTemplate;

    /// <summary>
    /// ?몄뒪?숉꽣 李몄“媛 鍮꾩뼱 ?덉쓣 ???먯떇 ?대쫫?쇰줈 遺??UI ?뚰뭹???ㅼ떆 ?곌껐?⑸땲??
    /// </summary>
    public void ResolveReferences()
    {
        if (Rect_Root == null)
            Rect_Root = transform as RectTransform;

        Rect_CauldronDropArea = ResolveRect(Rect_CauldronDropArea, "Rect_CauldronDropArea");
        Text_PotContent = ResolveText(Text_PotContent, "Text_PotContent");
        Rect_CuttingboardDropArea = ResolveRect(Rect_CuttingboardDropArea, "Rect_CuttingboardDropArea");
        Root_GuideBubble = ResolveGameObject(Root_GuideBubble, "Panel_CauldronGuide");
        Text_GuideTitle = ResolveTextInRoot(Text_GuideTitle, Root_GuideBubble, "Text_GuideTitle");
        Text_GuideBody = ResolveTextInRoot(Text_GuideBody, Root_GuideBubble, "Text_GuideBody");
        Button_GuideConfirm = ResolveButtonInRoot(Button_GuideConfirm, Root_GuideBubble, "Button_GuideConfirm");
        Root_CuttingboardGuideBubble = ResolveGameObject(Root_CuttingboardGuideBubble, "Panel_CuttingboardGuide");
        Text_CuttingboardGuideTitle = ResolveTextInRoot(Text_CuttingboardGuideTitle, Root_CuttingboardGuideBubble, "Text_GuideTitle");
        Text_CuttingboardGuideBody = ResolveTextInRoot(Text_CuttingboardGuideBody, Root_CuttingboardGuideBubble, "Text_GuideBody");
        Button_CuttingboardGuideConfirm = ResolveButtonInRoot(Button_CuttingboardGuideConfirm, Root_CuttingboardGuideBubble, "Button_GuideConfirm");
        Root_InventoryGuideArrow = ResolveGameObject(Root_InventoryGuideArrow, "Text_InventoryGuideArrow");
        Root_CauldronGuideArrow = ResolveGameObject(Root_CauldronGuideArrow, "Text_CauldronGuideArrow");
        Root_CuttingboardGuideArrow = ResolveGameObject(Root_CuttingboardGuideArrow, "Text_CuttingboardGuideArrow");
        Text_Status = ResolveText(Text_Status, "Text_Status");
        Text_InventoryNewBadge = ResolveText(Text_InventoryNewBadge, "NewBadge_Inventory");
        Rect_DragGhostTemplate = ResolveRect(Rect_DragGhostTemplate, "Slot_DragGhostTemplate");
    }

    private GameObject ResolveGameObject(GameObject currentObject, string objectName)
    {
        if (currentObject != null)
            return currentObject;

        Transform targetTransform = RequestChildObjectByName(transform, objectName);
        return targetTransform != null ? targetTransform.gameObject : null;
    }

    private RectTransform ResolveRect(RectTransform currentRect, string objectName)
    {
        if (currentRect != null)
            return currentRect;

        Transform targetTransform = RequestChildObjectByName(transform, objectName);
        return targetTransform as RectTransform;
    }

    private Button ResolveButton(Button currentButton, string objectName)
    {
        if (currentButton != null)
            return currentButton;

        Transform targetTransform = RequestChildObjectByName(transform, objectName);
        return targetTransform != null ? targetTransform.GetComponent<Button>() : null;
    }

    private Button ResolveButtonInRoot(Button currentButton, GameObject rootObject, string objectName)
    {
        if (currentButton != null)
            return currentButton;

        Transform targetTransform = rootObject != null ? RequestChildObjectByName(rootObject.transform, objectName) : null;
        return targetTransform != null ? targetTransform.GetComponent<Button>() : null;
    }

    private TextMeshProUGUI ResolveText(TextMeshProUGUI currentText, string objectName)
    {
        if (currentText != null)
            return currentText;

        Transform targetTransform = RequestChildObjectByName(transform, objectName);
        return targetTransform != null ? targetTransform.GetComponent<TextMeshProUGUI>() : null;
    }

    private TextMeshProUGUI ResolveTextInRoot(TextMeshProUGUI currentText, GameObject rootObject, string objectName)
    {
        if (currentText != null)
            return currentText;

        Transform targetTransform = rootObject != null ? RequestChildObjectByName(rootObject.transform, objectName) : null;
        return targetTransform != null ? targetTransform.GetComponent<TextMeshProUGUI>() : null;
    }

    private Transform RequestChildObjectByName(Transform rootTransform, string objectName)
    {
        if (rootTransform == null)
            return null;

        if (rootTransform.name == objectName)
            return rootTransform;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            Transform foundTransform = RequestChildObjectByName(rootTransform.GetChild(index), objectName);

            if (foundTransform != null)
                return foundTransform;
        }

        return null;
    }
}

