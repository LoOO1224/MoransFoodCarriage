// =============================================================================
// OO_MFC ??븷 二쇱꽍
// - ?ㅽ겕由쏀듃: OOTechWorldMapOverlayView.cs
// - ??븷: UI ?섏씠?대씪?ㅼ쓽 踰꾪듉, ?대?吏, ?띿뒪??李몄“瑜?紐⑥븘 ??View 而댄룷?뚰듃?낅땲??
// - 媛먮룆 愿?? 臾대? ???뚰뭹 ?꾩튂?쒖엯?덈떎. ?먮떒?섏? ?딄퀬 ?뚰뭹??蹂댁뿬 二쇰뒗 ?쇰쭔 留≪뒿?덈떎.
// - ?좎?蹂댁닔 ?ъ씤?? 踰꾪듉 ?숈옉 ?먮떒, ?곗씠??濡쒕뵫, 洹몃９ ?꾪솚 濡쒖쭅? Controller??Manager???〓땲??
// =============================================================================
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// WorldMapGroup???쒕옒洹??덈궡 留먰뭾??UI瑜??ㅺ퀬 ?덈뒗 View 而댄룷?뚰듃?낅땲??
/// ?붾뱶留??먯껜??臾대? 諛곌꼍?닿퀬, ??View???뚮젅?댁뼱?먭쾶 議곗옉踰뺤쓣 ?뚮젮二쇰뒗 ?덈궡 ?뚰뭹?낅땲??
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
    /// ?붾뱶留??덈궡 留먰뭾?좉낵 ?リ린 踰꾪듉???먯떇 ?ㅻ툕?앺듃?먯꽌 ?곌껐?⑸땲??
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

        Transform targetTransform = RequestChildObjectByName(transform, objectName);
        return targetTransform != null ? targetTransform.gameObject : null;
    }

    private Button ResolveButton(Button currentButton, string objectName)
    {
        if (currentButton != null)
            return currentButton;

        Transform targetTransform = RequestChildObjectByName(transform, objectName);
        return targetTransform != null ? targetTransform.GetComponent<Button>() : null;
    }

    private TextMeshProUGUI ResolveText(TextMeshProUGUI currentText, string objectName)
    {
        if (currentText != null)
            return currentText;

        Transform targetTransform = RequestChildObjectByName(transform, objectName);
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

