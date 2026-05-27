using System;

[Serializable]
public class OO_Ingredient : GameDataBase
{
    public string Name;                        // 재료 이름
    public string Description;                 // 재료 설명
    public string IconPath;                    // 아이콘 이미지 경로
    public string Grade;                       // 등급 (일반, 좋음, 희귀, 신화)
    public int MaxStackCount;                  // 최대 중첩 수
}