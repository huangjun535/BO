using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SPrediction;
using Color = System.Drawing.Color;

namespace Sharpshooter.Champions
{
    public static class KogMaw
    {
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Orbwalking.Orbwalker Orbwalker { get { return SharpShooter.Orbwalker; } }

        static Spell Q, W, E, R;

        static readonly int DefaltRange = 565;

        //W 1 = 695
        //W 2 = 715
        //W 3 = 735
        //W 4 = 755
        //W 5 = 775
        static int GetWActiveRange { get { return DefaltRange + 110 + (W.Level * 20); } }

        //R 1 = 1200
       // R 2 = 1500
        //R 3 = 1700
        static int GetRRange { get { return  900 + (R.Level * 300); } }

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 900f) { MinHitChance = HitChance.High };
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1200f) { MinHitChance = HitChance.High };
            R = new Spell(SpellSlot.R) { MinHitChance = HitChance.High };

            Q.SetSkillshot(0.25f, 60f, 1650f, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 110f, 1400f, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(1.2f, 120f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            var drawDamageMenu = new MenuItem("Draw_RDamage", "Draw (R) Damage", true).SetValue(true);
            var drawFill = new MenuItem("Draw_Fill", "Draw (R) Damage Fill", true).SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));

            SharpShooter.Menu.SubMenu("Combo").AddItem(new MenuItem("comboUseQ", "Use Q", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Combo").AddItem(new MenuItem("comboUseW", "Use W", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Combo").AddItem(new MenuItem("comboUseE", "Use E", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Combo").AddItem(new MenuItem("comboUseR", "Use R", true).SetValue(true));

            SharpShooter.Menu.SubMenu("Harass").AddItem(new MenuItem("harassUseQ", "Use Q", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Harass").AddItem(new MenuItem("harassUseW", "Use W", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Harass").AddItem(new MenuItem("harassUseE", "Use E", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Harass").AddItem(new MenuItem("harassUseR", "Use R", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Harass").AddItem(new MenuItem("harassMana", "if Mana % >", true).SetValue(new Slider(50, 0, 100)));

            SharpShooter.Menu.SubMenu("Laneclear").AddItem(new MenuItem("laneclearUseW", "Use W", true).SetValue(false));
            SharpShooter.Menu.SubMenu("Laneclear").AddItem(new MenuItem("laneclearUseE", "Use E", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Laneclear").AddItem(new MenuItem("laneclearUseR", "Use R", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Laneclear").AddItem(new MenuItem("laneclearMana", "if Mana % >", true).SetValue(new Slider(60, 0, 100)));

            SharpShooter.Menu.SubMenu("Jungleclear").AddItem(new MenuItem("jungleclearUseQ", "Use Q", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Jungleclear").AddItem(new MenuItem("jungleclearUseW", "Use W", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Jungleclear").AddItem(new MenuItem("jungleclearUseE", "Use E", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Jungleclear").AddItem(new MenuItem("jungleclearUseR", "Use R", true).SetValue(true));
            SharpShooter.Menu.SubMenu("Jungleclear").AddItem(new MenuItem("jungleclearMana", "if Mana % >", true).SetValue(new Slider(20, 0, 100)));

            SharpShooter.Menu.SubMenu("Misc").AddItem(new MenuItem("empty1", "Empty", true));

            SharpShooter.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingAA", "Real AA Range", true).SetValue(new Circle(true, Color.FromArgb(0, 216, 255))));
            SharpShooter.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingQ", "Q Range", true).SetValue(new Circle(false, Color.FromArgb(0, 216, 255))));
            SharpShooter.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingW", "W Range", true).SetValue(new Circle(true, Color.FromArgb(0, 216, 255))));
            SharpShooter.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingE", "E Range", true).SetValue(new Circle(false, Color.FromArgb(0, 216, 255))));
            SharpShooter.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingR", "R Range", true).SetValue(new Circle(true, Color.FromArgb(0, 216, 255))));

            SharpShooter.Menu.SubMenu("Drawings").AddItem(drawDamageMenu);
            SharpShooter.Menu.SubMenu("Drawings").AddItem(drawFill);

            DamageIndicator.DamageToUnit = GetComboDamage;
            DamageIndicator.Enabled = drawDamageMenu.GetValue<Boolean>();
            DamageIndicator.Fill = drawFill.GetValue<Circle>().Active;
            DamageIndicator.FillColor = drawFill.GetValue<Circle>().Color;

            drawDamageMenu.ValueChanged +=
            delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                DamageIndicator.Enabled = eventArgs.GetNewValue<Boolean>();
            };

            drawFill.ValueChanged +=
            delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                DamageIndicator.Fill = eventArgs.GetNewValue<Circle>().Active;
                DamageIndicator.FillColor = eventArgs.GetNewValue<Circle>().Color;
            };

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                Combo();

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                Harass();

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                Laneclear();
                Jungleclear();
            }
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawingAA = SharpShooter.Menu.Item("drawingAA", true).GetValue<Circle>();
            var drawingQ = SharpShooter.Menu.Item("drawingQ", true).GetValue<Circle>();
            var drawingW = SharpShooter.Menu.Item("drawingW", true).GetValue<Circle>();
            var drawingE = SharpShooter.Menu.Item("drawingE", true).GetValue<Circle>();
            var drawingR = SharpShooter.Menu.Item("drawingR", true).GetValue<Circle>();

            if (drawingAA.Active)
                Render.Circle.DrawCircle(Player.Position, Orbwalking.GetRealAutoAttackRange(Player), drawingAA.Color);

            if (drawingQ.Active && Q.IsReady())
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawingQ.Color);

            if (drawingW.Active && W.IsReady())
                Render.Circle.DrawCircle(Player.Position, GetWActiveRange, drawingW.Color);

            if (drawingE.Active && E.IsReady())
                Render.Circle.DrawCircle(Player.Position, E.Range, drawingE.Color);

            if (drawingR.Active && R.IsReady())
                Render.Circle.DrawCircle(Player.Position, GetRRange, drawingR.Color);

        }


        static float GetComboDamage(Obj_AI_Base enemy)
        {
            return R.IsReady() ? R.GetDamage(enemy) : 0;
        }

        static void Combo()
        {
            if (!Orbwalking.CanMove(1))
                return;

            if (SharpShooter.Menu.Item("comboUseQ", true).GetValue<Boolean>() && Q.IsReady())
            {
                var Qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical, true);

                if (Q.CanCast(Qtarget))
                    Q.SPredictionCast(Qtarget, Q.MinHitChance);
            }

            if (SharpShooter.Menu.Item("comboUseW", true).GetValue<Boolean>() && W.IsReady())
            {
                var Wtarget = TargetSelector.GetTarget(GetWActiveRange, TargetSelector.DamageType.Physical, true);

                if (W.IsReady() && Wtarget.IsValidTarget(GetWActiveRange))
                    W.Cast();
            }

            if (SharpShooter.Menu.Item("comboUseE", true).GetValue<Boolean>() && E.IsReady())
            {
                var Etarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical, true);

                if (E.CanCast(Etarget))
                    E.SPredictionCast(Etarget, E.MinHitChance);
            }

            if (SharpShooter.Menu.Item("comboUseR", true).GetValue<Boolean>() && R.IsReady())
            {
                var Rtarget = TargetSelector.GetTarget(GetRRange, TargetSelector.DamageType.Magical, true);
                
                if (ConfigMenu.SelectedPrediction.SelectedIndex == 0)
                {
                    var Rpred = R.GetSPrediction(Rtarget);
                    if (R.IsReady() && Rtarget.IsValidTarget(GetRRange) && Player.ServerPosition.To2D().Distance(Rpred.CastPosition, false) < GetRRange && Rpred.HitChance >= HitChance.High)
                        R.Cast(Rpred.CastPosition);
                }
                else
                {
                    var Rpred = R.GetPrediction(Rtarget);
                    if (R.IsReady() && Rtarget.IsValidTarget(GetRRange) && Player.ServerPosition.Distance(Rpred.CastPosition, false) < GetRRange && Rpred.Hitchance >= HitChance.High)
                        R.Cast(Rpred.CastPosition);
                }
            }
        }

        static void Harass()
        {
            if (!Orbwalking.CanMove(1) || !(SharpShooter.getManaPer > SharpShooter.Menu.Item("harassMana", true).GetValue<Slider>().Value))
                return;

            if (SharpShooter.Menu.Item("harassUseQ", true).GetValue<Boolean>() && Q.IsReady())
            {
                var Qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical, true);

                if (Q.CanCast(Qtarget) )
                {
                    Q.Cast(Qtarget);
                    return;
                }
            }

            if (SharpShooter.Menu.Item("harassUseW", true).GetValue<Boolean>() && W.IsReady())
            {
                var Wtarget = TargetSelector.GetTarget(GetWActiveRange, TargetSelector.DamageType.Physical, true);

                if (W.IsReady() && Wtarget.IsValidTarget(GetWActiveRange))
                    W.Cast();
            }

            if (SharpShooter.Menu.Item("harassUseE", true).GetValue<Boolean>() && E.IsReady())
            {
                var Etarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical, true);

                if (E.CanCast(Etarget) )
                {
                    E.Cast(Etarget);
                    return;
                }
            }

            if (SharpShooter.Menu.Item("harassUseR", true).GetValue<Boolean>() && R.IsReady())
            {
                var Rtarget = TargetSelector.GetTarget(GetRRange, TargetSelector.DamageType.Magical, true);
                var Rpred = R.GetPrediction(Rtarget);

                if (R.IsReady() && Rtarget.IsValidTarget(GetRRange) && Player.ServerPosition.Distance(Rpred.CastPosition, false) < GetRRange && Rpred.Hitchance >= HitChance.VeryHigh)
                {
                    R.Cast(Rpred.CastPosition);
                    return;
                }
            }
        }

        static void Laneclear()
        {
            var Minions = MinionManager.GetMinions(Player.ServerPosition, 1700, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (SharpShooter.Menu.Item("laneclearUseW", true).GetValue<Boolean>() && W.IsReady())
            {
                if (Minions.Where(x => x.IsValidTarget(GetWActiveRange + 100)).Count() >= 3)
                    W.Cast();
            }

            if (!Orbwalking.CanMove(1) || !(SharpShooter.getManaPer > SharpShooter.Menu.Item("laneclearMana", true).GetValue<Slider>().Value))
                return;

            if (SharpShooter.Menu.Item("laneclearUseE", true).GetValue<Boolean>() && E.IsReady())
            {
                var Eloc = E.GetLineFarmLocation(Minions.Where(x => x.IsValidTarget(E.Range)).ToList());

                if (Eloc.MinionsHit >= 5)
                {
                    E.Cast(Eloc.Position);
                    return;
                }
            }

            if (SharpShooter.Menu.Item("laneclearUseR", true).GetValue<Boolean>() && R.IsReady())
            {
                var Rloc = R.GetCircularFarmLocation(Minions.Where(x => x.IsValidTarget(GetRRange)).ToList());

                if (Rloc.MinionsHit >= 4)
                {
                    R.Cast(Rloc.Position);
                    return;
                }
            }
        }

        static void Jungleclear()
        {
            var Mobs = MinionManager.GetMinions(Player.ServerPosition, Orbwalking.GetRealAutoAttackRange(Player) + 100, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (SharpShooter.Menu.Item("jungleclearUseW", true).GetValue<Boolean>() && W.IsReady())
            {
                W.Cast();
                return;
            }

            if (!Orbwalking.CanMove(1) || !(SharpShooter.getManaPer > SharpShooter.Menu.Item("jungleclearMana", true).GetValue<Slider>().Value))
                return;

            if (SharpShooter.Menu.Item("jungleclearUseQ", true).GetValue<Boolean>() && Q.IsReady())
            {
                Q.Cast(Mobs[0]);
                return;
            }

            if (SharpShooter.Menu.Item("jungleclearUseE", true).GetValue<Boolean>() && E.IsReady())
            {
                E.Cast(Mobs[0]);
                return;
            }

            if (SharpShooter.Menu.Item("jungleclearUseR", true).GetValue<Boolean>() && R.IsReady())
            {
                R.Cast(Mobs[0]);
                return;
            }
        }
    }
}
