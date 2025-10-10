using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class IntModifierCollector
{
    public int TotalValue { get; set; }
    private List<IntModifier> _modifiers = new List<IntModifier>();

    public int AddModifier(IntModifier modifier)
    {
        _modifiers.Add(modifier);
        Update();
        return TotalValue;
    }
    
    public int RemoveModifier(IntModifier modifier)
    {
        _modifiers.Remove(modifier);
        Update();
        return TotalValue;
    }

    public void Update()
    {
        TotalValue = 0;
        foreach (IntModifier item in _modifiers)
        {
            TotalValue += item.value;
        }
    }
}

public class IntModifier
{
    public int value;

    public IntModifier(int value)
    {
        this.value = value;
    }
}
