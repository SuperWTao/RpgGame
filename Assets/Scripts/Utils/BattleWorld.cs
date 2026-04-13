using System;
using UnityEngine;

public sealed class BattleWorld : MonoBehaviour
{
    public static BattleWorld instance { get; private set; }

    [Header("World Config")]
    [SerializeField] private long battleId = 1;
    [SerializeField] private int randomSeed = 123456;
    [SerializeField] private int enemyEntityIdSeed = 1000;

    private long _requestIdSeed = 10000;
    private int _nextEnemyEntityId;

    public BattleContext battle { get; private set; }
    public ICombatEventBus eventBus { get; private set; }
    public BattleEffectRegistry effectRegistry { get; private set; }
    public IBattlePipeline pipeline { get; private set; }

    public int playerEntityId { get; private set; } = -1;
    public Transform playerTransform { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        battle = new BattleContext(battleId, randomSeed);
        eventBus = new CombatEventBus();
        effectRegistry = new BattleEffectRegistry();
        pipeline = new StandardBattlePipeline(eventBus, effectRegistry);
        _nextEnemyEntityId = Mathf.Max(1, enemyEntityIdSeed);
    }

    private void Update()
    {
        battle.AdvanceTick();
    }

    public void RegisterPlayer(
        int entityId,
        string entityName,
        int maxHp,
        int attack,
        int defense,
        Transform playerTransform)
    {
        playerEntityId = entityId;
        this.playerTransform = playerTransform;

        if (!battle.TryGetEntity(entityId, out _))
        {
            battle.AddEntity(new BattleEntity(entityId, entityName, maxHp, attack, defense));
        }
    }

    public int RegisterEnemy(
        int desiredEntityId,
        string entityName,
        int maxHp,
        int attack,
        int defense)
    {
        int entityId = ResolveEnemyEntityId(desiredEntityId);

        if (!battle.TryGetEntity(entityId, out _))
        {
            battle.AddEntity(new BattleEntity(entityId, entityName, maxHp, attack, defense));
        }

        return entityId;
    }

    public void UnregisterEntity(int entityId)
    {
        battle?.RemoveEntity(entityId);
    }

    public long NextRequestId()
    {
        _requestIdSeed++;
        return _requestIdSeed;
    }

    public int NextEnemyEntityId()
    {
        return ResolveEnemyEntityId(0);
    }

    private int ResolveEnemyEntityId(int desiredEntityId)
    {
        if (desiredEntityId > 0
            && desiredEntityId != playerEntityId
            && !battle.TryGetEntity(desiredEntityId, out _))
        {
            _nextEnemyEntityId = Mathf.Max(_nextEnemyEntityId, desiredEntityId + 1);
            return desiredEntityId;
        }

        while (true)
        {
            int candidate = _nextEnemyEntityId;
            _nextEnemyEntityId++;

            if (candidate <= 0) continue;
            if (candidate == playerEntityId) continue;
            if (battle.TryGetEntity(candidate, out _)) continue;

            return candidate;
        }
    }
}