using System;
using System.Collections.Generic;

public sealed class BattleContext
{
    public long battleId { get; }
    public int tick { get; private set; }

    // 后续用于确定性战斗（暴击/闪避等）
    public Random rng { get; }

    private readonly Dictionary<int, BattleEntity> _entities = new Dictionary<int, BattleEntity>();

    public BattleContext(long battleId, int randomSeed)
    {
        if (battleId <= 0) throw new ArgumentException("battleId must be > 0");

        this.battleId = battleId;
        rng = new Random(randomSeed);
        tick = 0;
    }

    public IReadOnlyDictionary<int, BattleEntity> entities => _entities;

    public void AdvanceTick()
    {
        tick++;
    }

    public void AddEntity(BattleEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (_entities.ContainsKey(entity.entityId))
        {
            throw new InvalidOperationException($"entity already exists: {entity.entityId}");
        }

        _entities.Add(entity.entityId, entity);
    }

    public bool RemoveEntity(int entityId)
    {
        return _entities.Remove(entityId);
    }

    public bool TryGetEntity(int entityId, out BattleEntity entity)
    {
        return _entities.TryGetValue(entityId, out entity);
    }

    public BattleEntity GetEntityOrThrow(int entityId)
    {
        if (!_entities.TryGetValue(entityId, out var entity))
        {
            throw new KeyNotFoundException($"entity not found: {entityId}");
        }

        return entity;
    }
}