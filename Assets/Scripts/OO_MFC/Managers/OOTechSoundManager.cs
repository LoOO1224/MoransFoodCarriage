using UnityEngine;

public class OOTechSoundManager : MonoBehaviour
{
    public static OOTechSoundManager Inst { get; private set; }

    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioSource _sfxSource;

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

    public void StopBGM()
    {
        _bgmSource.Stop();
    }

    // ==================== SFX ====================
    public void PlaySFX(AudioClip sfxClip)
    {
        if (sfxClip == null) return;
        _sfxSource.PlayOneShot(sfxClip);
    }
}