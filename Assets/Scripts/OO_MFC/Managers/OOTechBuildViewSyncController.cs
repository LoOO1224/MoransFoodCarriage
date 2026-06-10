// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OOTechBuildViewSyncController.cs
// - 역할: 게임 전체에서 공유되는 매니저 역할을 담당합니다.
// - 유지보수: 싱글톤 초기화 순서와 씬 전환 중 유지되는 데이터를 함께 확인합니다.
// =============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class OOTechBuildViewSyncController : MonoBehaviour
{
    [Header("Build View")]
    [SerializeField] private bool _isApplyOnlyInBuild = true;
    [SerializeField] private bool _isForceResolutionInBuild = true;
    [SerializeField] private int _targetWidth = 1920;
    [SerializeField] private int _targetHeight = 1080;
    [SerializeField] private FullScreenMode _targetFullScreenMode = FullScreenMode.Windowed;

    [Header("Canvas")]
    [SerializeField] private Vector2 _referenceResolution = new Vector2(1920f, 1080f);
    [SerializeField, Range(0f, 1f)] private float _matchWidthOrHeight = 0.5f;
    [SerializeField] private float _screenChangeCheckInterval = 0.5f;

    private int _lastScreenWidth;
    private int _lastScreenHeight;
    private float _screenChangeCheckTimer;

    /// <summary>
    /// 게임 시작 시 상영 환경을 한 번 정리합니다.
    /// Game View에서는 기존 에디터 상태를 우선하므로, 기본 설정상 빌드에서만 동작합니다.
    /// </summary>
    private void Awake()
    {
        if (!CanApplyRuntimeSync())
            return;

        DontDestroyOnLoad(gameObject);
        RequestPrepareRuntimeView();
        StartCoroutine(RequestApplyResolutionAfterFirstFrameRoutine());
    }

    /// <summary>
    /// 빌드 창 크기가 바뀌었을 때 CanvasScaler를 다시 맞춥니다.
    /// 플레이어가 exe 창을 건드려도 UI가 1920x1080 기준 연출에서 크게 벗어나지 않게 하는 보험입니다.
    /// </summary>
    private void Update()
    {
        if (!CanApplyRuntimeSync())
            return;

        _screenChangeCheckTimer += Time.unscaledDeltaTime;

        if (_screenChangeCheckTimer < _screenChangeCheckInterval)
            return;

        _screenChangeCheckTimer = 0f;

        if (_lastScreenWidth == Screen.width && _lastScreenHeight == Screen.height)
            return;

        _lastScreenWidth = Screen.width;
        _lastScreenHeight = Screen.height;
        RequestNormalizeCanvasScalerArray();
    }

    /// <summary>
    /// 빌드 환경에서만 적용할지 판단합니다.
    /// 감독이 Editor에서 배치한 리허설 상태를 방해하지 않는 것이 이 컴포넌트의 첫 번째 원칙입니다.
    /// </summary>
    private bool CanApplyRuntimeSync()
    {
        return !_isApplyOnlyInBuild || !Application.isEditor;
    }

    /// <summary>
    /// 커서, 타임스케일, 프레임 기준, CanvasScaler를 정리합니다.
    /// Game View에서는 보통 이미 맞아 있지만, 빌드에서는 이전 UI/컷신 상태가 남으면 진행이 멈출 수 있어 초기화합니다.
    /// </summary>
    private void RequestPrepareRuntimeView()
    {
        Application.targetFrameRate = 60;
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        _lastScreenWidth = Screen.width;
        _lastScreenHeight = Screen.height;
        RequestNormalizeCanvasScalerArray();
    }

    /// <summary>
    /// Unity Player가 창을 만든 직후 한 프레임 기다렸다가 해상도를 맞춥니다.
    /// 영화로 치면 영사기가 켜진 직후 화면비를 다시 잡는 단계입니다.
    /// </summary>
    private IEnumerator RequestApplyResolutionAfterFirstFrameRoutine()
    {
        yield return null;

        if (!_isForceResolutionInBuild || Application.isEditor)
            yield break;

        if (Screen.width == _targetWidth &&
            Screen.height == _targetHeight &&
            Screen.fullScreenMode == _targetFullScreenMode)
            yield break;

        Screen.SetResolution(_targetWidth, _targetHeight, _targetFullScreenMode);
        Debug.Log($"[OOTechBuildViewSyncController] Build resolution synced to {_targetWidth}x{_targetHeight} {_targetFullScreenMode}.");
    }

    /// <summary>
    /// 씬에 있는 모든 CanvasScaler를 1920x1080 기준으로 맞춥니다.
    /// 각 UI의 위치를 직접 바꾸지 않고, 같은 무대 기준 좌표계만 통일합니다.
    /// </summary>
    private void RequestNormalizeCanvasScalerArray()
    {
        List<CanvasScaler> canvasScalerArray = OOTechSceneQuery.RequestCollectComponents<CanvasScaler>(true);

        foreach (CanvasScaler canvasScaler in canvasScalerArray)
        {
            if (canvasScaler == null)
                continue;

            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = _referenceResolution;
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = _matchWidthOrHeight;
        }
    }
}
