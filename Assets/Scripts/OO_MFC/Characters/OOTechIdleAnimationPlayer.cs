// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OOTechIdleAnimationPlayer.cs
// - 역할: 캐릭터 이동, 애니메이션, 상호작용 보조 처리를 담당합니다.
// - 유지보수: Animator state 이름과 씬 배치 콜라이더가 직접 연결되므로 이름 변경에 주의합니다.
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
