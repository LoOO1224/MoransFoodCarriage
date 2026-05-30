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
