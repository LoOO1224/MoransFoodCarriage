using UnityEngine;

public class DaniTechRefCode_MonsterView : MonoBehaviour
{
    [SerializeField] private float _rayCastDistance = 10.0f;
    Collider[] _overlapHitResults = new Collider[10];

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            // 주석 해제 - 관통형
            // PerformPenetrationRaycast();
            //ShotRaycast();
            DetectEnemies();
            DetectEnemiesNonAlloc();
        }
    }

    void DetectEnemiesNonAlloc()
    {
        // 중심점, 반지름, 탐색할 레이어 설정
        Vector3 center = transform.position;
        float radius = 5.0f;
        int layerMask = LayerMask.GetMask("Dummy");

        // 영역 내의 모든 콜라이더 검출
        var hitColliderSize = Physics.OverlapSphereNonAlloc(center, radius, _overlapHitResults, layerMask);

        for (int i = 0; i < hitColliderSize; i++)
        {
            var hitCollider = _overlapHitResults[i];
            Debug.Log(hitCollider.name + " 감지됨!");
            Destroy(hitCollider.gameObject);
        }
    }

    void DetectEnemies()
    {
        // 중심점, 반지름, 탐색할 레이어 설정
        Vector3 center = transform.position;
        float radius = 5.0f;
        int layerMask = LayerMask.GetMask("Dummy");

        // 영역 내의 모든 콜라이더 검출
        Collider[] hitColliders = Physics.OverlapSphere(center, radius, layerMask);

        foreach (var hitCollider in hitColliders)
        {
            Debug.Log(hitCollider.name + " 감지됨!");
            Destroy(hitCollider.gameObject);
        }
    }

    void ShotRaycast()
    {
        // 정보를 저장할 변수 선언 - 구조체!
        RaycastHit hit;

        // 2. 레이캐스트 발사
        // Physics.Raycast(시작점, 방향, 결과저장, 거리)
        if (Physics.Raycast(transform.position, transform.forward, out hit, _rayCastDistance))
        {
            // 3. 감지된 오브젝트의 정보 확인
            Debug.Log($"감지된 물체: {hit.collider.gameObject.name}");

            // 4. 감지된 오브젝트 삭제
            // hit.collider.gameObject를 통해 실제 게임 오브젝트에 접근합니다.
            Destroy(hit.collider.gameObject);
        }
    }

    void PerformPenetrationRaycast()
    {
        // 1. RaycastAll은 RaycastHit의 '배열'을 반환합니다.
        RaycastHit[] hits;
        hits = Physics.RaycastAll(transform.position, transform.forward, _rayCastDistance);

        // 2. 루프를 돌며 맞은 모든 물체를 처리합니다.
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            Debug.Log($"{i}번째 관통된 물체: {hit.collider.name}");

            // 맞은 물체 삭제
            Destroy(hit.collider.gameObject);
        }

        Debug.Log($"총 {hits.Length}개의 물체를 관통했습니다.");
    }

    // 에디터 뷰에서 레이를 시각적으로 확인하기 위함
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * _rayCastDistance);
        Gizmos.DrawWireSphere(transform.position, 5.0f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.LogWarning($"{collision.gameObject.name}와 충돌!");

    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Map"))
        {
            return;
        }

        Debug.Log($"{collision.gameObject.name}와 충돌 중이다!");
    }

    private void OnCollisionExit(Collision collision)
    {
        Debug.LogWarning($"{collision.gameObject.name}와 충돌 끝");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Gold"))
        {
            Destroy(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.LogWarning($"{other.name}를 빠져나갔다!");
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log($"{other.name}에 머무는 중이다");
    }
}
