using System;
using System.Collections.Generic;

[Serializable]
public class OO_Recipe : GameDataBase
{
    public string Name;                        // 요리 이름
    public string Description;                 // 요리 설명
    public string ResultItemId;                // 완성 후 생성되는 아이템 ID
    public List<string> RequiredIngredients;   // 필요 재료 ID 목록
    public int MaxDuplicateCount = 1;          // 같은 재료 허용 횟수
    public string RequiredTool;                // 필요한 도구 (가마솥, 절구 등)
}