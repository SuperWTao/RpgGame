public sealed class RagePercentDamageModifier : IDamageModifier
{
    private readonly float _percent;
    public int Priority => 250;

    public RagePercentDamageModifier(float percent)
    {
        _percent = percent;
    }

    public bool CanApply(ActionExecutionContext ctx, DamagePacket packet)
    {
        return packet.ActionType == BattleActionType.NormalAttack
            || packet.ActionType == BattleActionType.Skill;
    }

    public void Apply(ActionExecutionContext ctx, DamagePacket packet)
    {
        packet.PercentAdd += _percent;
    }
}