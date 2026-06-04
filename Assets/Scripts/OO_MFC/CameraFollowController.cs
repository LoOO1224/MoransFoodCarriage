// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: CameraFollowController.cs
// - 역할: 카메라가 지정된 배우를 따라가도록 하는 카메라 컴포넌트입니다.
// - 감독 관점: 촬영감독이 현재 말하는 배우를 프레임 안에 잡도록 돕습니다.
// - 유지보수 포인트: 장면마다 카메라를 강제로 움직이는 코드는 최소화하고, SetTarget 같은 명확한 요청만 사용합니다.
// =============================================================================
using UnityEngine;

/// <summary>
/// CameraFollowController
/// 
/// 지정된 대상을 부드럽게 따라가는 카메라 컨트롤러입니다.
/// Senario1Group에서는 Jaeik을 시작으로, 이후 새로운 캐릭터로 대상을 전환할 수 있도록 설계되었습니다.
/// </summary>
public class CameraFollowController : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform _target;           // 현재 따라갈 대상
    [SerializeField] private float _smoothSpeed = 8f;     // 부드러운 따라가기 속도

    [Header("Camera Boundary")]
    [SerializeField] private float _minX = -14.3f;
    [SerializeField] private float _maxX = 20f;
    [SerializeField] private float _minY = -8f;
    [SerializeField] private float _maxY = 15f;

    /// <summary>
    /// 모든 배우 이동이 끝난 뒤 카메라 위치를 따라가게 해 흔들림을 줄입니다.
    /// </summary>
    private void LateUpdate()
    {
        UpdateCameraPosition();
    }

    /// <summary>
    /// 카메라가 따라갈 대상을 변경합니다.
    /// (Jaeik → 다른 캐릭터로 전환할 때 사용)
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        if (newTarget == null)
        {
            Debug.LogWarning("[CameraFollowController] 팔로우 대상이 비어 있어 카메라 타겟을 변경하지 않았습니다.");
            return;
        }

        _target = newTarget;
        Debug.Log($"[CameraFollowController] 팔로우 대상 변경 → {newTarget.name}");
    }

    /// <summary>
    /// 현재 타겟을 따라 카메라를 부드럽게 이동하고, 지정된 경계 안에 묶습니다.
    /// </summary>
    private void UpdateCameraPosition()
    {
        if (_target == null) return;

        Vector3 targetPosition = new Vector3(_target.position.x, _target.position.y, transform.position.z);

        // 경계 제한
        targetPosition.x = Mathf.Clamp(targetPosition.x, _minX, _maxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, _minY, _maxY);

        // 부드럽게 이동
        transform.position = Vector3.Lerp(transform.position, targetPosition, _smoothSpeed * Time.deltaTime);
    }
}
