using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class GameDataTester
{
    public static void StartDataTest()
    {
        DaniTechGameUtil.LoadFullData();


        var myCostume = DaniTechGameDataManager.Instance.GetCostumeData("Costume_02");
        Debug.Log(myCostume.Name);


        // 딕셔너리도 포이치 가능 (순서는 보장되지 않을 수 있지만)
        foreach(var kv in DaniTechGameDataManager.Instance.CostumeDataList) 
        {
            string key = kv.Key;
            var data = kv.Value;
            Debug.Log($"키는 {key} 데이터의 이름 : {data.Name} ");
        }

        // 2. 데이터 사용 (어디서나 호출 가능)
        var myHero = DaniTechGameDataManager.Instance.GetCharacterData("character_hellena_01");

        if (myHero != null)
        {
            Debug.Log($"로드된 캐릭터 이름: {myHero.Name}");
        }


        DNCostumeData heroCostumeData = DaniTechGameDataManager.Instance.GetCostumeData(myHero.BasicCostumeId);
        // 굳이? 그냥 없으면 넘어갈게요~~~!
        if(heroCostumeData != null) {
            Debug.Log(heroCostumeData.Name);
        }


        // 스킬 정보가 있다면
        if (myHero.SkillList != string.Empty)
        {
            string[] skillNameList = myHero.SkillList.Split(',');
            foreach (string skillName in skillNameList)
            {
                var skillData = DaniTechGameDataManager.Instance.GetSkill(skillName);
                if (skillData != null)
                {
                    Debug.Log($"로드된 캐릭터: {myHero.Name}는 {skillData.Name}을 갖고 있다!");
                }
            }
        }

        if (string.IsNullOrEmpty(myHero.UseWeaponId) == false)
        {
            var weaponData = DaniTechGameDataManager.Instance.GetWeaponData(myHero.UseWeaponId);
            if (weaponData != null)
            {
                Debug.Log($"로드된 캐릭터: {myHero.Name}는 사용무기로 {weaponData.Name}을 갖고 있다!");
            }
        }
    }
}
