// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechIdleAnimationPlayer.cs
// - 역할: 캐릭터 배우의 이동, 입력, 애니메이션 상태를 담당합니다.
// - 감독 관점: 배우가 무대 위에서 어떻게 걷고 멈추고 반응하는지 정하는 연기 지도표입니다.
// - 유지보수 포인트: 장면 진행 순서는 Group Controller가 맡고, 캐릭터 스크립트는 자기 몸의 움직임만 맡게 합니다.
// =============================================================================
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

/// <summary>
/// Plays a looping idle clip without requiring an AnimatorController state.
/// This is used for Scenario1 actors that enter the stage after the scene starts.
/// </summary>
public class OOTechIdleAnimationPlayer : MonoBehaviour
{
    private PlayableGraph _idlePlayableGraph;

    public void PlayIdle(AnimationClip idleClip)
    {
        if (idleClip == null)
            return;

        Animator animator = GetComponent<Animator>();

        if (animator == null)
            animator = gameObject.AddComponent<Animator>();

        animator.enabled = true;

        DestroyIdlePlayableGraph();
        AnimationPlayableUtilities.PlayClip(animator, idleClip, out _idlePlayableGraph);
        _idlePlayableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
    }

    private void OnDisable()
    {
        DestroyIdlePlayableGraph();
    }

    private void OnDestroy()
    {
        DestroyIdlePlayableGraph();
    }

    private void DestroyIdlePlayableGraph()
    {
        if (_idlePlayableGraph.IsValid())
            _idlePlayableGraph.Destroy();
    }
}
