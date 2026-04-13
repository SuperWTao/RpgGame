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
        _modifiers.Sort((a, b) => a.priority.CompareTo(b.priority));

        for (int i = 0; i < _modifiers.Count; i++)
        {
            var modifier = _modifiers[i];
            if (modifier.CanApply(ctx, packet))
            {
                modifier.Apply(ctx, packet);
            }
        }

        float value = (packet.baseDamage + packet.flatAdd) * (1f + packet.percentAdd);
        value -= packet.flatReduce;

        int finalDamage = (int)Math.Round(value);
        if (finalDamage < packet.minDamage) finalDamage = packet.minDamage;
        if (finalDamage > packet.maxDamage) finalDamage = packet.maxDamage;
        if (finalDamage < 0) finalDamage = 0;

        packet.finalDamage = finalDamage;
        return finalDamage;
    }
}