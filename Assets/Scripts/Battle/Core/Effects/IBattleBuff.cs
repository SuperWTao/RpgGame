public interface IBattleBuff
{
    string Id { get; }
    bool IsExpired { get; }

    void OnAdd(int ownerEntityId);
    void OnRemove(int ownerEntityId);

    void OnPreResolve(ActionExecutionContext ctx, int ownerEntityId, bool ownerIsSource, DamageModifierChain chain);
    void OnPostResolve(ActionExecutionContext ctx, int ownerEntityId, bool ownerIsSource);

    void Tick();
}