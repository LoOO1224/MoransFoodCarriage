using UnityEngine;

public class DaniTechRefCode2_PysicsStructAndVector : MonoBehaviour
{
    public GameObject TestObject;
    public GameObject TestTargetObject;
    public GameObject TestLookAtObject;
    public Rigidbody TestObjectRigidBody;

    public int RotationSpeed = 10;

    private Quaternion _rotationSlerp;
    private bool _toggleSlerpRotation = false;

    private bool _movePlayer = false;
    private bool _movePlayerByRigidBody = false;
    private bool _isMovePlayerRigidBodyLeft = false;


    void DaniTechRefCode_VectorBasic()
    {
        // X, Y를 멤버변수로 갖는 Vector2
        var vector2 = new Vector2(0, 0);
        vector2.x = 10;
        vector2.y = 10;
        Debug.Log($"{vector2.x} {vector2.y}");

        // X, Y, Z를 멤버변수로 갖는 Vector3
        var vector3 = new Vector3(0, 0, 0);
        vector3.x = 10;
        vector3.y = 10;
        vector3.z = 10;
        Debug.Log($"{vector3.x} {vector3.y} {vector3.z}");
    }

    void DaniTechRefCode_QuaternionBasicOnUpdate()
    {
        if(TestObject == null)
        {
            return;
        }

        // 0) 이렇게 절대 Rotation을 직접 수정하지 않는다!
        // this.transform.rotation.x = 45.0f;

        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            // 쿼터니언 1) 원하는 각도 대입하기
            TestObject.transform.rotation = Quaternion.Euler(0, 0.0f, 0);
            TestObject.transform.rotation = Quaternion.identity; // 위에 Euler (0,0,0과 같다)
        }
        else if(Input.GetKeyDown(KeyCode.Keypad1))
        {
            // 쿼터니언 2) 현재 회전에서 추가 회전하기
            TestObject.transform.rotation *= Quaternion.Euler(0, 15.0f, 0);
        }
        else if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            // 쿼터니언 3) 특정 방향(dir)을 바라보게 만드는 회전 값을 구할 때
            TestObject.transform.rotation = Quaternion.LookRotation(TestTargetObject.transform.position);
        }
        else if (Input.GetKeyDown(KeyCode.Keypad4))
        {
           
            _toggleSlerpRotation = (!_toggleSlerpRotation);
        }
        else if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            // 쿼터니언 5) 특정 축을 기준으로 몇 도 회전시킬 때
            TestObject.transform.rotation = Quaternion.AngleAxis(45.0f, Vector3.up); 
        }

        if (_toggleSlerpRotation == true)
        {
            var startRotation = TestObject.transform.rotation;
            _rotationSlerp = TestObject.transform.rotation * Quaternion.Euler(0, 45, 0);

            // 쿼터니언 4) A에서 B로 부드럽게 회전시킬 때 Slerp
            TestObject.transform.rotation = Quaternion.Slerp(startRotation, _rotationSlerp, Time.deltaTime * RotationSpeed);
        }
    }

    void DaniTechRefCode_TranslateOnUpdate()
    {
        if (TestObject == null)
        {
            return;
        }


        if (Input.GetKeyDown(KeyCode.D))
        {
            _movePlayerByRigidBody = false;

            TestObject.transform.rotation = Quaternion.Euler(0, -90.0f, 0);
            _movePlayer = true;
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            _movePlayerByRigidBody = false;

            TestObject.transform.rotation = Quaternion.Euler(0, 90.0f, 0);
            _movePlayer = true;
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            _movePlayerByRigidBody = false;

            _movePlayer = false;
            // 월드 좌표의 위쪽 방향으로 1만큼 이동
            TestObject.transform.Translate(Vector3.up * 2, Space.World);
        }

        if (_movePlayer == true)
        {
            _movePlayerByRigidBody = false;

            // 오브젝트의 로컬 앞 뒤 방향으로 이동
            // Vector3.back으로 넣으면 뒤로 간다
            TestObject.transform.Translate(Vector3.forward * Time.deltaTime, Space.Self);
        }
    }

    void DaniTechRefCode_AddForceAndVelocityOnUpdate()
    {
        if(TestObjectRigidBody == null)
        {
            return;
        }


        if (Input.GetKeyDown(KeyCode.F))
        {
            _movePlayer = false;
            _movePlayerByRigidBody = false;

            float jumpPower = 5;

            // 속도 초기화 후 점프 (더블점프 버그 방지)
            TestObjectRigidBody.linearVelocity = Vector2.zero;

            // 질량이 적용되는 ForceMode.Impulse는 점프를 거의 하지 못하게 된다!
            // TestObjectRigidBody.mass = 100;
            TestObjectRigidBody.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            TestObjectRigidBody.linearVelocity = Vector2.zero;
            TestObject.transform.rotation = Quaternion.Euler(0, 90.0f, 0);

            _movePlayer = false;
            _movePlayerByRigidBody = true;
            _isMovePlayerRigidBodyLeft = false;
            //TestObjectRigidBody.AddForce(1, ForceMode.VelocityChange);
        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            TestObjectRigidBody.linearVelocity = Vector2.zero;
            TestObject.transform.rotation = Quaternion.Euler(0, -90.0f, 0);

            _movePlayer = false;
            _movePlayerByRigidBody = true;
            _isMovePlayerRigidBodyLeft = true;
            //TestObjectRigidBody.AddForce(Vector2.right * 1, ForceMode.VelocityChange);
        }

        if (_movePlayerByRigidBody == true)
        {
            int moveSpeed = 1;
            float currentYVelocity = TestObjectRigidBody.linearVelocity.y;
            float targetXVelocity = _isMovePlayerRigidBodyLeft ? -moveSpeed : moveSpeed;

            TestObjectRigidBody.linearVelocity = new Vector2(targetXVelocity, currentYVelocity);
        }

    }

    void DaniTechRefCode_WhyVectorIsStruct()
    {
        // 구조체인 이유 1) 값복사를 통한 원본 값 안정성 확보
        // 이렇게 받아서 쓸 때, 원본의 값에 영향주지 않도록 함
        Vector3 currentPosition = this.gameObject.transform.position;
        currentPosition.x = 20; // 내 오브젝트의 위치가 X가 수정되지 않는다!!


        // 구조체인 이유 2) 불필요한 힙 메모리 방지
        // C#에서 값형인 구조체의 new는 힙 메모리 저장을 의미하지 않습니다! -> 생성자를 부른다는 의미에서만 new
        // 즉 다음 코드는 스택에 만들어집니다.
        // 특정 메서드에서 사용되고 나서 알아서 제거됩니다. (GC 발생하지 않음)
        Vector3 spawnPoint = new Vector3(500, 10, 20);

        // 구조체인 이유 3) 값복사를 통해 확장성 유지 (기존 기능에는 영향 없고 바리에이션 된 값만 추가로 변형)
        Vector3 spawnPointBasicCreature = spawnPoint;
        Vector3 spawnPointFlyCreature = spawnPoint;

        // 3==) 나는 몬스터는 더 높은 곳에서 스폰되어야 해서 100을 더해줌
        // Vector3가 참조형인 class였다면 모든 크리처들이 하늘 위에서 나오게 됨
        spawnPointFlyCreature.y = (spawnPointFlyCreature.y + 100);
    }

    void DaniTechRefCode_LookAtAndRotateOnUpdate()
    {
        if(TestObject == null || TestLookAtObject == null)
        {
            return;
        }

        // 주석 해제 후 테스트 (아이템 회전 뷰 느낌 낼 때, 선풍기나 회전목마 같은 것도)
        // TestObject.transform.Rotate(Vector3.up * 45f * Time.deltaTime, Space.World);

        if (Input.GetKeyDown(KeyCode.Y))
        {
            // 바라본다
            TestObject.transform.LookAt(TestLookAtObject.transform.position);
        }
        else if(Input.GetKeyDown(KeyCode.U)) 
        {
            // Y축 기준으로 회전
            TestObject.transform.Rotate(Vector3.up * 90f);
        }
    }

    void OnEnable()
    {
        DaniTechRefCode_WhyVectorIsStruct();
        DaniTechRefCode_VectorBasic();
    }

    // Update is called once per frame
    void Update()
    {
        DaniTechRefCode_QuaternionBasicOnUpdate();
        DaniTechRefCode_TranslateOnUpdate();
        DaniTechRefCode_AddForceAndVelocityOnUpdate();
        DaniTechRefCode_LookAtAndRotateOnUpdate();
    }
}