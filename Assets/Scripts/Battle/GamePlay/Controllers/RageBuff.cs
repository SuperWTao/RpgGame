public sealed class RageBuff : IBattleBuff
{
    public string Id => "rage_buff";
    public bool IsExpired => _remainingActions <= 0;

    private int _remainingActions;
    private readonly float _bonusPercent;

    public RageBuff(int actionCount, float bonusPercent)
    {
        _remainingActions = actionCount < 1 ? 1 : actionCount;
        _bonusPercent = bonusPercent;
    }

    public void OnAdd(int ownerEntityId) { }

    public void OnRemove(int ownerEntityId) { }

    public void OnPreResolve(ActionExecutionContext ctx, int ownerEntityId, bool ownerIsSource, DamageModifierChain chain)
    {
        if (!ownerIsSource) return;
        chain.Add(new RagePercentDamageModifier(_bonusPercent));
    }

    public void OnPostResolve(ActionExecutionContext ctx, int ownerEntityId, bool ownerIsSource) { }

    public void Tick()
    {
        _remainingActions--;
    }
}