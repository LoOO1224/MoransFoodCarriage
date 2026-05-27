using System;

[Serializable]
public class OO_Codex : GameDataBase
{
    public string Category;                    // "Character", "Food", "Ingredient", "Story", "Region"
    public string Title;                       // 도감 제목
    public string Description;                 // 도감 설명
    public string ImagePath;                   // 도감 이미지 경로
    public string UnlockCondition;             // 해금 조건
}