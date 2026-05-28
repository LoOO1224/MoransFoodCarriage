using UnityEngine;

/// <summary>
/// Senario1Controller
/// 
/// Senario1Group의 전체 흐름을 관리합니다.
/// Jaeik 등장 시 카메라 타겟을 Jaeik으로 전환합니다.
/// </summary>
public class Senario1Controller : MonoBehaviour
{
    [Header("Character Reference")]
    [SerializeField] private Transform _jaeikTransform;

    [Header("Camera Reference")]
    [SerializeField] private CameraFollowController _cameraFollowController;

    private void OnEnable()
    {
        Debug.Log("[Senario1Controller] Senario1Group 시작 - Jaeik으로 카메라 타겟 변경");

        if (_cameraFollowController != null && _jaeikTransform != null)
        {
            _cameraFollowController.SetTarget(_jaeikTransform);
        }
        else
        {
            Debug.LogWarning("[Senario1Controller] CameraFollowController 또는 Jaeik 참조가 없습니다.");
        }
    }
}