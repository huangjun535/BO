using System.Linq;

using LeagueSharp;
using LeagueSharp.SDK;

using Settings = Ashe.Config.Modes.Flee;

namespace Ashe.Modes
{
    internal sealed class Flee : ModeBase
    {
        internal override bool ShouldBeExecuted()
        {
            return Config.Keys.FleeActive;
        }

        internal override void Execute()
        {
            Variables.Orbwalker.Move(Game.CursorPos);

            if (Settings.UseW && W.IsReady())
            {
                //가장 가까운 적에게 W 맞추기
                W.Cast(GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(W.Range)).OrderBy(x => x.DistanceToPlayer()).FirstOrDefault());
            }
        }
    }
}
