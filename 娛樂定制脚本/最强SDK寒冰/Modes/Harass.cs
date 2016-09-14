using LeagueSharp.SDK;

using Settings = Ashe.Config.Modes.Harass;

namespace Ashe.Modes
{
    internal sealed class Harass : ModeBase
    {
        internal override bool ShouldBeExecuted()
        {
            return Config.Keys.HarassActive;
        }

        internal override void Execute()
        {
            if (!Variables.Orbwalker.CanMove)
            {//평타모션중엔 리턴
                return;
            }

            if (Settings.UseW && W.IsReady() && GameObjects.Player.ManaPercent > Settings.MinMana)
            {
                var target = Variables.TargetSelector.GetTargetNoCollision(W);
                if (target != null)
                {//충돌없는 적 챔프있으면 W맞추기
                    W.Cast(target);
                }
            }
        }
    }
}
