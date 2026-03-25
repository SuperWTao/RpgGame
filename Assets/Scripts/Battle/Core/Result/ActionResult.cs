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
    public long RequestId;

    public ActionResultCode Code;
    public bool Success => Code == ActionResultCode.Success;

    public int SourceEntityId;
    public int MainTargetEntityId;

    // 第一阶段先保留基础数值，后续第三阶段接DamagePacket
    public int DamageApplied;
    public int HealApplied;

    public string Message;

    public static ActionResult Fail(long requestId, ActionResultCode code, string message)
    {
        return new ActionResult
        {
            RequestId = requestId,
            Code = code,
            Message = message,
            DamageApplied = 0,
            HealApplied = 0
        };
    }

    public static ActionResult Ok(long requestId, int sourceId, int targetId, int damageApplied, int healApplied, string message = "")
    {
        return new ActionResult
        {
            RequestId = requestId,
            Code = ActionResultCode.Success,
            SourceEntityId = sourceId,
            MainTargetEntityId = targetId,
            DamageApplied = damageApplied,
            HealApplied = healApplied,
            Message = message
        };
    }
}