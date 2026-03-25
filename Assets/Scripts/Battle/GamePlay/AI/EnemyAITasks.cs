using UnityEngine;
using Wjybxx.BTree;

public sealed class BTCanAttackTarget : LeafTask<EnemyAIBlackboard>
{
    protected override int Execute()
    {
        if (blackboard.Target == null) return TaskStatus.ERROR;
        if (!blackboard.Battle.TryGetEntity(blackboard.TargetEntityId, out var targetEntity)) return TaskStatus.ERROR;
        if (targetEntity.IsDead) return TaskStatus.ERROR;

        float dist = Vector3.Distance(blackboard.Self.position, blackboard.Target.position);
        return dist <= blackboard.AttackRange ? TaskStatus.SUCCESS : TaskStatus.ERROR;
    }

    protected override void OnEventImpl(object eventObj) { }
}

public sealed class BTMoveToTarget : LeafTask<EnemyAIBlackboard>
{
    protected override int Execute()
    {
        if (blackboard.Target == null) return TaskStatus.ERROR;

        Vector3 selfPos = blackboard.Self.position;
        Vector3 targetPos = blackboard.Target.position;
        targetPos.y = selfPos.y;

        Vector3 dir = (targetPos - selfPos);
        float dist = dir.magnitude;
        if (dist <= blackboard.AttackRange)
        {
            return TaskStatus.SUCCESS;
        }

        Vector3 step = dir.normalized * blackboard.MoveSpeed * Time.deltaTime;
        if (step.magnitude > dist) step = dir;
        blackboard.Self.position += step;

        if (step.sqrMagnitude > 0.0001f)
        {
            blackboard.Self.forward = step.normalized;
        }

        return TaskStatus.RUNNING;
    }

    protected override void OnEventImpl(object eventObj) { }
}

public sealed class BTDoAttack : LeafTask<EnemyAIBlackboard>
{
    protected override int Execute()
    {
        if (Time.time < blackboard.NextAttackTime) return TaskStatus.RUNNING;
        if (blackboard.Target == null) return TaskStatus.ERROR;

        long reqId = blackboard.NextRequestId != null ? blackboard.NextRequestId() : (long)Time.frameCount;
        var req = ActionRequest.CreateNormalAttack(reqId, blackboard.SourceEntityId, blackboard.TargetEntityId);
        var result = blackboard.Pipeline.Execute(blackboard.Battle, req);

        if (!result.Success)
        {
            return TaskStatus.ERROR;
        }

        blackboard.NextAttackTime = Time.time + blackboard.AttackCooldown;
        return TaskStatus.SUCCESS;
    }

    protected override void OnEventImpl(object eventObj) { }
}