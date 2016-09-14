﻿namespace YuLeAzir
{
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;
    using System;
    using System.Linq;
    using Color = System.Drawing.Color;

    public static class Insec
    {
        public static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public static int LastJump;
        public static Vector3 InsecPoint = new Vector3();

        internal static Obj_AI_Hero GetInsecTarget
        {
            get
            {
                Obj_AI_Hero target = null;

                var Qtarget = TargetSelector.GetTarget(2000, TargetSelector.DamageType.Magical);

                if (Qtarget.IsValidTarget())
                {
                    target = Qtarget;
                    TargetSelector.SetTarget(target);
                }

                return target;
            }
        }

        public static void Initialize()
        {
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Program.drawinsecLine)
                return;

            var target = GetInsecTarget;

            if (target.IsValidTarget() && !target.IsZombie)
            {
                Render.Circle.DrawCircle(target.Position, 100, Color.Yellow);
                if (InsecPoint.IsValid())
                {
                    var point = target.Distance(InsecPoint) >= 400 ? target.Position.Extend(InsecPoint, 400) : InsecPoint;
                    Drawing.DrawLine(Drawing.WorldToScreen(target.Position), Drawing.WorldToScreen(point), 3, Color.Red);
                }
            }
            if (InsecPoint.IsValid())
                Render.Circle.DrawCircle(InsecPoint, 100, Color.Pink);
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            switch (Program.insecmode)
            {
                case 0:
                    var hero = HeroManager.Allies.Where(x => !x.IsMe && !x.IsDead).OrderByDescending(x => x.Distance(Player.Position)).LastOrDefault();
                    if (hero != null)
                        InsecPoint = hero.Position;
                    break;
                case 1:
                    var turret = GameObjects.AllyTurrets.OrderByDescending(x => x.Distance(Player.Position)).LastOrDefault();
                    if (turret != null)
                        InsecPoint = turret.Position;
                    break;
                case 2:
                    InsecPoint = Game.CursorPos;
                    break;
            }

            if (!Program.insec)
                return;

            if (OrbwalkCommands.CanMove())
            {
                OrbwalkCommands.MoveTo(Game.CursorPos);
            }

            if (!InsecPoint.IsValid())
                return;

            var target = GetInsecTarget;

            if (!target.IsValidTarget() || target.IsZombie)
                return;

            if (!Program._r2.IsReady())
                return;

            Vector2 start1 = Player.Position.To2D().Extend(InsecPoint.To2D(), -300);
            Vector2 end1 = start1.Extend(Player.Position.To2D(), 750);

            float width1 = Program._r.Level == 3 ? 125 * 6 / 2 : Program._r.Level == 2 ? 125 * 5 / 2 : 125 * 4 / 2;

            var Rect1 = new Geometry.Polygon.Rectangle(start1, end1, width1 - 100);
            var Predicted1 = Prediction.GetPrediction(target, Game.Ping / 1000f + 0.25f).UnitPosition;

            if (Rect1.IsInside(target.Position) && Rect1.IsInside(Predicted1))
            {
                Program._r2.Cast(InsecPoint);
                return;
            }

            if (Environment.TickCount - LastJump < 1500)
                return;

            if (!Program._e.IsReady())
                return;

            var sold2 = Soldiers.soldier.Where(x => Player.Distance(x.Position) <= 1100).OrderBy(x => x.Position.Distance(target.Position)).FirstOrDefault();

            if (sold2 != null)
            {
                if (!Program._q2.IsReady())
                {
                    var time = Player.Position.Distance(sold2.Position) / 1700f;
                    var predicted2 = Prediction.GetPrediction(target, time).UnitPosition;

                    Vector2 start2 = sold2.Position.To2D().Extend(InsecPoint.To2D(), -300);
                    Vector2 end2 = start2.Extend(InsecPoint.To2D(), 750);

                    float width2 = Program._r.Level == 3 ? 125 * 6 / 2 : Program._r.Level == 2 ? 125 * 5 / 2 : 125 * 4 / 2;

                    var Rect2 = new Geometry.Polygon.Rectangle(start2, end2, width2 - 100);

                    if (Rect2.IsInside(target.Position) && Rect2.IsInside(predicted2))
                    {
                        Program._e.Cast(sold2.Position);
                        LastJump = Environment.TickCount;
                        return;
                    }
                }

                if (Program._q2.IsReady() && target.Distance(sold2.Position) <= 875 - 100)
                {
                    var time = (Player.Distance(sold2.Position) + sold2.Position.Distance(target.Position)) / 1700f;
                    var predicted2 = Prediction.GetPrediction(target, time).UnitPosition;

                    Vector2 start2 = target.Position.To2D().Extend(InsecPoint.To2D(), -300);
                    Vector2 end2 = start2.Extend(InsecPoint.To2D(), 750);

                    float width2 = Program._r.Level == 3 ? 125 * 6 / 2 : Program._r.Level == 2 ? 125 * 5 / 2 : 125 * 4 / 2;

                    var Rect2 = new Geometry.Polygon.Rectangle(start2, end2, width2 - 100);

                    if (Rect2.IsInside(target.Position) && Rect2.IsInside(predicted2))
                    {
                        var timetime = sold2.Position.Distance(Player.Position) * 1000 / 1700;
                        Program._e.Cast(sold2.Position);
                        Utility.DelayAction.Add((int)timetime - 150 - 100, () => Program._q2.Cast(target.Position));
                        LastJump = Environment.TickCount;
                        return;
                    }
                }
            }

            if(Program._w.IsReady())
            {
                var posWs = GeoAndExten.GetWsPosition(target.Position.To2D()).Where(x => x != null);

                foreach (var posW in posWs)
                {
                    if (!Program._q2.IsReady())
                    {
                        var time = Player.Position.To2D().Distance((Vector2)posW) / 1700f + 0.3f;
                        var predicted2 = Prediction.GetPrediction(target, time).UnitPosition;

                        Vector2 start2 = ((Vector2)posW).Extend(InsecPoint.To2D(), -300);
                        Vector2 end2 = start2.Extend(InsecPoint.To2D(), 750);

                        float width2 = Program._r.Level == 3 ? 125 * 6 / 2 : Program._r.Level == 2 ? 125 * 5 / 2 : 125 * 4 / 2;

                        var Rect2 = new Geometry.Polygon.Rectangle(start2, end2, width2 - 100);

                        if (Rect2.IsInside(target.Position) && Rect2.IsInside(predicted2))
                        {
                            var timetime = ((Vector2)posW).Distance(Player.Position) * 1000 / 1700;
                            Program._w.Cast(Player.Position.To2D().Extend((Vector2)posW, Program._w.Range));
                            Utility.DelayAction.Add(0, () => Program._e.Cast((Vector2)posW));
                            Utility.DelayAction.Add((int)timetime + 300 - 150 - 100, () => Program._q2.Cast(target.Position));
                            LastJump = Environment.TickCount;
                            return;
                        }
                    }

                    if (Program._q2.IsReady() && target.Distance((Vector2)posW) <= 875 - 100)
                    {
                        var time = (Player.Distance((Vector2)posW) + ((Vector2)posW).Distance(target.Position)) / 1700f + 0.3f;
                        var predicted2 = Prediction.GetPrediction(target, time).UnitPosition;

                        Vector2 start2 = target.Position.To2D().Extend(InsecPoint.To2D(), -300);
                        Vector2 end2 = start2.Extend(InsecPoint.To2D(), 750);

                        float width2 = Program._r.Level == 3 ? 125 * 6 / 2 : Program._r.Level == 2 ? 125 * 5 / 2 : 125 * 4 / 2;

                        var Rect2 = new Geometry.Polygon.Rectangle(start2, end2, width2 - 100);

                        if (Rect2.IsInside(target.Position) && Rect2.IsInside(predicted2))
                        {
                            var timetime = ((Vector2)posW).Distance(Player.Position) * 1000 / 1700;
                            Program._w.Cast(Player.Position.To2D().Extend((Vector2)posW, Program._w.Range));
                            Utility.DelayAction.Add(0, () => Program._e.Cast((Vector2)posW));
                            Utility.DelayAction.Add((int)timetime + 300 - 150 - 100, () => Program._q2.Cast(target.Position));
                            LastJump = Environment.TickCount;
                            return;
                        }
                    }
                }
            }
        }
    }
}
