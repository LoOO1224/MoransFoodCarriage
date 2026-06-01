using UnityEngine;

public class DaniTech_ClickMovePlayer : MonoBehaviour
{
    [Header("플레이어 설정")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 15f; // 자연스러운 회전을 위해 값을 조금 키웁니다.

    [Header("점프 및 물리")]
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private bool _isGrounded;

    [Header("컴포넌트")]
    [SerializeField] private Rigidbody Rigidbody_Player;
    [SerializeField] private DaniTech_GroundDetector GroundDetector;
    [SerializeField] private DaniTech_AnimationController AnimController;

    // 클릭 이동을 위한 변수들
    private Vector3 _targetPosition;
    private bool _isMovingToTarget = false;

    void Start()
    {
        // 마우스를 화면에 자유롭게 둡니다.
        Cursor.lockState = CursorLockMode.None;
    }

    private void OnEnable()
    {
        GroundDetector.GroundTriggeredEvent += OnGroundTriggered;
    }

    private void OnDisable()
    {
        GroundDetector.GroundTriggeredEvent -= OnGroundTriggered;
    }

    void Update()
    {
        CheckMouseClick();
        MoveToTargetOnUpdate();

        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
        {
            StartJump();
        }

        // 공격 F키로 변경
        if (Input.GetKeyDown(KeyCode.F))
        {
            StartAttack();
        }
    }

    void CheckMouseClick()
    {
        // 마우스 오른쪽 버튼(1) 클릭 시 이동 명령
        if (Input.GetMouseButtonDown(0))
        {
            // 메인 카메라에서 마우스 커서 위치를 향해 레이(광선)를 생성합니다.
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // 레이를 쏴서 무언가(보통은 바닥)에 맞았다면
            if (Physics.Raycast(ray, out hit))
            {
                // 목표 위치를 맞은 지점으로 설정합니다.
                _targetPosition = hit.point;
                _isMovingToTarget = true;
            }
        }
    }

    void MoveToTargetOnUpdate()
    {
        if (!_isMovingToTarget) return;

        // 목표 위치를 향하는 방향 벡터 계산 (Y축 높이 차이는 무시해서 바닥을 보지 않게 함)
        Vector3 direction = _targetPosition - transform.position;
        direction.y = 0f;

        // 목표 지점에 거의 도달했는지 확인 (0.1f는 오차 허용 범위)
        if (direction.magnitude > 0.1f)
        {
            AnimController.SetState(EntityState.Run);

            // 1. 캐릭터 회전: 목표 방향을 부드럽게 바라봄
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);

            // 2. 캐릭터 이동
            transform.position += direction.normalized * _moveSpeed * Time.deltaTime;
        }
        else
        {
            // 목표 지점 도착 시 정지
            StopMovingImmediate();
        }
    }

    void StopMovingImmediate()
    {
        _isMovingToTarget = false;
        AnimController.SetState(EntityState.Idle);
    }

    void StartJump()
    {
        StopMovingImmediate();

        Rigidbody_Player.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        _isGrounded = false;
        _isMovingToTarget = false; // 점프 시 이동을 취소하려면 추가
    }

    void StartAttack()
    {
        StopMovingImmediate();

        Debug.Log("플레이어가 공격했습니다!");
        AnimController.SetState(EntityState.Attack);
        _isMovingToTarget = false; // 공격 시 이동을 멈추려면 추가
    }

    private void OnGroundTriggered(bool isGrounded)
    {
        _isGrounded = isGrounded;
    }
}
