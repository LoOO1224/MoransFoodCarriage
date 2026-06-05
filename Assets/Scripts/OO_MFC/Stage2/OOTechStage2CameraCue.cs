// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechStage2CameraCue.cs
// - 역할: Stage2Group의 카메라 초점, 줌, CameraFollow 잠금/복구를 담당합니다.
// - 영화 비유: 촬영감독이 배우 클로즈업과 전체 무대 샷을 잡고, 무대감독은 "누구를 비춰"만 말합니다.
// - 유지보수 포인트: 카메라 이동 공식은 Controller에서 빼고, Stage2 전용 촬영 컴포넌트에 모읍니다.
// =============================================================================
using System.Collections;
using UnityEngine;

/// <summary>
/// Stage2 카메라 연출을 담당합니다.
/// Game View에서는 전체 배경 샷, LeeMongRyong 클로즈업, GreedyDuck 퇴장 추적을 이 컴포넌트가 수행합니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechStage2CameraCue : MonoBehaviour
{
    private Camera Camera_Main;
    private CameraFollowController Camera_Follow;
    private bool _hasSavedCameraFollowState;
    private bool _savedCameraFollowEnabled;
    private bool _hasSavedCameraViewState;
    private Vector3 _savedCameraPosition;
    private float _savedCameraOrthographicSize;
    private bool _savedCameraOrthographic;

    /// <summary>
    /// 현재 메인 카메라와 CameraFollowController를 찾습니다.
    /// </summary>
    public void RequestResolveCamera()
    {
        Camera_Main = Camera_Main != null ? Camera_Main : Camera.main;

        if (Camera_Main != null && Camera_Follow == null)
            Camera_Main.TryGetComponent(out Camera_Follow);
    }

    /// <summary>
    /// 자동 팔로우를 잠시 끄고, Stage2 연출용 수동 카메라로 전환합니다.
    /// </summary>
    public void RequestSaveAndDisableCameraFollow()
    {
        RequestResolveCamera();

        SaveCameraViewStateIfNeeded();

        if (Camera_Follow == null)
            return;

        if (!_hasSavedCameraFollowState)
        {
            _savedCameraFollowEnabled = Camera_Follow.enabled;
            _hasSavedCameraFollowState = true;
        }

        Camera_Follow.enabled = false;
    }

    /// <summary>
    /// Stage2가 끝나거나 꺼질 때 이전 CameraFollow 상태로 되돌립니다.
    /// </summary>
    public void RequestRestoreCameraFollow()
    {
        RequestResolveCamera();

        RestoreCameraViewStateIfNeeded();

        if (_hasSavedCameraFollowState && Camera_Follow != null)
        {
            Camera_Follow.enabled = _savedCameraFollowEnabled;
            _hasSavedCameraFollowState = false;
        }
    }

    /// <summary>
    /// 배경 전체가 Game View에 들어오도록 카메라를 맞춥니다.
    /// </summary>
    public void RequestFocusCameraOnBounds(Bounds bounds, float padding)
    {
        RequestResolveCamera();

        if (Camera_Main == null)
            return;

        Camera_Main.orthographic = true;
        Vector3 cameraPosition = bounds.center;
        cameraPosition.z = Camera_Main.transform.position.z;
        Camera_Main.transform.position = cameraPosition;
        float aspect = Mathf.Max(0.01f, Camera_Main.aspect);
        float sizeByHeight = bounds.extents.y;
        float sizeByWidth = bounds.extents.x / aspect;
        Camera_Main.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth) * Mathf.Max(1f, padding);
    }

    /// <summary>
    /// 특정 배우에게 카메라를 부드럽게 이동하고 줌합니다.
    /// </summary>
    public IEnumerator RequestFocusCameraOnActorRoutine(Transform targetTransform, float targetSize, float duration)
    {
        RequestResolveCamera();

        if (Camera_Main == null || targetTransform == null)
            yield break;

        Vector3 startPosition = Camera_Main.transform.position;
        Vector3 endPosition = targetTransform.position;
        endPosition.z = startPosition.z;
        float startSize = Camera_Main.orthographicSize;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / Mathf.Max(0.01f, duration));
            Camera_Main.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            Camera_Main.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
            yield return null;
        }
    }

    /// <summary>
    /// Stage2가 전체 배경 촬영을 위해 바꾼 카메라 값을 저장합니다.
    /// 영화로 치면 특수 촬영 렌즈를 끼기 전, 원래 렌즈와 삼각대 위치를 기록해 두는 단계입니다.
    /// </summary>
    private void SaveCameraViewStateIfNeeded()
    {
        if (_hasSavedCameraViewState || Camera_Main == null)
            return;

        _savedCameraPosition = Camera_Main.transform.position;
        _savedCameraOrthographicSize = Camera_Main.orthographicSize;
        _savedCameraOrthographic = Camera_Main.orthographic;
        _hasSavedCameraViewState = true;
    }

    /// <summary>
    /// Stage2가 끝나면 이전 그룹들이 쓰던 카메라 위치와 줌 값을 되돌립니다.
    /// 이 복구가 없으면 다음 무대가 Stage2의 광각 카메라로 찍혀 배경이 점처럼 작아집니다.
    /// </summary>
    private void RestoreCameraViewStateIfNeeded()
    {
        if (!_hasSavedCameraViewState || Camera_Main == null)
            return;

        Camera_Main.transform.position = _savedCameraPosition;
        Camera_Main.orthographicSize = _savedCameraOrthographicSize;
        Camera_Main.orthographic = _savedCameraOrthographic;
        _hasSavedCameraViewState = false;
    }
}
