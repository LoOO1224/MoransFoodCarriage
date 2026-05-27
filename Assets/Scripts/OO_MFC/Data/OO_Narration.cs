using System;
using System.Collections.Generic;

[Serializable]
public class OO_Narration : GameDataBase
{
    public string Title;                       // 나레이션 제목 (프롤로그 1 등)
    public int PartNumber;                     // 파트 번호
    public List<string> NarrationTexts;        // 나레이션 텍스트 목록 (순서대로)
    public List<string> BackgroundImagePaths;  // 각 파트별 배경 이미지 경로
    public string BGMPath;                     // 나레이션 동안 재생할 BGM
    public string NextGroup;                   // 다음으로 넘어갈 그룹 (TutorialGroup 등)
}