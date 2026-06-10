// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OOTechStage4ClearPanelView.cs
// - 역할: Stage4 토끼/거북이 마지막 임무와 발표용 진행 보장을 담당합니다.
// - 유지보수: Moran 표시, SpeechBubble, CookingGroup 왕복 보험은 엔딩 진행에 직접 연결되므로 임의 삭제하지 않습니다.
// =============================================================================
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class OOTechStage4ClearPanelView : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI Text_Title;
    [SerializeField] private TextMeshProUGUI Text_Body;

    [Header("Button")]
    [SerializeField] private Button Button_NextStage;

    /// <summary>
    /// ?⑤꼸???ъ뿉 耳쒖쭏 ???먯떇 UI瑜??대쫫?쇰줈 ??踰???蹂댁젙?⑸땲??
    /// Game View?먯꽌??誘몃━ 諛곗튂???덈궡?먯쓣 ?ъ궗?⑺븯誘濡? ?붾㈃ 諛곗튂瑜?媛쒕컻?먭? 吏곸젒 議곗젙?????덉뒿?덈떎.
    /// </summary>
    private void Awake()
    {
        ResolveReferences();
        RequestHide();
    }

    /// <summary>
    /// Stage4 ?대━??臾멸뎄瑜??쒖떆?섍퀬 ?섏뼱媛湲?踰꾪듉???ㅼ쓬 ?먮? ?곌껐?⑸땲??
    /// </summary>
    public void RequestShow(string title, string body, UnityAction onClickNext)
    {
        ResolveReferences();
        gameObject.SetActive(true);

        if (Text_Title != null)
        {
            Text_Title.text = title;
            Text_Title.color = new Color(1f, 0.92f, 0.2f, 1f);
            OOTechTMPFontUtility.ApplyProjectFont(Text_Title);
        }

        if (Text_Body != null)
        {
            Text_Body.text = body;
            OOTechTMPFontUtility.ApplyProjectFont(Text_Body);
        }

        if (Button_NextStage != null)
        {
            Button_NextStage.onClick.RemoveAllListeners();
            Button_NextStage.onClick.AddListener(onClickNext);
        }
    }

    /// <summary>
    /// ?대━???⑤꼸???レ뒿?덈떎. ?ъ엯?????댁쟾 踰꾪듉 ?먭? ?욎씠吏 ?딅룄濡?Listener??鍮꾩썎?덈떎.
    /// </summary>
    public void RequestHide()
    {
        if (Button_NextStage != null)
            Button_NextStage.onClick.RemoveAllListeners();

        gameObject.SetActive(false);
    }

    private void ResolveReferences()
    {
        if (Text_Title == null)
            Text_Title = RequestChildComponentByName<TextMeshProUGUI>("Text_Title");

        if (Text_Body == null)
            Text_Body = RequestChildComponentByName<TextMeshProUGUI>("Text_Body");

        if (Button_NextStage == null)
            Button_NextStage = RequestChildComponentByName<Button>("Button_NextStage");
    }

    private T RequestChildComponentByName<T>(string objectName) where T : Component
    {
        Transform childTransform = RequestChildObjectByName(transform, objectName);
        return childTransform != null ? childTransform.GetComponent<T>() : null;
    }

    private Transform RequestChildObjectByName(Transform rootTransform, string objectName)
    {
        if (rootTransform == null || string.IsNullOrWhiteSpace(objectName))
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


