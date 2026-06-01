using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class DaniTech_SimplePopup : DaniTechUIBase
{
    [SerializeField] Text Text_Msg;

    // 유니테스크 취소 1-1) 대기 시간이 긴 경우 이 UI가 중간에 닫힌다면 메모리누수를 방지하기 위해 취소처리하자
    private CancellationTokenSource _cancelToken;


    //WaitForSeconds _waitForSec = new WaitForSeconds(1.5f)

    // 기능 확장
    private void OnEnable()
    {
        // 아래 코루틴 대신 UniTask로 바꿔보자
            // StartCoroutine(CoCloseSelf());
        CloseSelfAsync().Forget();

        // 유니테스크를 이용한 샘플
        UIWaitUntilNextFrame().Forget();

    }

    private void OnDisable()
    {
        // 혹시 이 팝업이 1.5초보다 빨리 닫히는 경우 비동기 대기를 취소
        if (_cancelToken != null)
        {
            _cancelToken.Cancel();
            _cancelToken.Dispose();
            _cancelToken = null;
        }
    }

    private async UniTask UIWaitUntilNextFrame()
    {
        // 호출 후 1프레임 지난 후 어떤 작업을 해야될 때 이 로직을 사용하세요!
        Debug.Log("이 영역에는 프레임 대기 전에 일어나야하는 작업들이 있어야 합니다");

        await UniTask.NextFrame();

        // [UI에서 유니테스크가 유용한 경우 예시]
            // UI 레이아웃 갱신: LayoutGroup이나 ContentSizeFitter가 적용된 UI의 크기를 즉시 가져오면 0으로 나오는 경우가 많음.
            // 이때 한 프레임 쉬고 가져오면 정확한 크기를 알 수 있다!

        Debug.Log("이 영역에는 다음 프레임에 1번 불러져야 한다면 여기서 작업");
    }


    private async UniTaskVoid CloseSelfAsync()
    {
        if(_cancelToken != null)
        {
            _cancelToken.Cancel(); // 취소
            _cancelToken.Dispose(); // 자원해제
        }

        _cancelToken = new CancellationTokenSource();
        Debug.Log("코루틴처럼 대기 상태에서 해야하는 일이 있다면 여기서 먼저 진행");

        // 지정된 시간만큼 대기
            // 취소 토큰은 꼭 필요한 것은 아니지만, 대기 시간이 긴 경우는 이 작업이 끝나기 전에
            // UI가 닫히거나 파괴될 수 있다면 비동기 취소 토큰 처리를 해줄 필요가 있다
            // 이 게임오브젝트가 비활성화가 아니라 파괴형태로 사라진다면 편하게 이걸 전달 해줘도 된다->>> this.GetCancellationTokenOnDestroy();
        await UniTask.Delay(TimeSpan.FromSeconds(1.5), cancellationToken: _cancelToken.Token);

        // 완료 후 스스로 닫자
        DaniTechUIManager.Instance.ClosePopupUI(DaniTechUIType.DNSimplePopup);
    }

    public void SetUI(string msg)
    {
        Text_Msg.text = msg;
        CheckAndChangeColor(msg);
    }

    private void CheckAndChangeColor(string msg)
    {
        if (msg.Contains("출력"))
        {
            Text_Msg.color = Color.red;
        }
    }

    //IEnumerator CoCloseSelf()
    //{
    //    yield return new WaitForSeconds(1.5f);
    //    DaniTechUIManager.Instance.CloseSpecificUI(UIType.SimplePopup);
    //    // 이렇게 스스로 비활성화 하지 말고 꼭 매니저를 통해 비활성화 하자! this.gameObject.SetActive(false);
    //} 

    // UIBase 만들면 거기에 OnBeforeEnable() //
    // Awake랑 OnEnable가 순서를 보장하지 않는 경우가 있어서
    // 확실하게 OnEnable하기 전에 데이터 관련 처리가 필요한 경우를 위함

}
