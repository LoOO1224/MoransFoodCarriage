using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class DaniTechGameObjectManager : MonoBehaviour
{
    // 생성할 몬스터의 프리팹
    [SerializeField] private GameObject Prefab_Enemy;
    [SerializeField] private Transform Root_Enemy;

    public static DaniTechGameObjectManager Inst { get; set; }

    // 생성된 오브젝트의 키가 됨
    private int _objectInstanceKeyGenerator = 0;

    // 생성된 오브젝트의 생명을 보관
    private Dictionary<int, GameObject> _createdGameObjectContainer = new Dictionary<int, GameObject>();
    private Dictionary<int, DaniTech_2DFieldObject> _fieldObjectContainer = new Dictionary<int, DaniTech_2DFieldObject>();
    private Dictionary<int, DaniTech_GameMonster> _monsterObjectContainer = new Dictionary<int, DaniTech_GameMonster>();

    // 게임 오브젝트 매니저가 살아있는 동안, 이 플레이어를 보관(캐싱)해둔다
    private DaniTech_2DPlayer _localPlayer;

    private void Awake()
    {
        Inst = this;
    }

    // 등록과 가져오기
    public void RegisterLocalPlayer(DaniTech_2DPlayer localPlayer)
    {
        _localPlayer = localPlayer;
    }

    // 프로퍼티 기능이 있긴 하지만, 그래도 그 프로퍼티를 직접 참조하는 것보다는 Get함수를 한정적으로 사용
    public DaniTech_2DPlayer GetLocalPlayer()
    {
        if(_localPlayer == null)
        {
            Debug.LogError("등록된 플레이어가 없는데! 참조하려고 시도하고 있습니다!!");
            return null;
        }

        // 우리가 배웠던 원시적인 Get함수입니다. -> 원시적이지만 유용함
        return _localPlayer;
    }


    public void RequestSpawnEnemy()
    {
        if(Prefab_Enemy == null)
        {
            Debug.LogWarning("프리팹이 등록되지 않은 오브젝트 입니다.");
            return;
        }

        var gObj = Instantiate(Prefab_Enemy, Root_Enemy);
        if(gObj == null)
        {
            Debug.LogWarning("생성에 실패한 게임 오브젝트 입니다.");
            return;
        }

        // 1-1 생성에 성공했다면, 미리 Key를 발급한다.
        _objectInstanceKeyGenerator++;

        // 1-2 Dictionary에 추가하기 전에 미리 키 검사한다
        if (_createdGameObjectContainer.ContainsKey(_objectInstanceKeyGenerator) == true)
        {
            Debug.LogWarning("이미 동일한 키가 발급된 게임 오브젝트가 존재합니다");
            return;
        }

        // 1-3 동적생성(실체화)된 오브젝트를 게임 오브젝트 매니저의 자료구조(Dictionary)에 보관하자!
        _createdGameObjectContainer.Add(_objectInstanceKeyGenerator, gObj);
        InitGeneratedEntityObject(_objectInstanceKeyGenerator, gObj);

        Debug.Log($"키: {_objectInstanceKeyGenerator}의 객체 {gObj.name}이 호출되었습니다.");
    }

    private void InitGeneratedEntityObject(int generatedId, GameObject gObj)
    {
        // 4-1 지금은 Enemy지만, 나중에 IGameEntity 같은 인터페이스로 개선하면 더 좋다
        DaniTech_2DEnemy gameEntity = gObj.GetComponent<DaniTech_2DEnemy>();
        if(gameEntity == null)
        {
            Debug.LogWarning($"생성된 {gObj.name}의 InstanceId를 대입할 수 있는 컴포넌트를 가져올 수 없습니다!");
            return;
        }

        // 4-2 생성된 객체에 정보를 부여한다!
        gameEntity.InitEnemyInfo(generatedId);
    }


    public GameObject GetEntityObjectCanBeNull(int instanceId)
    {
        if(_createdGameObjectContainer.ContainsKey(instanceId) == false)
        {
            Debug.LogWarning($"{instanceId}는 존재하지 않습니다.");
            return null;
        }

        // 2-1 실체화하면서 등록된 게임 오브젝트가 있다면 반환
        return _createdGameObjectContainer[instanceId];
    } 

    public void RequestDestroyEntityObject(int instanceId)
    {
        var gObj = GetEntityObjectCanBeNull(instanceId);
        if(gObj == null)
        {
            return;
        }

        // 3-1 요청된 객체를 제거함
        _createdGameObjectContainer.Remove(instanceId);
        Destroy(gObj);
    }

    //[몬스터] ====================================================================================================
    public async UniTaskVoid CreateMonsterObject(string monsterDataId, Transform spawnSpot)
    {
        // 만드려는 몬스터의 정보를 받아옵시다
        var monsterData = DaniTechGameDataManager.Instance.GetDNMonsterData(monsterDataId);
        if (monsterData == null) return;

        // 비동기라 조금 어려우므로 그냥 따라 치시면 됩니다
        var createdObj = await DaniTechResourceManager.Inst.InstantiateAsync(monsterData.PrefabPath, Root_Enemy, true);
        createdObj.transform.position = spawnSpot.position; // 위치를 스폰 스팟의 위치로 맞춰준다

        AddMonsterObjectOnCreate(createdObj, monsterDataId);
    }

    private void AddMonsterObjectOnCreate(GameObject createdObject, string monsterDataId)
    {
        _objectInstanceKeyGenerator++; // 게임 시작하고 1~ 1000개 이상~~ 계속 만들어지는 오브젝트들의 InstanceId가 겹치지 않도록 관리해줌
        int generatedInstanceId = _objectInstanceKeyGenerator;

        // 생성된 애는 게임오브젝트이기 때문에, MonsterBase <- GameMonster로 상속구조 되어 있음
        var monsterComponent = createdObject.GetComponent<DaniTech_GameMonster>();
        if (monsterComponent == null) return;

        // 생성이 되었고, 컴포넌트도 잘 가져왔다면 이제 뭐해야되나 -> 보관 -> 자료구조
        // List, Dictionary
        _monsterObjectContainer.Add(generatedInstanceId, monsterComponent);

        // UI든 필드 오브젝트든, 몬스터든 만들어지는 시점에서 Init(생성자처럼 정보를 세팅해주는 함수는 거의 자주 보게 된다)
        monsterComponent.InitMonster(generatedInstanceId, monsterDataId);
    }

    public DaniTech_GameMonster GetMonsterObjectByInstanceId(int monsterInstanceId)
    {
        if (_monsterObjectContainer.ContainsKey(monsterInstanceId) == false)
        {
            Debug.LogError($"{monsterInstanceId} 찾으려는 몬스터가 유효하지 않습니다");
            return null;
        }
        
        return _monsterObjectContainer[monsterInstanceId];
    }


    //[필드 오브젝트] ====================================================================================================

    public async UniTaskVoid CreateFieldObject(string fieldObjectDataId, Transform spawnSpot)
    {
        var fieldObject = DaniTechGameDataManager.Instance.GetDNFieldObjectData(fieldObjectDataId);
        if (fieldObject != null)
        {
            var createdObj = await DaniTechResourceManager.Inst.InstantiateAsync(fieldObject.PrefabPath, Root_Enemy, true);
            createdObj.transform.position = spawnSpot.position;
            AddFieldObjectOnCreate(createdObj, fieldObjectDataId);
        }
    }

    private void AddFieldObjectOnCreate(GameObject createdObject, string fieldObjectDataId)
    {
        _objectInstanceKeyGenerator++;
        var generatedInstanceId = _objectInstanceKeyGenerator;
        var fieldObject = createdObject.GetComponent<DaniTech_2DFieldObject>();

        if(fieldObject != null)
        {
            _fieldObjectContainer.Add(generatedInstanceId, fieldObject);
            fieldObject.InitFieldObjectInfoOnCreated(generatedInstanceId, fieldObjectDataId);
        }
    }

    public void RequestDestroyFieldObject(int instanceId)
    {
        var fieldObjectComponent = GetFieldObjectByInstanceId(instanceId);
        if (fieldObjectComponent == null)
        {
            return;
        }

        // 요청된 필드 오브젝트를 제거함
        _fieldObjectContainer.Remove(instanceId);
        Destroy(fieldObjectComponent.gameObject);
    }

    public DaniTech_2DFieldObject GetFieldObjectByInstanceId(int fieldObjectInstanceId)
    {
        if(_fieldObjectContainer.ContainsKey(fieldObjectInstanceId) == false)
        {
            Debug.LogError($"{fieldObjectInstanceId} 찾으려는 필드 오브젝트가 유효하지 않습니다");
            return null;
        }

        return _fieldObjectContainer[fieldObjectInstanceId];
    } 
}
