using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 行动点，一次战斗行动会触发战斗实体一系列的行动点
/// </summary>
public sealed class ActionPoint
{
    public List<Action<CombatAction>> listeners = new List<Action<CombatAction>>();
}

public enum ActionPointType
{
    PreCauseDamage,//造成伤害前
    PreReceiveDamage,//承受伤害前

    PostCauseDamage,//造成伤害后
    PostReceiveDamage,//承受伤害后
}

/// <summary>
/// 行动管理器，管理一个战斗实体所有行动点的添加监听、移除监听、触发流程
/// </summary>
public class ActionPointManager
{
    private Dictionary<ActionPointType, ActionPoint> _actionPoints = new Dictionary<ActionPointType, ActionPoint>();

    public ActionPointManager()
    {
        _actionPoints.Add(ActionPointType.PreCauseDamage, new ActionPoint());
        _actionPoints.Add(ActionPointType.PreReceiveDamage, new ActionPoint());
        _actionPoints.Add(ActionPointType.PostCauseDamage, new ActionPoint());
        _actionPoints.Add(ActionPointType.PostReceiveDamage, new ActionPoint());
    }

    public void AddListener(ActionPointType type, Action<CombatAction> action)
    {
        _actionPoints[type].listeners.Add(action);
    }
    
    public void RemoveListener(ActionPointType type, Action<CombatAction> action)
    {
        _actionPoints[type].listeners.Remove(action);
    }

    public void TriggerActionPoint(ActionPointType type, CombatAction action)
    {
        foreach (var item  in _actionPoints[type].listeners)
        {
            item.Invoke(action);
        }
    }
}
