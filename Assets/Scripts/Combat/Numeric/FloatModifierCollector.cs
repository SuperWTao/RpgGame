using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatModifierCollector
{
    public int TotalValue { get; set; }
    private List<FloatModifier> _modifiers = new List<FloatModifier>();

    public int AddModifier(FloatModifier modifier)
    {
        _modifiers.Add(modifier);
        Update();
        return TotalValue;
    }
    
    public int RemoveModifier(FloatModifier modifier)
    {
        _modifiers.Remove(modifier);
        Update();
        return TotalValue;
    }

    public void Update()
    {
        TotalValue = 0;
        foreach (FloatModifier item in _modifiers)
        {
            TotalValue += item.value;
        }
    }
}

public class FloatModifier
{
    public int value;
    
    public FloatModifier(int value)
    {
        this.value = value;
    }
}
