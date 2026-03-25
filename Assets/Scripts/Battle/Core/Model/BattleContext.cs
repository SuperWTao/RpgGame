using System;
using System.Collections.Generic;

public sealed class BattleContext
{
    public long BattleId { get; }
    public int Tick { get; private set; }

    // 后续用于确定性战斗（暴击/闪避等）
    public Random Rng { get; }

    private readonly Dictionary<int, BattleEntity> _entities = new Dictionary<int, BattleEntity>();

    public BattleContext(long battleId, int randomSeed)
    {
        if (battleId <= 0) throw new ArgumentException("battleId must be > 0");

        BattleId = battleId;
        Rng = new Random(randomSeed);
        Tick = 0;
    }

    public IReadOnlyDictionary<int, BattleEntity> Entities => _entities;

    public void AdvanceTick()
    {
        Tick++;
    }

    public void AddEntity(BattleEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (_entities.ContainsKey(entity.EntityId))
        {
            throw new InvalidOperationException($"entity already exists: {entity.EntityId}");
        }

        _entities.Add(entity.EntityId, entity);
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