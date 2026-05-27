using System;
using System.Collections.Generic;

[Serializable]
public class OO_Character : GameDataBase
{
    public string Name;                    // 캐릭터 이름
    public string Description;             // 캐릭터 컨셉 및 스토리 설명
    public string ProfileImagePath;        // 프로필 이미지 경로
    public string BasicCostumeId;          // 기본 의상 ID
    public List<string> DefaultSkillList;  // 기본 스킬 목록
}