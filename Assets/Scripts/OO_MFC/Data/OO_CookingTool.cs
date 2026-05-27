using System;

[Serializable]
public class OO_CookingTool : GameDataBase
{
    public string Name;                        // 도구 이름
    public string Description;                 // 도구 설명
    public string IconPath;                    // 아이콘 경로
    public string UnlockCondition;             // 해금 조건
}