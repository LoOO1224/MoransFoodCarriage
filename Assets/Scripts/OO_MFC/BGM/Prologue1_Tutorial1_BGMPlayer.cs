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
