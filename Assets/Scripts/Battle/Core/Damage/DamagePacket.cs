using System.Collections.Generic;

public sealed class DamagePacket
{
    public long RequestId;
    public int SourceEntityId;
    public int TargetEntityId;
    public BattleActionType ActionType;
    public DamageType DamageType;

    // 计算输入
    public int BaseDamage;
    public int FlatAdd;
    public float PercentAdd;
    public int FlatReduce;

    // 安全钳制
    public int MinDamage;
    public int MaxDamage = int.MaxValue;

    // 计算输出
    public int FinalDamage;

    // 可选标签，后续给 Buff/被动用
    public HashSet<string> Tags = new HashSet<string>();

    public void ResetRuntime()
    {
        BaseDamage = 0;
        FlatAdd = 0;
        PercentAdd = 0f;
        FlatReduce = 0;
        MinDamage = 0;
        MaxDamage = int.MaxValue;
        FinalDamage = 0;
        Tags.Clear();
    }
}