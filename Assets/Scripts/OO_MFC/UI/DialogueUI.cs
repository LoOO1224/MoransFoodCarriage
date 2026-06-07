// =============================================================================
// OO_MFC ??븷 二쇱꽍
// - ?ㅽ겕由쏀듃: DialogueUI.cs
// - ??븷: UI ?쒖떆? ?낅젰 ?곌껐???대떦?섎뒗 UI 而댄룷?뚰듃?낅땲??
// - 媛먮룆 愿?? 愿媛앹뿉寃?蹂댁씠???⑤꼸怨?踰꾪듉??臾대? ?숈꽑???대떦?⑸땲??
// - ?좎?蹂댁닔 ?ъ씤?? ?ъ슜?먭? 吏곸젒 ?몄쭛??UI???섏씠?대씪???꾨━?뱀뿉 ?먭퀬, 肄붾뱶?먯꽌 利됱꽍 ?앹꽦?섏? ?딆뒿?덈떎.
// =============================================================================
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// ??붿갹???붿옄 ?대쫫, 蹂몃Ц, ?ㅼ쓬 踰꾪듉, ?ㅽ겕濡??곸뿭??愿由ы빀?덈떎.
/// ?꾨·濡쒓렇 ?섎젅?댁뀡泥섎읆 湲?臾몄옣???ㅼ뼱?ㅻ뒗 寃쎌슦?먮뒗 ScrollRect??Content瑜?媛깆떊?섍퀬
/// ??臾몄옣???쒖떆???뚮쭏???ㅽ겕濡??꾩튂瑜?留??꾨줈 ?섎룎由쎈땲??
/// </summary>
public class DialogueUI : MonoBehaviour
{
    // ?쎈뒗 ?쒖꽌:
    // 1. OpenDialogue/StartDialogue 怨꾩뿴: ?몃? Controller媛 ???ID瑜??섍린硫??곗씠?곗뿉????щ? ?쎌뒿?덈떎.
    // 2. SetSpeaker 怨꾩뿴: OO_Character ?곗씠?곕? ?댁슜???붿옄 ?대쫫???쒖떆?⑸땲??
    // 3. SetDialogueText 怨꾩뿴: OO_Dialogue ?먮뒗 OO_Narration???ㅼ젣 臾몄옣???⑤꼸???ｌ뒿?덈떎.
    // 4. NextDialogue: ?댁뼱媛湲?踰꾪듉 ?먮뒗 ?⑤꼸 諛??대┃?쇰줈 ?ㅼ쓬 ??щ? 吏꾪뻾?⑸땲??
    // 5. CloseDialogue 怨꾩뿴: ??ш? ?앸굹硫?DialogueGroup???リ굅???ㅼ쓬 ?먮줈 ?섍퉩?덈떎.
    // ?좎?蹂댁닔 二쇱쓽:
    // - ????댁슜? 肄붾뱶???곗? 留먭퀬 OO_Dialogue/OO_Narration/OO_DialogueGroup?먯꽌 ?쒕━釉먰빀?덈떎.
    // - DialoguePanel ?꾩튂? 踰꾪듉 ?대?吏???섏씠?대씪?ㅼ뿉??吏곸젒 ?섏젙?⑸땲??
    // - ?붿옄 ?대쫫 諛곌꼍? OOTechDialogueSpeakerNameBackdrop??蹂댁“?⑸땲??

    [Header("Text")]
    [FormerlySerializedAs("_speakerNameText")]
    [SerializeField] private TextMeshProUGUI Text_SpeakerName;

    [FormerlySerializedAs("_dialogueText")]
    [SerializeField] private TextMeshProUGUI Text_Dialogue;

    [Header("Button")]
    [FormerlySerializedAs("_nextButton")]
    [SerializeField] private Button Button_Next;

    [Header("Choice")]
    [SerializeField] private GameObject Root_ChoicePanel;
    [SerializeField] private Button Button_ChoiceYes;
    [SerializeField] private Button Button_ChoiceNo;
    [SerializeField] private TextMeshProUGUI Text_ChoiceYes;
    [SerializeField] private TextMeshProUGUI Text_ChoiceNo;

    [Header("Scroll")]
    [SerializeField] private ScrollRect Scroll_Dialogue;
    [SerializeField] private RectTransform Rect_DialogueContent;
    [SerializeField] private float _scrollContentPadding = 16f;

    [Header("Outside Click")]
    [SerializeField] private bool _isAdvanceByOutsideClick = true;
    [SerializeField] private float _outsideClickDelay = 0.08f;
    [SerializeField] private int _roadViewSortingOrder = 1400;

    // ==================== ????곹깭 ====================
    private OO_Dialogue _currentDialogue;
    private readonly List<string> _narrationTextList = new List<string>();
    private int _currentNarrationTextIndex;
    private Action _onDialogueEnd;
    private OO_Choice _currentChoice;
    private Action<int> _onChoiceSelected;
    private bool _isChoiceActive;
    private Coroutine _refreshScrollCoroutine;
    private OOTechDialogueLayout Layout_Dialogue;
    private OOTechDialogueSpeakerNameBackdrop Backdrop_SpeakerName;
    private GameObject Root_OutsideClickBlocker;
    private Button Button_OutsideClickBlocker;
    private float _outsideClickReadyTime;

    /// <summary>
    /// ???UI媛 泥섏쓬 以鍮꾨맆 ???고듃, ?붿옄 ?대쫫 諛곌꼍, 湲곕낯 ?덉씠?꾩썐??以鍮꾪빀?덈떎.
    /// </summary>
    private void Awake()
    {
        ApplyProjectFont();
        PrepareSpeakerNameBackdrop();
        ApplyDialogueLayout();
        PrepareChoicePanel();
    }

    /// <summary>
    /// DialogueGroup???대┫ ??踰꾪듉 ?대깽?몄? ?쒖떆 ?ㅽ??쇱쓣 ?ㅼ떆 ?곌껐?⑸땲??
    /// </summary>
    private void OnEnable()
    {
        ApplyProjectFont();
        PrepareSpeakerNameBackdrop();
        ApplyDialogueLayout();
        PrepareChoicePanel();
        BindButtonEvent();
    }

    /// <summary>
    /// DialogueGroup???ロ옄 ??踰꾪듉 ?대깽?? ?ㅽ겕濡?媛깆떊, 諛붽묑 ?대┃ 釉붾줈而ㅻ? ?뺣━?⑸땲??
    /// </summary>
    private void OnDisable()
    {
        UnbindButtonEvent();
        StopRefreshScrollCoroutine();
        ClearChoiceState();
        SetOutsideClickBlockerActive(false);
    }

    /// <summary>
    /// 踰꾪듉???꾨땲????붿갹 諛뽰쓣 ?대┃?대룄 ?댁뼱媛湲곗쿂??吏꾪뻾?섎룄濡?媛먯떆?⑸땲??
    /// </summary>
    private void Update()
    {
        if (_isChoiceActive)
        {
            UpdateChoiceKeyboardInput();
            return;
        }

        if (!CanAdvanceByOutsideClick())
            return;

        NextDialogue();
    }

    // ==================== 踰꾪듉 諛붿씤??====================

    /// <summary>
    /// Dialogue_NextButton? ?꾩옱 ?쒖떆 以묒씤 臾몄옣 臾띠쓬留?吏꾪뻾?⑸땲??
    /// ?꾨·濡쒓렇 而룹뵮 ?꾪솚? DialogueUI媛 ?앸궗?뚯쓣 PrologueController???뚮젮??泥섎━?⑸땲??
    /// </summary>
    private void ApplyDialogueLayout()
    {
        if (Layout_Dialogue == null)
            Layout_Dialogue = GetComponent<OOTechDialogueLayout>();

        if (Layout_Dialogue != null)
            Layout_Dialogue.ApplyLayout();
    }

    /// <summary>
    /// Road View?먯꽌??HUD媛 ?섎떒???덉쑝誘濡???붿갹??以묒븰 臾대?濡??щ━怨?HUD蹂대떎 ?꾩뿉 ?뚮뜑留곹빀?덈떎.
    /// </summary>
    public void RequestRoadViewLayout()
    {
        if (Layout_Dialogue == null)
            Layout_Dialogue = GetComponent<OOTechDialogueLayout>();

        if (Layout_Dialogue == null)
            Layout_Dialogue = gameObject.AddComponent<OOTechDialogueLayout>();

        Layout_Dialogue.ApplyRoadViewLayout();
        RaiseCanvasForRoadViewDialogue();
    }

    /// <summary>
    /// ?꾨줈?앺듃 怨듯넻 ?쒓? ?고듃瑜??붿옄 ?대쫫怨?蹂몃Ц???곸슜?⑸땲??
    /// </summary>
    private void ApplyProjectFont()
    {
        OOTechTMPFontUtility.ApplyProjectFont(Text_SpeakerName);
        OOTechTMPFontUtility.ApplyProjectFont(Text_Dialogue);
        OOTechTMPFontUtility.ApplyProjectFont(Text_ChoiceYes);
        OOTechTMPFontUtility.ApplyProjectFont(Text_ChoiceNo);
    }

    /// <summary>
    /// SpeakerNameText ?ㅼ쓽 諛섑닾紐??대쫫??而댄룷?뚰듃瑜?以鍮꾪빀?덈떎.
    /// </summary>
    private void PrepareSpeakerNameBackdrop()
    {
        if (Backdrop_SpeakerName == null)
            Backdrop_SpeakerName = GetComponent<OOTechDialogueSpeakerNameBackdrop>();

        if (Backdrop_SpeakerName == null)
            Backdrop_SpeakerName = gameObject.AddComponent<OOTechDialogueSpeakerNameBackdrop>();
    }

    /// <summary>
    /// Next 踰꾪듉???꾩옱 ???吏꾪뻾 ?⑥닔???곌껐?⑸땲??
    /// </summary>
    private void BindButtonEvent()
    {
        if (Button_Next == null)
            return;

        Button_Next.onClick.RemoveListener(NextDialogue);
        Button_Next.onClick.AddListener(NextDialogue);
    }

    /// <summary>
    /// Next 踰꾪듉 ?대깽?몃? ?댁젣??以묐났 ?몄텧??留됱뒿?덈떎.
    /// </summary>
    private void UnbindButtonEvent()
    {
        if (Button_Next == null)
            return;

        Button_Next.onClick.RemoveListener(NextDialogue);
    }

    // ==================== ?쇰컲 ????쒖떆 ====================

    /// <summary>
    /// OO_Dialogue ?곗씠?곕? 湲곕컲?쇰줈 ?쇰컲 ??붾? ?쒖떆?⑸땲??
    /// </summary>
    public void ShowDialogue(OO_Dialogue dialogueData, Action onDialogueEnd = null)
    {
        if (dialogueData == null)
        {
            Debug.LogWarning("[DialogueUI] ?쒖떆??????곗씠?곌? ?놁뒿?덈떎.");
            return;
        }

        _currentDialogue = dialogueData;
        _onDialogueEnd = onDialogueEnd;
        _narrationTextList.Clear();
        _currentNarrationTextIndex = 0;
        ClearChoiceState();

        gameObject.SetActive(true);
        BlockOutsideClickBriefly();

        SetSpeakerName(dialogueData.SpeakerName);
        SetDialogueText(dialogueData.Text);
        SetNextButtonActive(dialogueData.SelectionNameList == null || dialogueData.SelectionNameList.Count == 0);
    }

    // ==================== ?섎젅?댁뀡 ?쒖떆 ====================

    /// <summary>
    /// OO_Narration ?곗씠?곕? 湲곕컲?쇰줈 ?섎젅?댁뀡???쒖떆?⑸땲??
    /// SpeakerNameText???붽뎄?ы빆???곕씪 ??긽 "?섎젅?댁뀡"?쇰줈 怨좎젙?⑸땲??
    /// </summary>
    /// <summary>
    /// OO_Choice ?곗씠?곕? 湲곗〈 DialoguePanel ?꾩뿉 ?좏깮吏 紐⑤뱶濡??쒖떆?⑸땲??
    /// ?곹솕 鍮꾩쑀濡쒕뒗 媛숈? 臾대? ?명듃??"愿媛??좏깮 ??留?異붽?濡??대젮?볥뒗 諛⑹떇?낅땲??
    /// </summary>
    public void ShowChoice(OO_Choice choiceData, Action<int> onChoiceSelected)
    {
        if (choiceData == null)
        {
            Debug.LogWarning("[DialogueUI] ?쒖떆???좏깮吏 ?곗씠?곌? ?놁뒿?덈떎.");
            return;
        }

        _currentDialogue = null;
        _onDialogueEnd = null;
        _narrationTextList.Clear();
        _currentNarrationTextIndex = 0;
        _currentChoice = choiceData;
        _onChoiceSelected = onChoiceSelected;
        _isChoiceActive = true;

        gameObject.SetActive(true);
        PrepareChoicePanel();
        BlockOutsideClickBriefly();

        SetSpeakerName(choiceData.SpeakerName);
        SetDialogueText(choiceData.PromptText);
        SetNextButtonActive(false);
        ApplyChoiceButtonText(choiceData);
        SetChoicePanelActive(true);
    }

    public void ShowNarration(OO_Narration narrationData, Action onDialogueEnd = null)
    {
        if (narrationData == null)
        {
            Debug.LogWarning("[DialogueUI] ?쒖떆???섎젅?댁뀡 ?곗씠?곌? ?놁뒿?덈떎.");
            return;
        }

        _currentDialogue = null;
        _onDialogueEnd = onDialogueEnd;
        _narrationTextList.Clear();
        _currentNarrationTextIndex = 0;
        ClearChoiceState();

        AddNarrationTextList(narrationData.NarrationTexts);

        if (_narrationTextList.Count == 0)
        {
            Debug.LogWarning($"[DialogueUI] ?섎젅?댁뀡 ?띿뒪?멸? 鍮꾩뼱 ?덉뒿?덈떎: {narrationData.Id}");
            FinishDialogue();
            return;
        }

        gameObject.SetActive(true);

        SetSpeakerName("?섎젅?댁뀡");
        BlockOutsideClickBriefly();
        SetNextButtonActive(true);
        ShowCurrentNarrationText();
    }

    /// <summary>
    /// ?섎젅?댁뀡 ?띿뒪?몃? ?쒖떆 ?⑥쐞濡??뺣━?⑸땲??
    /// 湲곕낯? JSON??????ぉ????踰덉뿉 蹂댁뿬二쇰ŉ, ?꾩슂?섎㈃ ?띿뒪???덉쓽 &lt;np&gt; ?쒓렇濡??섏씠吏瑜??섎닃 ???덉뒿?덈떎.
    /// </summary>
    /// <summary>
    /// ?섎젅?댁뀡 臾몄옣 臾띠쓬???꾩옱 ?섏씠吏 ?⑥쐞濡?異붽??⑸땲??
    /// </summary>
    private void AddNarrationTextList(List<string> narrationTexts)
    {
        if (narrationTexts == null)
            return;

        foreach (string narrationText in narrationTexts)
            AddNarrationText(narrationText);
    }

    /// <summary>
    /// 湲??섎젅?댁뀡 ?덉쓽 &lt;np&gt; ?쒓렇瑜?湲곗??쇰줈 ?쒖떆 ?섏씠吏瑜??섎닏?덈떎.
    /// </summary>
    private void AddNarrationText(string narrationText)
    {
        if (string.IsNullOrWhiteSpace(narrationText))
            return;

        string normalizedText = narrationText.Trim();
        string[] splitTextArray = normalizedText.Split(new[] { "<np>" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string splitText in splitTextArray)
        {
            if (string.IsNullOrWhiteSpace(splitText))
                continue;

            _narrationTextList.Add(splitText.Trim());
        }
    }

    /// <summary>
    /// ?꾩옱 ?섎젅?댁뀡 ?섏씠吏瑜???붿갹 蹂몃Ц??蹂댁뿬以띾땲??
    /// </summary>
    private void ShowCurrentNarrationText()
    {
        if (_currentNarrationTextIndex < 0 || _currentNarrationTextIndex >= _narrationTextList.Count)
        {
            FinishDialogue();
            return;
        }

        SetSpeakerName("?섎젅?댁뀡");
        SetDialogueText(_narrationTextList[_currentNarrationTextIndex]);
    }

    // ==================== ?ㅼ쓬 ???吏꾪뻾 ====================

    /// <summary>
    /// ?ㅼ쓬 踰꾪듉 ?대┃ ???꾩옱 ?섎젅?댁뀡???ㅼ쓬 ?쒖떆 ?⑥쐞濡?吏꾪뻾?⑸땲??
    /// ?꾩옱 ?뚰듃???섎젅?댁뀡??紐⑤몢 ?앸굹硫?醫낅즺 肄쒕갚???몄텧?섏뿬 PrologueController媛 ?ㅼ쓬 而룹뵮???닿쾶 ?⑸땲??
    /// </summary>
    public void NextDialogue()
    {
        if (_isChoiceActive)
            return;

        if (_narrationTextList.Count > 0)
        {
            MoveNextNarrationText();
            return;
        }

        MoveNextDialogue();
    }

    /// <summary>
    /// ?섎젅?댁뀡?대㈃ ?ㅼ쓬 ?섏씠吏濡? ?쇰컲 ??붾㈃ ?꾩옱 ??붾? 醫낅즺?⑸땲??
    /// </summary>
    private void MoveNextNarrationText()
    {
        _currentNarrationTextIndex++;

        if (_currentNarrationTextIndex < _narrationTextList.Count)
        {
            ShowCurrentNarrationText();
            return;
        }

        FinishDialogue();
    }

    /// <summary>
    /// ?쇰컲 ??붾뒗 ??踰??쎌쑝硫?醫낅즺 肄쒕갚?쇰줈 ?ㅼ쓬 肄쒖떆?몄뿉 ?섍퉩?덈떎.
    /// </summary>
    private void MoveNextDialogue()
    {
        if (_currentDialogue == null)
        {
            FinishDialogue();
            return;
        }

        if (!string.IsNullOrEmpty(_currentDialogue.NextDialogueId))
            Debug.Log($"[DialogueUI] ?ㅼ쓬 ???ID媛 吏?뺣릺???덉뒿?덈떎: {_currentDialogue.NextDialogueId}");

        FinishDialogue();
    }

    // ==================== ??붿갹 ?リ린 ====================

    /// <summary>
    /// ?몃??먯꽌 ??붿갹??媛뺤젣濡??レ쓣 ???ъ슜?⑸땲??
    /// DialogueGroup ?먯껜媛 ?꾨땲??DialoguePanel留??꾧퀬 ?띠쓣 ?뚮룄 ??硫붿꽌?쒕? ?몄텧?⑸땲??
    /// </summary>
    public void CloseDialogue()
    {
        SetOutsideClickBlockerActive(false);
        ClearDialogueState();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// ??붽? ?앸궗?뚯쓣 ?몃? 而⑦듃濡ㅻ윭???뚮━怨??꾩옱 ?곹깭瑜?鍮꾩썎?덈떎.
    /// </summary>
    private void FinishDialogue()
    {
        Action onDialogueEnd = _onDialogueEnd;
        SetOutsideClickBlockerActive(false);
        ClearDialogueState();
        onDialogueEnd?.Invoke();
    }

    private void ClearDialogueState()
    {
        _currentDialogue = null;
        _narrationTextList.Clear();
        _currentNarrationTextIndex = 0;
        _onDialogueEnd = null;
        ClearChoiceState();
    }

    // ==================== UI 媛??ㅼ젙 ====================

    /// <summary>
    /// ?붿옄 ?대쫫 移몄뿉 ?쒖떆???대쫫???곸슜?⑸땲?? 鍮꾩뼱 ?덉쑝硫??섎젅?댁뀡?쇰줈 ?쒖떆?⑸땲??
    /// </summary>
    private void SetSpeakerName(string speakerName)
    {
        if (Text_SpeakerName != null)
            Text_SpeakerName.text = string.IsNullOrEmpty(speakerName) ? "?섎젅?댁뀡" : speakerName;
    }

    /// <summary>
    /// 蹂몃Ц ?띿뒪?몃? ?곸슜?섍퀬 ?ㅽ겕濡??믪씠瑜?利됱떆 媛깆떊?⑸땲??
    /// </summary>
    private void SetDialogueText(string dialogueText)
    {
        if (Text_Dialogue != null)
        {
            Text_Dialogue.text = dialogueText ?? string.Empty;
            Text_Dialogue.gameObject.SetActive(true);
            Text_Dialogue.enabled = true;
            Text_Dialogue.color = new Color(Text_Dialogue.color.r, Text_Dialogue.color.g, Text_Dialogue.color.b, 1f);
            Text_Dialogue.canvasRenderer.SetAlpha(1f);
            Text_Dialogue.textWrappingMode = TextWrappingModes.Normal;
            Text_Dialogue.overflowMode = TextOverflowModes.Overflow;
            Text_Dialogue.maskable = true;
        }

        ResizeDialogueTextToPreferredHeight();
        RebuildScrollContent();
        ResetScrollPosition();
        RequestRefreshScrollOnNextFrame();
    }

    /// <summary>
    /// ?댁뼱媛湲?踰꾪듉 ?쒖떆? 諛붽묑 ?대┃ 吏꾪뻾 媛???곹깭瑜??④퍡 留욎땅?덈떎.
    /// </summary>
    private void SetNextButtonActive(bool isActive)
    {
        if (Button_Next != null)
            Button_Next.gameObject.SetActive(isActive);

        SetOutsideClickBlockerActive(isActive && HasActiveDialogueText());
    }

    // ==================== Outside click advance ====================

    /// <summary>
    /// ??붿갹 諛붽묑 ?대┃???댁뼱媛湲??낅젰?쇰줈 泥섎━?????덈뒗吏 ?뺤씤?⑸땲??
    /// </summary>
    private bool CanAdvanceByOutsideClick()
    {
        if (!_isAdvanceByOutsideClick)
            return false;

        if (_isChoiceActive)
            return false;

        if (Root_OutsideClickBlocker != null && Root_OutsideClickBlocker.activeInHierarchy)
            return false;

        if (!gameObject.activeInHierarchy)
            return false;

        if (!HasActiveDialogueText())
            return false;

        if (Time.unscaledTime < _outsideClickReadyTime)
            return false;

        if (Button_Next != null && !Button_Next.gameObject.activeInHierarchy)
            return false;

        if (!Input.GetMouseButtonDown(0))
            return false;

        return !IsPointerInsideDialoguePanel();
    }

    /// <summary>
    /// ??붿갹 ?ㅼ뿉 ?щ챸 踰꾪듉??源붿븘 諛붽묑 ?대┃??諛쏆쓣 以鍮꾨? ?⑸땲??
    /// </summary>
    private void PrepareOutsideClickBlocker()
    {
        if (Root_OutsideClickBlocker != null)
            return;

        Transform parentTransform = transform.parent;

        if (parentTransform == null)
            return;

        Root_OutsideClickBlocker = new GameObject("Button_DialogueOutsideClickBlocker", typeof(RectTransform));
        Root_OutsideClickBlocker.transform.SetParent(parentTransform, false);

        RectTransform blockerRect = Root_OutsideClickBlocker.transform as RectTransform;

        if (blockerRect != null)
        {
            blockerRect.anchorMin = Vector2.zero;
            blockerRect.anchorMax = Vector2.one;
            blockerRect.pivot = new Vector2(0.5f, 0.5f);
            blockerRect.offsetMin = Vector2.zero;
            blockerRect.offsetMax = Vector2.zero;
            blockerRect.localScale = Vector3.one;
        }

        Image blockerImage = Root_OutsideClickBlocker.AddComponent<Image>();
        blockerImage.color = new Color(0f, 0f, 0f, 0f);
        blockerImage.raycastTarget = true;

        Button_OutsideClickBlocker = Root_OutsideClickBlocker.AddComponent<Button>();
        Button_OutsideClickBlocker.transition = Selectable.Transition.None;
        Button_OutsideClickBlocker.onClick.AddListener(HandleOutsideClickBlockerClicked);
        Root_OutsideClickBlocker.SetActive(false);
    }

    /// <summary>
    /// 諛붽묑 ?대┃ 釉붾줈而ㅻ? 耳쒓퀬 ?뺣땲??
    /// </summary>
    private void SetOutsideClickBlockerActive(bool isActive)
    {
        if (!_isAdvanceByOutsideClick)
            isActive = false;

        if (isActive)
            PrepareOutsideClickBlocker();

        if (Root_OutsideClickBlocker == null)
            return;

        Root_OutsideClickBlocker.SetActive(isActive);

        if (!isActive)
            return;

        Root_OutsideClickBlocker.transform.SetSiblingIndex(transform.GetSiblingIndex());
        transform.SetAsLastSibling();
    }

    /// <summary>
    /// ?щ챸 釉붾줈而ㅺ? ?대┃?섎㈃ NextDialogue? 媛숈? ?숈옉???ㅽ뻾?⑸땲??
    /// </summary>
    private void HandleOutsideClickBlockerClicked()
    {
        if (Time.unscaledTime < _outsideClickReadyTime)
            return;

        NextDialogue();
    }

    private bool HasActiveDialogueText()
    {
        return _currentDialogue != null || _narrationTextList.Count > 0 || _currentChoice != null;
    }

    /// <summary>
    /// ChoicePanel??以鍮꾪빀?덈떎. ?ъ뿉 諛곗튂?섏뼱 ?덉쑝硫??ъ궗?⑺븯怨? ?놁쑝硫?DialoguePanel ?덉뿉 理쒖냼 踰꾪듉留?留뚮벊?덈떎.
    /// </summary>
    private void PrepareChoicePanel()
    {
        if (Root_ChoicePanel == null)
            Root_ChoicePanel = RequestChildObjectByName(transform, "ChoicePanel");

        if (Root_ChoicePanel == null)
            Root_ChoicePanel = CreateChoicePanelObject();

        if (Button_ChoiceYes == null)
            Button_ChoiceYes = RequestChoiceButton("Button_ChoiceY", Root_ChoicePanel);

        if (Button_ChoiceNo == null)
            Button_ChoiceNo = RequestChoiceButton("Button_ChoiceN", Root_ChoicePanel);

        if (Button_ChoiceYes == null)
            Button_ChoiceYes = CreateChoiceButton(Root_ChoicePanel.transform, "Button_ChoiceY", "예");

        if (Button_ChoiceNo == null)
            Button_ChoiceNo = CreateChoiceButton(Root_ChoicePanel.transform, "Button_ChoiceN", "아니오");

        Text_ChoiceYes = Text_ChoiceYes != null ? Text_ChoiceYes : Button_ChoiceYes.GetComponentInChildren<TextMeshProUGUI>(true);
        Text_ChoiceNo = Text_ChoiceNo != null ? Text_ChoiceNo : Button_ChoiceNo.GetComponentInChildren<TextMeshProUGUI>(true);

        BindChoiceButtonEvent();
        SetChoicePanelActive(false);
    }

    private GameObject CreateChoicePanelObject()
    {
        GameObject panelObject = new GameObject("ChoicePanel", typeof(RectTransform));
        panelObject.transform.SetParent(transform, false);

        RectTransform panelRect = panelObject.transform as RectTransform;
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 28f);
        panelRect.sizeDelta = new Vector2(520f, 84f);

        HorizontalLayoutGroup layoutGroup = panelObject.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.spacing = 24f;
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = false;

        return panelObject;
    }

    private Button CreateChoiceButton(Transform parentTransform, string buttonName, string labelText)
    {
        GameObject buttonObject = new GameObject(buttonName, typeof(RectTransform));
        buttonObject.transform.SetParent(parentTransform, false);

        RectTransform buttonRect = buttonObject.transform as RectTransform;
        buttonRect.sizeDelta = new Vector2(220f, 64f);

        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(0.08f, 0.08f, 0.08f, 0.88f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;

        GameObject textObject = new GameObject("Text_Label", typeof(RectTransform));
        textObject.transform.SetParent(buttonObject.transform, false);

        RectTransform textRect = textObject.transform as RectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = textObject.AddComponent<TextMeshProUGUI>();
        label.text = labelText;
        label.fontSize = 30f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        OOTechTMPFontUtility.ApplyProjectFont(label);

        return button;
    }

    private Button RequestChoiceButton(string buttonName, GameObject rootObject)
    {
        if (rootObject == null)
            return null;

        GameObject buttonObject = RequestChildObjectByName(rootObject.transform, buttonName);
        return buttonObject != null ? buttonObject.GetComponent<Button>() : null;
    }

    private void BindChoiceButtonEvent()
    {
        if (Button_ChoiceYes != null)
        {
            Button_ChoiceYes.onClick.RemoveListener(SelectChoiceYes);
            Button_ChoiceYes.onClick.AddListener(SelectChoiceYes);
        }

        if (Button_ChoiceNo != null)
        {
            Button_ChoiceNo.onClick.RemoveListener(SelectChoiceNo);
            Button_ChoiceNo.onClick.AddListener(SelectChoiceNo);
        }
    }

    private void ApplyChoiceButtonText(OO_Choice choiceData)
    {
        string yesText = GetChoiceButtonText(choiceData, 0, "예");
        string noText = GetChoiceButtonText(choiceData, 1, "아니오");

        if (Text_ChoiceYes != null)
            Text_ChoiceYes.text = yesText;

        if (Text_ChoiceNo != null)
            Text_ChoiceNo.text = noText;
    }

    private string GetChoiceButtonText(OO_Choice choiceData, int index, string fallbackText)
    {
        if (choiceData == null || choiceData.OptionTextList == null || index < 0 || index >= choiceData.OptionTextList.Count)
            return fallbackText;

        string optionText = choiceData.OptionTextList[index];
        return string.IsNullOrWhiteSpace(optionText) ? fallbackText : optionText;
    }

    private void UpdateChoiceKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Y) || Input.GetKeyDown(KeyCode.Return))
            SelectChoice(0);
        else if (Input.GetKeyDown(KeyCode.N) || Input.GetKeyDown(KeyCode.Escape))
            SelectChoice(1);
    }

    private void SelectChoiceYes()
    {
        SelectChoice(0);
    }

    private void SelectChoiceNo()
    {
        SelectChoice(1);
    }

    private void SelectChoice(int choiceIndex)
    {
        if (!_isChoiceActive)
            return;

        Action<int> onChoiceSelected = _onChoiceSelected;
        ClearChoiceState();
        onChoiceSelected?.Invoke(choiceIndex);
    }

    private void SetChoicePanelActive(bool isActive)
    {
        if (Root_ChoicePanel != null)
            Root_ChoicePanel.SetActive(isActive);
    }

    private void ClearChoiceState()
    {
        _currentChoice = null;
        _onChoiceSelected = null;
        _isChoiceActive = false;
        SetChoicePanelActive(false);
    }

    private GameObject RequestChildObjectByName(Transform rootTransform, string objectName)
    {
        if (rootTransform == null || string.IsNullOrEmpty(objectName))
            return null;

        if (rootTransform.name == objectName)
            return rootTransform.gameObject;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            GameObject foundObject = RequestChildObjectByName(rootTransform.GetChild(index), objectName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }

    private bool IsPointerInsideDialoguePanel()
    {
        RectTransform panelRect = transform as RectTransform;

        if (panelRect == null)
            return false;

        return RectTransformUtility.RectangleContainsScreenPoint(panelRect, Input.mousePosition, GetCanvasCamera());
    }

    private Camera GetCanvasCamera()
    {
        Canvas canvas = GetComponentInParent<Canvas>();

        if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;

        return canvas.worldCamera;
    }

    private void BlockOutsideClickBriefly()
    {
        _outsideClickReadyTime = Time.unscaledTime + _outsideClickDelay;
    }

    /// <summary>
    /// Road View ??붿갹??HUD蹂대떎 ?욎뿉 蹂댁씠?꾨줉 Canvas ?뺣젹 ?쒖꽌瑜??щ┰?덈떎.
    /// </summary>
    private void RaiseCanvasForRoadViewDialogue()
    {
        Canvas canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
            return;

        canvas.overrideSorting = true;
        canvas.sortingOrder = Mathf.Max(canvas.sortingOrder, _roadViewSortingOrder);
    }

    // ==================== ?ㅽ겕濡?媛깆떊 ====================

    /// <summary>
    /// TMP ?띿뒪?몄쓽 ?ㅼ젣 ?좏샇 ?믪씠瑜?Content??諛섏쁺?⑸땲??
    /// ??蹂댁젙???놁쑝硫?湲??섎젅?댁뀡??留덉뒪??諛뽰쑝濡?諛??蹂댁씠嫄곕굹 Content ?믪씠媛 0?쇰줈 ?⑥쓣 ???덉뒿?덈떎.
    /// </summary>
    /// <summary>
    /// 蹂몃Ц ?댁슜 湲몄씠??留욎떠 TMP ?띿뒪?몄? ?ㅽ겕濡?Content ?믪씠瑜?媛깆떊?⑸땲??
    /// </summary>
    private void ResizeDialogueTextToPreferredHeight()
    {
        if (Text_Dialogue == null)
            return;

        RectTransform dialogueTextRect = Text_Dialogue.rectTransform;
        float contentWidth = GetDialogueContentWidth(dialogueTextRect);

        dialogueTextRect.anchorMin = new Vector2(0f, 1f);
        dialogueTextRect.anchorMax = new Vector2(1f, 1f);
        dialogueTextRect.pivot = new Vector2(0.5f, 1f);
        dialogueTextRect.anchoredPosition = Vector2.zero;
        dialogueTextRect.localScale = Vector3.one;

        dialogueTextRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentWidth);
        Text_Dialogue.ForceMeshUpdate();

        float preferredHeight = Text_Dialogue.GetPreferredValues(Text_Dialogue.text, contentWidth, 0f).y;
        float finalHeight = Mathf.Max(preferredHeight + _scrollContentPadding, 80f);

        dialogueTextRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, finalHeight);

        if (Rect_DialogueContent != null)
        {
            Rect_DialogueContent.anchorMin = new Vector2(0f, 1f);
            Rect_DialogueContent.anchorMax = new Vector2(1f, 1f);
            Rect_DialogueContent.pivot = new Vector2(0.5f, 1f);
            Rect_DialogueContent.anchoredPosition = Vector2.zero;
            Rect_DialogueContent.localScale = Vector3.one;
            Rect_DialogueContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, finalHeight);
        }

        Text_Dialogue.SetVerticesDirty();
        Text_Dialogue.SetLayoutDirty();
        Text_Dialogue.ForceMeshUpdate();
    }

    private float GetDialogueContentWidth(RectTransform dialogueTextRect)
    {
        if (Rect_DialogueContent != null && Rect_DialogueContent.rect.width > 1f)
            return Rect_DialogueContent.rect.width;

        if (dialogueTextRect != null && dialogueTextRect.rect.width > 1f)
            return dialogueTextRect.rect.width;

        return 1200f;
    }

    /// <summary>
    /// Vertical Layout Group怨?Content Size Fitter媛 怨꾩궛???믪씠瑜?利됱떆 諛섏쁺?⑸땲??
    /// Rect_DialogueContent瑜??곌껐?섏? ?딆? 寃쎌슦?먮룄 湲곗〈 ???異쒕젰? 怨꾩냽 ?숈옉?⑸땲??
    /// </summary>
    private void RebuildScrollContent()
    {
        Canvas.ForceUpdateCanvases();

        if (Rect_DialogueContent != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(Rect_DialogueContent);

        Canvas.ForceUpdateCanvases();
    }

    /// <summary>
    /// ??臾몄옣???쒖떆?????ㅽ겕濡ㅼ쓣 留??꾨줈 ?щ┰?덈떎.
    /// </summary>
    private void ResetScrollPosition()
    {
        if (Rect_DialogueContent != null)
            Rect_DialogueContent.anchoredPosition = Vector2.zero;

        if (Scroll_Dialogue == null)
            return;

        if (Rect_DialogueContent != null)
            Scroll_Dialogue.content = Rect_DialogueContent;

        Scroll_Dialogue.StopMovement();
        Scroll_Dialogue.verticalNormalizedPosition = 1f;

        if (Rect_DialogueContent != null)
            Rect_DialogueContent.anchoredPosition = Vector2.zero;
    }

    /// <summary>
    /// DialoguePanel??耳쒖쭊 吏곹썑?먮뒗 RectTransform ??낵 ?믪씠媛 ?ㅼ쓬 ?꾨젅?꾩뿉 ?뺤젙?????덉뒿?덈떎.
    /// 洹몃옒?????꾨젅???ㅼ뿉 媛숈? 蹂댁젙????踰????섑뻾??以꾧굅由ш? 鍮덉뭏泥섎읆 蹂댁씠???곹솴??留됱뒿?덈떎.
    /// </summary>
    private void RequestRefreshScrollOnNextFrame()
    {
        StopRefreshScrollCoroutine();

        if (gameObject.activeInHierarchy)
            _refreshScrollCoroutine = StartCoroutine(RefreshScrollOnNextFrame());
    }

    private IEnumerator RefreshScrollOnNextFrame()
    {
        yield return null;

        ResizeDialogueTextToPreferredHeight();
        RebuildScrollContent();
        ResetScrollPosition();

        _refreshScrollCoroutine = null;
    }

    private void StopRefreshScrollCoroutine()
    {
        if (_refreshScrollCoroutine == null)
            return;

        StopCoroutine(_refreshScrollCoroutine);
        _refreshScrollCoroutine = null;
    }
}


