using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatNumeric
{
    public float Value { get; set; }
    public float BaseValue { get; set; } // 基础攻击力
    public float Add { get; set; } // 固定加成
    public float PctAdd { get; set; } // 百分比加成
    public float FinalAdd { get; set; } // 固定buff加成
    public float FinalPctAdd { get; set; } // 百分比buff加成

    private FloatModifierCollector _addCollector { get; } = new FloatModifierCollector();
    private FloatModifierCollector _pctAddCollector { get; } = new FloatModifierCollector();
    private FloatModifierCollector _finalAddCollector { get; } = new FloatModifierCollector();
    private FloatModifierCollector _finalPctAddCollector { get; } = new FloatModifierCollector();

    public void Initialize()
    {
        Value = BaseValue = Add = PctAdd = FinalAdd = FinalPctAdd = 0;
    }
    
    public float SetBase(float value)
    {
        BaseValue += value;
        Update();
        return Value;
    }

    public void AddModifier(FloatModifier modifier)
    {
        Add = _addCollector.AddModifier(modifier);
        Update();
    }
    
    public void RemoveAddModifier(FloatModifier modifier)
    {
        Add = _addCollector.RemoveModifier(modifier);
        Update();
    }
    
    public void PctAddModifier(FloatModifier modifier)
    {
        PctAdd = _pctAddCollector.AddModifier(modifier);
        Update();
    }
    
    public void RemovePctAddModifier(FloatModifier modifier)
    {
        PctAdd = _pctAddCollector.RemoveModifier(modifier);
        Update();
    }
    
    public void FinalAddModifier(FloatModifier modifier)
    {
        FinalAdd = _finalAddCollector.AddModifier(modifier);
        Update();
    }
    
    public void RemoveFinalAddModifier(FloatModifier modifier)
    {
        FinalAdd = _finalAddCollector.RemoveModifier(modifier);
        Update();
    }
    
    public void FinalPctAddModifier(FloatModifier modifier)
    {
        FinalPctAdd = _finalPctAddCollector.AddModifier(modifier);
        Update();
    }
    
    public void RemoveFinalPctAddModifier(FloatModifier modifier)
    {
        FinalPctAdd = _finalPctAddCollector.RemoveModifier(modifier);
        Update();
    }

    public void Update()
    {
        var value1 = BaseValue;
        var value2 = (value1 + Add) * (100 + PctAdd) / 100f;
        var value3 = (value2 + FinalAdd) * (100 + FinalPctAdd) / 100f;
        Value = (float)value3;
    }
}
