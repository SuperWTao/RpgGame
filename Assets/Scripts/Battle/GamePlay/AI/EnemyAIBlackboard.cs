using UnityEngine;

public sealed class EnemyAIBlackboard
{
    public Transform Self;
    public Transform Target;

    public float MoveSpeed;
    public float AttackRange;
    public float AttackCooldown;
    public float NextAttackTime;

    public BattleContext Battle;
    public IBattlePipeline Pipeline;
    public int SourceEntityId;
    public int TargetEntityId;

    public System.Func<long> NextRequestId;
}