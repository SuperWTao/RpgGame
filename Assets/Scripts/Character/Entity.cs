using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public HealthPoint CurrentHealth { get; private set; } = new HealthPoint();
    public ActionPointManager actionPointManager = new ActionPointManager();

    public void Init()
    {
        // TODO:后续从配表读取
        CurrentHealth.SetMaxValue(1);
        CurrentHealth.Reset();
    }
    public void ReceiveDamage(CombatAction combatAction)
    {
        var damageAction = combatAction as DamageAction;
        CurrentHealth.Minus(damageAction.damageValue);
    }

    public void ReceiveCure(CombatAction combatAction)
    {
        // 治疗
        var cureAction = combatAction as CureAction;
        CurrentHealth.Add(cureAction.curValue);
    }
    
    public void AddListener(ActionPointType type, Action<CombatAction> action)
    {
        actionPointManager.AddListener(type, action);
    }
    
    public void RemoveListener(ActionPointType type, Action<CombatAction> action)
    {
        actionPointManager.RemoveListener(type, action);
    }
    
    public void TriggerActionPoint(ActionPointType type, CombatAction action)
    {
        actionPointManager.TriggerActionPoint(type, action);
    }
}
