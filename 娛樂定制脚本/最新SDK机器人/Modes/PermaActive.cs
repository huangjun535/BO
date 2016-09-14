using System.Linq;

using LeagueSharp.SDK;
using LeagueSharp.SDK.UI;

using Settings = Blitzcrank.Config.Auto;

namespace Blitzcrank.Modes
{
    internal sealed class PermaActive : ModeBase
    {
        internal override bool ShouldBeExecuted()
        {
            return true;
        }

        internal override void Execute()
        {
            if (Settings.AutoQ.Enabled && Q.IsReady())
            {
                var ignorechamps = GameObjects.EnemyHeroes.Where(x => !Settings.AutoQ.Menu.GetValue<MenuBool>(x.ChampionName).Value);
                var target = Variables.TargetSelector.GetTargetNoCollision(Q, false, ignorechamps);
                if (target != null)
                {
                    Extensions.CastQ(target);
                }
            }
        }
    }
}
