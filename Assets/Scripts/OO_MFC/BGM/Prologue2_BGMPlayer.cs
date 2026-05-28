using UnityEngine;

/// <summary>
/// Prologue2_BGMPlayer
/// 
/// Prologue2Group 전용 BGM을 관리합니다.
/// </summary>
public class Prologue2_BGMPlayer : MonoBehaviour
{
    [Header("BGM Settings")]
    [SerializeField] private AudioClip _prologue2BGM;

    private void OnEnable()
    {
        if (OOTechSoundManager.Inst != null && _prologue2BGM != null)
        {
            OOTechSoundManager.Inst.PlayBGM(_prologue2BGM, loop: true);
            Debug.Log("[Prologue2_BGMPlayer] Prologue2 BGM 재생 시작");
        }
    }

    private void OnDisable()
    {
        if (OOTechSoundManager.Inst != null)
        {
            OOTechSoundManager.Inst.StopBGM();
            Debug.Log("[Prologue2_BGMPlayer] BGM 정지");
        }
    }
}