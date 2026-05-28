using UnityEngine;

/// <summary>
/// Prologue1_Tutorial1_BGMPlayer
/// 
/// Prologue1Group과 Tutorial1Group에서 사용되는 전용 BGM을 관리합니다.
/// Prologue2Group으로 넘어갈 때 자동으로 BGM이 정지됩니다.
/// </summary>
public class Prologue1_Tutorial1_BGMPlayer : MonoBehaviour
{
    [Header("BGM Settings")]
    [SerializeField] private AudioClip _prologue1Tutorial1BGM;

    private void OnEnable()
    {
        if (OOTechSoundManager.Inst != null && _prologue1Tutorial1BGM != null)
        {
            OOTechSoundManager.Inst.PlayBGM(_prologue1Tutorial1BGM, loop: true);
            Debug.Log("[Prologue1_Tutorial1_BGMPlayer] Prologue1 + Tutorial1 BGM 재생 시작");
        }
    }

    private void OnDisable()
    {
        if (OOTechSoundManager.Inst != null)
        {
            OOTechSoundManager.Inst.StopBGM();
            Debug.Log("[Prologue1_Tutorial1_BGMPlayer] BGM 정지");
        }
    }
}