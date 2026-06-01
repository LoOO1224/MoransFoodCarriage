using UnityEngine;

// UIBase -> 까먹을까봐 모노대신 UIBase를 상속하자!
public class DaniTech_RobbyUI : DaniTechUIBase
{
    [SerializeField] private DaniTechUIButton Button_GameStart;
    [SerializeField] private DaniTechUIButton Button_GameQuit;

    private void OnEnable()
    {
        Button_GameStart.BindOnClickButtonEvent(OnClick_GameStart);
        Button_GameQuit.BindOnClickButtonEvent(OnClick_GameQuit);

    }

    // 유니티 에서 원래 등록되었어야하는 버튼 이벤트인데, 유니티 에디터에서 등록하면
    // 나중에 어떤 프리팹에서 오는건지 찾기도 힘들고, 관리도 힘드니까, 이렇게 코드에서 등록해준다
    public void OnClick_GameStart()
    {
        // 게임 시작에 대한 처리를 여기서 몰아서 해줄수가 있게 된다.
        // DaniTechGameManager.Inst.게임 시작할때 맵구성이라던가 부가적인거 여기서 해도 됩니다.

        DaniTechUIManager.Instance.CloseContentUI(DaniTechUIType.DNRobbyUI);
    }

    public void OnClick_GameQuit()
    {
        // 게임 종료처리를 여기서 몰아서 해줄수가 있게 된다.
        // 게임매니저 만들어놨으니까 개한테만 부탁 한다. 끝
        DaniTechGameManager.Inst.SaveAndEndGame();
    }


}
