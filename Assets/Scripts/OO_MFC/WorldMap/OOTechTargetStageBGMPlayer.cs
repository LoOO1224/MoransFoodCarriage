using UnityEngine;

/// <summary>
/// Plays a target-stage BGM when a road group opens a matching stage group.
/// This keeps audio resource fallback out of the road movement controller.
/// </summary>
[DisallowMultipleComponent]
public class OOTechTargetStageBGMPlayer : MonoBehaviour
{
    [SerializeField] private string[] _targetGroupNameArray =
    {
        "Stage4_1Group",
        "Stage4Group"
    };

    [SerializeField] private AudioClip _bgmClip;
    [SerializeField] private string _resourcePath = "Audio/BGM/Stage4_BGM";

    [SerializeField] private string _editorAssetPath = "Assets/Sounds/BGM/Stage4_BGM.mp3";

    public bool RequestPlayForTargetGroup(string targetGroupName)
    {
        if (!IsTargetGroup(targetGroupName))
            return false;

        AudioClip bgmClip = ResolveBGMClip();

        if (bgmClip == null)
        {
            Debug.LogWarning($"[OOTechTargetStageBGMPlayer] BGM clip missing for target group: {targetGroupName}");
            return true;
        }

        if (OOTechSoundManager.Inst != null)
            OOTechSoundManager.Inst.PlayBGM(bgmClip, true);

        return true;
    }

    private bool IsTargetGroup(string targetGroupName)
    {
        if (string.IsNullOrWhiteSpace(targetGroupName) || _targetGroupNameArray == null)
            return false;

        for (int index = 0; index < _targetGroupNameArray.Length; index++)
        {
            if (targetGroupName == _targetGroupNameArray[index])
                return true;
        }

        return false;
    }

    private AudioClip ResolveBGMClip()
    {
        return OOTechAudioClipResolver.Resolve(_bgmClip, _resourcePath, _editorAssetPath);
    }
}
