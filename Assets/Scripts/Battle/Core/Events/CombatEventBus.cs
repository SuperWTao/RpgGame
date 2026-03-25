using System;
using System.Collections.Generic;

public sealed class CombatEventBus : ICombatEventBus
{
    private event Action<CombatEvent> _allHandlers;

    private readonly Dictionary<Type, List<Delegate>> _typedHandlers =
        new Dictionary<Type, List<Delegate>>();

    public void Publish(CombatEvent evt)
    {
        if (evt == null) return;

        _allHandlers?.Invoke(evt);

        var eventType = evt.GetType();
        if (_typedHandlers.TryGetValue(eventType, out var handlers))
        {
            for (int i = 0; i < handlers.Count; i++)
            {
                ((Action<CombatEvent>)WrapToBaseDelegate(handlers[i]))?.Invoke(evt);
            }
        }
    }

    public void SubscribeAll(Action<CombatEvent> handler)
    {
        _allHandlers += handler;
    }

    public void UnsubscribeAll(Action<CombatEvent> handler)
    {
        _allHandlers -= handler;
    }

    public void Subscribe<T>(Action<T> handler) where T : CombatEvent
    {
        if (handler == null) return;

        var t = typeof(T);
        if (!_typedHandlers.TryGetValue(t, out var handlers))
        {
            handlers = new List<Delegate>();
            _typedHandlers[t] = handlers;
        }

        handlers.Add(handler);
    }

    public void Unsubscribe<T>(Action<T> handler) where T : CombatEvent
    {
        if (handler == null) return;

        var t = typeof(T);
        if (!_typedHandlers.TryGetValue(t, out var handlers))
        {
            return;
        }

        handlers.Remove(handler);
        if (handlers.Count == 0)
        {
            _typedHandlers.Remove(t);
        }
    }

    private Delegate WrapToBaseDelegate(Delegate typedDelegate)
    {
        if (typedDelegate is Action<CombatEvent> baseDelegate)
        {
            return baseDelegate;
        }

        return new Action<CombatEvent>(evt => typedDelegate.DynamicInvoke(evt));
    }
}