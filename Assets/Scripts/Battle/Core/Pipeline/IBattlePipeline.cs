public interface IBattlePipeline
{
    ActionResult Execute(BattleContext battle, ActionRequest request);
}