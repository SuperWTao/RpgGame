using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EventCenter
{
    private static Dictionary<string, Action<object>> eventDict = new Dictionary<string, Action<object>>();

    public static void AddListener(string eventName, Action<object> listener)
    {
        if (!eventDict.ContainsKey(eventName))
            eventDict[eventName] = null;
        eventDict[eventName] += listener;
    }

    public static void RemoveListener(string eventName, Action<object> listener)
    {
        if(!eventDict.ContainsKey(eventName))
            return;
        eventDict[eventName] -= listener;
    }

    public static void TriggerEvent(string eventName, object param = null)
    {
        if(eventDict.ContainsKey(eventName))
            eventDict[eventName]?.Invoke(param);
    }
}
