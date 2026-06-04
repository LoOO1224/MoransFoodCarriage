// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: Prologue2_BGMPlayer.cs
// - 역할: 특정 그룹 또는 장면에서 BGM을 재생하는 음향 큐 스크립트입니다.
// - 감독 관점: 장면이 켜질 때 어떤 음악을 틀지 알려 주는 음향 스태프입니다.
// - 유지보수 포인트: 사운드 전환 규칙이 커지면 SoundManager로 옮기고, 이 스크립트는 AudioClip 참조와 재생 요청만 유지합니다.
// =============================================================================
using UnityEngine;

/// <summary>
/// Prologue2Group 전용 BGM을 재생합니다.
/// Game View에서는 두 번째 프롤로그의 분위기를 여는 음향 큐입니다.
/// </summary>
public class Prologue2_BGMPlayer : MonoBehaviour
{
    [Header("BGM Settings")]
    [SerializeField] private AudioClip _prologue2BGM;

    /// <summary>
    /// Prologue2Group이 켜지면 전용 BGM을 반복 재생합니다.
    /// </summary>
    private void OnEnable()
    {
        if (OOTechSoundManager.Inst != null && _prologue2BGM != null)
        {
            OOTechSoundManager.Inst.PlayBGM(_prologue2BGM, loop: true);
            Debug.Log("[Prologue2_BGMPlayer] Prologue2 BGM 재생 시작");
        }
    }

    /// <summary>
    /// Prologue2Group이 닫히면 음악을 정지합니다.
    /// </summary>
    private void OnDisable()
    {
        if (OOTechSoundManager.Inst != null)
        {
            OOTechSoundManager.Inst.StopBGM();
            Debug.Log("[Prologue2_BGMPlayer] BGM 정지");
        }
    }
}
