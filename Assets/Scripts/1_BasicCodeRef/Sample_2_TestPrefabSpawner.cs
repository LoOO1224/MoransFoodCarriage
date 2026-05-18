using UnityEngine;

public class Sample2_TestPrefabSpawner : MonoBehaviour
{
    public GameObject Prefab_Sample;
    public Transform Transform_SpawnPos;

    public GameObject Root_InstanceRoot;

    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 1) 프리팹으로 게임 오브젝트 객제 동적 생성
            Instantiate(Prefab_Sample, Transform_SpawnPos.position, Quaternion.identity);

            // 2) 프리팹으로 게임 오브젝트 객체 동적 생성하고 특정 오브젝트에 하이어라키 설정하기 (Parenting)
            var testGameObject = Instantiate(Prefab_Sample, Transform_SpawnPos.position, Quaternion.identity);
            testGameObject.transform.SetParent(Root_InstanceRoot.transform);

            // 2==) 2번과 같음, 프리팹으로 게임 오브젝트 객체 동적 생성하고 바로 부모 설정
            Instantiate(Prefab_Sample, Transform_SpawnPos.position, Quaternion.identity, Root_InstanceRoot.transform);

        }
    }
}
