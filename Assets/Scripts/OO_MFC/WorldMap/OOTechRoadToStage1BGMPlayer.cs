// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OOTechRoadToStage1BGMPlayer.cs
// - 역할: 월드맵, 도로, 스테이지 진입, HUD 흐름을 연결합니다.
// - 유지보수: UIManager 전환과 그룹 활성/비활성 순서가 게임 진행을 결정하므로 호출 순서를 유지합니다.
// =============================================================================
using UnityEngine;

/// <summary>
/// RoadGroup이 켜져 있는 동안 월드맵 이동 BGM을 재생합니다.
/// Game View에서는 마차가 길을 달리는 장면의 배경음악 큐입니다.
/// </summary>
public class OOTechRoadToStage1BGMPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip _worldMapRoadBGM;
    [SerializeField] private string _worldMapRoadBGMAssetPath = "Assets/Sounds/BGM/WorldMap_Road_BGM.mp3";
    [SerializeField] private AudioClip _secondRoadStage2BGM;
    [SerializeField] private string _secondRoadStage2BGMAssetPath = "Assets/Sounds/BGM/2_Road__Stage2_BGM.mp3";
    [SerializeField] private string _secondRoadGroupName = "2nd_Road_to_Stage2";

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
        if (IsSecondRoadStage2Group())
            return;

        AudioClip bgmClip = ResolveClip();

        if (OOTechSoundManager.Inst != null && bgmClip != null)
            OOTechSoundManager.Inst.StopBGM(bgmClip);
    }

    /// <summary>
    /// 인스펙터 클립을 우선 사용하고, 에디터에서는 경로 기반 클립을 보조로 찾습니다.
    /// </summary>
    private AudioClip ResolveClip()
    {
        if (IsSecondRoadStage2Group())
            return ResolveSecondRoadStage2Clip();

        return OOTechAudioClipResolver.Resolve(_worldMapRoadBGM, "Audio/BGM/WorldMap_Road_BGM", _worldMapRoadBGMAssetPath);
    }

    /// <summary>
    /// 2nd Road는 Stage2까지 같은 BGM을 이어 씁니다.
    /// 영화로 치면 길 장면에서 시작한 음악을 다음 무대 첫 장면까지 자연스럽게 물고 가는 큐입니다.
    /// </summary>
    private bool IsSecondRoadStage2Group()
    {
        return !string.IsNullOrEmpty(_secondRoadGroupName) && gameObject.name == _secondRoadGroupName;
    }

    private AudioClip ResolveSecondRoadStage2Clip()
    {
        return OOTechAudioClipResolver.Resolve(_secondRoadStage2BGM, "Audio/BGM/2_Road__Stage2_BGM", _secondRoadStage2BGMAssetPath);
    }
}
