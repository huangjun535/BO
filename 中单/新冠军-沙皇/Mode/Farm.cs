namespace YuLeAzir
{
    using LeagueSharp;
    using LeagueSharp.Common;
    using System;

    public static class Farm
    {
        public static void Initialize()
        {
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Program._orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LastHit && Program._orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LaneClear)
                return;

            if (OrbwalkCommands.CanDoAttack())
            {
                var minion = OrbwalkCommands.GetClearMinionsAndBuildings();

                if (minion.IsValidTarget())
                {
                    OrbwalkCommands.AttackTarget(minion);
                }
            }
        }
    }
}
