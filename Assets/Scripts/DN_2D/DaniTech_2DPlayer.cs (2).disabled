using System;
using System.Collections;
using System.Collections.Generic;
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

    [Header("애니메이터")]
    [SerializeField] private DaniTech_2DAnimatorController AnimatorController_Entity;

    [Header("스킬")]
    [SerializeField] private Collider2D Collider_PlayerNormalAttack;
    [SerializeField] private GameObject Prefab_SkillProjectile;
    [SerializeField] private Transform Transform_SkillProjectileRoot;

    [Header("전투 관련 정보")]
    [SerializeField] private int _maxHp;
    [SerializeField] private int _playerHp = 1000;
    [SerializeField] private int _playerBaseAtk = 100;


    // 우선 직접 들고 있다가 추후에 UI매니저한테 요청하도록 개선해볼 것
    [SerializeField] private DaniTech_ScoreUI _scoreUI;

    private Rigidbody2D _rigidBody;
    private bool _isGrounded;
    private float _horizontalInput;
    private bool _lookRight = true;
    private bool _isSkillUsing = false;

    // 추후에는 이런 데이터가 저장될 수 있도록 UI에 있는 것보다 한곳으로 모여지는게 좋다
    private int _currentScore;

    // 스킬 관련 =====================================================================
    public enum ViewType { SideView, TopDown, Isometric }
    public ViewType _currentView = ViewType.SideView;
    public Vector2 _lookDirection = Vector2.up; // 플레이어가 바라보고 있는 방향

    private Vector2 _lastOverlapOffset;
    private float _lastOverlapRadius;
    private bool _isOverlapSkillVisible = false;


    private event Action<int, int> _onHpChanged;
    private event Action<int, int> _onMpChanged;

    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();

        // 2D 캐릭터가 물리 충돌 시 회전해서 넘어지는 것 방지
        _rigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;
        Collider_PlayerNormalAttack.gameObject.SetActive(false);

        _playerHp = 1000;
        _maxHp = _playerHp;
    }

    private void Start()
    {
        // 나 스스로를 등록한다. -> 씬에 있는 그 2D 플레이어가 등록됨
        DaniTechGameObjectManager.Inst.RegisterLocalPlayer(this);
        DaniTechUIManager.Instance.AddHudSlot(0, this.gameObject.transform);
    }

    private void OnDisable()
    {
        ResetStatChangedEvent();
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

        // 이동을 한다라는 판정만 우선 해봅시다
        bool isMoving = (_horizontalInput != 0);
        ChangePlayerState(isMoving ? DaniTech_EntityAnimState.Walk : DaniTech_EntityAnimState.Idle);

        if (Input.GetKeyDown(KeyCode.F))
        {
            UseNormalAttack();
        }

    }

    private void ChangePlayerState(DaniTech_EntityAnimState newState)
    {
        // 이런 곳에 UI나 플레이어의 별도 처리를 넣어줄 수도 있다


        // 우선 애니메이션만 바꿔 봅시다
        AnimatorController_Entity.SetState(newState);
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

    public bool CheckSkillUseable(bool isShowMsg = true)
    {
        // 원하는 스킬의 사용 가능 조건 체크들이 더 많이 들어가게 된다
        // 우선은 중복스킬이 사용 되는지 안되는지 먼저 체크
        if(_isSkillUsing == true)
        {
            if(isShowMsg == true)
            {
                DaniTechUIManager.Instance.OpenSimplePopup("스킬이 이미 사용중입니다");
            }
            return false;
        }

        return true;
    }



    public void UseNormalAttack()
    {
        if (CheckSkillUseable(isShowMsg:false) == false) return;
  

        ChangePlayerState(DaniTech_EntityAnimState.Atk);
        Collider_PlayerNormalAttack.gameObject.SetActive(true);
        StartCoroutine(CoStartNormalAttack());

    }

    public void UseFirstSkill()
    {
        if (CheckSkillUseable() == false) return;

        // 오버랩이나 레이케스트는 유니티 에디터에서도 확인 가능하도록
        // AI가 보통 일반적으로 DrawGizmo...
        UseOverlapSkill(new Vector2(5.0f, 0.0f), 3f);
    }

    public void UseSecondSkill()
    {
        if (CheckSkillUseable() == false) return;

    }

    public void UseThirdSkill()
    {
        if (CheckSkillUseable() == false) return;
        CreateProjectileSkillObject();
    }

    private void CreateProjectileSkillObject()
    {
        // UI에서도 동적생성했듯, 지금 스킬 투사체 오브젝트도 소환!(실체화-동적생성)
        var gObj = Instantiate(Prefab_SkillProjectile, Transform_SkillProjectileRoot);
        if (gObj == null) return;

        var skillProjectileComponent = gObj.GetComponent<DaniTech_SkillProjectile>();
        if(skillProjectileComponent == null) return;

        var tag = this.gameObject.tag;
        skillProjectileComponent.InitSkillObject(0, _lookRight, this.transform.position, 500, tag, OnMonsterCollied);
    }

    private void OnMonsterCollied(int monsterInstanceId, int skillDamage)
    {
        // 게임 오브젝트 매니저는 모든 몬스터를 관리한다
        // 그 자료구조는 Dictionary로 key - instanceId다
        // 몬스터를 게임오브젝트 매니저를 통해 받아올 수 있다!


        // 몬스터때도 구현했던 2번 방식) 플레이어한테 스킬이 충돌정보를 알려주기만 하고, 실제 몬스터와의 상호작용은 플레이어가
        // 주도권을 갖고 한다!
        var monsterComponent = DaniTechGameObjectManager.Inst.GetMonsterObjectByInstanceId(monsterInstanceId);
        if (monsterComponent == null) return;

        Debug.LogWarning($"플레이어가 {monsterInstanceId}에 데미지 {skillDamage} 부여");
        monsterComponent.TakeDamage(skillDamage);
    }


    // 시간제어가 필요하므로 코루틴 , 유니테스크를 사용해야한다
    IEnumerator CoStartNormalAttack()
    {
        _isSkillUsing = true;
        yield return new WaitForSeconds(1.0f);
        Collider_PlayerNormalAttack.gameObject.SetActive(false);
        _isSkillUsing = false;
    }

    // 뷰 타입별로 방향 벡터를 보정해주는 헬퍼 메서드
    private Vector2 GetAdjustedDirection(Vector2 rawDir)
    {
        switch (_currentView)
        {
            case ViewType.Isometric:
                // 아이소메트릭은 엑스축과 와이축이 사선으로 왜곡됨 (X는 우상향, Y는 좌상향 형태를 투영)
                return new Vector2(rawDir.x - rawDir.y, (rawDir.x + rawDir.y) * 0.5f).normalized;

            case ViewType.SideView:
                // 사이드뷰는 기본적으로 좌우(X축)만 고려
                rawDir = _lookRight ? Vector2.right : Vector2.left;
                return new Vector2(rawDir.x, 0).normalized;

            case ViewType.TopDown:
            default:
                return rawDir.normalized;
        }
    }

    // AI가 준 코드에서 -> 메서드 단위로 기능을 분리해서 명료하게 나눠줘
    // 파라미터(매개변수), 지역변수, 반환형, 멤버변수
    public void UseOverlapSkill(Vector2 offsetPosition, float radius)
    {
        _lastOverlapOffset = offsetPosition;
        _lastOverlapRadius = radius;

        Vector2 adjustedDir = GetAdjustedDirection(_lookDirection);
        // 바라보는 방향으로 오프셋만큼 떨어진 중심점 계산
        Vector2 center = (Vector2)transform.position + new Vector2(adjustedDir.x * offsetPosition.x, adjustedDir.y * offsetPosition.y);

        // 중심점 기준으로 반지름(radius) 안에 들어온 모든 콜리더를 검출
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(center, radius);

        foreach (Collider2D col in hitColliders)
        {
            if (col != null && col.gameObject != this.gameObject)
            {
                Debug.Log($"오버랩 스킬 적중: {col.name}");
            }
        }
    }


    // 플레이어의 전투와 관련된 부분은 사실 나중에 다른 곳으로 빠질 수 있기 때문에
    // 데이터의 와리가리 하는 부분은 -> Rpg 재접하면 풀피 -> 세이브 -> GameManager
    // 결국 인스턴스 데이터가 이 플레이어 코드안에 있는게 아니라 -> 저장이 가능하도록 GameManager에 플레이어 인스턴스 데이터로 저장이 되어야 함
    // PlayerModel

    public void TakeDamage(int damage)
    {
        _playerHp -= damage;
        Debug.Log($"{_playerHp}");

        InvokeStatChangedEvent();
        if (_playerHp < 0)
        {
            // 죽음 처리를 여기서 해두고
            PlayerDie();
            DaniTechUIManager.Instance.RemoveHudSlot(0);
        }
    }

    public void PlayerDie()
    {
        // bool _isAlive = false;
    }

    public void BindOnStatChangedEvent(Action<int, int> hpChangeCallback, Action<int, int> mpChangeCallback)
    {
        _onHpChanged += hpChangeCallback;
        _onMpChanged += mpChangeCallback;
    }

    public void ResetStatChangedEvent()
    {
        _onHpChanged = null;
        _onMpChanged = null;
    }

    private void InvokeStatChangedEvent()
    {
        // 우선 HP든 MP든 하나라도 바뀌면 다 호출해준다
        _onHpChanged?.Invoke(_playerHp, _maxHp);
        // _onMpChanged?.Invoke(_playerMp);
    }

    private void OnDrawGizmos()
    {
        if (_groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_groundCheck.position, _checkRadius);
        }

        Gizmos.color = new Color(1f, 1f, 0f, 0.5f); // 반투명 노란색
        Vector2 adjustedDir = GetAdjustedDirection(_lookDirection);
        Vector3 center = transform.position + new Vector3(adjustedDir.x * _lastOverlapOffset.x, adjustedDir.y * _lastOverlapOffset.y, 0);
        Gizmos.DrawWireSphere(center, _lastOverlapRadius);
    }


    public void AddHp(int hp)
    {
        _playerHp += hp;
        InvokeStatChangedEvent();
    }

    public void AddAtk(int atk)
    {
        _playerBaseAtk += atk;
    }
}
