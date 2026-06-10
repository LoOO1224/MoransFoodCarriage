// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OOTechCookingDragGhostPresenter.cs
// - 역할: CookingGroup의 조리 입력, 도구 판정, 인벤토리 연동을 나누어 담당합니다.
// - 유지보수: Stage3/Stage4 발표용 진행 보험이 섞여 있으므로 제거 전 실제 리허설 흐름을 반드시 확인합니다.
// =============================================================================
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OOTechCookingDragGhostPresenter
{
    /// <summary>
    /// 誘몃━ 留뚮뱾????DragGhost ?쒗뵆由우쓣 蹂듭젣???꾩씠肄섎쭔 蹂댁씠寃??ㅼ젙?⑸땲??
    /// </summary>
    public RectTransform RequestCreateDragGhost(RectTransform rootRect, RectTransform dragGhostTemplate, string itemDataId, Vector2 screenPosition, int itemQuantity, Vector2 iconSize)
    {
        if (rootRect == null || dragGhostTemplate == null)
            return null;

        GameObject ghostObject = Object.Instantiate(dragGhostTemplate.gameObject, rootRect, false);
        ghostObject.name = "Image_DragGhost";
        ghostObject.SetActive(true);

        RectTransform ghostRect = ghostObject.transform as RectTransform;
        Image iconImage = RequestFindImage(ghostObject.transform, "Image_ItemIcon");
        TextMeshProUGUI quantityText = null;

        foreach (TextMeshProUGUI ghostText in ghostObject.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (quantityText == null)
                quantityText = ghostText;

            ghostText.gameObject.SetActive(false);
        }

        foreach (Image ghostImage in ghostObject.GetComponentsInChildren<Image>(true))
        {
            if (ghostImage == iconImage)
                continue;

            ghostImage.enabled = false;
            ghostImage.raycastTarget = false;
        }

        if (iconImage != null)
            RequestSetupIconImage(iconImage, itemDataId);

        if (quantityText != null && itemQuantity > 1)
            RequestSetupQuantityText(quantityText, itemQuantity);

        if (ghostRect != null)
        {
            ghostRect.sizeDelta = iconSize;
            ghostRect.position = screenPosition;
            ghostRect.SetAsLastSibling();
        }

        CanvasGroup canvasGroup = ghostObject.GetComponent<CanvasGroup>();

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false;

        return ghostRect;
    }

    private void RequestSetupIconImage(Image iconImage, string itemDataId)
    {
        Sprite iconSprite = OOTechItemCatalogManager.RequestItemIconSprite(itemDataId);
        iconImage.gameObject.SetActive(true);
        iconImage.sprite = iconSprite;
        iconImage.enabled = iconSprite != null;
        iconImage.color = Color.white;
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;

        if (iconImage.transform is RectTransform iconRect)
        {
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
        }
    }

    private void RequestSetupQuantityText(TextMeshProUGUI quantityText, int itemQuantity)
    {
        quantityText.gameObject.SetActive(true);
        quantityText.text = $"x{itemQuantity}";
        quantityText.alignment = TextAlignmentOptions.BottomRight;
        quantityText.fontSize = 26f;
        quantityText.color = Color.white;
        quantityText.raycastTarget = false;
        OOTechTMPFontUtility.ApplyProjectFont(quantityText);
    }

    private Image RequestFindImage(Transform rootTransform, string childName)
    {
        Transform childTransform = RequestChildTransformByName(rootTransform, childName);
        return childTransform != null ? childTransform.GetComponent<Image>() : null;
    }

    private Transform RequestChildTransformByName(Transform rootTransform, string childName)
    {
        if (rootTransform == null)
            return null;

        if (rootTransform.name == childName)
            return rootTransform;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            Transform foundTransform = RequestChildTransformByName(rootTransform.GetChild(index), childName);

            if (foundTransform != null)
                return foundTransform;
        }

        return null;
    }
}

