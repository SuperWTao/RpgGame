using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CureAction : CombatAction
{
    public int curValue;

    public CureAction(ActionType type, Entity creator, Entity target, int curValue) : base(type, creator, target)
    {
        this.curValue = curValue;
    }

    public override void ApplyAction()
    {
        target.ReceiveCure(this);
    }
}
