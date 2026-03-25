using UnityEngine;
using Wjybxx.BTree;
using Wjybxx.BTree.Branch;

public sealed class EnemyAIDemo : MonoBehaviour
{
    [Header("Runtime Bind")]
    public Transform target;

    [Header("Config")]
    public float moveSpeed = 2.5f;
    public float attackRange = 1.8f;
    public float attackCooldown = 1.0f;

    private TaskEntry<EnemyAIBlackboard> _treeEntry;
    private EnemyAIBlackboard _blackboard;

    public void Initialize(BattleContext battle, IBattlePipeline pipeline, int sourceEntityId, int targetEntityId, System.Func<long> nextRequestId)
    {
        _blackboard = new EnemyAIBlackboard
        {
            Self = transform,
            Target = target,
            MoveSpeed = moveSpeed,
            AttackRange = attackRange,
            AttackCooldown = attackCooldown,
            NextAttackTime = 0f,
            Battle = battle,
            Pipeline = pipeline,
            SourceEntityId = sourceEntityId,
            TargetEntityId = targetEntityId,
            NextRequestId = nextRequestId
        };

        Task<EnemyAIBlackboard> root = BuildTree();
        _treeEntry = new TaskEntry<EnemyAIBlackboard>("EnemySimpleAI", root, _blackboard, this, null);
    }

    private Task<EnemyAIBlackboard> BuildTree()
    {
        var root = new Selector<EnemyAIBlackboard>();

        var attackSeq = new Sequence<EnemyAIBlackboard>();
        attackSeq.AddChild(new BTCanAttackTarget());
        attackSeq.AddChild(new BTDoAttack());

        root.AddChild(attackSeq);
        root.AddChild(new BTMoveToTarget());

        return root;
    }

    private void Update()
    {
        if (_treeEntry != null)
        {
            _treeEntry.Update(Time.frameCount);
        }
    }
}