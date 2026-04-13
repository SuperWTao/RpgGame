using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class BattleEntity
{
    public int entityId { get; }
    public string name { get; }

    public int maxHp { get; private set; }
    public int currentHp { get; private set; }

    public int attack { get; private set; }
    public int defense { get; private set; }

    public bool isDead => currentHp <= 0;

    // 预留：Buff/标签/阵营等在后续步骤接入
    public HashSet<string> tags { get; } = new HashSet<string>();

    public BattleEntity(
        int entityId,
        string name,
        int maxHp,
        int attack,
        int defense)
    {
        if (entityId <= 0) throw new ArgumentException("entityId must be > 0");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name is required");

        this.entityId = entityId;
        this.name = name;
        this.maxHp = Mathf.Max(1, maxHp);
        currentHp = this.maxHp;
        this.attack = Mathf.Max(0, attack);
        this.defense = Mathf.Max(0, defense);
    }

    public void SetBaseStats(int maxHp, int attack, int defense, bool resetHpToFull = false)
    {
        this.maxHp = Mathf.Max(1, maxHp);
        this.attack = Mathf.Max(0, attack);
        this.defense = Mathf.Max(0, defense);

        if (resetHpToFull)
        {
            currentHp = this.maxHp;
        }
        else
        {
            currentHp = Mathf.Clamp(currentHp, 0, this.maxHp);
        }
    }

    public int ApplyDamage(int value)
    {
        int damage = Mathf.Max(0, value);
        if (damage == 0 || isDead) return 0;

        int oldHp = currentHp;
        currentHp = Mathf.Max(0, currentHp - damage);
        return oldHp - currentHp;
    }

    public int ApplyHeal(int value)
    {
        int heal = Mathf.Max(0, value);
        if (heal == 0 || isDead) return 0;

        int oldHp = currentHp;
        currentHp = Mathf.Min(maxHp, currentHp + heal);
        return currentHp - oldHp;
    }
}