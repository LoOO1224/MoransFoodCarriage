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

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float _bgmVolume = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float _sfxVolume = 1f;

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
        ApplyVolumeSettings();
    }

    /// <summary>
    /// 인스펙터에서 볼륨을 손보는 순간에도 실제 AudioSource 값을 맞춥니다.
    /// Game View에서는 모든 BGM이 이 중앙 믹서 값을 기준으로 재생됩니다.
    /// </summary>
    private void OnValidate()
    {
        ApplyVolumeSettings();
    }

    // ==================== BGM ====================
    /// <summary>
    /// 배경음악을 재생합니다. 같은 클립이 이미 재생 중이면 다시 시작하지 않습니다.
    /// </summary>
    public void PlayBGM(AudioClip bgmClip, bool loop = true)
    {
        if (bgmClip == null)
        {
            Debug.LogWarning("[OOTechSoundManager] PlayBGM failed. AudioClip is null.");
            return;
        }

        if (_bgmSource == null)
        {
            Debug.LogWarning("[OOTechSoundManager] PlayBGM failed. BGM AudioSource is missing.");
            return;
        }

        ApplyVolumeSettings();

        if (_bgmSource.clip != bgmClip)
        {
            _bgmSource.clip = bgmClip;
            _bgmSource.loop = loop;
            _bgmSource.Play();
            Debug.Log($"[OOTechSoundManager] BGM Play: {bgmClip.name}, Volume: {_bgmSource.volume:0.00}");
            return;
        }

        _bgmSource.loop = loop;

        if (!_bgmSource.isPlaying)
        {
            _bgmSource.Play();
            Debug.Log($"[OOTechSoundManager] BGM Restart: {bgmClip.name}, Volume: {_bgmSource.volume:0.00}");
        }
    }

    /// <summary>
    /// 현재 배경음악을 정지합니다.
    /// </summary>
    public void StopBGM()
    {
        if (_bgmSource == null)
            return;

        _bgmSource.Stop();
    }

    /// <summary>
    /// 요청한 클립이 현재 재생 중인 BGM일 때만 음악을 멈춥니다.
    /// 이전 무대가 퇴장하면서 다음 무대의 음악까지 꺼버리는 사고를 막기 위한 안전장치입니다.
    /// </summary>
    public void StopBGM(AudioClip bgmClip)
    {
        if (_bgmSource == null || bgmClip == null)
            return;

        if (_bgmSource.clip != bgmClip)
            return;

        StopBGM();
        Debug.Log($"[OOTechSoundManager] BGM Stop: {bgmClip.name}");
    }

    // ==================== SFX ====================
    /// <summary>
    /// 효과음을 한 번 재생합니다.
    /// </summary>
    public void PlaySFX(AudioClip sfxClip)
    {
        if (sfxClip == null || _sfxSource == null)
            return;

        ApplyVolumeSettings();
        _sfxSource.PlayOneShot(sfxClip);
    }

    /// <summary>
    /// 모든 소리 큐가 지나가는 공연장 믹서 볼륨을 한곳에서 통일합니다.
    /// 각 장면 배우는 AudioSource를 직접 만지지 않고 이 결과만 사용합니다.
    /// </summary>
    private void ApplyVolumeSettings()
    {
        if (_bgmSource != null)
            _bgmSource.volume = _bgmVolume;

        if (_sfxSource != null)
            _sfxSource.volume = _sfxVolume;
    }
}
