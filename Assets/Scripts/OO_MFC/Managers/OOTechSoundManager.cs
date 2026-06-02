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
