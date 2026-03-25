using System;

public interface ICombatEventBus
{
    void Publish(CombatEvent evt);

    void SubscribeAll(Action<CombatEvent> handler);
    void UnsubscribeAll(Action<CombatEvent> handler);

    void Subscribe<T>(Action<T> handler) where T : CombatEvent;
    void Unsubscribe<T>(Action<T> handler) where T : CombatEvent;
}