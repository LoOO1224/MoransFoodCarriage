// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OOTechCookingInventoryBridge.cs
// - 역할: CookingGroup의 조리 입력, 도구 판정, 인벤토리 연동을 나누어 담당합니다.
// - 유지보수: Stage3/Stage4 발표용 진행 보험이 섞여 있으므로 제거 전 실제 리허설 흐름을 반드시 확인합니다.
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

