using System.Collections.Generic;

public sealed class DamagePacket
{
    public long requestId;
    public int sourceEntityId;
    public int targetEntityId;
    public BattleActionType actionType;
    public DamageType damageType;

    // 计算输入
    public int baseDamage;
    public int flatAdd;
    public float percentAdd;
    public int flatReduce;

    // 安全钳制
    public int minDamage;
    public int maxDamage = int.MaxValue;

    // 计算输出
    public int finalDamage;

    // 可选标签，后续给 Buff/被动用
    public HashSet<string> tags = new HashSet<string>();

    public void ResetRuntime()
    {
        baseDamage = 0;
        flatAdd = 0;
        percentAdd = 0f;
        flatReduce = 0;
        minDamage = 0;
        maxDamage = int.MaxValue;
        finalDamage = 0;
        tags.Clear();
    }
}