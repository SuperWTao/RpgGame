public interface IBattlePassive
{
    string Id { get; }

    void OnPreResolve(ActionExecutionContext ctx, int ownerEntityId, bool ownerIsSource, DamageModifierChain chain);
    void OnPostResolve(ActionExecutionContext ctx, int ownerEntityId, bool ownerIsSource);
}