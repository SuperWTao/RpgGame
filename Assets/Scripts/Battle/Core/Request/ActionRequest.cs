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
    public long requestId;

    public BattleActionType actionType;

    // 行动发起者
    public int sourceEntityId;

    // 主目标（可选）
    public int mainTargetEntityId;

    // 技能ID（ActionType=Skill时使用）
    public int skillId;

    // 指向位置（AOE/位移技能可用）
    public Vector3 targetPoint;

    // 调试与同步字段
    public int clientFrame;
    public string debugReason;

    public static ActionRequest CreateNormalAttack(long requestId, int sourceId, int targetId)
    {
        return new ActionRequest
        {
            requestId = requestId,
            actionType = BattleActionType.NormalAttack,
            sourceEntityId = sourceId,
            mainTargetEntityId = targetId,
            skillId = 0,
            targetPoint = Vector3.zero,
            clientFrame = 0,
            debugReason = "normal_attack"
        };
    }

    public static ActionRequest CreateSkill(long requestId, int sourceId, int targetId, int skillId, Vector3 targetPoint)
    {
        return new ActionRequest
        {
            requestId = requestId,
            actionType = BattleActionType.Skill,
            sourceEntityId = sourceId,
            mainTargetEntityId = targetId,
            skillId = skillId,
            targetPoint = targetPoint,
            clientFrame = 0,
            debugReason = "skill_cast"
        };
    }
}