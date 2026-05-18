using UnityEngine;

public class Sample3_ObjectC : MonoBehaviour, SampleIObjectable
{
    [SerializeField] private string _objectName;
    public string ObjectName { get { return _objectName; } set { _objectName = value; } }

    [SerializeField] private int _objectNumber;
    public int ObjectNumber { get { return _objectNumber; } set { _objectNumber = value; } }

    public void PrintSomthing()
    {
        Debug.Log("오브젝트 C에서 아무거나 출력");
    }

    public void SetTextMeshNameOnInit()
    {
    }
}
