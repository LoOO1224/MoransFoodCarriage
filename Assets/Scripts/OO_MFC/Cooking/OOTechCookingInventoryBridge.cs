// =============================================================================
// OO_MFC ??븷 二쇱꽍
// - ?ㅽ겕由쏀듃: OOTechCookingInventoryBridge.cs
// - ??븷: CookingGroup怨?Road HUD ?몃깽?좊━/?꾨Т UI ?ъ씠???뚮┝???곌껐?⑸땲??
// - ?곹솕 鍮꾩쑀: 遺?뚯뿉?????뚯떇???꾩꽦?섎㈃ 媛앹꽍 ?덈궡?먭낵 ?뚰뭹??μ뿉寃??숈떆???뚮━???곕씫 ?대떦?낅땲??
// - ?좎?蹂댁닔 ?ъ씤?? ?붾━ ?먯젙? CookingManager媛 留↔퀬, HUD 媛깆떊 ?좏샇留???釉뚮┸吏媛 ?대떦?⑸땲??
// =============================================================================
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 遺????NEW 諛곗?? ?몃? Road HUD 媛깆떊???대떦?⑸땲??
/// </summary>
[DisallowMultipleComponent]
public class OOTechCookingInventoryBridge : MonoBehaviour
{
    private TextMeshProUGUI Text_InventoryNewBadge;
    private float _newBadgeBlinkSpeed = 7f;
    private float _newBadgeMinimumAlpha = 0.25f;
    private Coroutine Coroutine_InventoryNewBadge;

    /// <summary>
    /// CookingGroup Controller媛 ?ъ뿉 諛곗튂??NEW ?띿뒪?몄? 源쒕묀??媛믪쓣 ?곌껐?⑸땲??
    /// </summary>
    public void RequestSetup(TextMeshProUGUI inventoryNewBadgeText, float newBadgeBlinkSpeed, float newBadgeMinimumAlpha)
    {
        Text_InventoryNewBadge = inventoryNewBadgeText;
        _newBadgeBlinkSpeed = newBadgeBlinkSpeed;
        _newBadgeMinimumAlpha = newBadgeMinimumAlpha;
    }

    /// <summary>
    /// 遺???대? ?몃깽?좊━ NEW 諛곗?瑜?耳쒓굅???뺣땲??
    /// </summary>
    public void RequestSetInventoryNewBadgeActive(bool isActive)
    {
        if (Text_InventoryNewBadge == null)
            return;

        RequestStopInventoryNewBadgeBlink();
        Text_InventoryNewBadge.gameObject.SetActive(isActive);

        Color badgeColor = Text_InventoryNewBadge.color;
        badgeColor.a = 1f;
        Text_InventoryNewBadge.color = badgeColor;

        if (isActive && gameObject.activeInHierarchy)
            Coroutine_InventoryNewBadge = StartCoroutine(PlayInventoryNewBadgeBlinkRoutine());
    }

    /// <summary>
    /// ?대┛ Road HUD?ㅼ뿉寃????꾩씠???쒖떆? ?몃깽?좊━ ?덈줈怨좎묠???붿껌?⑸땲??
    /// </summary>
    public void RequestNotifyRoadHUDInventoryNewBadge()
    {
        foreach (OOTechRoadHUDController hudController in RequestActiveRoadHUDControllers())
        {
            hudController.RequestRefreshInventoryView();
            hudController.SetInventoryNewBadgeActive(true);
        }
    }

    /// <summary>
    /// ?대┛ Road HUD?ㅼ뿉寃??몃깽?좊━ 紐⑸줉留??ㅼ떆 洹몃━?쇨퀬 ?붿껌?⑸땲??
    /// </summary>
    public void RequestNotifyRoadHUDInventoryRefresh()
    {
        foreach (OOTechRoadHUDController hudController in RequestActiveRoadHUDControllers())
            hudController.RequestRefreshInventoryView();
    }

    /// <summary>
    /// 泥??붾━ ?꾨Т媛 ?앸궗?ㅺ퀬 Road HUD ?꾨Т?먯뿉 ?꾨떖?⑸땲??
    /// </summary>
    public void RequestNotifyRoadHUDCookingQuestComplete()
    {
        foreach (OOTechRoadHUDController hudController in RequestActiveRoadHUDControllers())
            hudController.RequestCompleteCookingQuest();
    }

    /// <summary>
    /// CookingGroup???ロ옄 ??諛곗? 源쒕묀?꾩쓣 ?뺣━?⑸땲??
    /// </summary>
    public void RequestStopInventoryNewBadgeBlink()
    {
        if (Coroutine_InventoryNewBadge == null)
            return;

        StopCoroutine(Coroutine_InventoryNewBadge);
        Coroutine_InventoryNewBadge = null;
    }

    private IEnumerator PlayInventoryNewBadgeBlinkRoutine()
    {
        while (Text_InventoryNewBadge != null && Text_InventoryNewBadge.gameObject.activeSelf)
        {
            Color badgeColor = Text_InventoryNewBadge.color;
            float wave = (Mathf.Sin(Time.unscaledTime * _newBadgeBlinkSpeed) + 1f) * 0.5f;
            badgeColor.a = Mathf.Lerp(_newBadgeMinimumAlpha, 1f, wave);
            Text_InventoryNewBadge.color = badgeColor;
            yield return null;
        }
    }

    private List<OOTechRoadHUDController> RequestActiveRoadHUDControllers()
    {
        List<OOTechRoadHUDController> hudControllerArray = OOTechSceneQuery.RequestCollectComponents<OOTechRoadHUDController>(true);
        List<OOTechRoadHUDController> activeHudList = new List<OOTechRoadHUDController>();

        for (int index = 0; index < hudControllerArray.Count; index++)
        {
            OOTechRoadHUDController hudController = hudControllerArray[index];

            if (hudController == null || !hudController.gameObject.activeInHierarchy)
                continue;

            activeHudList.Add(hudController);
        }

        return activeHudList;
    }
}

