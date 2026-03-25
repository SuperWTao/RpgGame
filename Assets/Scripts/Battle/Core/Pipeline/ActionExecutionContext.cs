public sealed class ActionExecutionContext
{
    public BattleContext Battle;
    public ActionRequest Request;
    public ActionResult Result;

    public BattleEntity Source;
    public BattleEntity Target;

    // 第二步字段保留，兼容旧逻辑
    public int PendingDamage;
    public int PendingHeal;

    // 第三步新增
    public DamagePacket DamagePacket;
    public DamageModifierChain DamageChain;

    public ActionExecutionContext(BattleContext battle, ActionRequest request)
    {
        Battle = battle;
        Request = request;

        Result = new ActionResult
        {
            RequestId = request.RequestId,
            SourceEntityId = request.SourceEntityId,
            MainTargetEntityId = request.MainTargetEntityId,
            Code = ActionResultCode.UnknownError,
            Message = "not executed"
        };

        DamagePacket = new DamagePacket
        {
            RequestId = request.RequestId,
            SourceEntityId = request.SourceEntityId,
            TargetEntityId = request.MainTargetEntityId,
            ActionType = request.ActionType,
            DamageType = DamageType.Physical
        };

        DamageChain = new DamageModifierChain();
    }
}