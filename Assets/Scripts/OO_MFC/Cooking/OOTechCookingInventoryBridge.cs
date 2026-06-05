// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechCookingInventoryBridge.cs
// - 역할: CookingGroup과 Road HUD 인벤토리/임무 UI 사이의 알림을 연결합니다.
// - 영화 비유: 부엌에서 새 음식이 완성되면 객석 안내판과 소품대장에게 동시에 알리는 연락 담당입니다.
// - 유지보수 포인트: 요리 판정은 CookingManager가 맡고, HUD 갱신 신호만 이 브릿지가 담당합니다.
// =============================================================================
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 부엌 안 NEW 배지와 외부 Road HUD 갱신을 담당합니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechCookingInventoryBridge : MonoBehaviour
{
    private TextMeshProUGUI Text_InventoryNewBadge;
    private float _newBadgeBlinkSpeed = 7f;
    private float _newBadgeMinimumAlpha = 0.25f;
    private Coroutine Coroutine_InventoryNewBadge;

    /// <summary>
    /// CookingGroup Controller가 씬에 배치된 NEW 텍스트와 깜빡임 값을 연결합니다.
    /// </summary>
    public void RequestSetup(TextMeshProUGUI inventoryNewBadgeText, float newBadgeBlinkSpeed, float newBadgeMinimumAlpha)
    {
        Text_InventoryNewBadge = inventoryNewBadgeText;
        _newBadgeBlinkSpeed = newBadgeBlinkSpeed;
        _newBadgeMinimumAlpha = newBadgeMinimumAlpha;
    }

    /// <summary>
    /// 부엌 내부 인벤토리 NEW 배지를 켜거나 끕니다.
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
    /// 열린 Road HUD들에게 새 아이템 표시와 인벤토리 새로고침을 요청합니다.
    /// </summary>
    public void RequestNotifyRoadHUDInventoryNewBadge()
    {
        foreach (OOTechRoadHUDController hudController in FindActiveRoadHUDControllers())
        {
            hudController.RequestRefreshInventoryView();
            hudController.SetInventoryNewBadgeActive(true);
        }
    }

    /// <summary>
    /// 열린 Road HUD들에게 인벤토리 목록만 다시 그리라고 요청합니다.
    /// </summary>
    public void RequestNotifyRoadHUDInventoryRefresh()
    {
        foreach (OOTechRoadHUDController hudController in FindActiveRoadHUDControllers())
            hudController.RequestRefreshInventoryView();
    }

    /// <summary>
    /// 첫 요리 임무가 끝났다고 Road HUD 임무판에 전달합니다.
    /// </summary>
    public void RequestNotifyRoadHUDCookingQuestComplete()
    {
        foreach (OOTechRoadHUDController hudController in FindActiveRoadHUDControllers())
            hudController.RequestCompleteCookingQuest();
    }

    /// <summary>
    /// CookingGroup이 닫힐 때 배지 깜빡임을 정리합니다.
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

    private OOTechRoadHUDController[] FindActiveRoadHUDControllers()
    {
        OOTechRoadHUDController[] hudControllerArray = FindObjectsByType<OOTechRoadHUDController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int activeCount = 0;

        for (int index = 0; index < hudControllerArray.Length; index++)
        {
            OOTechRoadHUDController hudController = hudControllerArray[index];

            if (hudController != null && hudController.gameObject.activeInHierarchy)
                activeCount++;
        }

        OOTechRoadHUDController[] activeHudArray = new OOTechRoadHUDController[activeCount];
        int activeIndex = 0;

        for (int index = 0; index < hudControllerArray.Length; index++)
        {
            OOTechRoadHUDController hudController = hudControllerArray[index];

            if (hudController == null || !hudController.gameObject.activeInHierarchy)
                continue;

            activeHudArray[activeIndex] = hudController;
            activeIndex++;
        }

        return activeHudArray;
    }
}
