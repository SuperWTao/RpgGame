using UnityEngine;

public sealed class BattleDemoBootstrap : MonoBehaviour
{
    public Transform playerTransform;
    public EnemyAIDemo enemyDemo;

    private BattleContext _battle;
    private ICombatEventBus _eventBus;
    private BattleEffectRegistry _effectRegistry;
    private IBattlePipeline _pipeline;

    private long _requestIdSeed = 10000;

    private const int PlayerEntityId = 1;
    private const int EnemyEntityId = 2;

    private void Awake()
    {
        _battle = new BattleContext(1, 123456);
        _eventBus = new CombatEventBus();
        _effectRegistry = new BattleEffectRegistry();
        _pipeline = new StandardBattlePipeline(_eventBus, _effectRegistry);

        _eventBus.SubscribeAll(e =>
        {
            Debug.Log($"[BattleEvent] stage={e.Stage}, request={e.RequestId}, tick={e.Tick}");
        });

        _battle.AddEntity(new BattleEntity(PlayerEntityId, "Player", 100, 15, 3));
        _battle.AddEntity(new BattleEntity(EnemyEntityId, "Enemy", 80, 10, 2));

        // 演示效果：敌人有2次攻击的狂暴buff + 斩杀号回复被动
        _effectRegistry.AddBuff(EnemyEntityId, new RageBuff(2, 0.2f));
        _effectRegistry.AddPassive(EnemyEntityId, new ExecutionHealPassive(5));
    }

    private void Start()
    {
        if (enemyDemo != null)
        {
            enemyDemo.target = playerTransform;
            enemyDemo.Initialize(_battle, _pipeline, EnemyEntityId, PlayerEntityId, NextRequestId);
        }
    }

    private long NextRequestId()
    {
        _requestIdSeed++;
        return _requestIdSeed;
    }
}