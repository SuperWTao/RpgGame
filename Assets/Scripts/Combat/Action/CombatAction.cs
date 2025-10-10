using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 战斗行动概念，造成伤害、治疗英雄、赋给效果等属于战斗行动，需要继承自CombatAction
/// 战斗行动由战斗实体主动发起，包含本次行动所需要用到的所有数据，并且会触发一系列行动点事件
/// </summary>
public class CombatAction
{
    public ActionType type;
    public Entity creator;
    public Entity target;
    
    public CombatAction(ActionType type, Entity creator, Entity target)
    {
        this.type = type;
        this.creator = creator;
        this.target = target;
    }
    public virtual void ApplyAction()
    {
        
    }
}

public enum ActionType
{
    Damage,
    Cure,
    Skill
}
