﻿using LeagueSharp.SDK;

using Settings = Ashe.Config.Modes.LaneClear;

namespace Ashe.Modes
{
    internal sealed class LaneClear : ModeBase
    {
        internal override bool ShouldBeExecuted()
        {
            return Config.Keys.LaneClearActive;
        }

        internal override void Execute()
        {
            if (!Variables.Orbwalker.CanMove)
            {
                return;
            }

        }
    }
}
