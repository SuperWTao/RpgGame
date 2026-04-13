using UnityEngine;

public sealed class EnemyAIBlackboard
{
    public Transform self;
    public Transform target;

    public float moveSpeed;
    public float attackRange;
    public float attackCooldown;
    public float nextAttackTime;

    public BattleContext battle;
    public IBattlePipeline pipeline;
    public int sourceEntityId;
    public int targetEntityId;

    public System.Func<long> nextRequestId;
}