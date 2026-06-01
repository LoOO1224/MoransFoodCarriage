using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Adds a strong white plate behind DialoguePanel/SpeakerNameText.
/// The speaker name is treated like an actor name card: black text on a white
/// prop so it stays readable over every scene background.
/// </summary>
[DisallowMultipleComponent]
public class OOTechDialogueSpeakerNameBackdrop : MonoBehaviour
{
    [SerializeField] private string _speakerNameObjectName = "SpeakerNameText";
    [SerializeField] private Color _backdropColor = new Color(1f, 1f, 1f, 0.94f);
    [SerializeField] private Color _speakerNameColor = Color.black;
    [SerializeField] private Vector2 _padding = new Vector2(32f, 18f);

    private RectTransform Rect_SpeakerName;
    private RectTransform Rect_Backdrop;
    private Image Image_Backdrop;
    private TextMeshProUGUI Text_SpeakerName;

    private void Awake()
    {
        PrepareBackdrop();
    }

    private void OnEnable()
    {
        PrepareBackdrop();
        ApplyBackdropLayout();
    }

    private void LateUpdate()
    {
        ApplyBackdropLayout();
    }

    private void PrepareBackdrop()
    {
        ApplyRequestedStyle();
        CacheSpeakerNameReference();

        if (Rect_SpeakerName == null)
            return;

        if (Rect_Backdrop != null)
            return;

        GameObject backdropObject = new GameObject("Image_SpeakerNameBackdrop", typeof(RectTransform));
        backdropObject.layer = Rect_SpeakerName.gameObject.layer;
        backdropObject.transform.SetParent(Rect_SpeakerName.parent, false);
        backdropObject.transform.SetSiblingIndex(Rect_SpeakerName.GetSiblingIndex());

        Rect_Backdrop = backdropObject.transform as RectTransform;
        Image_Backdrop = backdropObject.AddComponent<Image>();
        Image_Backdrop.color = _backdropColor;
        Image_Backdrop.raycastTarget = false;
    }

    private void CacheSpeakerNameReference()
    {
        if (Rect_SpeakerName != null)
            return;

        Transform speakerNameTransform = transform.Find(_speakerNameObjectName);

        if (speakerNameTransform == null)
            speakerNameTransform = GetComponentInChildren<RectTransform>(true);

        if (speakerNameTransform != null && speakerNameTransform.name == _speakerNameObjectName)
            Rect_SpeakerName = speakerNameTransform as RectTransform;

        if (Rect_SpeakerName != null)
            Text_SpeakerName = Rect_SpeakerName.GetComponent<TextMeshProUGUI>();
    }

    private void ApplyBackdropLayout()
    {
        if (Rect_SpeakerName == null || Rect_Backdrop == null)
            return;

        Rect_Backdrop.anchorMin = Rect_SpeakerName.anchorMin;
        Rect_Backdrop.anchorMax = Rect_SpeakerName.anchorMax;
        Rect_Backdrop.pivot = Rect_SpeakerName.pivot;
        Rect_Backdrop.anchoredPosition = Rect_SpeakerName.anchoredPosition;
        Rect_Backdrop.localScale = Rect_SpeakerName.localScale;
        Rect_Backdrop.sizeDelta = Rect_SpeakerName.sizeDelta + _padding;

        if (Image_Backdrop != null)
            Image_Backdrop.color = _backdropColor;

        if (Text_SpeakerName != null)
            Text_SpeakerName.color = _speakerNameColor;
    }

    private void ApplyRequestedStyle()
    {
        _backdropColor = new Color(1f, 1f, 1f, 0.94f);
        _speakerNameColor = Color.black;
    }
}
