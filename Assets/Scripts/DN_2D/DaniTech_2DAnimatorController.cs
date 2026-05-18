using UnityEngine;

public enum DaniTech_EntityAnimState
{
    None = 0,
    Idle,
    Walk,
    Atk
}

public class DaniTech_2DAnimatorController : MonoBehaviour
{
    [SerializeField] private Animator Animator_Entity;

    private DaniTech_EntityAnimState _currentAnimState;

    // 외부에서 쉽게 변경을 요청하려고
    // 이 상태에 따른 애니메이션 재생을 여기서만 모아서 해줄려고
    public void SetState(DaniTech_EntityAnimState newState) // 새로운 상태
    {
        if (newState == DaniTech_EntityAnimState.Idle && _currentAnimState == DaniTech_EntityAnimState.Idle)
        {
            return;
        }

        //비교를 했는데, 같은 값이 아니고, 이제 동작을 바꿔도 된다면 이렇게 대입
        _currentAnimState = newState;

        switch (_currentAnimState)
        {
            case DaniTech_EntityAnimState.Idle:
                ResetAllAnimParameters();
                break;
            case DaniTech_EntityAnimState.Walk:
                Animator_Entity.SetBool("IsWalk", true);
                break;
            case DaniTech_EntityAnimState.Atk:
                Animator_Entity.SetTrigger("IsAtk");
                break;
            default:
                // 의도되지 않은 상황이라면 모든 파라미터를 초기화한다
                ResetAllAnimParameters();
                break;
        }
    }

    private void ResetAllAnimParameters()
    {
        Animator_Entity.SetBool("IsWalk", false);
    } 




}
