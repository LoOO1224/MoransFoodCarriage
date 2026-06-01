using UnityEngine;

public class DaniTechSoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource AudioSourcePlayer; // 효과음용
    [SerializeField] private AudioSource BGMSourcePlayer; // 배경음용

    public static DaniTechSoundManager Inst { get; set; }

    private void Awake()
    {
        Inst = this;
    }

    public string GetSoundPath(string soundDataId)
    {
        string path = soundDataId;
        // 여기서 데이터 매니저를 통해 사운드 Id로
        // 실제 사운드 데이터 경로를 받아오면 좋다
        return path;
    }

    // 효과음 재생 (겹쳐서 재생 가능)
    public void PlaySFX(string soundDataId)
    {

        DaniTechGameUtil.LoadAndPlayAudioClip(AudioSourcePlayer, soundDataId).Forget();
    }

    // 배경음 재생 (교체 재생)
    public void PlayBGM(string soundDataId)
    {
        DaniTechGameUtil.LoadAndPlayAudioClip(BGMSourcePlayer, soundDataId, isLoop:true).Forget();
    }

    public void StopBGM()
    {
        BGMSourcePlayer.Stop();
    }

    public void StopSFX()
    {
        AudioSourcePlayer.Stop();
    }

}
