using UnityEngine;
using Wjybxx.BTree;
using Wjybxx.BTree.Branch;

public sealed class BattleEnemyActor : MonoBehaviour
{
    [System.Serializable]
    public struct EnemySpawnConfig
    {
        public int entityId;
        public string entityName;
        public int maxHp;
        public int attack;
        public int defense;
        public float moveSpeed;
        public float attackRange;
        public float attackCooldown;
    }

    [Header("Battle Identity")]
    [SerializeField] private int entityId = 0;
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
    private bool _runtimeConfigured;

    private TaskEntry<EnemyAIBlackboard> _treeEntry;
    private EnemyAIBlackboard _blackboard;

    public int battleEntityId => entityId;

    public void ConfigureFromSpawn(EnemySpawnConfig config)
    {
        if (config.entityId > 0) entityId = config.entityId;

        if (!string.IsNullOrWhiteSpace(config.entityName))
        {
            entityName = config.entityName;
        }

        maxHp = Mathf.Max(1, config.maxHp);
        attack = Mathf.Max(0, config.attack);
        defense = Mathf.Max(0, config.defense);

        moveSpeed = Mathf.Max(0f, config.moveSpeed);
        attackRange = Mathf.Max(0f, config.attackRange);
        attackCooldown = Mathf.Max(0f, config.attackCooldown);

        _runtimeConfigured = true;
    }

    public void ConfigureIdentityAndStats(int runtimeEntityId, string runtimeName, int runtimeMaxHp, int runtimeAttack, int runtimeDefense)
    {
        var config = new EnemySpawnConfig
        {
            entityId = runtimeEntityId,
            entityName = runtimeName,
            maxHp = runtimeMaxHp,
            attack = runtimeAttack,
            defense = runtimeDefense,
            moveSpeed = moveSpeed,
            attackRange = attackRange,
            attackCooldown = attackCooldown
        };

        ConfigureFromSpawn(config);
    }

    private void OnEnable()
    {
        _bound = false;
        _deadPrinted = false;
        _aiEnabled = true;

        if (!_runtimeConfigured)
        {
            // Ensure inspector defaults are valid for runtime spawns.
            maxHp = Mathf.Max(1, maxHp);
            attack = Mathf.Max(0, attack);
            defense = Mathf.Max(0, defense);
            moveSpeed = Mathf.Max(0f, moveSpeed);
            attackRange = Mathf.Max(0f, attackRange);
            attackCooldown = Mathf.Max(0f, attackCooldown);
        }
    }

    private void OnDisable()
    {
        var world = BattleWorld.instance;
        if (_bound && world != null && entityId > 0)
        {
            world.UnregisterEntity(entityId);
        }

        _bound = false;
        _treeEntry = null;
        _blackboard = null;
    }

    private void Update()
    {
        var world = BattleWorld.instance;
        if (world == null || world.battle == null || world.pipeline == null) return;

        if (!_bound)
        {
            TryBindToWorld();
            return;
        }

        if (!_aiEnabled || _blackboard == null || _treeEntry == null) return;

        if (!world.battle.TryGetEntity(entityId, out var selfEntity)) return;

        if (selfEntity.isDead)
        {
            if (!_deadPrinted)
            {
                _deadPrinted = true;
                Debug.Log("[Battle] Enemy Dead: " + entityName + " (" + entityId + ")");
            }

            _aiEnabled = false;
            return;
        }

        if (!world.battle.TryGetEntity(_blackboard.targetEntityId, out var targetEntity)) return;
        if (targetEntity.isDead) return;

        _treeEntry.Update(Time.frameCount);
    }

    private void TryBindToWorld()
    {
        var world = BattleWorld.instance;
        if (world == null || world.battle == null || world.pipeline == null) return;
        if (world.playerTransform == null || world.playerEntityId <= 0) return;

        entityId = world.RegisterEnemy(entityId, entityName, maxHp, attack, defense);

        _blackboard = new EnemyAIBlackboard
        {
            self = transform,
            target = world.playerTransform,
            moveSpeed = moveSpeed,
            attackRange = attackRange,
            attackCooldown = attackCooldown,
            nextAttackTime = 0f,
            battle = world.battle,
            pipeline = world.pipeline,
            sourceEntityId = entityId,
            targetEntityId = world.playerEntityId,
            nextRequestId = world.NextRequestId
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