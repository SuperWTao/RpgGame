using System;

public abstract class CombatEvent
{
    public long BattleId;
    public int Tick;
    public long RequestId;
    public BattleStage Stage;
    public DateTime UtcTime;
}

public sealed class StageEvent : CombatEvent
{
    public string Message;
}

public sealed class ActionFinishedEvent : CombatEvent
{
    public ActionResultCode ResultCode;
    public int SourceEntityId;
    public int MainTargetEntityId;
    public int DamageApplied;
    public int HealApplied;
    public string Message;
}