namespace YuLeAzir
{
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;
    using System;
    using System.Linq;

    public static class JumpToMouse
    {
        public static Obj_AI_Hero Player { get{ return ObjectManager.Player; } }
        public static int LastJump;
        public static void Initialize()
        {
            Game.OnUpdate += Game_OnUpdate;
            CustomEvents.Unit.OnDash += Unit_OnDash;
        }

        private static void Unit_OnDash(Obj_AI_Base sender, Dash.DashItem args)
        {
            //if (!sender.IsMe)
            //    return;
            //Game.PrintChat(args.Speed.ToString());
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!Program.eqmouse)
                return;

            if (OrbwalkCommands.CanMove())
            {
                OrbwalkCommands.MoveTo(Game.CursorPos);
            }

            if (Environment.TickCount - LastJump < 500)
                return;

            if (Program.Eisready && Program.Qisready())
            {
                var position = Game.CursorPos;
                var distance = Player.Position.Distance(position);
                var sold = Soldiers.soldier.Where(x => Player.Distance(x.Position) <= 1100).OrderBy(x => x.Position.Distance(Game.CursorPos)).FirstOrDefault();
                var posWs = GeoAndExten.GetWsPosition(position.To2D()).Where(x => x != null);
               
                // case 1
                if (sold != null && sold.Position.Distance(position) <= 875)
                {
                    var time = sold.Position.Distance(Player.Position) * 1000 / 1700;
                    Program._e.Cast(sold.Position);
                    Utility.DelayAction.Add((int)time - 150 - 100, () => Program._q2.Cast(position));
                    LastJump = Environment.TickCount;
                    return;
                }
             
                // case 2
                var posW2 = posWs.FirstOrDefault(x => ((Vector2)x).Distance(position) <= 875);

                if (Program._w.IsReady() && posW2 != null)
                {
                    var time = ((Vector2)posW2).Distance(Player.Position) * 1000 / 1700;
                    Program._w.Cast(Player.Position.To2D().Extend((Vector2)posW2,Program._w.Range));
                    Utility.DelayAction.Add(0, () => Program._e.Cast((Vector2)posW2));
                    Utility.DelayAction.Add((int)time + 300 - 150 - 100, () => Program._q2.Cast(position));
                    LastJump = Environment.TickCount;
                    return;
                }
                // case 3
                var posW3 = posWs.OrderBy(x => ((Vector2)x).Distance(position)).FirstOrDefault();

                if (sold != null && (posW3 == null || sold.Position.Distance(position) <= ((Vector2)posW3).Distance(position)))
                {
                    var time = sold.Position.Distance(Player.Position) * 1000 / 1700;
                    Program._e.Cast(sold.Position);
                    Utility.DelayAction.Add((int)time - 150 - 100, () => Program._q2.Cast(position));
                    LastJump = Environment.TickCount;
                    return;
                }

                if (Program._w.IsReady() && posW3 != null)
                {
                    var time = ((Vector2)posW3).Distance(Player.Position) * 1000 / 1700;
                    Program._w.Cast(Player.Position.To2D().Extend((Vector2)posW3, Program._w.Range));
                    Utility.DelayAction.Add(0, () => Program._e.Cast((Vector2)posW3));
                    Utility.DelayAction.Add((int)time + 300 - 150 - 100, () => Program._q2.Cast(position));
                    LastJump = Environment.TickCount;
                    return;
                }

            }
        }
    }
}
