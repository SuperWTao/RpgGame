using UnityEngine;
using Wjybxx.BTree;

public sealed class BTCanAttackTarget : LeafTask<EnemyAIBlackboard>
{
    protected override int Execute()
    {
        if (blackboard.target == null) return TaskStatus.ERROR;
        if (!blackboard.battle.TryGetEntity(blackboard.targetEntityId, out var targetEntity)) return TaskStatus.ERROR;
        if (targetEntity.isDead) return TaskStatus.ERROR;

        float dist = Vector3.Distance(blackboard.self.position, blackboard.target.position);
        return dist <= blackboard.attackRange ? TaskStatus.SUCCESS : TaskStatus.ERROR;
    }

    protected override void OnEventImpl(object eventObj) { }
}

public sealed class BTMoveToTarget : LeafTask<EnemyAIBlackboard>
{
    protected override int Execute()
    {
        if (blackboard.target == null) return TaskStatus.ERROR;

        Vector3 selfPos = blackboard.self.position;
        Vector3 targetPos = blackboard.target.position;
        targetPos.y = selfPos.y;

        Vector3 dir = (targetPos - selfPos);
        float dist = dir.magnitude;
        if (dist <= blackboard.attackRange)
        {
            return TaskStatus.SUCCESS;
        }

        Vector3 step = dir.normalized * blackboard.moveSpeed * Time.deltaTime;
        if (step.magnitude > dist) step = dir;
        blackboard.self.position += step;

        if (step.sqrMagnitude > 0.0001f)
        {
            blackboard.self.forward = step.normalized;
        }

        return TaskStatus.RUNNING;
    }

    protected override void OnEventImpl(object eventObj) { }
}

public sealed class BTDoAttack : LeafTask<EnemyAIBlackboard>
{
    protected override int Execute()
    {
        if (Time.time < blackboard.nextAttackTime) return TaskStatus.RUNNING;
        if (blackboard.target == null) return TaskStatus.ERROR;

        long reqId = blackboard.nextRequestId != null ? blackboard.nextRequestId() : (long)Time.frameCount;
        var req = ActionRequest.CreateNormalAttack(reqId, blackboard.sourceEntityId, blackboard.targetEntityId);
        var result = blackboard.pipeline.Execute(blackboard.battle, req);

        if (!result.success)
        {
            return TaskStatus.ERROR;
        }

        blackboard.nextAttackTime = Time.time + blackboard.attackCooldown;
        return TaskStatus.SUCCESS;
    }

    protected override void OnEventImpl(object eventObj) { }
}