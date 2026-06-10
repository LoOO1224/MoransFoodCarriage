// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OOTechStageViewController.cs
// - 역할: 월드맵, 도로, 스테이지 진입, HUD 흐름을 연결합니다.
// - 유지보수: UIManager 전환과 그룹 활성/비활성 순서가 게임 진행을 결정하므로 호출 순서를 유지합니다.
// =============================================================================
using UnityEngine;

[DisallowMultipleComponent]
public class OOTechStageViewController : MonoBehaviour
{
    [Header("View")]
    [SerializeField] private string _backgroundObjectName = "Stage4_1Background";
    [SerializeField] private bool _isShowHUD = true;
    [SerializeField] private int _backgroundSortingOrder = -1000;

    /// <summary>
    /// StageGroup??耳쒖쭏 ????placeholder瑜??④린怨??ㅼ젣 諛곌꼍 ?꾩껜瑜?移대찓?쇱뿉 留욎땅?덈떎.
    /// </summary>
    private void OnEnable()
    {
        RequestHidePlaceholderView();
        RequestPrepareBackground();
        RequestPrepareHUD();
    }

    /// <summary>
    /// StageGroup??爰쇱쭏 ??FinalStage泥섎읆 HUD媛 ?꾩슂 ?녿뒗 怨녹? HUD瑜??뺣━?⑸땲??
    /// </summary>
    private void OnDisable()
    {
        if (_isShowHUD)
            return;

        OOTechRoadHUDController hudController = GetComponent<OOTechRoadHUDController>();

        if (hudController != null)
            hudController.SetHUDVisible(false);
    }

    /// <summary>
    /// 諛곗튂紐⑤뱶?먯꽌 媛?StageGroup ?대쫫??留욌뒗 諛곌꼍怨?HUD 洹쒖튃??二쇱엯?????ъ슜?⑸땲??
    /// </summary>
    public void Configure(string backgroundObjectName, bool isShowHUD)
    {
        if (!string.IsNullOrWhiteSpace(backgroundObjectName))
            _backgroundObjectName = backgroundObjectName;

        _isShowHUD = isShowHUD;
    }

    private void RequestHidePlaceholderView()
    {
        Transform placeholderCanvas = RequestChildObjectByName(transform, "Canvas_StagePlaceholder");

        if (placeholderCanvas != null)
            placeholderCanvas.gameObject.SetActive(false);

        Transform whiteBackground = RequestChildObjectByName(transform, "Image_WhiteBackground");

        if (whiteBackground != null)
            whiteBackground.gameObject.SetActive(false);
    }

    private void RequestPrepareBackground()
    {
        GameObject backgroundObject = ResolveBackgroundObject();

        if (backgroundObject == null)
            return;

        backgroundObject.SetActive(true);

        SpriteRenderer backgroundRenderer = backgroundObject.GetComponent<SpriteRenderer>();

        if (backgroundRenderer == null || backgroundRenderer.sprite == null)
            return;

        backgroundRenderer.enabled = true;
        backgroundRenderer.sortingLayerName = "Background";
        backgroundRenderer.sortingOrder = _backgroundSortingOrder;

        Color color = backgroundRenderer.color;

        if (color.a <= 0.01f)
        {
            color.a = 1f;
            backgroundRenderer.color = color;
        }

        FitCameraToBackground(backgroundRenderer);
    }

    private void RequestPrepareHUD()
    {
        OOTechRoadHUDController hudController = GetComponent<OOTechRoadHUDController>();

        if (hudController == null)
            return;

        hudController.SetOwnerGroupName(gameObject.name);
        hudController.PrepareHUD();
        hudController.SetCookingUnlocked(true);
        hudController.SetHUDVisible(_isShowHUD);
    }

    private GameObject ResolveBackgroundObject()
    {
        Transform backgroundTransform = RequestChildObjectByName(transform, _backgroundObjectName);

        if (backgroundTransform != null)
            return backgroundTransform.gameObject;

        SpriteRenderer[] rendererArray = GetComponentsInChildren<SpriteRenderer>(true);
        SpriteRenderer bestRenderer = null;
        float bestArea = 0f;

        foreach (SpriteRenderer spriteRenderer in rendererArray)
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null)
                continue;

            string objectName = spriteRenderer.gameObject.name;

            if (!objectName.Contains("Background") && !objectName.Contains("Backound"))
                continue;

            float area = spriteRenderer.bounds.size.x * spriteRenderer.bounds.size.y;

            if (area <= bestArea)
                continue;

            bestRenderer = spriteRenderer;
            bestArea = area;
        }

        return bestRenderer != null ? bestRenderer.gameObject : null;
    }

    private void FitCameraToBackground(SpriteRenderer backgroundRenderer)
    {
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        CameraFollowController cameraFollow = mainCamera.GetComponent<CameraFollowController>();

        if (cameraFollow != null)
            cameraFollow.enabled = false;

        mainCamera.orthographic = true;

        Bounds backgroundBounds = backgroundRenderer.bounds;
        float verticalSize = backgroundBounds.extents.y;
        float horizontalSize = backgroundBounds.extents.x / Mathf.Max(0.01f, mainCamera.aspect);

        mainCamera.orthographicSize = Mathf.Max(verticalSize, horizontalSize);

        Vector3 cameraPosition = mainCamera.transform.position;
        cameraPosition.x = backgroundBounds.center.x;
        cameraPosition.y = backgroundBounds.center.y;
        mainCamera.transform.position = cameraPosition;
    }

    private Transform RequestChildObjectByName(Transform rootTransform, string objectName)
    {
        if (rootTransform == null || string.IsNullOrWhiteSpace(objectName))
            return null;

        if (rootTransform.name == objectName)
            return rootTransform;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            Transform foundTransform = RequestChildObjectByName(rootTransform.GetChild(index), objectName);

            if (foundTransform != null)
                return foundTransform;
        }

        return null;
    }
}

