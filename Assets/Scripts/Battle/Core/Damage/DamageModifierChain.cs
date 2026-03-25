using System;
using System.Collections.Generic;

public sealed class DamageModifierChain
{
    private readonly List<IDamageModifier> _modifiers = new List<IDamageModifier>();

    public void Clear()
    {
        _modifiers.Clear();
    }

    public void Add(IDamageModifier modifier)
    {
        if (modifier == null) return;
        _modifiers.Add(modifier);
    }

    public void AddRange(IEnumerable<IDamageModifier> modifiers)
    {
        if (modifiers == null) return;
        foreach (var m in modifiers)
        {
            Add(m);
        }
    }

    public int Resolve(ActionExecutionContext ctx, DamagePacket packet)
    {
        _modifiers.Sort((a, b) => a.Priority.CompareTo(b.Priority));

        for (int i = 0; i < _modifiers.Count; i++)
        {
            var modifier = _modifiers[i];
            if (modifier.CanApply(ctx, packet))
            {
                modifier.Apply(ctx, packet);
            }
        }

        float value = (packet.BaseDamage + packet.FlatAdd) * (1f + packet.PercentAdd);
        value -= packet.FlatReduce;

        int finalDamage = (int)Math.Round(value);
        if (finalDamage < packet.MinDamage) finalDamage = packet.MinDamage;
        if (finalDamage > packet.MaxDamage) finalDamage = packet.MaxDamage;
        if (finalDamage < 0) finalDamage = 0;

        packet.FinalDamage = finalDamage;
        return finalDamage;
    }
}