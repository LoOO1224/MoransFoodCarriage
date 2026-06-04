// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechRoadToStage1BGMPlayer.cs
// - 역할: 로드맵, 월드맵, 스테이지 전환 흐름을 담당하는 장면 Controller입니다.
// - 감독 관점: 길 위의 장면 전환 큐시트를 들고 있는 무대감독입니다.
// - 유지보수 포인트: 배경/버튼/캐릭터 배치는 오브젝트와 View가 맡고, 이 스크립트는 순서 지휘만 맡아야 합니다.
// =============================================================================
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

/// <summary>
/// RoadGroup이 켜져 있는 동안 월드맵 이동 BGM을 재생합니다.
/// Game View에서는 마차가 길을 달리는 장면의 배경음악 큐입니다.
/// </summary>
public class OOTechRoadToStage1BGMPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip _worldMapRoadBGM;
    [SerializeField] private string _worldMapRoadBGMAssetPath = "Assets/Sounds/BGM/WorldMap_Road_BGM.mp3";

    /// <summary>
    /// RoadGroup이 열리면 이동 BGM을 반복 재생합니다.
    /// </summary>
    private void OnEnable()
    {
        AudioClip bgmClip = ResolveClip();

        if (OOTechSoundManager.Inst != null && bgmClip != null)
            OOTechSoundManager.Inst.PlayBGM(bgmClip, true);
    }

    /// <summary>
    /// RoadGroup이 닫히면 다음 무대 음악과 겹치지 않도록 정리합니다.
    /// </summary>
    private void OnDisable()
    {
        if (OOTechSoundManager.Inst != null)
            OOTechSoundManager.Inst.StopBGM();
    }

    /// <summary>
    /// 인스펙터 클립을 우선 사용하고, 에디터에서는 경로 기반 클립을 보조로 찾습니다.
    /// </summary>
    private AudioClip ResolveClip()
    {
        if (_worldMapRoadBGM != null)
            return _worldMapRoadBGM;

#if UNITY_EDITOR
        if (!string.IsNullOrEmpty(_worldMapRoadBGMAssetPath))
            return AssetDatabase.LoadAssetAtPath<AudioClip>(_worldMapRoadBGMAssetPath);
#endif

        return null;
    }
}
