// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OO_Stage4CueSheet.cs
// - 역할: Stage4_1/Stage4_2의 배우, 대사, 임무, 아이템 ID를 모아 둔 큐시트 데이터입니다.
// - 영화 비유: 감독이 들고 있는 촬영 순서표입니다. 배우를 직접 붙잡지 않고 "이 ID의 대사, 이 ID의 소품"만 지시합니다.
// - 유지보수 포인트: Stage4 연출 순서가 바뀌면 Controller보다 OO_Stage4CueSheet.xlsx를 먼저 수정합니다.
// =============================================================================
using System;

/// <summary>
/// Stage4 토끼와 거북이 퀘스트의 핵심 ID 묶음입니다.
/// Game View에서는 Controller가 이 값을 읽어 Turtle 대화, Rabbit 말풍선, 당근전 제작 흐름을 진행합니다.
/// </summary>
[Serializable]
public class OO_Stage4CueSheet : GameDataBase
{
    public string Name;
    public string Description;
    public string Stage4_1GroupName;
    public string Stage4_2GroupName;
    public string PreFinalGroupName;

    public string TurtleRoleId;
    public string RabbitRoleId;
    public string SleepingRabbitRoleId;
    public string MoranRoleId;
    public string StumpRoleId;
    public string Stump2RoleId;
    public string StopPointARoleId;

    public string TurtleDialogueIdList;
    public string TurtleClearDialogueIdList;
    public string RabbitIntroDialogueId;
    public string RabbitStopDialogueId;
    public string RabbitCarrotCakeDialogueIdList;
    public string RabbitSpeechBubbleIdList;
    public string RabbitSleepingBubbleId;

    public string StageQuestId;
    public string CarrotIngredientId;
    public string KoreanCakeItemId;
    public string CarrotStarchItemId;
    public string CarrotCakeItemId;

    public string Stage4BGMPath;
    public string NextTutorialNarrationId;

    public float InteractionDistance = 95f;
    public float RabbitRunSpeed = 480f;
    public float RabbitReachTimeoutSeconds = 3f;
}
