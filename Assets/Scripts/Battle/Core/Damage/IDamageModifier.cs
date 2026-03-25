public interface IDamageModifier
{
    int Priority { get; }

    bool CanApply(ActionExecutionContext ctx, DamagePacket packet);

    void Apply(ActionExecutionContext ctx, DamagePacket packet);
}