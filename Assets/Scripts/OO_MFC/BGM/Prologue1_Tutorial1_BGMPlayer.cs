// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: Prologue1_Tutorial1_BGMPlayer.cs
// - 역할: 특정 그룹 또는 장면에서 BGM을 재생하는 음향 큐 스크립트입니다.
// - 감독 관점: 장면이 켜질 때 어떤 음악을 틀지 알려 주는 음향 스태프입니다.
// - 유지보수 포인트: 사운드 전환 규칙이 커지면 SoundManager로 옮기고, 이 스크립트는 AudioClip 참조와 재생 요청만 유지합니다.
// =============================================================================
using UnityEngine;

/// <summary>
/// Prologue1Group과 Tutorial1Group에서 함께 쓰는 BGM을 재생합니다.
/// 두 그룹은 같은 정서의 무대라서 하나의 음악 큐로 묶어 둡니다.
/// </summary>
public class Prologue1_Tutorial1_BGMPlayer : MonoBehaviour
{
    [Header("BGM Settings")]
    [SerializeField] private AudioClip _prologue1Tutorial1BGM;

    /// <summary>
    /// 해당 그룹이 켜지는 순간 음악 감독에게 반복 재생을 요청합니다.
    /// </summary>
    private void OnEnable()
    {
        if (OOTechSoundManager.Inst != null && _prologue1Tutorial1BGM != null)
        {
            OOTechSoundManager.Inst.PlayBGM(_prologue1Tutorial1BGM, loop: true);
            Debug.Log("[Prologue1_Tutorial1_BGMPlayer] Prologue1 + Tutorial1 BGM 재생 시작");
        }
    }

    /// <summary>
    /// 무대가 닫히면 다음 그룹의 BGM이 깨끗하게 들어오도록 현재 음악을 정리합니다.
    /// </summary>
    private void OnDisable()
    {
        if (OOTechSoundManager.Inst != null)
        {
            OOTechSoundManager.Inst.StopBGM();
            Debug.Log("[Prologue1_Tutorial1_BGMPlayer] BGM 정지");
        }
    }
}
