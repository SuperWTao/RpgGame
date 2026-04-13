public sealed class ActionExecutionContext
{
    public BattleContext battle;
    public ActionRequest request;
    public ActionResult result;

    public BattleEntity source;
    public BattleEntity target;

    // 第二步字段保留，兼容旧逻辑
    public int pendingDamage;
    public int pendingHeal;

    // 第三步新增
    public DamagePacket damagePacket;
    public DamageModifierChain damageChain;

    public ActionExecutionContext(BattleContext battle, ActionRequest request)
    {
        this.battle = battle;
        this.request = request;

        result = new ActionResult
        {
            requestId = request.requestId,
            sourceEntityId = request.sourceEntityId,
            mainTargetEntityId = request.mainTargetEntityId,
            code = ActionResultCode.UnknownError,
            message = "not executed"
        };

        damagePacket = new DamagePacket
        {
            requestId = request.requestId,
            sourceEntityId = request.sourceEntityId,
            targetEntityId = request.mainTargetEntityId,
            actionType = request.actionType,
            damageType = DamageType.Physical
        };

        damageChain = new DamageModifierChain();
    }
}