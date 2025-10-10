using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPoint
{
    public int Value { get; set; }
    public int MaxValue { get; private set; }

    public void Reset()
    {
        Value = MaxValue;
    }

    public void SetMaxValue(int maxValue)
    {
        MaxValue = maxValue;
    }

    public void Minus(int value)
    {
        Value = Mathf.Max(0, Value - value);
    }

    public void Add(int value)
    {
        Value = Mathf.Max(MaxValue, Value + value);
    }
    
    public float GetPercentage()
    {
        return (float)Value / MaxValue;
    }
}
