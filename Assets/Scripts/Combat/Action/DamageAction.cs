using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageAction : CombatAction
{
    public int damageValue;
    
    public DamageAction(ActionType type, Entity creator, Entity target, int damageValue) : base(type, creator, target)
    {
        this.damageValue = damageValue;
    }

    public override void ApplyAction()
    {
        target.ReceiveDamage(this);
        PostProcess();
    }

    private void PostProcess()
    {
        creator.TriggerActionPoint(ActionPointType.PostCauseDamage, this);
        target.TriggerActionPoint(ActionPointType.PostReceiveDamage, this);
    }
}
