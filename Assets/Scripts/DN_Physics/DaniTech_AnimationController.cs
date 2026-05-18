using UnityEngine;

public enum EntityState
{
    Idle,
    Walk,
    Run,
    Attack,
    Dead
}

public class DaniTech_AnimationController : MonoBehaviour
{
    [SerializeField] private Animator Animator_Player;

    private EntityState _currentState;

    public void SetState(EntityState newState)
    {
        if(newState == EntityState.Idle && _currentState == EntityState.Idle)
        {
            return;
        }

        _currentState = newState;
        switch (_currentState)
        {
            case EntityState.Dead:
                Animator_Player.SetBool("IsDead", true);
                break;
            case EntityState.Run:
                Animator_Player.SetBool("IsRun", true);
                break;
            case EntityState.Attack:
                Animator_Player.SetTrigger("IsAttack");
                break;
            case EntityState.Idle:
                ResetAllParameters();
                break;
        }
    }

    private void ResetAllParameters()
    {
        Animator_Player.SetBool("IsRun", false);
        Animator_Player.SetBool("IsDead", false);
    }
}
