using System;

public sealed class StandardBattlePipeline : IBattlePipeline
{
    private readonly ICombatEventBus _eventBus;
    private readonly BattleEffectRegistry _effectRegistry;

    public StandardBattlePipeline(ICombatEventBus eventBus, BattleEffectRegistry effectRegistry = null)
    {
        _eventBus = eventBus;
        _effectRegistry = effectRegistry ?? new BattleEffectRegistry();
    }

    public ActionResult Execute(BattleContext battle, ActionRequest request)
    {
        var ctx = new ActionExecutionContext(battle, request);

        StageSubmit(ctx);

        if (!StageValidate(ctx))
        {
            EmitActionFinished(ctx);
            return ctx.Result;
        }

        StageBuild(ctx);
        StagePreResolve(ctx);
        StageResolve(ctx);
        StagePostResolve(ctx);
        StageCommit(ctx);
        StagePublish(ctx);

        return ctx.Result;
    }

    private void StageSubmit(ActionExecutionContext ctx)
    {
        EmitStage(ctx, BattleStage.Submit, "request submitted");
    }

    private bool StageValidate(ActionExecutionContext ctx)
    {
        EmitStage(ctx, BattleStage.Validate, "validate request");

        if (!ctx.Battle.TryGetEntity(ctx.Request.SourceEntityId, out var source))
        {
            ctx.Result = ActionResult.Fail(ctx.Request.RequestId, ActionResultCode.InvalidSource, "source not found");
            return false;
        }

        if (!ctx.Battle.TryGetEntity(ctx.Request.MainTargetEntityId, out var target))
        {
            ctx.Result = ActionResult.Fail(ctx.Request.RequestId, ActionResultCode.InvalidTarget, "target not found");
            return false;
        }

        if (source.IsDead)
        {
            ctx.Result = ActionResult.Fail(ctx.Request.RequestId, ActionResultCode.SourceDead, "source is dead");
            return false;
        }

        if (target.IsDead)
        {
            ctx.Result = ActionResult.Fail(ctx.Request.RequestId, ActionResultCode.TargetDead, "target is dead");
            return false;
        }

        ctx.Source = source;
        ctx.Target = target;
        return true;
    }

    private void StageBuild(ActionExecutionContext ctx)
    {
        EmitStage(ctx, BattleStage.Build, "build runtime context");

        ctx.PendingDamage = 0;
        ctx.PendingHeal = 0;

        ctx.DamagePacket.ResetRuntime();
        ctx.DamagePacket.RequestId = ctx.Request.RequestId;
        ctx.DamagePacket.SourceEntityId = ctx.Request.SourceEntityId;
        ctx.DamagePacket.TargetEntityId = ctx.Request.MainTargetEntityId;
        ctx.DamagePacket.ActionType = ctx.Request.ActionType;
        ctx.DamagePacket.DamageType = DamageType.Physical;

        ctx.DamageChain.Clear();
        ctx.DamageChain.Add(new SourceAttackAsBaseModifier());
        ctx.DamageChain.Add(new SkillFlatBonusModifier());
        ctx.DamageChain.Add(new TargetDefenseReductionModifier());
        ctx.DamageChain.Add(new OffensiveActionMinOneModifier());
    }

    private void StagePreResolve(ActionExecutionContext ctx)
    {
        EmitStage(ctx, BattleStage.PreResolve, "pre-resolve");
        _effectRegistry.ApplyPreResolve(ctx);
    }

    private void StageResolve(ActionExecutionContext ctx)
    {
        EmitStage(ctx, BattleStage.Resolve, "resolve");

        switch (ctx.Request.ActionType)
        {
            case BattleActionType.NormalAttack:
            case BattleActionType.Skill:
            {
                ctx.PendingDamage = ctx.DamageChain.Resolve(ctx, ctx.DamagePacket);
                ctx.PendingHeal = 0;
                break;
            }
            default:
            {
                ctx.PendingDamage = 0;
                ctx.PendingHeal = 0;
                break;
            }
        }
    }

    private void StagePostResolve(ActionExecutionContext ctx)
    {
        EmitStage(ctx, BattleStage.PostResolve, "post-resolve");
        _effectRegistry.ApplyPostResolve(ctx);
    }

    private void StageCommit(ActionExecutionContext ctx)
    {
        EmitStage(ctx, BattleStage.Commit, "commit");

        int realDamage = ctx.Target.ApplyDamage(ctx.PendingDamage);

        // 后置阶段可能写入回复
        int realHeal = 0;
        if (ctx.PendingHeal > 0)
        {
            realHeal = ctx.Source.ApplyHeal(ctx.PendingHeal);
        }

        ctx.Result = ActionResult.Ok(
            ctx.Request.RequestId,
            ctx.Request.SourceEntityId,
            ctx.Request.MainTargetEntityId,
            realDamage,
            realHeal,
            "ok");
    }

    private void StagePublish(ActionExecutionContext ctx)
    {
        EmitStage(ctx, BattleStage.Publish, "publish result");
        EmitActionFinished(ctx);
        _effectRegistry.TickAndCleanup();
    }

    private void EmitStage(ActionExecutionContext ctx, BattleStage stage, string message)
    {
        _eventBus?.Publish(new StageEvent
        {
            BattleId = ctx.Battle.BattleId,
            Tick = ctx.Battle.Tick,
            RequestId = ctx.Request.RequestId,
            Stage = stage,
            UtcTime = DateTime.UtcNow,
            Message = message
        });
    }

    private void EmitActionFinished(ActionExecutionContext ctx)
    {
        _eventBus?.Publish(new ActionFinishedEvent
        {
            BattleId = ctx.Battle.BattleId,
            Tick = ctx.Battle.Tick,
            RequestId = ctx.Request.RequestId,
            Stage = BattleStage.Publish,
            UtcTime = DateTime.UtcNow,
            ResultCode = ctx.Result.Code,
            SourceEntityId = ctx.Result.SourceEntityId,
            MainTargetEntityId = ctx.Result.MainTargetEntityId,
            DamageApplied = ctx.Result.DamageApplied,
            HealApplied = ctx.Result.HealApplied,
            Message = ctx.Result.Message
        });
    }
}