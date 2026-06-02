using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 슬롯 한 칸의 표시 텍스트를 담당합니다.
/// Game View에서는 "쌀 x2" 같은 슬롯 문구를 보여주는 작은 소품입니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechInventorySlotView : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI Text_Label;

    [Header("Icon")]
    [SerializeField] private Image Image_Icon;

    /// <summary>
    /// 슬롯 안의 TMP 텍스트를 찾아 저장합니다.
    /// </summary>
    public void ResolveReferences()
    {
        if (Text_Label == null)
            Text_Label = GetComponentInChildren<TextMeshProUGUI>(true);

        if (Image_Icon == null)
        {
            Transform iconTransform = FindChildByName(transform, "Image_ItemIcon");
            Image_Icon = iconTransform != null ? iconTransform.GetComponent<Image>() : null;
        }
    }

    /// <summary>
    /// 슬롯에 표시할 아이템 이름과 수량 문구를 적용합니다.
    /// </summary>
    public void RequestSetupText(string labelText)
    {
        ResolveReferences();

        if (Text_Label != null)
            Text_Label.text = labelText;
    }

    /// <summary>
    /// 슬롯에 아이템 아이콘과 이름/수량 문구를 함께 적용합니다.
    /// </summary>
    public void RequestSetupItem(string itemDataId, string labelText, Sprite iconSprite)
    {
        RequestSetupText(labelText);

        if (Image_Icon == null)
            return;

        Image_Icon.sprite = iconSprite;
        Image_Icon.enabled = iconSprite != null;
    }

    private Transform FindChildByName(Transform rootTransform, string childName)
    {
        if (rootTransform == null)
            return null;

        if (rootTransform.name == childName)
            return rootTransform;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            Transform foundTransform = FindChildByName(rootTransform.GetChild(index), childName);

            if (foundTransform != null)
                return foundTransform;
        }

        return null;
    }
}
