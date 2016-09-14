﻿using LeagueSharp.SDK;

namespace Ashe.Modes
{
    internal abstract class ModeBase
    {
        protected static Spell Q => SpellManager.Q;

        protected static Spell W => SpellManager.W;

        protected static Spell E => SpellManager.E;

        protected static Spell R => SpellManager.R;

        internal abstract bool ShouldBeExecuted();

        internal abstract void Execute();
    }
}
