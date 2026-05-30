using UnityEngine;

// +) 어떤 컴포넌트가 필수로 필요하다는 것을 강제할 수 있다
[RequireComponent(typeof(Rigidbody2D))]
public class DaniTech_2DPlayer : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float _moveSpeed = 8f;
    [SerializeField] private float _jumpForce = 12f;

    [Header("지면 체크 설정")]
    [SerializeField] private Transform _groundCheck;    // 발 밑에 배치할 빈 오브젝트
    [SerializeField] private float _checkRadius = 0.5f; // 체크 범위
    [SerializeField] private LayerMask _groundLayer;    // 지면으로 인식할 레이어 (Platforms 등)

    // 우선 직접 들고 있다가 추후에 UI매니저한테 요청하도록 개선해볼 것
    [SerializeField] private DaniTechScoreUI _scoreUI;

    private Rigidbody2D _rigidBody;
    private bool _isGrounded;
    private float _horizontalInput;
    private bool _lookRight = true;

    // 추후에는 이런 데이터가 저장될 수 있도록 UI에 있는 것보다 한곳으로 모여지는게 좋다
    private int _currentScore;

    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();

        // 2D 캐릭터가 물리 충돌 시 회전해서 넘어지는 것 방지
        _rigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        // 1. 입력 받기 (Update에서 수행)
        _horizontalInput = Input.GetAxisRaw("Horizontal");

        // 2. 점프 입력
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            Jump();
        }

        // 3. 캐릭터 방향 전환 (Flip)
        if (_horizontalInput > 0 && !_lookRight)
        {
            Flip();
        }
        else if (_horizontalInput < 0 && _lookRight) 
        { 
            Flip(); 
        }
    }

    void FixedUpdate()
    {
        // 4. 지면 체크 (물리 연산 전 수행)
        _isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _checkRadius, _groundLayer);

        // 5. 좌우 이동 처리
        Move();
    }

    void Move()
    {
        // Y축 속도는 유지하면서 X축 속도만 변경 (관성 유지)
        _rigidBody.linearVelocity = new Vector2(_horizontalInput * _moveSpeed, _rigidBody.linearVelocity.y);
    }

    void Jump()
    {
        // 순간적인 힘을 위로 가함
        _rigidBody.linearVelocity = new Vector2(_rigidBody.linearVelocity.x, _jumpForce);
    }

    void Flip()
    {
        _lookRight = !_lookRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    // 에디터 뷰에서 지면 체크 범위를 시각적으로 확인
    private void OnDrawGizmos()
    {
        if (_groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_groundCheck.position, _checkRadius);
        }
    }

    // 6) 적 충돌 시 처리를 해보자
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 6-1) 플레이어의 > 콜리전에 충돌한 객체가 어떤 Tag인지 1차 검사한다.
            // 지면 같은 오브젝트와 점프시 충돌이 계속 오므로 이렇게 태그로 먼저 비교하는게 좋다
            // 중단점을 찍어보면서 확인 추천
        if (collision.gameObject.CompareTag("Enemy") == false)
        {
            return;
        }

        // 6-2) 충돌한 몬스터의 정보를 받아오려고 시도해보자
        var enemyComponent = collision.gameObject.GetComponent<DaniTech_2DEnemy>();
        if (enemyComponent == null)
        {
            Debug.Log($"충돌한 적 객체에서 컴포넌트를 찾을 수 없습니다 : {gameObject.name}");
            return;
        }

        // 6-3) 충돌된 오브젝트를 플레이어가 직접 제거하는게 아니라, Id로 게임오브젝트매니저한테 삭제를 요청한다
        DaniTechGameObjectManager.Inst.RequestDestroyEntityObject(enemyComponent.EntityInstancId);

        // 6-4) 피그미를 잡으면 스코어를 올려주자!
        AddGameScore();
    }

    private void AddGameScore()
    {
        // 7) 여기서 맥락 -> UI를 갱신해주기 위해 과연 플레이어가 이렇게 UI를 직접
            // 알고 있는게 좋은걸까?

        _currentScore++;
        _scoreUI.AddGameScore(_currentScore);
    }
}
