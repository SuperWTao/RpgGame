public sealed class SourceAttackAsBaseModifier : IDamageModifier
{
    public int priority => 100;

    public bool CanApply(ActionExecutionContext ctx, DamagePacket packet)
    {
        return packet.actionType == BattleActionType.NormalAttack
            || packet.actionType == BattleActionType.Skill;
    }

    public void Apply(ActionExecutionContext ctx, DamagePacket packet)
    {
        packet.baseDamage = ctx.source.attack;
    }
}

public sealed class SkillFlatBonusModifier : IDamageModifier
{
    public int priority => 200;

    public bool CanApply(ActionExecutionContext ctx, DamagePacket packet)
    {
        return packet.actionType == BattleActionType.Skill;
    }

    public void Apply(ActionExecutionContext ctx, DamagePacket packet)
    {
        // 先给一个基础技能增量，后续可按SkillId读取配置
        packet.flatAdd += 5;
    }
}

public sealed class TargetDefenseReductionModifier : IDamageModifier
{
    public int priority => 300;

    public bool CanApply(ActionExecutionContext ctx, DamagePacket packet)
    {
        // 真实伤害不吃防御
        return packet.damageType != DamageType.True;
    }

    public void Apply(ActionExecutionContext ctx, DamagePacket packet)
    {
        packet.flatReduce += ctx.target.defense;
    }
}

public sealed class OffensiveActionMinOneModifier : IDamageModifier
{
    public int priority => 1000;

    public bool CanApply(ActionExecutionContext ctx, DamagePacket packet)
    {
        return packet.actionType == BattleActionType.NormalAttack
            || packet.actionType == BattleActionType.Skill;
    }

    public void Apply(ActionExecutionContext ctx, DamagePacket packet)
    {
        if (packet.minDamage < 1)
        {
            packet.minDamage = 1;
        }
    }
}