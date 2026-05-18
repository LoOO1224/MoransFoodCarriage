using UnityEngine;

public class Sample3_ObjectA : MonoBehaviour, SampleIObjectable
{
    [SerializeField] private string _objectName;
    public string ObjectName { get { return _objectName; } set { _objectName = value; } }

    [SerializeField] private int _objectNumber;
    public int ObjectNumber { get { return _objectNumber; } set { _objectNumber = value; } }


    [SerializeField] private TextMesh TextMesh_ObjectName;

    private void Awake()
    {
        // 1) 씬에서 미리 설정했더라도, Awake에 들어오면서 덮어 씌워진다.
        ObjectName = "감자";
        ObjectNumber = 777;

        // 2) 텍스트 메쉬에 이름 표시!
        TextMesh_ObjectName.text = this.ObjectName;
    }

    // 3) 이 메서드는 다른 객체가 부를 것이다!
    public void PrintSomthing()
    {
        Debug.Log("오브젝트 A에서 아무거나 출력");
    }

    public void SetTextMeshNameOnInit()
    {
        TextMesh_ObjectName.text = this.ObjectName;
    }

}
