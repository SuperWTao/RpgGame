using System;
using UnityEngine;

public sealed class BattleWorld : MonoBehaviour
{
    public static BattleWorld Instance { get; private set; }

    [Header("World Config")]
    [SerializeField] private long battleId = 1;
    [SerializeField] private int randomSeed = 123456;

    private long _requestIdSeed = 10000;

    public BattleContext Battle { get; private set; }
    public ICombatEventBus EventBus { get; private set; }
    public BattleEffectRegistry EffectRegistry { get; private set; }
    public IBattlePipeline Pipeline { get; private set; }

    public int PlayerEntityId { get; private set; } = -1;
    public Transform PlayerTransform { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Battle = new BattleContext(battleId, randomSeed);
        EventBus = new CombatEventBus();
        EffectRegistry = new BattleEffectRegistry();
        Pipeline = new StandardBattlePipeline(EventBus, EffectRegistry);
    }

    private void Update()
    {
        if (Battle != null)
        {
            Battle.AdvanceTick();
        }
    }

    public void RegisterPlayer(
        int entityId,
        string entityName,
        int maxHp,
        int attack,
        int defense,
        Transform playerTransform)
    {
        if (Battle == null || playerTransform == null) return;

        PlayerEntityId = entityId;
        PlayerTransform = playerTransform;

        if (!Battle.TryGetEntity(entityId, out _))
        {
            Battle.AddEntity(new BattleEntity(entityId, entityName, maxHp, attack, defense));
        }
    }

    public void RegisterEnemy(
        int entityId,
        string entityName,
        int maxHp,
        int attack,
        int defense)
    {
        if (Battle == null) return;

        if (!Battle.TryGetEntity(entityId, out _))
        {
            Battle.AddEntity(new BattleEntity(entityId, entityName, maxHp, attack, defense));
        }
    }

    public void UnregisterEntity(int entityId)
    {
        Battle?.RemoveEntity(entityId);
    }

    public long NextRequestId()
    {
        _requestIdSeed++;
        return _requestIdSeed;
    }
}