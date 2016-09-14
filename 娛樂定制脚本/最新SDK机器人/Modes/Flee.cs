using LeagueSharp;
using LeagueSharp.SDK;

using Settings = Blitzcrank.Config.Modes.Flee;

namespace Blitzcrank.Modes
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
                W.Cast();
            }
        }
    }
}
