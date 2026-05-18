using UnityEngine;

public class SampleBasicCode_Mono : MonoBehaviour
{
    public SampleBasicCode_NonMono _emptyCodeNonMono = new SampleBasicCode_NonMono();
    bool _isFirstFixedUpdate = false;
    bool _isFirstUpdate = false;
    bool _isFirstLateUpdate = false;


    private void Awake()
    {
        Debug.Log("저는 Awake 입니다");
    }

    private void OnEnable()
    {
        Debug.Log("저는 OnEnable 입니다");
    }


    void Start()
    {
        Debug.Log("저는 Start 입니다");

        EmptyStaticClassSample.PrintStaticText();
    }

    private void FixedUpdate()
    {
        if (_isFirstFixedUpdate == true)
        {
            return;
        }

        _isFirstFixedUpdate = true;
        Debug.Log("저는 FixedUpdate 입니다");

    }

    void Update()
    {
        // 한번만 부르기 위해서 예외처리
        if(_isFirstUpdate == true)
        {
            return;
        }

        _isFirstUpdate = true;
        Debug.Log("저는 Update 입니다");

        // _emptyCodeNonMono.PrintSomeText();
        // Debug.Log("월드 틱-");
    }

    private void LateUpdate()
    {
        // 한번만 부르기 위해서 예외처리
        if (_isFirstLateUpdate == true)
        {
            return;
        }

        _isFirstLateUpdate = true;
        Debug.Log("저는 LateUpdate 입니다");

        Destroy(this.gameObject);
    }

    private void OnDisable()
    {
        Debug.Log("저는 OnDisable 입니다");
    }

    private void OnDestroy()
    {
        Debug.Log("저는 OnDestroy 입니다");
    }
}
