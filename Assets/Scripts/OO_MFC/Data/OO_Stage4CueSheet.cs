// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OO_Stage4CueSheet.cs
// - 역할: JSON/Excel에서 로드되는 정적 데이터 한 행을 표현합니다.
// - 유지보수: 필드명은 JsonConverter 결과와 맞아야 하므로 이름 변경 시 Excel, JSON, GameDataManager 매핑을 함께 확인합니다.
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
    public string RoadMissionDataId;
    public string RoadMissionFallbackText;

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
