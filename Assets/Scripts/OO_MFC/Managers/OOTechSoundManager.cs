// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechSoundManager.cs
// - 역할: 여러 장면에서 함께 쓰는 공통 Manager입니다.
// - 감독 관점: 각 부서에 공통 창구를 열어 주는 제작 본부입니다.
// - 유지보수 포인트: 특정 장면의 세부 연출을 직접 처리하지 말고, 공통 조회/등록/요청 API만 유지합니다.
// =============================================================================
using UnityEngine;

/// <summary>
/// BGM과 효과음을 중앙에서 재생하는 음향 매니저입니다.
/// 각 장면 배우는 직접 AudioSource를 만지지 않고 이 매니저에게 음악 큐를 요청합니다.
/// </summary>
public class OOTechSoundManager : MonoBehaviour
{
    public static OOTechSoundManager Inst { get; private set; }

    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioSource _sfxSource;

    /// <summary>
    /// 씬 전환 후에도 유지되는 단일 음향 매니저로 등록합니다.
    /// </summary>
    private void Awake()
    {
        if (Inst != null && Inst != this)
        {
            Destroy(gameObject);
            return;
        }
        Inst = this;
        DontDestroyOnLoad(gameObject);
    }

    // ==================== BGM ====================
    /// <summary>
    /// 배경음악을 재생합니다. 같은 클립이 이미 재생 중이면 다시 시작하지 않습니다.
    /// </summary>
    public void PlayBGM(AudioClip bgmClip, bool loop = true)
    {
        if (bgmClip == null) return;

        if (_bgmSource.clip != bgmClip)
        {
            _bgmSource.clip = bgmClip;
            _bgmSource.loop = loop;
            _bgmSource.Play();
        }
    }

    /// <summary>
    /// 현재 배경음악을 정지합니다.
    /// </summary>
    public void StopBGM()
    {
        _bgmSource.Stop();
    }

    // ==================== SFX ====================
    /// <summary>
    /// 효과음을 한 번 재생합니다.
    /// </summary>
    public void PlaySFX(AudioClip sfxClip)
    {
        if (sfxClip == null) return;
        _sfxSource.PlayOneShot(sfxClip);
    }
}
