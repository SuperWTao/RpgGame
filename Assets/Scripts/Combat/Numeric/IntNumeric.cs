using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntNumeric
{
    public int Value { get; set; }
    public int BaseValue { get; set; } // 基础攻击力
    public int Add { get; set; } // 固定加成
    public int PctAdd { get; set; } // 百分比加成
    public int FinalAdd { get; set; } // 固定buff加成
    public int FinalPctAdd { get; set; } // 百分比buff加成

    private IntModifierCollector _addCollector { get; } = new IntModifierCollector();
    private IntModifierCollector _pctAddCollector { get; } = new IntModifierCollector();
    private IntModifierCollector _finalAddCollector { get; } = new IntModifierCollector();
    private IntModifierCollector _finalPctAddCollector { get; } = new IntModifierCollector();

    public void Initialize()
    {
        Value = BaseValue = Add = PctAdd = FinalAdd = FinalPctAdd = 0;
    }
    
    public int SetBase(int value)
    {
        BaseValue += value;
        Update();
        return Value;
    }

    public void AddModifier(IntModifier modifier)
    {
        Add = _addCollector.AddModifier(modifier);
        Update();
    }
    
    public void RemoveAddModifier(IntModifier modifier)
    {
        Add = _addCollector.RemoveModifier(modifier);
        Update();
    }
    
    public void PctAddModifier(IntModifier modifier)
    {
        PctAdd = _pctAddCollector.AddModifier(modifier);
        Update();
    }
    
    public void RemovePctAddModifier(IntModifier modifier)
    {
        PctAdd = _pctAddCollector.RemoveModifier(modifier);
        Update();
    }
    
    public void FinalAddModifier(IntModifier modifier)
    {
        FinalAdd = _finalAddCollector.AddModifier(modifier);
        Update();
    }
    
    public void RemoveFinalAddModifier(IntModifier modifier)
    {
        FinalAdd = _finalAddCollector.RemoveModifier(modifier);
        Update();
    }
    
    public void FinalPctAddModifier(IntModifier modifier)
    {
        FinalPctAdd = _finalPctAddCollector.AddModifier(modifier);
        Update();
    }
    
    public void RemoveFinalPctAddModifier(IntModifier modifier)
    {
        FinalPctAdd = _finalPctAddCollector.RemoveModifier(modifier);
        Update();
    }

    public void Update()
    {
        var value1 = BaseValue;
        var value2 = (value1 + Add) * (100 + PctAdd) / 100f;
        var value3 = (value2 + FinalAdd) * (100 + FinalPctAdd) / 100f;
        Value = (int)value3;
    }
}
