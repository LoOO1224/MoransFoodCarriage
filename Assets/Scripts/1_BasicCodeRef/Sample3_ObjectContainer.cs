using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Sample3_ObjectContainer : MonoBehaviour
{
    [SerializeField] private GameObject Prefab_SampleObject;
    [SerializeField] int FindTestKey = 2;
    [SerializeField] private GameObject GObj_RemoveTargetC;

    private Dictionary<int,GameObject> _objectDictionary = new Dictionary<int, GameObject>();
    private int _generatedInstanceIdx = 0;


    private string GetGeneratedName(int curIdx)
    {
        int currentNum = (curIdx % 3);
        if(currentNum == 0)
        {
            return "감자";
        }
        else if(currentNum == 1)
        {
            return "만두";
        }
        else if(currentNum == 2)
        {
            return "순대";
        }
        else
        {
            return "피자";
        }
    }

    private void Update()
    {
        // 스페이스바를 누르면 객체를 동적 생성
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _generatedInstanceIdx++;

            var instantiatedObj = Instantiate(Prefab_SampleObject, this.gameObject.transform);
            if (instantiatedObj != null)
            {
                // 실체화된 게임오브젝트의 이름을 바꿔보자 (에디터 하이어라키 뷰에서 확인용)
                instantiatedObj.name = $"{instantiatedObj.name}(Id_{_generatedInstanceIdx})";

                // 실체화된 게임오브젝트에서 인터페이스를 가져와보자!
                var objectable = instantiatedObj.GetComponent<SampleIObjectable>();
                objectable.ObjectNumber = _generatedInstanceIdx;
                objectable.ObjectName = GetGeneratedName(_generatedInstanceIdx);
                objectable.SetTextMeshNameOnInit();

                // 유니티의 게임오브젝트를 담을때는 인터페이스 같은 순수 C#클래스가 아니라 GameObject를 넣는게 좋다!
                _objectDictionary.Add(objectable.ObjectNumber, instantiatedObj);
            }
        }
        else if(Input.GetKeyDown(KeyCode.F))
        {
            // 오브젝트가 잘 있는지 찾아보자!

            if(_objectDictionary.ContainsKey(FindTestKey) == true)
            {
                var gObj = _objectDictionary[FindTestKey];
                if(gObj != null)
                {
                    var objectable = gObj.GetComponent<SampleIObjectable>();
                    Debug.LogWarning($"인덱스 {FindTestKey}인 오브젝트 : {objectable.ObjectName} 찾아짐!");
                }
            }
            else
            {
                Debug.LogError($"인덱스 {FindTestKey}인 오브젝트가 없습니다!");
            }
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            GObj_RemoveTargetC.RemoveComponent<Sample3_ObjectC>();
        }
    }

}
