using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public static class GameUtil
{
    public static void LoadFullData()
    {
        // 게임 로딩할 때 불러올 데이터는 여기서! 
        GameDataManager.Instance.LoadSkillData(GetFullDataPath("Skill"));
        GameDataManager.Instance.LoadCharacterData(GetFullDataPath("Character"));
        GameDataManager.Instance.LoadWeaponData(GetFullDataPath("Weapon"));
        GameDataManager.Instance.LoadCostumeData(GetFullDataPath("Costume"));
        GameDataManager.Instance.LoadDNItemData(GetFullDataPath("DNItem"));
    }

    public static string GetFullDataPath(string dataTableName)
    {
        if (string.IsNullOrEmpty(dataTableName))
        {
            Debug.Log("테이블 이름이 올바르지 않습니다!");
            return string.Empty;
        }

        // string path = @"C:예시-여러분의 상위 폴더\ComputerBasics_DaniTech_Work\JsonConverter\JsonOutput\Character.json";
        // 제이슨 컨버터를 한번 실행해서 JsonCoutput에 Character.json을 미리 만들어뒀는지도 꼭 확인해주세요!
        // 상대경로 ../../ 추후 툴 이름이나 경로가 변경되면 여길 확인하세요!
        string relativePath = $"JsonConverter/JsonOutput/{dataTableName}.json";
        string fullPath = Path.GetFullPath(relativePath);
        return fullPath;
    }


    public static int CalcCharacterFinalDamage(int curCharacterLevel, int levelPerDamage, bool isCritical)
    {
        int damagePerLevel = (curCharacterLevel + levelPerDamage);
        int finalDamage = isCritical ? (damagePerLevel * 2) : damagePerLevel;
        return finalDamage;
    }

    public static Sprite LoadSpriteCanBeNull(string spriteName)
    {
        // 1. Resources/ 경로에서 이름으로 스프라이트 로드
        // 예: spriteName이 "Sword"라면 Assets/Resources/2D/Sword.png를 찾음
        // 이 2D같은 경로는 나중에 Sprite, Texture 등등 다양하게 바꿔도 무관합니다!
        Sprite loadedSprite = Resources.Load<Sprite>($"{spriteName}");

        if (loadedSprite != null)
        {
            return loadedSprite;
        }

        Debug.LogError($"에셋을 찾을 수 없습니다: {spriteName}");
        return null;
    }

    public static async UniTask<Sprite> LoadAndSetSpriteImage(Image targetImage, string spritePath)
    {
        Sprite sprite = await DaniTechResourceManager.Inst.LoadSprite(spritePath);
        if (sprite != null)
        {
            targetImage.sprite = sprite;
        }
        return sprite;
    }
}