namespace YuLeAzir
{
    using LeagueSharp;
    using LeagueSharp.Common;
    using System;
    using System.Linq;

    internal class LaneClear
    {
        public static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        internal static void Initialize()
        {
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Program._menu.Item("EnabledFarm").GetValue<bool>())
            {
                var min = MinionManager.GetMinions(Program._q.Range);

                if (Program._w.IsReady() && Program.wclear)
                {
                    if (min.Count() >= 2)
                    {
                        Program._w.Cast(min.FirstOrDefault().Position);
                    }
                }

                if (Program._q.IsReady() && Program.qclear)
                {
                    var QPred = Program._q.GetCircularFarmLocation(min, Program._q.Width);

                    if (QPred.MinionsHit >= 3)
                    {
                        Program._q.Cast(QPred.Position);
                    }
                }
            }
        }
    }
}