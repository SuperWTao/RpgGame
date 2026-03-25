using UnityEngine;

public sealed class ExecutionHealPassive : IBattlePassive
{
    public string Id => "execution_heal_passive";

    private readonly int _healValueOnKillPreview;

    public ExecutionHealPassive(int healValueOnKillPreview)
    {
        _healValueOnKillPreview = healValueOnKillPreview < 0 ? 0 : healValueOnKillPreview;
    }

    public void OnPreResolve(ActionExecutionContext ctx, int ownerEntityId, bool ownerIsSource, DamageModifierChain chain) { }

    public void OnPostResolve(ActionExecutionContext ctx, int ownerEntityId, bool ownerIsSource)
    {
        if (!ownerIsSource) return;
        Debug.Log("[ExecutionHealPassive] OnPostResolve: ownerEntityId=" + ownerEntityId);
        int hpAfterDamagePreview = ctx.Target.CurrentHp - ctx.PendingDamage;
        if (hpAfterDamagePreview <= 0)
        {
            ctx.PendingHeal += _healValueOnKillPreview;
        }
    }
}