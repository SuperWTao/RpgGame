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
            return ctx.result;
        }

        StageBuild(ctx);
        StagePreResolve(ctx);
        StageResolve(ctx);
        StagePostResolve(ctx);
        StageCommit(ctx);
        StagePublish(ctx);

        return ctx.result;
    }

    private void StageSubmit(ActionExecutionContext ctx)
    {
        EmitStage(ctx, BattleStage.Submit, "request submitted");
    }

    private bool StageValidate(ActionExecutionContext ctx)
    {
        EmitStage(ctx, BattleStage.Validate, "validate request");

        if (!ctx.battle.TryGetEntity(ctx.request.sourceEntityId, out var source))
        {
            ctx.result = ActionResult.Fail(ctx.request.requestId, ActionResultCode.InvalidSource, "source not found");
            return false;
        }

        if (!ctx.battle.TryGetEntity(ctx.request.mainTargetEntityId, out var target))
        {
            ctx.result = ActionResult.Fail(ctx.request.requestId, ActionResultCode.InvalidTarget, "target not found");
            return false;
        }

        if (source.isDead)
        {
            ctx.result = ActionResult.Fail(ctx.request.requestId, ActionResultCode.SourceDead, "source is dead");
            return false;
        }

        if (target.isDead)
        {
            ctx.result = ActionResult.Fail(ctx.request.requestId, ActionResultCode.TargetDead, "target is dead");
            return false;
        }

        ctx.source = source;
        ctx.target = target;
        return true;
    }

    private void StageBuild(ActionExecutionContext ctx)
    {
        EmitStage(ctx, BattleStage.Build, "build runtime context");

        ctx.pendingDamage = 0;
        ctx.pendingHeal = 0;

        ctx.damagePacket.ResetRuntime();
        ctx.damagePacket.requestId = ctx.request.requestId;
        ctx.damagePacket.sourceEntityId = ctx.request.sourceEntityId;
        ctx.damagePacket.targetEntityId = ctx.request.mainTargetEntityId;
        ctx.damagePacket.actionType = ctx.request.actionType;
        ctx.damagePacket.damageType = DamageType.Physical;

        ctx.damageChain.Clear();
        ctx.damageChain.Add(new SourceAttackAsBaseModifier());
        ctx.damageChain.Add(new SkillFlatBonusModifier());
        ctx.damageChain.Add(new TargetDefenseReductionModifier());
        ctx.damageChain.Add(new OffensiveActionMinOneModifier());
    }

    private void StagePreResolve(ActionExecutionContext ctx)
    {
        EmitStage(ctx, BattleStage.PreResolve, "pre-resolve");
        _effectRegistry.ApplyPreResolve(ctx);
    }

    private void StageResolve(ActionExecutionContext ctx)
    {
        EmitStage(ctx, BattleStage.Resolve, "resolve");

        switch (ctx.request.actionType)
        {
            case BattleActionType.NormalAttack:
            case BattleActionType.Skill:
            {
                ctx.pendingDamage = ctx.damageChain.Resolve(ctx, ctx.damagePacket);
                ctx.pendingHeal = 0;
                break;
            }
            default:
            {
                ctx.pendingDamage = 0;
                ctx.pendingHeal = 0;
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

        int realDamage = ctx.target.ApplyDamage(ctx.pendingDamage);

        // 后置阶段可能写入回复
        int realHeal = 0;
        if (ctx.pendingHeal > 0)
        {
            realHeal = ctx.source.ApplyHeal(ctx.pendingHeal);
        }

        ctx.result = ActionResult.Ok(
            ctx.request.requestId,
            ctx.request.sourceEntityId,
            ctx.request.mainTargetEntityId,
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
            battleId = ctx.battle.battleId,
            tick = ctx.battle.tick,
            requestId = ctx.request.requestId,
            stage = stage,
            utcTime = DateTime.UtcNow,
            message = message
        });
    }

    private void EmitActionFinished(ActionExecutionContext ctx)
    {
        _eventBus?.Publish(new ActionFinishedEvent
        {
            battleId = ctx.battle.battleId,
            tick = ctx.battle.tick,
            requestId = ctx.request.requestId,
            stage = BattleStage.Publish,
            utcTime = DateTime.UtcNow,
            resultCode = ctx.result.code,
            sourceEntityId = ctx.result.sourceEntityId,
            mainTargetEntityId = ctx.result.mainTargetEntityId,
            damageApplied = ctx.result.damageApplied,
            healApplied = ctx.result.healApplied,
            message = ctx.result.message
        });
    }
}