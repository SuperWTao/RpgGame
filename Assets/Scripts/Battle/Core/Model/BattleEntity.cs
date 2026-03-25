using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class BattleEntity
{
    public int EntityId { get; }
    public string Name { get; }

    public int MaxHp { get; private set; }
    public int CurrentHp { get; private set; }

    public int Attack { get; private set; }
    public int Defense { get; private set; }

    public bool IsDead => CurrentHp <= 0;

    // 预留：Buff/标签/阵营等在后续步骤接入
    public HashSet<string> Tags { get; } = new HashSet<string>();

    public BattleEntity(
        int entityId,
        string name,
        int maxHp,
        int attack,
        int defense)
    {
        if (entityId <= 0) throw new ArgumentException("entityId must be > 0");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name is required");

        EntityId = entityId;
        Name = name;
        MaxHp = Mathf.Max(1, maxHp);
        CurrentHp = MaxHp;
        Attack = Mathf.Max(0, attack);
        Defense = Mathf.Max(0, defense);
    }

    public void SetBaseStats(int maxHp, int attack, int defense, bool resetHpToFull = false)
    {
        MaxHp = Mathf.Max(1, maxHp);
        Attack = Mathf.Max(0, attack);
        Defense = Mathf.Max(0, defense);

        if (resetHpToFull)
        {
            CurrentHp = MaxHp;
        }
        else
        {
            CurrentHp = Mathf.Clamp(CurrentHp, 0, MaxHp);
        }
    }

    public int ApplyDamage(int value)
    {
        int damage = Mathf.Max(0, value);
        if (damage == 0 || IsDead) return 0;

        int oldHp = CurrentHp;
        CurrentHp = Mathf.Max(0, CurrentHp - damage);
        return oldHp - CurrentHp;
    }

    public int ApplyHeal(int value)
    {
        int heal = Mathf.Max(0, value);
        if (heal == 0 || IsDead) return 0;

        int oldHp = CurrentHp;
        CurrentHp = Mathf.Min(MaxHp, CurrentHp + heal);
        return CurrentHp - oldHp;
    }
}