#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

/// <summary>
/// Plays the first road-trip BGM while the road group is active.
/// </summary>
public class OOTechRoadToStage1BGMPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip _worldMapRoadBGM;
    [SerializeField] private string _worldMapRoadBGMAssetPath = "Assets/Sounds/BGM/WorldMap_Road_BGM.mp3";

    private void OnEnable()
    {
        AudioClip bgmClip = ResolveClip();

        if (OOTechSoundManager.Inst != null && bgmClip != null)
            OOTechSoundManager.Inst.PlayBGM(bgmClip, true);
    }

    private void OnDisable()
    {
        if (OOTechSoundManager.Inst != null)
            OOTechSoundManager.Inst.StopBGM();
    }

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
