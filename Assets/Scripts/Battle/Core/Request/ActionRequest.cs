using UnityEngine;

public enum BattleActionType
{
    NormalAttack = 0,
    Skill = 1,
    Item = 2,
    BuffTick = 3
}

public sealed class ActionRequest
{
    // 客户端或AI提交时生成
    public long RequestId;

    public BattleActionType ActionType;

    // 行动发起者
    public int SourceEntityId;

    // 主目标（可选）
    public int MainTargetEntityId;

    // 技能ID（ActionType=Skill时使用）
    public int SkillId;

    // 指向位置（AOE/位移技能可用）
    public Vector3 TargetPoint;

    // 调试与同步字段
    public int ClientFrame;
    public string DebugReason;

    public static ActionRequest CreateNormalAttack(long requestId, int sourceId, int targetId)
    {
        return new ActionRequest
        {
            RequestId = requestId,
            ActionType = BattleActionType.NormalAttack,
            SourceEntityId = sourceId,
            MainTargetEntityId = targetId,
            SkillId = 0,
            TargetPoint = Vector3.zero,
            ClientFrame = 0,
            DebugReason = "normal_attack"
        };
    }

    public static ActionRequest CreateSkill(long requestId, int sourceId, int targetId, int skillId, Vector3 targetPoint)
    {
        return new ActionRequest
        {
            RequestId = requestId,
            ActionType = BattleActionType.Skill,
            SourceEntityId = sourceId,
            MainTargetEntityId = targetId,
            SkillId = skillId,
            TargetPoint = targetPoint,
            ClientFrame = 0,
            DebugReason = "skill_cast"
        };
    }
}