// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechSpeechBubbleView.cs
// - 역할: Rabbit 머리 위 말풍선 안의 텍스트를 OO_SpeechBubble 데이터로 타이핑 출력합니다.
// - 영화 비유: 배우 머리 위에 붙은 작은 자막 담당 스태프입니다. 감독은 대본 ID만 넘기고,
//   실제 글자 타이밍과 반복 표시는 이 컴포넌트가 처리합니다.
// - 유지보수 포인트: 말풍선 문장은 코드에 박지 말고 OO_SpeechBubble.xlsx/JSON에서 관리합니다.
// =============================================================================
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class OOTechSpeechBubbleView : MonoBehaviour
{
    [Header("View")]
    [SerializeField] private TextMeshProUGUI Text_Body;
    [SerializeField] private Canvas Canvas_Bubble;
    [SerializeField] private ScrollRect Scroll_Body;

    [Header("Typing")]
    [SerializeField] private float _baseCharacterInterval = 0.04f;

    private Coroutine Coroutine_Type;
    private bool _isLooping;

    /// <summary>
    /// 말풍선이 켜질 때 필요한 텍스트와 Canvas를 찾아 둡니다.
    /// Game View에서는 Rabbit이 움직여도 이 오브젝트가 자식으로 따라가며 텍스트만 보이게 됩니다.
    /// </summary>
    private void Awake()
    {
        ResolveReferences();
        PrepareTransparentScrollView();
    }

    /// <summary>
    /// 지정한 말풍선 ID를 데이터에서 읽어 타이핑합니다.
    /// 반복 말풍선이면 끝난 뒤 같은 문장을 다시 시작합니다.
    /// </summary>
    public void RequestPlaySpeechBubble(string speechBubbleId, float speedMultiplier = 1f)
    {
        ResolveReferences();

        OO_SpeechBubble speechBubbleData = OOTechGameDataManager.Inst != null
            ? OOTechGameDataManager.Inst.GetSpeechBubbleData(speechBubbleId)
            : null;

        string bodyText = speechBubbleData != null && !string.IsNullOrEmpty(speechBubbleData.Text)
            ? speechBubbleData.Text
            : speechBubbleId;
        bool isLoop = speechBubbleData != null && speechBubbleData.IsLoop;
        float dataSpeed = speechBubbleData != null && speechBubbleData.TypingSpeed > 0f
            ? speechBubbleData.TypingSpeed
            : 1f;

        RequestPlayText(bodyText, dataSpeed * Mathf.Max(0.01f, speedMultiplier), isLoop);
    }

    /// <summary>
    /// 외부에서 직접 문장을 넘길 때 사용하는 fallback API입니다.
    /// 데이터가 아직 없을 때도 리허설이 막히지 않게 합니다.
    /// </summary>
    public void RequestPlayText(string bodyText, float speedMultiplier = 1f, bool isLoop = false)
    {
        ResolveReferences();

        if (Text_Body == null)
            return;

        gameObject.SetActive(true);

        if (Coroutine_Type != null)
            StopCoroutine(Coroutine_Type);

        _isLooping = isLoop;
        Coroutine_Type = StartCoroutine(PlayTypingRoutine(bodyText, speedMultiplier));
    }

    /// <summary>
    /// 말풍선 표시를 닫고 현재 타이핑을 정지합니다.
    /// </summary>
    public void RequestHide()
    {
        if (Coroutine_Type != null)
        {
            StopCoroutine(Coroutine_Type);
            Coroutine_Type = null;
        }

        _isLooping = false;
        gameObject.SetActive(false);
    }

    private IEnumerator PlayTypingRoutine(string bodyText, float speedMultiplier)
    {
        string safeText = string.IsNullOrEmpty(bodyText) ? string.Empty : bodyText;
        float interval = _baseCharacterInterval / Mathf.Max(0.01f, speedMultiplier);

        do
        {
            Text_Body.text = string.Empty;

            for (int index = 0; index < safeText.Length; index++)
            {
                Text_Body.text += safeText[index];
                yield return new WaitForSeconds(interval);
            }

            if (_isLooping)
                yield return new WaitForSeconds(1.2f);
        }
        while (_isLooping);

        Coroutine_Type = null;
    }

    private void ResolveReferences()
    {
        if (Canvas_Bubble == null)
            Canvas_Bubble = GetComponentInChildren<Canvas>(true);

        if (Scroll_Body == null)
            Scroll_Body = GetComponentInChildren<ScrollRect>(true);

        if (Text_Body == null)
            Text_Body = GetComponentInChildren<TextMeshProUGUI>(true);

        OOTechTMPFontUtility.ApplyProjectFont(Text_Body);
    }

    /// <summary>
    /// ScrollView의 배경은 투명하게 두고, 텍스트만 말풍선 안에 보이게 정리합니다.
    /// </summary>
    private void PrepareTransparentScrollView()
    {
        if (Canvas_Bubble != null)
        {
            Canvas_Bubble.renderMode = RenderMode.WorldSpace;
            Canvas_Bubble.overrideSorting = true;
            Canvas_Bubble.sortingOrder = 6200;
        }

        if (Scroll_Body == null)
            return;

        Image[] imageArray = Scroll_Body.GetComponentsInChildren<Image>(true);

        foreach (Image image in imageArray)
        {
            if (image == null)
                continue;

            Color color = image.color;
            color.a = 0f;
            image.color = color;
        }
    }
}
