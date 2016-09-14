#region
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Marksman.Common;
using Marksman.Orb;
using Marksman.Utils;
using SharpDX;
using Color = System.Drawing.Color;
using Orbwalking = Marksman.Orb.Orbwalking;

#endregion

namespace Marksman.Champions
{
    internal class Lucian : Champion
    {
        public static Spell Q, Q2;

        public static Spell W;

        public static Spell E;

        public static Spell R;

        public static bool DoubleHit = false;

        private static int xAttackLeft;

        private static float xPassiveUsedTime;

        public Lucian()
        {
            Utils.Utils.PrintMessage("Lucian loaded.");

            Q = new Spell(SpellSlot.Q, 760);
            Q2 = new Spell(SpellSlot.Q, 1100);
            W = new Spell(SpellSlot.W, 1000);

            Q.SetSkillshot(0.45f, 60f, 1100f, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.30f, 80f, 1600f, true, SkillshotType.SkillshotLine);
            E = new Spell(SpellSlot.E, 475);
            R = new Spell(SpellSlot.R, 1400);

            xPassiveUsedTime = Game.Time;

            Obj_AI_Base.OnProcessSpellCast += Game_OnProcessSpell;

        }

        public override void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (xAttackLeft == 1)
            {
                args.Process = false;
            }
            
        }
        public static Obj_AI_Base QMinion(Obj_AI_Hero t)
        {
            var m = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All,
                MinionTeam.NotAlly, MinionOrderTypes.None);

            return (from vM
                        in m.Where(vM => vM.IsValidTarget(Q.Range))
                    let endPoint = vM.ServerPosition.To2D().Extend(ObjectManager.Player.ServerPosition.To2D(), -Q2.Range).To3D()
                    where
                        vM.Distance(t) <= t.Distance(ObjectManager.Player) &&
                        Intersection(ObjectManager.Player.ServerPosition.To2D(), endPoint.To2D(), t.ServerPosition.To2D(), vM.BoundingRadius)
                    //Intersection(ObjectManager.Player.ServerPosition.To2D(), endPoint.To2D(), t.ServerPosition.To2D(), t.BoundingRadius + Q.Width/4)
                    select vM).FirstOrDefault();
            //get
            //{
            //    var vTarget = TargetSelector.GetTarget(Q2.Range, TargetSelector.DamageType.Physical);
            //    var vMinions = MinionManager.GetMinions(
            //        ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly,
            //        MinionOrderTypes.None);

            //    return (from vMinion in vMinions.Where(vMinion => vMinion.IsValidTarget(Q.Range))
            //            let endPoint =
            //                vMinion.ServerPosition.To2D()
            //                    .Extend(ObjectManager.Player.ServerPosition.To2D(), -Q2.Range)
            //                    .To3D()
            //            where
            //                vMinion.Distance(vTarget) <= vTarget.Distance(ObjectManager.Player) &&
            //                Intersection(ObjectManager.Player.ServerPosition.To2D(), endPoint.To2D(),
            //                    vTarget.ServerPosition.To2D(), vTarget.BoundingRadius + vMinion.BoundingRadius)
            //            select vMinion).FirstOrDefault();
            //}
        }
        public static bool IsPositionSafeForE(Obj_AI_Hero target, Spell spell)
        {
            var predPos = spell.GetPrediction(target).UnitPosition.To2D();
            var myPos = ObjectManager.Player.Position.To2D();
            var newPos = (target.Position.To2D() - myPos);
            newPos.Normalize();

            var checkPos = predPos + newPos * (spell.Range - Vector2.Distance(predPos, myPos));
            Obj_Turret closestTower = null;

            foreach (var tower in ObjectManager.Get<Obj_Turret>()
                .Where(tower => tower.IsValid && !tower.IsDead && Math.Abs(tower.Health) > float.Epsilon)
                .Where(tower => Vector3.Distance(tower.Position, ObjectManager.Player.Position) < 1450))
            {
                closestTower = tower;
            }

            if (closestTower == null)
                return true;

            if (Vector2.Distance(closestTower.Position.To2D(), checkPos) <= 910)
                return false;

            return true;
        }


        private static void GetJumpPosition()
        {
            foreach (var t in HeroManager.Enemies.Where(e => e.IsValidTarget(2500)))
            {

                var toPolygon = new CommonGeometry.Rectangle(ObjectManager.Player.Position.To2D(),
                                             ObjectManager.Player.Position.To2D().Extend(t.Position.To2D(), t.Distance(ObjectManager.Player.Position)),
                                             E.Range).ToPolygon();
                toPolygon.Draw(System.Drawing.Color.Red, 1);


                //Console.WriteLine(hero.ChampionName);

                for (int j = 20; j < 361; j += 20)
                {
                    Vector2 wcPositive = ObjectManager.Player.Position.To2D() + Vector2.Normalize(t.Position.To2D() - ObjectManager.Player.Position.To2D()).Rotated(j * (float)Math.PI / 180) * E.Range;
                    if (!wcPositive.IsWall() && t.Distance(wcPositive) > E.Range )
                    Render.Circle.DrawCircle(wcPositive.To3D(), 105f, Color.GreenYellow);
                    //if (!wcPositive.IsWall())
                    //{
                    //    ListWJumpPositions.Add(wcPositive);
                    //}

                    //Vector2 wcNegative = ObjectManager.Player.Position.To2D() +
                    //                     Vector2.Normalize(hero.Position.To2D() - ObjectManager.Player.Position.To2D())
                    //                         .Rotated(-j * (float)Math.PI / 180) * E.Range;

                    //Render.Circle.DrawCircle(wcNegative.To3D(), 105f, Color.White);
                    //if (!wcNegative.IsWall())
                    //{
                    //    ListWJumpPositions.Add(wcNegative);
                    //}
                }


            }

            //Vector2 location = ObjectManager.Player.Position.To2D() +
            //                   Vector2.Normalize(t.Position.To2D() - ObjectManager.Player.Position.To2D()) * W.Range;
            //Vector2 wCastPosition = location;

            ////Render.Circle.DrawCircle(wCastPosition.To3D(), 105f, System.Drawing.Color.Red);


            //if (!wCastPosition.IsWall())
            //{
            //    xList.Add(wCastPosition);
            //}

            //if (!wCastPosition.IsWall())
            //{
            //    ExistingJumpPositions.Add(new ListJumpPositions
            //    {
            //        Position = wCastPosition,
            //        Name = name
            //    });

            //    ListWJumpPositions.Add(wCastPosition);
            //}

            //if (wCastPosition.IsWall())
            //{
            //    for (int j = 20; j < 80; j += 20)
            //    {
            //        Vector2 wcPositive = ObjectManager.Player.Position.To2D() +
            //                             Vector2.Normalize(t.Position.To2D() - ObjectManager.Player.Position.To2D())
            //                                 .Rotated(j * (float)Math.PI / 180) * W.Range;
            //        if (!wcPositive.IsWall())
            //        {
            //            ListWJumpPositions.Add(wcPositive);
            //        }

            //        Vector2 wcNegative = ObjectManager.Player.Position.To2D() +
            //                             Vector2.Normalize(t.Position.To2D() - ObjectManager.Player.Position.To2D())
            //                                 .Rotated(-j * (float)Math.PI / 180) * W.Range;
            //        if (!wcNegative.IsWall())
            //        {
            //            ListWJumpPositions.Add(wcNegative);
            //        }
            //    }

            //    float xDiff = ObjectManager.Player.Position.X - t.Position.X;
            //    float yDiff = ObjectManager.Player.Position.Y - t.Position.Y;
            //    int angle = (int)(Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI);
            //}

            ////foreach (var aa in ListWJumpPositions)
            ////{
            ////    Render.Circle.DrawCircle(aa.To3D2(), 105f, System.Drawing.Color.White);
            ////}
            //var al1 = xList.OrderBy(al => al.Distance(t.Position)).First();

            //var color = System.Drawing.Color.DarkRed;
            //var width = 4;

            //var startpos = ObjectManager.Player.Position;
            //var endpos = al1.To3D();
            //if (startpos.Distance(endpos) > 100)
            //{
            //    var endpos1 = al1.To3D() +
            //                  (startpos - endpos).To2D().Normalized().Rotated(25 * (float)Math.PI / 180).To3D() * 75;
            //    var endpos2 = al1.To3D() +
            //                  (startpos - endpos).To2D().Normalized().Rotated(-25 * (float)Math.PI / 180).To3D() * 75;

            //    //var x1 = new LeagueSharp.Common.Geometry.Polygon.Line(startpos, endpos);
            //    //x1.Draw(color, width - 2);
            //    new LeagueSharp.Common.Geometry.Polygon.Line(startpos, endpos).Draw(color, width - 2);


            //    var y1 = new LeagueSharp.Common.Geometry.Polygon.Line(endpos, endpos1);
            //    y1.Draw(color, width - 2);
            //    var z1 = new LeagueSharp.Common.Geometry.Polygon.Line(endpos, endpos2);
            //    z1.Draw(color, width - 2);
            //}


            ////foreach (var al in ListWJumpPositions.OrderBy(al => al.Distance(t.Position)))
            ////{
            ////    Render.Circle.DrawCircle(al.To3D(), 105f, System.Drawing.Color.White);
            ////}
            ////            Render.Circle.DrawCircle(al1.To3D(), 85, System.Drawing.Color.White);
            //return al1;
        }

        public override void DrawingOnEndScene(EventArgs args)
        {
            return;
            if (Config.Item("Passive" + Id).GetValue<bool>() && xAttackLeft > 0)
            {
                return;
            }

            var nClosesEnemy = HeroManager.Enemies.Find(e => e.IsValidTarget(Orbwalking.GetRealAutoAttackRange(null)));
            if (nClosesEnemy != null)
            {

                var aaRange = Orbwalking.GetRealAutoAttackRange(null) + 65 - ObjectManager.Player.Distance(nClosesEnemy);
                Render.Circle.DrawCircle(ObjectManager.Player.Position, aaRange, Color.BurlyWood);

                Vector2 wcPositive = ObjectManager.Player.Position.To2D() - Vector2.Normalize(nClosesEnemy.Position.To2D() - ObjectManager.Player.Position.To2D()).Rotated((float)Math.PI / 180) * aaRange;
                Vector2 wcPositive2 = ObjectManager.Player.Position.To2D() - Vector2.Normalize(nClosesEnemy.Position.To2D() - ObjectManager.Player.Position.To2D()).Rotated(30 * (float)Math.PI / 180) * aaRange;
                Vector2 wcPositive3 = ObjectManager.Player.Position.To2D() - Vector2.Normalize(nClosesEnemy.Position.To2D() - ObjectManager.Player.Position.To2D()).Rotated(-30 * (float)Math.PI / 180) * aaRange;

                Vector2 wcPositive2x = ObjectManager.Player.Position.To2D() - Vector2.Normalize(nClosesEnemy.Position.To2D() - ObjectManager.Player.Position.To2D()).Rotated(60 * (float)Math.PI / 180) * aaRange;
                Vector2 wcPositive3x = ObjectManager.Player.Position.To2D() - Vector2.Normalize(nClosesEnemy.Position.To2D() - ObjectManager.Player.Position.To2D()).Rotated(-60 * (float)Math.PI / 180) * aaRange;

                if (E.IsReady())
                {
                    var runHere = Vector2.Zero;
                    if (!wcPositive.IsWall())
                        runHere = wcPositive;
                    else if (!wcPositive2.IsWall())
                        runHere = wcPositive2;
                    else if (!wcPositive3.IsWall())
                        runHere = wcPositive3;
                    else if (!wcPositive2x.IsWall())
                        runHere = wcPositive2x;
                    else if (!wcPositive3x.IsWall())
                        runHere = wcPositive3x;

                    if (runHere != Vector2.Zero && ObjectManager.Player.Distance(runHere) > ObjectManager.Player.BoundingRadius * 2)
                        E.Cast(runHere);
                }

                Render.Circle.DrawCircle(wcPositive2.To3D(), 80f, Color.Red);
                Render.Circle.DrawCircle(wcPositive3.To3D(), 80f, Color.Yellow);
                Render.Circle.DrawCircle(wcPositive.To3D(), 80, Color.BurlyWood);

                Render.Circle.DrawCircle(wcPositive2x.To3D(), 80f, Color.Red);
                Render.Circle.DrawCircle(wcPositive3x.To3D(), 80f, Color.Yellow);

            }
            //if (Q.IsReady())
            //{
            return;
                foreach (var t in HeroManager.Enemies.Where(e => e.IsValidTarget(1100)))
                {

                    var toPolygon =
                        new CommonGeometry.Rectangle(ObjectManager.Player.Position.To2D(),
                            ObjectManager.Player.Position.To2D()
                                .Extend(t.Position.To2D(), t.Distance(ObjectManager.Player.Position)), 30).ToPolygon();
                    toPolygon.Draw(System.Drawing.Color.Red, 1);

                    var o = ObjectManager
                        .Get<Obj_AI_Base>(
                            ).FirstOrDefault(e => e.IsEnemy && !e.IsDead && e.NetworkId != t.NetworkId && toPolygon.IsInside(e) &&
                                ObjectManager.Player.Distance(t.Position) > ObjectManager.Player.Distance(e) && e.Distance(t) > t.BoundingRadius && e.Distance(ObjectManager.Player) > ObjectManager.Player.BoundingRadius);

                    if (o != null)
                    {
                        Render.Circle.DrawCircle(o.Position, 105f, Color.GreenYellow);
                        Q.CastOnUnit(o);
                    }

                    Vector2 wcPositive = ObjectManager.Player.Position.To2D() - Vector2.Normalize(t.Position.To2D() - ObjectManager.Player.Position.To2D()).Rotated((float)Math.PI / 180) * (E.Range - 50);
                    Render.Circle.DrawCircle(wcPositive.To3D(), 60, Color.BurlyWood);
                    Render.Circle.DrawCircle(wcPositive.To3D(), 80f, Color.BurlyWood);
                    Render.Circle.DrawCircle(wcPositive.To3D(), 100f, Color.BurlyWood);

                }
            //}

            return;
            foreach (var t in HeroManager.Enemies.Where(e => e.IsValidTarget(1100)))
            {

                var toPolygon = new CommonGeometry.Rectangle(ObjectManager.Player.Position.To2D(), ObjectManager.Player.Position.To2D().Extend(t.Position.To2D(), t.Distance(ObjectManager.Player.Position)), 40).ToPolygon();
                toPolygon.Draw(System.Drawing.Color.Red, 1);


                foreach (var obj in ObjectManager.Get<Obj_AI_Base>())
                {
                    
                }

                //Console.WriteLine(hero.ChampionName);

                for (int j = 20; j < 361; j += 20)
                {
                    Vector2 wcPositive = ObjectManager.Player.Position.To2D() + Vector2.Normalize(t.Position.To2D() - ObjectManager.Player.Position.To2D()).Rotated(j * (float)Math.PI / 180) * E.Range;
                    if (!wcPositive.IsWall() && t.Distance(wcPositive) > E.Range)
                        Render.Circle.DrawCircle(wcPositive.To3D(), 105f, Color.GreenYellow);
                    //if (!wcPositive.IsWall())
                    //{
                    //    ListWJumpPositions.Add(wcPositive);
                    //}

                    //Vector2 wcNegative = ObjectManager.Player.Position.To2D() +
                    //                     Vector2.Normalize(hero.Position.To2D() - ObjectManager.Player.Position.To2D())
                    //                         .Rotated(-j * (float)Math.PI / 180) * E.Range;

                    //Render.Circle.DrawCircle(wcNegative.To3D(), 105f, Color.White);
                    //if (!wcNegative.IsWall())
                    //{
                    //    ListWJumpPositions.Add(wcNegative);
                    //}
                }


            }

        }

        public override void Drawing_OnDraw(EventArgs args)
        {
            GetJumpPosition();
            return;
            Spell[] spellList = { Q, Q2, W, E, R };
            foreach (var spell in spellList)
            {
                var menuItem = GetValue<Circle>("Draw" + spell.Slot);
                if (!menuItem.Active || spell.Level < 0 && spell.IsReady()) return;

                Render.Circle.DrawCircle(ObjectManager.Player.Position, spell.Range, menuItem.Color);
            }
        }

        public static bool Intersection(Vector2 p1, Vector2 p2, Vector2 pC, float radius)
        {
            var p3 = new Vector2(pC.X + radius, pC.Y + radius);

            var m = ((p2.Y - p1.Y) / (p2.X - p1.X));
            var constant = (m * p1.X) - p1.Y;
            var b = -(2f * ((m * constant) + p3.X + (m * p3.Y)));
            var a = (1 + (m * m));
            var c = ((p3.X * p3.X) + (p3.Y * p3.Y) - (radius * radius) + (2f * constant * p3.Y) + (constant * constant));
            var d = ((b * b) - (4f * a * c));

            return d > 0;
        }

        public void Game_OnProcessSpell(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (!unit.IsMe || spell.SData.Name.Contains("summoner") || !Config.Item("Passive" + Id).GetValue<bool>())
            {
                return;
            }

            //if (spell.Slot == SpellSlot.E || spell.Slot == SpellSlot.W || spell.Slot == SpellSlot.E || spell.Slot == SpellSlot.R)
            if (spell.SData.Name.ToLower().Contains("lucianq") || spell.SData.Name.ToLower().Contains("lucianw") ||
                spell.SData.Name.ToLower().Contains("luciane") || spell.SData.Name.ToLower().Contains("lucianr"))
            {
                xAttackLeft = 1;
                xPassiveUsedTime = Game.Time;
            }

            if (spell.SData.Name.ToLower().Contains("lucianpassiveattack"))
            {
                Utility.DelayAction.Add(500, () => { xAttackLeft -= 1; });
            }
        }

        public override void Game_OnUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsDead)
            {
                xAttackLeft = 0;
                return;
            }

            if (Game.Time > xPassiveUsedTime + 3 && xAttackLeft == 1)
            {
                xAttackLeft = 0;
            }

            if (Config.Item("Passive" + Id).GetValue<bool>() && xAttackLeft > 0)
            {
                return;
            }
            
            Obj_AI_Hero t;

            if (Q.IsReady() && GetValue<KeyBind>("UseQTH").Active && ToggleActive)
            {
                if (ObjectManager.Player.HasBuff("Recall"))
                    return;

                t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                if (t != null)
                    Q.CastOnUnit(t);
            }


            if (Q.IsReady() && GetValue<KeyBind>("UseQExtendedTH").Active && ToggleActive)
            {
                if (ObjectManager.Player.HasBuff("Recall"))
                    return;

                t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget() && QMinion(t).IsValidTarget())
                {
                    if (ObjectManager.Player.Distance(t) > Q.Range)
                        Q.CastOnUnit(QMinion(t));
                }
            }


            if ((!ComboActive && !HarassActive))
            {
                return;
            }

            var useQExtended = GetValue<StringList>("UseQExtendedC").SelectedIndex;
            if (useQExtended != 0)
            {
                switch (useQExtended)
                {
                    case 1:
                        {
                            t = TargetSelector.GetTarget(Q2.Range, TargetSelector.DamageType.Physical);
                            var tx = QMinion(t);
                            if (tx.IsValidTarget())
                            {
                                if (!Orbwalking.InAutoAttackRange(t))
                                    Q.CastOnUnit(tx);
                            }
                            break;
                        }

                    case 2:
                    {
                        var enemy = HeroManager.Enemies.Find(e => e.IsValidTarget(Q2.Range) && !e.IsZombie);
                        if (enemy != null)
                        {
                            var tx = QMinion(enemy);
                            if (tx.IsValidTarget())
                            {
                                Q.CastOnUnit(tx);
                            }
                        }
                        break;
                    }
                }
            }

            // Auto turn off Ghostblade Item if Ultimate active
            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Level > 0)
            {
                Config.Item("GHOSTBLADE")
                    .SetValue(ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Name == "LucianR");
            }

            //if (useQExtended && Q.IsReady())
            //{
            //    var t = TargetSelector.GetTarget(Q2.Range, TargetSelector.DamageType.Physical);
            //    if (t.IsValidTarget() && QMinion.IsValidTarget())
            //    {
            //        if (!Orbwalking.InAutoAttackRange(t))
            //            Q.CastOnUnit(QMinion);
            //    }
            //}

            t = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
            if (!t.IsValidTarget())
            {
                return;
            }

            var useQ = GetValue<bool>("UseQC");
            if (useQ && Q.IsReady())
            {
                if (t.IsValidTarget(Q.Range))
                {
                    Q.CastOnUnit(t);
                    //Orbwalking.ResetAutoAttackTimer();
                }
            }

            var useW = GetValue<bool>("UseWC");
            if (useW && W.CanCast(t))
            {
                if (t.Health <= W.GetDamage(t) || t.IsValidTarget(Orbwalking.GetRealAutoAttackRange(null) + 65))
                {
                    W.Cast(t.Position);
                }
            }

            var useE = GetValue<StringList>("UseEC").SelectedIndex;
            if (useE != 0 && E.IsReady())
            {
                if (t.Distance(ObjectManager.Player.Position) > Orbwalking.GetRealAutoAttackRange(null) 
                    && t.IsValidTarget(Orbwalking.GetRealAutoAttackRange(null) + E.Range - 100) && E.IsPositionSafe(t.Position.To2D()))
                {
                    E.Cast(t.Position);
                    //Orbwalking.ResetAutoAttackTimer();
                }
                else if (E.IsPositionSafe(Game.CursorPos.To2D()))
                {
                    E.Cast(Game.CursorPos);
                    //Orbwalking.ResetAutoAttackTimer();
                }
                //Orbwalker.ForceTarget(t);

                //if (t.IsValidTarget(Q.Range))
                //{
                //    E.Cast(Game.CursorPos);
                //}
            }
        }

        public override void ExecuteLaneClear()
        {
            int laneQValue = GetValue<StringList>("Lane.UseQ").SelectedIndex;
            if (laneQValue != 0)
            {
                var minion = Q.GetLineCollisionMinions(laneQValue);
                if (minion != null)
                {
                    Q.CastOnUnit(minion);
                }

                var allMinions = MinionManager.GetMinions(ObjectManager.Player.Position, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
                minion = allMinions.FirstOrDefault(minionn => minionn.Distance(ObjectManager.Player.Position) <= Q.Range && HealthPrediction.LaneClearHealthPrediction(minionn, (int)Q.Delay * 2) > 0);
                if (minion != null)
                {
                    Q.CastOnUnit(minion);
                }
            }

            int laneWValue = GetValue<StringList>("Lane.UseW").SelectedIndex;
            if (laneWValue != 0 && E.IsReady())
            {
                Vector2 minions = W.GetLineFarmMinions(laneWValue);
                if (minions != Vector2.Zero)
                {
                    W.Cast(minions);
                }
            }
        }

        public override void ExecuteJungleClear()
        {
            var jungleQValue = GetValue<StringList>("Jungle.UseQ").SelectedIndex;
            if (jungleQValue != 0 && Q.IsReady())
            {
                var bigMobsQ = Utils.Utils.GetMobs(Q.Range, jungleQValue == 2 ? Utils.Utils.MobTypes.BigBoys : Utils.Utils.MobTypes.All);
                if (bigMobsQ != null && bigMobsQ.Health > ObjectManager.Player.TotalAttackDamage * 2)
                {
                    Q.CastOnUnit(bigMobsQ);
                }
            }

            var jungleWValue = GetValue<StringList>("Jungle.UseQ").SelectedIndex;
            if (jungleWValue != 0 && W.IsReady())
            {
                var bigMobsQ = Utils.Utils.GetMobs(W.Range, jungleWValue == 2 ? Utils.Utils.MobTypes.BigBoys : Utils.Utils.MobTypes.All);
                if (bigMobsQ != null && bigMobsQ.Health > ObjectManager.Player.TotalAttackDamage * 2)
                {
                    W.Cast(bigMobsQ.Position);
                }
            }

            var jungleEValue = GetValue<StringList>("Jungle.UseE").SelectedIndex;
            if (jungleEValue != 0 && E.IsReady())
            {
                var jungleMobs =
                    Marksman.Utils.Utils.GetMobs(Q.Range + Orbwalking.GetRealAutoAttackRange(null) + 65,
                        Marksman.Utils.Utils.MobTypes.All);

                if (jungleMobs != null)
                {
                    switch (GetValue<StringList>("Jungle.UseE").SelectedIndex)
                    {
                        case 1:
                            {
                                if (!jungleMobs.SkinName.ToLower().Contains("baron") ||
                                    !jungleMobs.SkinName.ToLower().Contains("dragon"))
                                {
                                    if (jungleMobs.IsValidTarget(Orbwalking.GetRealAutoAttackRange(null) + 65))
                                        E.Cast(
                                            jungleMobs.IsValidTarget(
                                                Orbwalking.GetRealAutoAttackRange(null) + 65)
                                                ? Game.CursorPos
                                                : jungleMobs.Position);
                                }
                                break;
                            }

                        case 2:
                            {
                                if (!jungleMobs.SkinName.ToLower().Contains("baron") ||
                                    !jungleMobs.SkinName.ToLower().Contains("dragon"))
                                {
                                    jungleMobs =
                                        Marksman.Utils.Utils.GetMobs(
                                            E.Range + Orbwalking.GetRealAutoAttackRange(null) + 65,
                                            Marksman.Utils.Utils.MobTypes.BigBoys);
                                    if (jungleMobs != null)
                                    {
                                        E.Cast(
                                            jungleMobs.IsValidTarget(
                                                Orbwalking.GetRealAutoAttackRange(null) + 65)
                                                ? Game.CursorPos
                                                : jungleMobs.Position);
                                    }
                                }
                                break;
                            }
                    }
                }
            }
        }

        private static float GetRTotalDamage(Obj_AI_Hero t)
        {
            var baseAttackSpeed = 0.638;
            var wCdTime = 3;
            var passiveDamage = 0;

            var attackSpeed = (float)Math.Round(Math.Floor(1 / ObjectManager.Player.AttackDelay * 100) / 100, 2, MidpointRounding.ToEven);

            var RLevel = new[] { 7.5, 9, 10.5 };
            var shoots = 7.5 + RLevel[R.Level - 1];
            var shoots2 = shoots * attackSpeed;

            var aDmg = Math.Round(Math.Floor(ObjectManager.Player.GetAutoAttackDamage(t) * 100) / 100, 2, MidpointRounding.ToEven);
            aDmg = Math.Floor(aDmg);

            var totalAttackSpeedWithWActive = (float)Math.Round((attackSpeed + baseAttackSpeed / 100) * 100 / 100, 2, MidpointRounding.ToEven);

            var totalPossibleDamage = (float)Math.Round((totalAttackSpeedWithWActive * wCdTime * aDmg) * 100 / 100, 2, MidpointRounding.ToEven);

            return totalPossibleDamage + (float)passiveDamage;
        }

        public override bool ComboMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQC" + Id, "Q:").SetValue(true));
            config.AddItem(new MenuItem("UseQExtendedC" + Id, "Q Extended:").SetValue(new StringList(new[] { "Off", "Use for Selected Target", "Use for Any Target" }, 1)));
            config.AddItem(new MenuItem("UseWC" + Id, "W:").SetValue(true));
            config.AddItem(new MenuItem("UseEC" + Id, "E:").SetValue(new StringList(new []{ "Off", "On", "On: Protect AA Range" }, 2)));
            //config.AddItem(new MenuItem("UseRC" + Id, "E:").SetValue(true));
            return true;
        }

        public override bool HarassMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQTH" + Id, "Use Q (Toggle)").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));
            config.AddItem(new MenuItem("UseQExtendedTH" + Id, "Use Ext. Q (Toggle)").SetValue(new KeyBind("H".ToCharArray()[0], KeyBindType.Toggle)));
            return true;
        }

        public override bool MiscMenu(Menu config)
        {
            config.AddItem(new MenuItem("Passive" + Id, "Check Passive").SetValue(true));
            return true;
        }

        public override bool DrawingMenu(Menu config)
        {
            config.AddItem(new MenuItem("DrawQ" + Id, "Q range").SetValue(new Circle(true, Color.Gray)));
            config.AddItem(new MenuItem("DrawQ2" + Id, "Ext. Q range").SetValue(new Circle(true, Color.Gray)));
            config.AddItem(new MenuItem("DrawW" + Id, "W range").SetValue(new Circle(false, Color.Gray)));
            config.AddItem(new MenuItem("DrawE" + Id, "E range").SetValue(new Circle(false, Color.Gray)));
            config.AddItem(new MenuItem("DrawR" + Id, "R range").SetValue(new Circle(false, Color.Chocolate)));

            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Damage After Combo").SetValue(true);
            config.AddItem(dmgAfterComboItem);
            
            //Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate (object sender, OnValueChangeEventArgs eventArgs)
            {
                Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };
            
            return true;
        }

        public override bool LaneClearMenu(Menu config)
        {
            string[] strQ = new string[5];
            strQ[0] = "Off";

            for (var i = 1; i < 5; i++)
            {
                strQ[i] = "Minion Count >= " + i;
            }

            config.AddItem(new MenuItem("Lane.UseQ" + Id, "Q:").SetValue(new StringList(strQ, 3))).SetFontStyle(FontStyle.Regular, Q.MenuColor());
            config.AddItem(new MenuItem("Lane.UseQ2" + Id, "Q Extended:").SetValue(new StringList(new[] { "Off", "Out of AA Range" }, 1))).SetFontStyle(FontStyle.Regular, Q.MenuColor());

            string[] strW = new string[5];
            strW[0] = "Off";

            for (var i = 1; i < 5; i++)
            {
                strW[i] = "Minion Count >= " + i;
            }

            config.AddItem(new MenuItem("Lane.UseW" + Id, "W:").SetValue(new StringList(strW, 3))).SetFontStyle(FontStyle.Regular, W.MenuColor());

            config.AddItem(new MenuItem("Lane.UseE" + Id, "E:").SetValue(new StringList(new[] { "Off", "Under Ally Turrent Farm", "Out of AA Range", "Both" }, 1))).SetFontStyle(FontStyle.Regular, E.MenuColor());


            string[] strR = new string[4];
            strR[0] = "Off";

            for (var i = 1; i < 4; i++)
            {
                strR[i] = "Minion Count >= Ulti Attack Count x " + i.ToString();
            }
            config.AddItem(new MenuItem("Lane.UseR" + Id, "R:").SetValue(new StringList(strR, 2))).SetFontStyle(FontStyle.Regular, R.MenuColor());


            return true;
        }

        public override bool JungleClearMenu(Menu config)
        {
            config.AddItem(new MenuItem("Jungle.UseQ" + Id, "Q:").SetValue(new StringList(new[] { "Off", "On", "Just big Monsters" }, 2)));
            config.AddItem(new MenuItem("Jungle.UseW" + Id, "W:").SetValue(new StringList(new[] { "Off", "On", "Just big Monsters" }, 2)));
            config.AddItem(new MenuItem("Jungle.UseE" + Id, "E:").SetValue(new StringList(new[] { "Off", "On", "Just big Monsters" }, 2)));

            return true;
        }

        private bool LucianHavePassiveBuff()
        {
            return ObjectManager.Player.Buffs.Any(buff => buff.DisplayName == "LucianPassive");
        }

        public override void PermaActive()
        {
            if (!ComboActive)
            {
                return;
            }

            if (ComboActive && GetValue<StringList>("UseEC").SelectedIndex == 2 && E.IsReady())
            {
                var nRange = ObjectManager.Player.HealthPercent < 25 ? 550 : 450;
                var nSum = HeroManager.Enemies.Where(e => e.IsValidTarget(nRange) && e.IsFacing(ObjectManager.Player)).Sum(e => e.HealthPercent);
                if (nSum > ObjectManager.Player.HealthPercent)
                {
                    var nResult = HeroManager.Enemies.FirstOrDefault(e => e.IsValidTarget(nRange));
                    if (nResult != null)
                    {
                        var nPosition = ObjectManager.Player.Position.Extend(nResult.Position, -E.Range);
                        E.Cast(nPosition);
                    }
                }
                Render.Circle.DrawCircle(ObjectManager.Player.Position, nRange, Color.DarkSalmon);
            }


            var enemy = HeroManager.Enemies.Find(e => e.IsValidTarget(E.Range + (Q.IsReady() ? Q.Range : Orbwalking.GetRealAutoAttackRange(null) + 65)) && !e.IsZombie);
            if (enemy != null)
            {
                if (enemy.Health < ObjectManager.Player.TotalAttackDamage*2 && !LucianHavePassiveBuff() && enemy.IsValidTarget(Orbwalking.GetRealAutoAttackRange(null) + 65) && !Q.IsReady())
                {
                    if (W.IsReady() && GetValue<bool>("UseWC"))
                    {
                        W.Cast(enemy.Position);
                    }
                    else if (E.IsReady() && GetValue<StringList>("UseEC").SelectedIndex == 1)
                    {
                        E.Cast(enemy.Position);
                    }
                }

                var xPossibleComboDamage = 0f;
                xPossibleComboDamage += Q.IsReady() ? Q.GetDamage(enemy) + ObjectManager.Player.TotalAttackDamage * 2 : 0;
                xPossibleComboDamage += E.IsReady() ? ObjectManager.Player.TotalAttackDamage * 2 : 0;

                if (enemy.Health < xPossibleComboDamage)
                {
//                    if (enemy.Distance(ObjectManager.Player) > Orbwalking.GetRealAutoAttackRange(null) + 65))
                }

                if (E.IsReady() && Q.IsReady() && GetValue<StringList>("UseEC").SelectedIndex == 1)
                {
                    E.Cast(enemy.Position);
                }
            }
        }
    }
}
