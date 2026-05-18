using UnityEngine;

public class DaniTechMainUI : MonoBehaviour
{
    [SerializeField] private DaniTechUIButton Btn_MyProfile;
    [SerializeField] private DaniTechUIButton Btn_StartBattle;
    [SerializeField] private DaniTechUIButton Btn_MonsterSpawn;

    private void OnEnable()
    {
        Btn_MyProfile.BindOnClickButtonEvent(OnClick_OpenMyProfile);
        Btn_StartBattle.BindOnClickButtonEvent(OnClick_StartBattle);
        Btn_MonsterSpawn.BindOnClickButtonEvent(OnClicK_MonsterSpawn);
    }

    public void OnClick_OpenMyProfile()
    {
        //UIManager.Instance.OpenMyProfilePopup("character_hellena_01");
        DaniTechUIManager.Instance.OpenInventoryPopup();
        Debug.LogWarning("프로필 오픈");
    }

    public void OnClick_StartBattle()
    {
        DaniTechUIManager.Instance.OpenSimplePopup("배틀 스타트!");
        Debug.LogWarning("배틀 스타트");
    }

    public void OnClicK_MonsterSpawn()
    {
        Debug.LogWarning("몬스터 스폰");
    }



}
