using UnityEngine;

public class DaniTech_ThirdPlayerView : MonoBehaviour
{
    [Header("플레이어 설정")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] public float _mouseSensitivity = 200f;
    [SerializeField] public Transform _cameraTransform;

    [Header("점프 및 물리")]
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private bool _isGrounded;

    [Header("컴포넌트")]
    [SerializeField] private Rigidbody Rigidbody_Player;
    [SerializeField] private DaniTech_GroundDetector GroundDetector;
    [SerializeField] private DaniTech_AnimationController AnimController;

    // 카메라 상하 회전값 저장용
    private float _rotationX = 0f; 

    void Start()
    {
        // 게임 시작 시 마우스 커서를 화면 중앙에 고정하고 숨김
        Cursor.lockState = CursorLockMode.Locked;
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
        RotateCheckOnUpdate();
        MoveOnUpdate();

        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
        {
            StartJump();
        }

        // 공격 입력 (마우스 왼쪽 클릭)
        if (Input.GetMouseButtonDown(0))
        {
            StartAttack();
        }
    }

    void RotateCheckOnUpdate()
    {
        // 1. 마우스 입력 받기
        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity * Time.deltaTime;

        // 2. 상하 회전 (카메라만 까딱까딱)
        _rotationX -= mouseY;
            // 너무 뒤로 넘어가지 않게 Clamp를 써서 제한
        _rotationX = Mathf.Clamp(_rotationX, -90f, 90f);
        _cameraTransform.localRotation = Quaternion.Euler(_rotationX, 0f, 0f);

        // 3. 좌우 회전 (플레이어 몸통 전체를 회전)
        this.transform.Rotate(Vector3.up * mouseX);
    }

    void MoveOnUpdate()
    {
        // 1. 키보드 입력 받기 (W, S, A, D)
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        AnimController.SetState((x <= 0 && z <= 0) ? EntityState.Idle : EntityState.Run);

        // 2. "내가 바라보는 방향" 기준으로 이동 방향 계산
        // transform.right는 나의 오른쪽, transform.forward는 나의 앞쪽
        Vector3 move = (this.transform.right * x) + (this.transform.forward * z);

        // 3. 실제 이동 처리
        this.transform.position += ((move * _moveSpeed) * Time.deltaTime);
    }

    void StartJump()
    {
        // 위쪽 방향으로 즉각적인 힘을 가함
        Rigidbody_Player.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        _isGrounded = false;
    }

    void StartAttack()
    {
        // 실제 공격 로직(애니메이션 등)이 들어갈 자리
        Debug.Log("플레이어가 공격했습니다!");

        AnimController.SetState(EntityState.Attack);
    }

    private void OnGroundTriggered(bool isGrounded)
    {
        _isGrounded = isGrounded;
    }
}
