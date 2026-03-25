using System.Collections.Generic;

public sealed class BattleEffectRegistry
{
    private readonly Dictionary<int, List<IBattleBuff>> _buffs = new Dictionary<int, List<IBattleBuff>>();
    private readonly Dictionary<int, List<IBattlePassive>> _passives = new Dictionary<int, List<IBattlePassive>>();

    public void AddBuff(int ownerEntityId, IBattleBuff buff)
    {
        if (buff == null) return;
        if (!_buffs.TryGetValue(ownerEntityId, out var list))
        {
            list = new List<IBattleBuff>();
            _buffs[ownerEntityId] = list;
        }

        list.Add(buff);
        buff.OnAdd(ownerEntityId);
    }

    public void AddPassive(int ownerEntityId, IBattlePassive passive)
    {
        if (passive == null) return;
        if (!_passives.TryGetValue(ownerEntityId, out var list))
        {
            list = new List<IBattlePassive>();
            _passives[ownerEntityId] = list;
        }

        list.Add(passive);
    }

    public void ApplyPreResolve(ActionExecutionContext ctx)
    {
        ApplyPreResolveForOwner(ctx, ctx.Source.EntityId, true);
        ApplyPreResolveForOwner(ctx, ctx.Target.EntityId, false);
    }

    public void ApplyPostResolve(ActionExecutionContext ctx)
    {
        ApplyPostResolveForOwner(ctx, ctx.Source.EntityId, true);
        ApplyPostResolveForOwner(ctx, ctx.Target.EntityId, false);
    }

    public void TickAndCleanup()
    {
        foreach (var kv in _buffs)
        {
            var list = kv.Value;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                list[i].Tick();
                if (list[i].IsExpired)
                {
                    list[i].OnRemove(kv.Key);
                    list.RemoveAt(i);
                }
            }
        }
    }

    private void ApplyPreResolveForOwner(ActionExecutionContext ctx, int ownerEntityId, bool ownerIsSource)
    {
        if (_buffs.TryGetValue(ownerEntityId, out var buffList))
        {
            for (int i = 0; i < buffList.Count; i++)
            {
                buffList[i].OnPreResolve(ctx, ownerEntityId, ownerIsSource, ctx.DamageChain);
            }
        }

        if (_passives.TryGetValue(ownerEntityId, out var passiveList))
        {
            for (int i = 0; i < passiveList.Count; i++)
            {
                passiveList[i].OnPreResolve(ctx, ownerEntityId, ownerIsSource, ctx.DamageChain);
            }
        }
    }

    private void ApplyPostResolveForOwner(ActionExecutionContext ctx, int ownerEntityId, bool ownerIsSource)
    {
        if (_buffs.TryGetValue(ownerEntityId, out var buffList))
        {
            for (int i = 0; i < buffList.Count; i++)
            {
                buffList[i].OnPostResolve(ctx, ownerEntityId, ownerIsSource);
            }
        }

        if (_passives.TryGetValue(ownerEntityId, out var passiveList))
        {
            for (int i = 0; i < passiveList.Count; i++)
            {
                passiveList[i].OnPostResolve(ctx, ownerEntityId, ownerIsSource);
            }
        }
    }
}