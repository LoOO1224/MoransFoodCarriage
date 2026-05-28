using UnityEngine;

/// <summary>
/// 카메라가 지정된 타겟을 따라가도록 관리하는 컨트롤러입니다.
/// Tutorial1Group에서는 장영심 Transform을 직접 참조하여 따라가며, 별도의 검색 함수는 사용하지 않습니다.
/// </summary>
public class CameraFollowController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform _target;

    [Header("Follow Setting")]
    [SerializeField] private float _smoothSpeed = 8f;

    [Header("Camera Boundary")]
    [SerializeField] private float _minX = -14.3f;
    [SerializeField] private float _maxX = 20f;
    [SerializeField] private float _minY = -8f;
    [SerializeField] private float _maxY = 15f;

    // ==================== 카메라 추적 ====================

    private void LateUpdate()
    {
        UpdateCameraPosition();
    }

    /// <summary>
    /// 외부에서 카메라 추적 대상을 직접 지정할 때 사용합니다.
    /// </summary>
    public void SetTarget(Transform target)
    {
        _target = target;
    }

    /// <summary>
    /// 타겟 위치를 기준으로 카메라를 부드럽게 이동시키고, 지정된 범위 밖으로 나가지 않도록 제한합니다.
    /// </summary>
    private void UpdateCameraPosition()
    {
        if (_target == null)
            return;

        Vector3 targetPosition = new Vector3(_target.position.x, _target.position.y, transform.position.z);
        targetPosition.x = Mathf.Clamp(targetPosition.x, _minX, _maxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, _minY, _maxY);

        transform.position = Vector3.Lerp(transform.position, targetPosition, _smoothSpeed * Time.deltaTime);
    }
}
