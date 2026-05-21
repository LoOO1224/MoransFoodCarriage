/*using UnityEngine;

public enum UIType
{
    SimplePopup,
    MainUI,
    MyProfilePopup, // 신규UI추가 1) 새로운 UIType을 추가한다
    DaniTechInventory
}

public static class UIManagerExtension
{
    public static string GetUIPath(this DaniTechUIManager uiManager, UIType uiType)
    {
        string path = string.Empty; // "" == string.Empty
        switch (uiType)
        {
            case UIType.SimplePopup:
                path = "Prefabs/UI/SimplePopup";
                break;
            // 신규UI추가 2) Resources.Load를 할 경로를 직접 명시한다
                // 해당 경로는 프로젝트창에서 Resources/Prefabs/UI에 있는 프리팹 이름과 대응된다 (MyProfilePopup)
            case UIType.MyProfilePopup: 
                path = "Prefabs/UI/MyProfilePopup";
                break;
            case UIType.DaniTechInventory:
                path = "Prefabs/UI/SampleUI_InventoryStyle";
                break;
        }

        return path;
    }

    public static void OpenSimplePopup(this DaniTechUIManager uiManager, string msg)
    {
        UIType openUiType = UIType.SimplePopup;
        var gObj = uiManager.GetCreatedUI(openUiType);

        if (gObj != null)
        {
            uiManager.OpenUI(openUiType, gObj);

            var simplePopup = gObj.GetComponent<SimplePopup>();
            if (simplePopup == null)
            {
                return;
            }

            simplePopup.SetUI(msg);
        }
    }

    // 신규UI추가 3) 이렇게 어떤 팝업을 열고, 열때 전달해야하는 파라미터가 있다면 이렇게 전달한다.
        // 추가하기 편하게 그냥 빼둔 확장 메서드이므로, uiManager과 this는 우선 넘어가자
    public static void OpenMyProfilePopup(this DaniTechUIManager uiManager, string characterDataId)
    {
        // 신규UI추가 4) 이렇게 UI 타입을 던져서 UI 생성을 요청한다
        UIType openUiType = UIType.MyProfilePopup;
        var gObj = uiManager.GetCreatedUI(openUiType);

        if (gObj != null)
        {
            // 신규UI추가 5) 생성이 되었다면, 이제 해당 UI를 활성화(Active)하는 메서드를 호출한다
                // 이미Active 상태로 저장되었을 수 있지만, 그래도 혹시 수정중에 꺼져있을 수 있으니 확실하게 켜준다
            uiManager.OpenUI(openUiType, gObj);

            // 신규UI추가 6) 열때 해줘야하는 작업이 있다면 이렇게 해당 오브젝트에서 UI 컴포넌트를 가져오려고 시도한다 
            var simplePopup = gObj.GetComponent<MyProfilePopup>();
            if (simplePopup == null)
            {
                return;
            }

            simplePopup.RefreshCharacterUI(characterDataId);
        }
    }

    public static void OpenInventoryPopup(this DaniTechUIManager uiManger)
    {
        UIType openUiType = UIType.DaniTechInventory;
        var gObj = uiManger.GetCreatedUI(openUiType);

        if(gObj != null)
        {
            uiManger.OpenUI(openUiType, gObj);

            //var simplePopup = gObj.GetComponent<SimplePopup>();
            //if (simplePopup == null)
            //{
            //    return;
            //}

            //simplePopup.SetUI(msg);
        }
    }
}
*/

