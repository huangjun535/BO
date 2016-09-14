using LeagueSharp.SDK;
using LeagueSharp.SDK.Utils;
using NLog;
using System;
using System.Collections.Generic;
using YuLeAshe.Modes;

namespace YuLeAshe
{
    internal static class ModeManager
    {
        private static readonly List<Modes.Modes> Modes;

        static ModeManager()
        {
            Modes = new List<Modes.Modes>
            {
                new PermaActive(),
                new Combo(),
                new Harass(),
                new LaneClear(),
                new JungleClear(),
                new Flee()
            };

            new TickOperation(0x42, () =>
            {
                if (GameObjects.Player.IsDead)
                {
                    return;
                }

                Modes.ForEach(mode =>
                {
                    try
                    {
                        if (mode.ShouldBeExecuted())
                        {
                            mode.Execute();
                        }
                    }
                    catch (Exception e)
                    {
                        LogManager.GetCurrentClassLogger().Error($"Error executing mode '{mode.GetType().Name}'\n{e}");
                    }
                });
            }).Start();
        }

        internal static void Initialize() { }
    }
}
