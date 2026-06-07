// =============================================================================
// OO_MFC ??븷 二쇱꽍
// - ?ㅽ겕由쏀듃: OOTechStage4ClearPanelView.cs
// - ??븷: Stage4 ?대━???⑤꼸???띿뒪?몄? ?섏뼱媛湲?踰꾪듉留?愿由ы빀?덈떎.
// - ?곹솕 鍮꾩쑀: 臾대? ?꾩뿉 誘몃━ ?щ젮??"?쇰궇???덈궡???낅땲?? 媛먮룆? ?덈궡?먯뿉 臾멸뎄? ?ㅼ쓬 ?먮쭔 ?꾨떖?⑸땲??
// - ?좎?蹂댁닔 ?ъ씤?? UI 諛곗튂? ?대?吏??Hierarchy??Canvas_Stage4Clear?먯꽌 吏곸젒 議곗젙?섍퀬,
//   Controller???고??꾩뿉 ??UI瑜?留뚮뱾吏 ?딆뒿?덈떎.
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


