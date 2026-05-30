using UnityEngine;

public class DaniTech_MobilePlayerView : MonoBehaviour
{
    [Header("플레이어 설정")]
    [SerializeField] private float _moveSpeed = 5f;
    // 회전 속도 (도/초)
    [SerializeField] private float _rotationSpeed = 120f; 

    [Header("점프 및 물리")]
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private bool _isGrounded;

    [Header("컴포넌트")]
    [SerializeField] private Rigidbody Rigidbody_Player;
    [SerializeField] private DaniTech_GroundDetector GroundDetector;
    [SerializeField] private DaniTech_AnimationController AnimController;

    void Start()
    {
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
        MoveAndRotateOnUpdate();

        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
        {
            StartJump();
        }

        if (Input.GetMouseButtonDown(0))
        {
            StartAttack();
        }
    }

    void MoveAndRotateOnUpdate()
    {
        // 1. 회전 입력 (A: 왼쪽, D: 오른쪽)
        float rotationInput = 0f;
        if (Input.GetKey(KeyCode.A)) 
        {
            rotationInput = -1f; 
        }

        if (Input.GetKey(KeyCode.D)) 
        {
            rotationInput = 1f;
        } 

        // 실제 회전 적용 (스르륵 회전)
        transform.Rotate(Vector3.up * rotationInput * _rotationSpeed * Time.deltaTime);

        // 2. 이동 입력 (W, S: 전후)
        float vertical = Input.GetAxisRaw("Vertical"); // W, S

        // 3. 내 방향 기준으로 이동 벡터 계산 (중요: transform 기준)
        Vector3 moveDirection = (transform.forward * vertical).normalized;

        // 4. 애니메이션 및 이동 처리
        if (moveDirection.magnitude >= 0.1f || Mathf.Abs(rotationInput) > 0.1f)
        {
            // 이동 중이거나 회전 중일 때 Run (또는 원하는 애니메이션)
            AnimController.SetState(EntityState.Run);

            // 이동 처리
            this.transform.position += moveDirection * _moveSpeed * Time.deltaTime;
        }
        else
        {
            AnimController.SetState(EntityState.Idle);
        }
    }

    void StartJump()
    {
        Rigidbody_Player.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        _isGrounded = false;
    }

    void StartAttack()
    {
        Debug.Log("플레이어가 공격했습니다!");
        AnimController.SetState(EntityState.Attack);
    }

    private void OnGroundTriggered(bool isGrounded)
    {
        _isGrounded = isGrounded;
    }
}
