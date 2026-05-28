using UnityEngine;

/// <summary>
/// Senario1_BGMPlayer
/// 
/// Senario1Group에서 사용되는 전용 BGM을 관리합니다.
/// 그룹이 활성화되는 동안 BGM을 재생하고, 그룹이 비활성화되면 자동으로 정지합니다.
/// </summary>
public class Senario1_BGMPlayer : MonoBehaviour
{
    [Header("BGM Settings")]
    [SerializeField] private AudioClip _senario1BGM;

    private void OnEnable()
    {
        if (OOTechSoundManager.Inst != null && _senario1BGM != null)
        {
            OOTechSoundManager.Inst.PlayBGM(_senario1BGM, loop: true);
            Debug.Log("[Senario1_BGMPlayer] Senario1Group BGM 재생 시작");
        }
    }

    private void OnDisable()
    {
        if (OOTechSoundManager.Inst != null)
        {
            OOTechSoundManager.Inst.StopBGM();
            Debug.Log("[Senario1_BGMPlayer] BGM 정지");
        }
    }
}