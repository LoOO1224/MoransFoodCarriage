// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechUILayoutNormalizer.cs
// - 역할: UI 표시와 입력 연결을 담당하는 UI 컴포넌트입니다.
// - 감독 관점: 관객에게 보이는 패널과 버튼의 무대 동선을 담당합니다.
// - 유지보수 포인트: 사용자가 직접 편집할 UI는 하이어라키/프리팹에 두고, 코드에서 즉석 생성하지 않습니다.
// =============================================================================
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// OO_MFC 씬의 Screen Space UI가 같은 기준 해상도를 사용하도록 정리합니다.
/// 영화 세트의 여러 소품 팀이 서로 다른 줄자를 가져왔을 때,
/// 촬영 직전에 무대 감독이 하나의 기준 줄자로 다시 맞추는 역할입니다.
/// </summary>
public static class OOTechUILayoutNormalizer
{
    private const string _targetSceneName = "OO_MFC";
    private static readonly Vector2 _referenceResolution = new Vector2(1920f, 1080f);

    /// <summary>
    /// 씬이 열린 직후 현재 OO_MFC UI를 정리하고, 이후 씬 전환에도 같은 규칙을 적용합니다.
    /// 강사님 샘플 씬에는 영향을 주지 않도록 OO_MFC 씬 이름을 확인한 뒤 실행합니다.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RegisterSceneLoaded()
    {
        // UI size and placement are now directed by the user in the Unity editor.
        // This normalizer stays silent so it cannot resize a canvas behind the director's back.
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        NormalizeSceneUI(scene);
    }

    private static bool NormalizeSceneUI(Scene scene)
    {
        if (!scene.IsValid() || scene.name != _targetSceneName)
            return false;

        int normalizedCanvasCount = 0;

        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            CanvasScaler[] canvasScalerArray = rootObject.GetComponentsInChildren<CanvasScaler>(true);

            foreach (CanvasScaler canvasScaler in canvasScalerArray)
            {
                if (!CanNormalizeCanvasScaler(canvasScaler))
                    continue;

                if (ApplyScreenSpaceLayout(canvasScaler))
                    normalizedCanvasCount++;
            }
        }

        Debug.Log($"[OOTechUILayoutNormalizer] OO_MFC UI 기준 해상도 정리 완료: {normalizedCanvasCount}");
        return normalizedCanvasCount > 0;
    }

    /// <summary>
    /// 월드 공간 UI는 배우와 함께 무대 위에 놓인 소품이므로 화면 UI 규칙에서 제외합니다.
    /// </summary>
    private static bool CanNormalizeCanvasScaler(CanvasScaler canvasScaler)
    {
        if (canvasScaler == null)
            return false;

        Canvas canvas = canvasScaler.GetComponent<Canvas>();
        return canvas != null && canvas.renderMode != RenderMode.WorldSpace;
    }

    private static bool ApplyScreenSpaceLayout(CanvasScaler canvasScaler)
    {
        if (canvasScaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize &&
            canvasScaler.referenceResolution == _referenceResolution &&
            canvasScaler.screenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight &&
            Mathf.Approximately(canvasScaler.matchWidthOrHeight, 0.5f))
        {
            return false;
        }

        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = _referenceResolution;
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 0.5f;
        return true;
    }

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
    private static void RegisterEditorSceneOpened()
    {
        // Editor-time auto layout is intentionally disabled.
        // The scene view and game view should show the user's saved UI staging, not a runtime rewrite.
    }

    private static void OnEditorSceneOpened(Scene scene, UnityEditor.SceneManagement.OpenSceneMode openSceneMode)
    {
        if (NormalizeSceneUI(scene))
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
    }

    [UnityEditor.MenuItem("Tools/OO MFC/Normalize Canvas Scalers Manually")]
    private static void NormalizeActiveEditorSceneUI()
    {
        if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        Scene scene = SceneManager.GetActiveScene();
        if (NormalizeSceneUI(scene))
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
    }
#endif
}
