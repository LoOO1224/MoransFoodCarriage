using UnityEngine;

public class DaniTech_2DEnemy : MonoBehaviour
{
    [SerializeField] private SpriteRenderer SpriteRenderer_Enemy;

    // 1-1) GameObject 매니저에서 이 객체를 동적생성하고 등록할때 부여되는, 충돌시 상호작용 요청에 쓰이는 인스턴스 Id
   public int EntityInstancId { get; private set; }

    private Vector3 _moveDirection;

    void Start()
    {
        RandomPickDirection();
    }

    void Update()
    {
        SimpleEnemyMoveOnUpdate();
    }

    public void InitEnemyInfo(int instanceId)
    {
        EntityInstancId = instanceId;

        // + 추후에 여기서 여러 정보를 초기화해도 좋다!
    }

    void RandomPickDirection()
    {
        // 3-1) 변칙성을 위해서 0 또는 1 중 하나를 뽑아 왼쪽(-1) 혹은 오른쪽(1) 결정
        // 중요한 로직은 아님
        float randomX = Random.Range(0, 2) == 0 ? -1f : 1f;
        _moveDirection = new Vector3(randomX, 0, 0);
        SetMeshDirectionByMoveDirection((int)_moveDirection.x);
    }

    void SetMeshDirectionByMoveDirection(int x)
    {
        // + 디테일을 살리기 위해 방향에 따라 캐릭터 리소스를 뒤집는다
        // 역시 중요한 로직은 아니다!
        SpriteRenderer_Enemy.flipX = (x < 0);
    }

    void SimpleEnemyMoveOnUpdate()
    {
        // 2-1 결정된 방향으로 매 프레임 이동
        transform.position += _moveDirection * 5.0f * Time.deltaTime;
    }
}
