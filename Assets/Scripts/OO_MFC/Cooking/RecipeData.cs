using System;
using System.Collections.Generic;

[Serializable]
public class RecipeData : GameDataBase
{
    public string Name;
    public string Description;
    public string ResultItemId;                    // 요리 성공 시 생성되는 아이템 ID
    public List<string> RequiredIngredients;       // 필요 재료 ID 목록
    public int MaxDuplicateCount = 1;              // 같은 재료를 몇 번까지 허용할지 (기본 1)
    public string RequiredTool;                    // "가마솥", "절구", "도마" 
}