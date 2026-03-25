public sealed class SourceAttackAsBaseModifier : IDamageModifier
{
    public int Priority => 100;

    public bool CanApply(ActionExecutionContext ctx, DamagePacket packet)
    {
        return packet.ActionType == BattleActionType.NormalAttack
            || packet.ActionType == BattleActionType.Skill;
    }

    public void Apply(ActionExecutionContext ctx, DamagePacket packet)
    {
        packet.BaseDamage = ctx.Source.Attack;
    }
}

public sealed class SkillFlatBonusModifier : IDamageModifier
{
    public int Priority => 200;

    public bool CanApply(ActionExecutionContext ctx, DamagePacket packet)
    {
        return packet.ActionType == BattleActionType.Skill;
    }

    public void Apply(ActionExecutionContext ctx, DamagePacket packet)
    {
        // 先给一个基础技能增量，后续可按SkillId读取配置
        packet.FlatAdd += 5;
    }
}

public sealed class TargetDefenseReductionModifier : IDamageModifier
{
    public int Priority => 300;

    public bool CanApply(ActionExecutionContext ctx, DamagePacket packet)
    {
        // 真实伤害不吃防御
        return packet.DamageType != DamageType.True;
    }

    public void Apply(ActionExecutionContext ctx, DamagePacket packet)
    {
        packet.FlatReduce += ctx.Target.Defense;
    }
}

public sealed class OffensiveActionMinOneModifier : IDamageModifier
{
    public int Priority => 1000;

    public bool CanApply(ActionExecutionContext ctx, DamagePacket packet)
    {
        return packet.ActionType == BattleActionType.NormalAttack
            || packet.ActionType == BattleActionType.Skill;
    }

    public void Apply(ActionExecutionContext ctx, DamagePacket packet)
    {
        if (packet.MinDamage < 1)
        {
            packet.MinDamage = 1;
        }
    }
}