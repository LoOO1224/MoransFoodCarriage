// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: Prologue2_BGMPlayer.cs
// - 역할: 지정된 구간의 배경음악 재생을 SoundManager에 요청합니다.
// - 유지보수: 같은 BGM을 이어 재생해야 하는 구간에서는 중복 재시작 여부를 확인합니다.
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
