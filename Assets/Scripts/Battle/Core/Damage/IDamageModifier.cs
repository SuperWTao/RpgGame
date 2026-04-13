public interface IDamageModifier
{
    int priority { get; }

    bool CanApply(ActionExecutionContext ctx, DamagePacket packet);

    void Apply(ActionExecutionContext ctx, DamagePacket packet);
}