using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Directs the first road trip from the starting point to the Stage 1 entrance.
/// The map image is the stage backdrop. MFC is the actor and can travel only on the stone-road lane.
/// </summary>
public class OOTechRoadToStage1Controller : MonoBehaviour
{
    private const string _roleMFC = "MFC";
    private const string _roleStartPointMap = "StartPointMap";
    private const string _roleRoadMap1 = "RoadMap1";
    private const string _roleRoadMap2 = "RoadMap2";
    private const string _roleStage1EntryMap = "Stage1EntryMap";

    [Header("Scene Components")]
    [SerializeField] private OOTechSceneContext Context_Scene;
    [SerializeField] private OOTechRoadHUDController HUD_Road;

    [HideInInspector]
    [SerializeField] private GameObject Object_MFC;
    [HideInInspector]
    [SerializeField] private Animator Animator_MFC;
    [HideInInspector]
    [SerializeField] private SpriteRenderer Renderer_MFC;

    [HideInInspector]
    [SerializeField] private Camera Camera_Main;
    [HideInInspector]
    [SerializeField] private CameraFollowController Camera_Follow;

    [HideInInspector]
    [SerializeField] private GameObject Object_StartPointMap;
    [HideInInspector]
    [SerializeField] private GameObject Object_RoadMap1;
    [HideInInspector]
    [SerializeField] private GameObject Object_RoadMap2;
    [HideInInspector]
    [SerializeField] private GameObject Object_Stage1EntryMap;

    [Header("Road Lane")]
    [SerializeField] private KeyCode _moveRightKey = KeyCode.D;
    [SerializeField] private float _moveSpeed = 4.2f;
    [SerializeField, Range(0f, 1f)] private float _roadLaneNormalizedHeight = 0.18f;
    [SerializeField, Range(0f, 0.45f)] private float _entryMarginRatio = 0.1f;
    [SerializeField, Range(0f, 0.45f)] private float _exitMarginRatio = 0.08f;

    [Header("Map Transition")]
    [SerializeField] private float _fadeOutSeconds = 0.45f;
    [SerializeField] private float _blackoutHoldSeconds = 0.15f;
    [SerializeField] private float _fadeInSeconds = 0.55f;
    [SerializeField] private Color _fadeColor = Color.black;

    [Header("Destination")]
    [SerializeField] private string _currentGroupName = "1st_Road_to_Stage1";
    [SerializeField] private string _targetStageGroupName = "Stage1Group";
    [SerializeField] private string[] _stageGroupNameArray =
    {
        "Stage1Group",
        "Stage2Group",
        "Stage3Group",
        "Stage4Group",
        "FinalStageGroup"
    };

    private GameObject[] _mapObjectArray;
    private int _currentMapIndex;
    private bool _isChangingMap;
    private bool _isRoadTripComplete;
    private Canvas _fadeCanvas;
    private Image Image_FadeOverlay;

    private void Awake()
    {
        ResolveSceneReferences();
        ResolveCameraReference();
        CacheHUDReference();
        CreateFadeOverlayIfNeeded();
    }

    public void ConfigureRoadFlow(string currentGroupName, string targetStageGroupName)
    {
        if (!string.IsNullOrEmpty(currentGroupName))
            _currentGroupName = currentGroupName;

        if (!string.IsNullOrEmpty(targetStageGroupName))
            _targetStageGroupName = targetStageGroupName;

        if (HUD_Road != null)
            HUD_Road.SetOwnerGroupName(_currentGroupName);
    }

    private void OnEnable()
    {
        ResolveSceneReferences();
        ResolveCameraReference();
        CacheHUDReference();
        PrepareRoadHUD();
        PrepareRoadTrip();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        _isChangingMap = false;
        SetRoadHUDVisible(false);
        HideFadeOverlay();
    }

    private void Update()
    {
        if (_isChangingMap || _isRoadTripComplete || Object_MFC == null)
            return;

        if (Input.GetKey(_moveRightKey))
            MoveMFCRight();
    }

    /// <summary>
    /// Finds the actor and backdrops by their scene names.
    /// Serialized inspector references still take priority, so later map expansion stays easy to maintain.
    /// </summary>
    public void ResolveSceneReferences()
    {
        CacheSceneContextReference();

        Object_MFC = ResolveChild(Object_MFC, "MFC");
        Animator_MFC = ResolveComponent(Animator_MFC, Object_MFC);
        Renderer_MFC = ResolveComponent(Renderer_MFC, Object_MFC);

        Object_StartPointMap = ResolveChild(Object_StartPointMap, "StartPointMap", "RoadMap1");
        Object_RoadMap1 = ResolveChild(Object_RoadMap1, "RoadMap1", "RoadMap2");
        Object_RoadMap2 = ResolveChild(Object_RoadMap2, "RoadMap2", "RoadMap3");
        Object_Stage1EntryMap = ResolveChild(Object_Stage1EntryMap, "Stage1EntryMap", "Stage1_Entry");

        _mapObjectArray = new[]
        {
            Object_StartPointMap,
            Object_RoadMap1,
            Object_RoadMap2,
            Object_Stage1EntryMap
        };
    }

    /// <summary>
    /// Reads reusable role components before the legacy name fallback.
    /// This keeps the road director focused on travel rules instead of a list
    /// of concrete scene object slots.
    /// </summary>
    private void CacheSceneContextReference()
    {
        if (Context_Scene == null)
            Context_Scene = GetComponent<OOTechSceneContext>();

        if (Context_Scene == null)
            return;

        Context_Scene.CacheSceneObjects();
        Object_MFC = ResolveRoleObject(_roleMFC, Object_MFC);
        Object_StartPointMap = ResolveRoleObject(_roleStartPointMap, Object_StartPointMap);
        Object_RoadMap1 = ResolveRoleObject(_roleRoadMap1, Object_RoadMap1);
        Object_RoadMap2 = ResolveRoleObject(_roleRoadMap2, Object_RoadMap2);
        Object_Stage1EntryMap = ResolveRoleObject(_roleStage1EntryMap, Object_Stage1EntryMap);
    }

    private GameObject ResolveRoleObject(string roleId, GameObject fallback)
    {
        GameObject roleObject = Context_Scene.GetRoleObject(roleId);
        return roleObject != null ? roleObject : fallback;
    }

    private void PrepareRoadTrip()
    {
        if (_mapObjectArray == null || _mapObjectArray.Length == 0)
            ResolveSceneReferences();

        _currentMapIndex = 0;
        _isChangingMap = false;
        _isRoadTripComplete = false;

        SetOnlyCurrentMapActive();
        SetMFCActive(true);
        PlaceMFCAtMapEntry();
        FocusCameraOnMFC(true);
        HideFadeOverlay();

        if (Animator_MFC != null)
            Animator_MFC.Play("MFC", 0, 0f);
    }

    private void MoveMFCRight()
    {
        SpriteRenderer mapRenderer = GetCurrentMapRenderer();

        if (mapRenderer == null)
            return;

        Vector3 position = Object_MFC.transform.position;
        position.x += _moveSpeed * Time.deltaTime;
        position.y = CalculateRoadLaneY(mapRenderer);
        position.x = Mathf.Min(position.x, CalculateExitX(mapRenderer));
        Object_MFC.transform.position = position;

        if (position.x >= CalculateExitX(mapRenderer))
            StartCoroutine(ChangeToNextMapRoutine());
    }

    private IEnumerator ChangeToNextMapRoutine()
    {
        if (_isChangingMap)
            yield break;

        _isChangingMap = true;
        yield return FadeOverlayRoutine(0f, 1f, _fadeOutSeconds);

        if (_blackoutHoldSeconds > 0f)
            yield return new WaitForSeconds(_blackoutHoldSeconds);

        if (_currentMapIndex >= _mapObjectArray.Length - 1)
        {
            _isRoadTripComplete = true;
            OpenTargetStageGroup();
            yield return FadeOverlayRoutine(1f, 0f, _fadeInSeconds);
            _isChangingMap = false;
            SetSceneGroupActive(_currentGroupName, false);
            Debug.Log("[OOTechRoadToStage1Controller] Stage 1 entrance reached.");
            yield break;
        }

        _currentMapIndex++;
        SetOnlyCurrentMapActive();
        PlaceMFCAtMapEntry();
        FocusCameraOnMFC(true);

        yield return FadeOverlayRoutine(1f, 0f, _fadeInSeconds);
        _isChangingMap = false;
    }

    private void SetOnlyCurrentMapActive()
    {
        if (_mapObjectArray == null)
            return;

        for (int index = 0; index < _mapObjectArray.Length; index++)
        {
            if (_mapObjectArray[index] != null)
                _mapObjectArray[index].SetActive(index == _currentMapIndex);
        }
    }

    private void PlaceMFCAtMapEntry()
    {
        SpriteRenderer mapRenderer = GetCurrentMapRenderer();

        if (Object_MFC == null || mapRenderer == null)
            return;

        Bounds bounds = mapRenderer.bounds;
        Vector3 position = Object_MFC.transform.position;
        position.x = bounds.min.x + (bounds.size.x * _entryMarginRatio);
        position.y = CalculateRoadLaneY(mapRenderer);
        Object_MFC.transform.position = position;
    }

    private float CalculateRoadLaneY(SpriteRenderer mapRenderer)
    {
        Bounds bounds = mapRenderer.bounds;
        return bounds.min.y + (bounds.size.y * _roadLaneNormalizedHeight);
    }

    private float CalculateExitX(SpriteRenderer mapRenderer)
    {
        Bounds bounds = mapRenderer.bounds;
        return bounds.max.x - (bounds.size.x * _exitMarginRatio);
    }

    private SpriteRenderer GetCurrentMapRenderer()
    {
        if (_mapObjectArray == null || _currentMapIndex < 0 || _currentMapIndex >= _mapObjectArray.Length)
            return null;

        GameObject currentMapObject = _mapObjectArray[_currentMapIndex];
        return currentMapObject != null ? currentMapObject.GetComponent<SpriteRenderer>() : null;
    }

    private void ResolveCameraReference()
    {
        if (Camera_Main == null)
            Camera_Main = Camera.main;

        if (Camera_Follow == null && Camera_Main != null)
            Camera_Main.TryGetComponent(out Camera_Follow);
    }

    private void FocusCameraOnMFC(bool isSnapImmediately)
    {
        ResolveCameraReference();

        if (Object_MFC == null)
            return;

        if (Camera_Follow != null)
        {
            Camera_Follow.enabled = true;
            Camera_Follow.SetTarget(Object_MFC.transform);
        }

        if (!isSnapImmediately || Camera_Main == null)
            return;

        Vector3 cameraPosition = Camera_Main.transform.position;
        cameraPosition.x = Object_MFC.transform.position.x;
        cameraPosition.y = Object_MFC.transform.position.y;
        Camera_Main.transform.position = cameraPosition;
    }

    private void SetMFCActive(bool isActive)
    {
        if (Object_MFC != null)
            Object_MFC.SetActive(isActive);
    }

    private void CreateFadeOverlayIfNeeded()
    {
        if (_fadeCanvas != null)
            return;

        GameObject canvasObject = new GameObject("RoadMapFadeCanvas");
        canvasObject.transform.SetParent(transform, false);

        _fadeCanvas = canvasObject.AddComponent<Canvas>();
        _fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _fadeCanvas.sortingOrder = 5000;

        CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasScaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject imageObject = new GameObject("Image_FadeOverlay");
        imageObject.transform.SetParent(canvasObject.transform, false);

        RectTransform rectTransform = imageObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        Image_FadeOverlay = imageObject.AddComponent<Image>();
        Image_FadeOverlay.raycastTarget = false;
        SetFadeOverlayAlpha(0f);
    }

    private IEnumerator FadeOverlayRoutine(float fromAlpha, float toAlpha, float duration)
    {
        CreateFadeOverlayIfNeeded();
        _fadeCanvas.gameObject.SetActive(true);

        if (duration <= 0f)
        {
            SetFadeOverlayAlpha(toAlpha);
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            SetFadeOverlayAlpha(Mathf.Lerp(fromAlpha, toAlpha, elapsedTime / duration));
            yield return null;
        }

        SetFadeOverlayAlpha(toAlpha);

        if (toAlpha <= 0f)
            _fadeCanvas.gameObject.SetActive(false);
    }

    private void SetFadeOverlayAlpha(float alpha)
    {
        if (Image_FadeOverlay == null)
            return;

        Color color = _fadeColor;
        color.a = Mathf.Clamp01(alpha);
        Image_FadeOverlay.color = color;
    }

    private void HideFadeOverlay()
    {
        if (Image_FadeOverlay != null)
            SetFadeOverlayAlpha(0f);

        if (_fadeCanvas != null)
            _fadeCanvas.gameObject.SetActive(false);
    }

    private void CacheHUDReference()
    {
        if (HUD_Road == null)
            HUD_Road = GetComponent<OOTechRoadHUDController>();

        if (HUD_Road == null)
            HUD_Road = gameObject.AddComponent<OOTechRoadHUDController>();
    }

    private void PrepareRoadHUD()
    {
        if (HUD_Road == null)
            return;

        HUD_Road.SetOwnerGroupName(_currentGroupName);
        HUD_Road.PrepareHUD();
        HUD_Road.SetHUDVisible(true);
    }

    private void SetRoadHUDVisible(bool isVisible)
    {
        if (HUD_Road != null)
            HUD_Road.SetHUDVisible(isVisible);
    }

    private void OpenTargetStageGroup()
    {
        if (_stageGroupNameArray != null)
        {
            foreach (string stageGroupName in _stageGroupNameArray)
            {
                if (string.IsNullOrEmpty(stageGroupName))
                    continue;

                SetSceneGroupActive(stageGroupName, stageGroupName == _targetStageGroupName);
            }
        }

        SetSceneGroupActive(_targetStageGroupName, true);
    }

    private bool SetSceneGroupActive(string groupName, bool isActive)
    {
        if (string.IsNullOrEmpty(groupName))
            return false;

        GameObject groupObject = FindSceneObjectByName(groupName);

        if (OOTechUIManager.Inst != null && groupObject != null)
        {
            OOTechUIManager.Inst.RegisterUI(groupName, groupObject);
            bool isChangedByUIManager = isActive ? OOTechUIManager.Inst.OpenUI(groupName) : OOTechUIManager.Inst.CloseUI(groupName);

            if (isChangedByUIManager)
                return true;
        }

        if (groupObject == null)
        {
            if (isActive)
                Debug.LogWarning($"[OOTechRoadToStage1Controller] Scene group not found: {groupName}");

            return false;
        }

        groupObject.SetActive(isActive);
        return true;
    }

    private GameObject FindSceneObjectByName(string objectName)
    {
        Scene scene = SceneManager.GetActiveScene();

        if (!scene.IsValid())
            return null;

        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            GameObject foundObject = FindChildByName(rootObject.transform, objectName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }

    private GameObject FindChildByName(Transform rootTransform, string objectName)
    {
        if (rootTransform == null)
            return null;

        if (rootTransform.name == objectName)
            return rootTransform.gameObject;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            GameObject foundObject = FindChildByName(rootTransform.GetChild(index), objectName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }

    private GameObject ResolveChild(GameObject assignedObject, params string[] childNameArray)
    {
        if (assignedObject != null)
            return assignedObject;

        foreach (string childName in childNameArray)
        {
            Transform childTransform = transform.Find(childName);

            if (childTransform != null)
                return childTransform.gameObject;
        }

        return null;
    }

    private T ResolveComponent<T>(T assignedComponent, GameObject targetObject) where T : Component
    {
        if (assignedComponent != null)
            return assignedComponent;

        return targetObject != null ? targetObject.GetComponent<T>() : null;
    }
}
