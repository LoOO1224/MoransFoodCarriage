using UnityEngine;

public class Sample3_ObjectB : MonoBehaviour
{
    public GameObject TargetObjectAorC;
    // 인터페이스는 인스펙터에서 참조로 등록할 수 없다
        // 대신 위에 GameObject로 GetComponent로 인터페이스를 찾아올수는 있다
    // public IObjectable TargetObjectAorC_2;
    public Sample3_ObjectA TargetObjectA_2; 

    [SerializeField] private TextMesh TextMesh_ObjectName;

    private void PrintObjectAName()
    {
        // 1) GameObject로 받아왔다면 그 안에 있는 컴포넌트를 꺼내볼 수 있다
        var objectA = TargetObjectAorC.GetComponent<SampleIObjectable>();

        if(objectA == null)
        {
            return;
        }

        TextMesh_ObjectName.text = objectA.ObjectName;
        objectA.PrintSomthing();
    }

    private void PrintObjectANameByObjectA()
    {
        // 5) 이렇게 게임 오브젝트가 아니라 직접 컴포넌트를 받아올 수도 있다!
            // GetComponent를 쓰지 않아도 되는 장점이 있다
            // GetComponent는 특정 시점에 간헐적으로 쓰는 것은 괜찮으나 Update에서 남용되지 않도록 주의하자!
            // 그래서 이렇게 참조를 미리 인스펙터에서 등록해 사용하는 것이 성능상 이점이 있다
        if(TargetObjectA_2 == null)
        {
            return;
        }

        TextMesh_ObjectName.text = TargetObjectA_2.ObjectName;
        TargetObjectA_2.PrintSomthing();
    }

    private void Awake()
    {
        // PrintObjectAName();
    }

    private void OnEnable()
    {
        // 2) Awake와 OnEnable은 이 객체 기준으로는 순차적이지만
            // 다른 객체들과의 시점에서 볼 때는 Awake와 OnEnable은 같은 묶음으로 진행된다
            // 즉, 다른 객체의 초기화된 정보를 받아오려면 아래 Start에서부터 가능하다
        // PrintObjectAName();
    }
    
    // 3) Start부터는 ObjectA의 감자<를 받아올 수 있다! 모든 객체의 Awake/OnEnable 이후에 한박자 쉬고, 모든 객체의 Start가 돌아가는 느낌
    // 4) Start와 Update 사이에도 한박자 쉰다!
    private void Start()
    {
        PrintObjectAName();
        PrintObjectANameByObjectA();
    }


}
