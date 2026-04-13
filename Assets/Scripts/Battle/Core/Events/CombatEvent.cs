using System;

public abstract class CombatEvent
{
    public long battleId;
    public int tick;
    public long requestId;
    public BattleStage stage;
    public DateTime utcTime;
}

public sealed class StageEvent : CombatEvent
{
    public string message;
}

public sealed class ActionFinishedEvent : CombatEvent
{
    public ActionResultCode resultCode;
    public int sourceEntityId;
    public int mainTargetEntityId;
    public int damageApplied;
    public int healApplied;
    public string message;
}