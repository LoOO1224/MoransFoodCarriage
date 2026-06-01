using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Applies the shared 1920 x 1080 dialogue staging rule.
/// The speaker label sits above the dialogue frame, while the larger continue
/// button remains inside the lower-right corner of the frame.
/// </summary>
[DisallowMultipleComponent]
public class OOTechDialogueLayout : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private Vector2 _panelSizeDelta = new Vector2(-300f, 250f);
    [SerializeField] private Vector2 _panelAnchoredPosition = new Vector2(0f, 165f);
    [SerializeField] private Vector2 _speakerNameSize = new Vector2(470f, 68f);
    [SerializeField] private Vector2 _speakerNameAnchoredPosition = new Vector2(54f, 18f);
    [SerializeField] private Vector2 _nextButtonSize = new Vector2(270f, 78f);
    [SerializeField] private Vector2 _nextButtonAnchoredPosition = new Vector2(-46f, 32f);

    private RectTransform Rect_Panel;
    private RectTransform Rect_SpeakerName;
    private RectTransform Rect_NextButton;

    private void Awake()
    {
        // Dialogue UI layout is manually staged in the scene.
        // This component no longer moves or resizes the panel automatically.
    }

    private void OnEnable()
    {
        // Kept empty on purpose so reopening DialogueGroup does not rewrite user placement.
    }

    public void ApplyLayout()
    {
        // Manual layout mode: the user's RectTransform values are the source of truth.
    }

    private void CacheComponentReferences()
    {
        if (Rect_Panel == null)
            Rect_Panel = transform as RectTransform;

        if (Rect_SpeakerName == null)
        {
            Transform speakerName = transform.Find("SpeakerNameText");
            Rect_SpeakerName = speakerName as RectTransform;
        }

        if (Rect_NextButton == null)
        {
            Button nextButton = GetComponentInChildren<Button>(true);

            if (nextButton != null)
                Rect_NextButton = nextButton.transform as RectTransform;
        }
    }

    private void ApplyPanelLayout()
    {
        if (Rect_Panel == null)
            return;

        Rect_Panel.anchorMin = new Vector2(0f, 0f);
        Rect_Panel.anchorMax = new Vector2(1f, 0f);
        Rect_Panel.pivot = new Vector2(0.5f, 0.5f);
        Rect_Panel.anchoredPosition = _panelAnchoredPosition;
        Rect_Panel.sizeDelta = _panelSizeDelta;
        Rect_Panel.localScale = Vector3.one;
    }

    private void ApplySpeakerNameLayout()
    {
        if (Rect_SpeakerName == null)
            return;

        Rect_SpeakerName.anchorMin = new Vector2(0f, 1f);
        Rect_SpeakerName.anchorMax = new Vector2(0f, 1f);
        Rect_SpeakerName.pivot = new Vector2(0f, 0f);
        Rect_SpeakerName.anchoredPosition = _speakerNameAnchoredPosition;
        Rect_SpeakerName.sizeDelta = _speakerNameSize;
        Rect_SpeakerName.localScale = Vector3.one;
    }

    private void ApplyNextButtonLayout()
    {
        if (Rect_NextButton == null)
            return;

        Rect_NextButton.anchorMin = new Vector2(1f, 0f);
        Rect_NextButton.anchorMax = new Vector2(1f, 0f);
        Rect_NextButton.pivot = new Vector2(1f, 0f);
        Rect_NextButton.anchoredPosition = _nextButtonAnchoredPosition;
        Rect_NextButton.sizeDelta = _nextButtonSize;
        Rect_NextButton.localScale = Vector3.one;
    }
}
