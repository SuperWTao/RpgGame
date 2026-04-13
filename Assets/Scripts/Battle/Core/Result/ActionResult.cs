public enum ActionResultCode
{
    Success = 0,
    InvalidSource = 1,
    InvalidTarget = 2,
    SourceDead = 3,
    TargetDead = 4,
    InvalidSkill = 5,
    OutOfRange = 6,
    Cooldown = 7,
    UnknownError = 99
}

public sealed class ActionResult
{
    public long requestId;

    public ActionResultCode code;
    public bool success => code == ActionResultCode.Success;

    public int sourceEntityId;
    public int mainTargetEntityId;

    // 第一阶段先保留基础数值，后续第三阶段接DamagePacket
    public int damageApplied;
    public int healApplied;

    public string message;

    public static ActionResult Fail(long requestId, ActionResultCode code, string message)
    {
        return new ActionResult
        {
            requestId = requestId,
            code = code,
            message = message,
            damageApplied = 0,
            healApplied = 0
        };
    }

    public static ActionResult Ok(long requestId, int sourceId, int targetId, int damageApplied, int healApplied, string message = "")
    {
        return new ActionResult
        {
            requestId = requestId,
            code = ActionResultCode.Success,
            sourceEntityId = sourceId,
            mainTargetEntityId = targetId,
            damageApplied = damageApplied,
            healApplied = healApplied,
            message = message
        };
    }
}