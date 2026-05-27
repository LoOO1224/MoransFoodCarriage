using System;

[Serializable]
public class OO_WorldMapArea : GameDataBase
{
    public string Name;                        // 지역 이름 (동쪽 마을 등)
    public string BackgroundImagePath;         // 월드맵 배경 이미지
    public string StageId;                     // 진입하는 스테이지 ID
    public float MapPositionX;                 // 월드맵 X 좌표
    public float MapPositionY;                 // 월드맵 Y 좌표
}