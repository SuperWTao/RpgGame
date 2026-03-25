using UnityEngine;
using Wjybxx.BTree;
using Wjybxx.BTree.Branch;

public sealed class BattleEnemyActor : MonoBehaviour
{
    [Header("Battle Identity")]
    [SerializeField] private int entityId = 1001;
    [SerializeField] private string entityName = "Enemy";

    [Header("Battle Stats")]
    [SerializeField] private int maxHp = 80;
    [SerializeField] private int attack = 10;
    [SerializeField] private int defense = 2;

    [Header("AI Config")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private float attackCooldown = 1.0f;

    private bool _bound;
    private bool _deadPrinted;
    private bool _aiEnabled = true;

    private TaskEntry<EnemyAIBlackboard> _treeEntry;
    private EnemyAIBlackboard _blackboard;

    public int EntityId => entityId;

    private void OnEnable()
    {
        _bound = false;
        _deadPrinted = false;
        _aiEnabled = true;
    }

    private void Update()
    {
        if (!_bound)
        {
            TryBindToWorld();
            return;
        }

        var world = BattleWorld.Instance;
        if (world == null || world.Battle == null) return;

        if (!world.Battle.TryGetEntity(entityId, out var selfEntity)) return;

        if (selfEntity.IsDead)
        {
            if (!_deadPrinted)
            {
                _deadPrinted = true;
                Debug.Log("[Battle] Enemy Dead: " + entityName + " (" + entityId + ")");
            }

            _aiEnabled = false;
            return;
        }

        if (!_aiEnabled) return;
        if (_treeEntry == null || _blackboard == null) return;
        if (_blackboard.Target == null) return;

        if (!world.Battle.TryGetEntity(_blackboard.TargetEntityId, out var targetEntity)) return;
        if (targetEntity.IsDead) return;

        _treeEntry.Update(Time.frameCount);
    }

    private void TryBindToWorld()
    {
        var world = BattleWorld.Instance;
        if (world == null || world.Battle == null || world.Pipeline == null) return;
        if (world.PlayerTransform == null || world.PlayerEntityId <= 0) return;

        world.RegisterEnemy(entityId, entityName, maxHp, attack, defense);

        _blackboard = new EnemyAIBlackboard
        {
            Self = transform,
            Target = world.PlayerTransform,
            MoveSpeed = moveSpeed,
            AttackRange = attackRange,
            AttackCooldown = attackCooldown,
            NextAttackTime = 0f,
            Battle = world.Battle,
            Pipeline = world.Pipeline,
            SourceEntityId = entityId,
            TargetEntityId = world.PlayerEntityId,
            NextRequestId = world.NextRequestId
        };

        _treeEntry = new TaskEntry<EnemyAIBlackboard>("EnemyAI", BuildTree(), _blackboard, this, null);
        _bound = true;
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
}