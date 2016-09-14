﻿using System;
using System.Collections.Generic;
using System.Linq;
using Color = System.Drawing.Color;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using UnderratedAIO.Helpers;
using Environment = UnderratedAIO.Helpers.Environment;
using Orbwalking = UnderratedAIO.Helpers.Orbwalking;

namespace UnderratedAIO.Champions
{
    internal class Yorick
    {
        public static Menu config;
        private static Orbwalking.Orbwalker orbwalker;
        public static readonly Obj_AI_Hero player = ObjectManager.Player;
        public static Spell Q, W, E, R;
        public static bool hasGhost = false;
        public static int LastAATick;
        public static AutoLeveler autoLeveler;

        public Yorick()
        {
            InitYorick();
            InitMenu();
            //Game.PrintChat("<font color='#9933FF'>Soresu </font><font color='#FFFFFF'>- Yorick</font>");
            Jungle.setSmiteSlot();
            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += AfterAttack;
            Orbwalking.BeforeAttack += beforeAttack;
            Drawing.OnDraw += Game_OnDraw;
            HpBarDamageIndicator.DamageToUnit = ComboDamage;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (FpsBalancer.CheckCounter())
            {
                return;
            }
            switch (orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Clear();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    break;
                default:
                    break;
            }
            if (config.Item("autoWStun", true).GetValue<bool>() && W.IsReady())
            {
                var targ =
                    HeroManager.Enemies.Where(
                        hero =>
                            W.CanCast(hero) &&
                            (hero.HasBuffOfType(BuffType.Snare) || hero.HasBuffOfType(BuffType.Stun) ||
                             hero.HasBuffOfType(BuffType.Taunt) || hero.HasBuffOfType(BuffType.Suppression)))
                        .OrderByDescending(hero => TargetSelector.GetPriority(hero))
                        .ThenBy(hero => hero.Health)
                        .FirstOrDefault();
                if (targ != null)
                {
                    W.CastOnUnit(targ);
                }
            }
        }

        private static void AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (CanQ2)
            {
                return;
            }
            var nearestMob = Jungle.GetNearest(player.Position);
            if (unit.IsMe && Q.IsReady() &&
                (((orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && config.Item("useq", true).GetValue<bool>()) ||
                  (orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed &&
                   config.Item("useqH", true).GetValue<bool>())) && target is Obj_AI_Hero) ||
                (config.Item("useqLC", true).GetValue<bool>() && nearestMob != null &&
                 nearestMob.Distance(player.Position) < player.AttackRange + 30))
            {
                Q.Cast();
                Orbwalking.ResetAutoAttackTimer();
                //player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            }
        }

        private void beforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (CanQ2)
            {
                return;
            }
            var target = args.Target as Obj_AI_Base;
            if (args.Unit.IsMe && Q.IsReady() && orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear &&
                config.Item("useqLC", true).GetValue<bool>() && !(target is Obj_AI_Hero) && (args.Target.Health > 700))
            {
                Q.Cast();
                player.IssueOrder(GameObjectOrder.AutoAttack, target);
            }

            if (Q.IsReady() && target != null && config.Item("useqLH", true).GetValue<bool>() &&
                target.Health < Q.GetDamage(target) + player.GetAutoAttackDamage(target))
            {
                Q.Cast();
            }
        }

        private void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(1500, TargetSelector.DamageType.Physical);
            if (target == null)
            {
                return;
            }
            var combodmg = ComboDamage(target);
            if (config.Item("useItems").GetValue<bool>())
            {
                ItemHandler.UseItems(target, config, combodmg);
            }
            bool hasIgnite = player.Spellbook.CanUseSpell(player.GetSpellSlot("SummonerDot")) == SpellState.Ready;
            var wPred = W.GetPrediction(target);
            if (config.Item("usew", true).GetValue<bool>() && W.CanCast(target) && wPred.Hitchance >= HitChance.High)
            {
                W.Cast(wPred.CastPosition);
            }
            if (config.Item("usee", true).GetValue<bool>() && E.IsReady())
            {
                CastE(target);
            }
            if (config.Item("user", true).GetValue<bool>() && R.IsReady() && R.CanCast(target) &&
                target.HealthPercent < config.Item("userHp", true).GetValue<Slider>().Value &&
                ((!Orbwalking.CanAttack() && Orbwalking.CanMove(100)) ||
                 !config.Item("useronlyMelee", true).GetValue<bool>()))
            {
                R.CastOnUnit(target);
            }
            if (config.Item("useIgnite").GetValue<bool>() && combodmg > target.Health && hasIgnite)
            {
                player.Spellbook.CastSpell(player.GetSpellSlot("SummonerDot"), target);
            }
            if (Q.IsReady() && config.Item("useq2", true).GetValue<bool>() && CanQ2)
            {
                Q.Cast();
            }
        }

        private static bool CanQ2
        {
            get { return player.Spellbook.GetSpell(SpellSlot.Q).Name == "YorickQ2"; }
        }

        private void Harass()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            if (target == null)
            {
                return;
            }

            if (config.Item("useeH", true).GetValue<bool>())
            {
                CastE(target);
            }
            if (Q.IsReady() && config.Item("useq2H", true).GetValue<bool>() && CanQ2)
            {
                Q.Cast();
            }
        }

        private void CastE(Obj_AI_Hero target)
        {
            var pred = E.GetPrediction(target);
            var poly = GetPoly(pred.UnitPosition);
            if (pred.Hitchance >= HitChance.High && pred.CastPosition.Distance(player.Position) < 700)
            {
                E.Cast(pred.CastPosition);
            }
        }

        private void Clear()
        {
            float perc = (float) config.Item("minmana", true).GetValue<Slider>().Value / 100f;
            if (player.Mana < player.MaxMana * perc)
            {
                return;
            }

            if (config.Item("useeLC", true).GetValue<bool>() && E.IsReady())
            {
                var minis = MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.NotAlly);
                var ePos = E.GetCircularFarmLocation(minis, 300);
                var poly = GetPoly(ePos.Position.To3D());
                if (minis.Count(m => poly.IsInside(m.Position)) >= config.Item("useeMin", true).GetValue<Slider>().Value)
                {
                    E.Cast(ePos.Position);
                }
            }
            if (config.Item("useqLC", true).GetValue<bool>() && Q.IsReady() && !CanQ2)
            {
                var targetQ =
                    MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly)
                        .Where(
                            i =>
                                (i.Health < Damage.GetSpellDamage(player, i, SpellSlot.Q) &&
                                 !(i.Health < player.GetAutoAttackDamage(i))))
                        .OrderByDescending(i => i.Health)
                        .FirstOrDefault();
                if (targetQ == null)
                {
                    return;
                }
                Q.Cast();
                player.IssueOrder(GameObjectOrder.AutoAttack, targetQ);
            }
        }

        private void Game_OnDraw(EventArgs args)
        {
            DrawHelper.DrawCircle(config.Item("drawww", true).GetValue<Circle>(), W.Range);
            DrawHelper.DrawCircle(config.Item("drawee", true).GetValue<Circle>(), 700);
            DrawHelper.DrawCircle(config.Item("drawrr", true).GetValue<Circle>(), R.Range);
            HpBarDamageIndicator.Enabled = config.Item("drawcombo").GetValue<bool>();
        }

        private float ComboDamage(Obj_AI_Hero hero)
        {
            double damage = 0;
            if (W.IsReady())
            {
                damage += Damage.GetSpellDamage(player, hero, SpellSlot.W);
            }
            if ((Items.HasItem(ItemHandler.Bft.Id) && Items.CanUseItem(ItemHandler.Bft.Id)) ||
                (Items.HasItem(ItemHandler.Dfg.Id) && Items.CanUseItem(ItemHandler.Dfg.Id)))
            {
                damage = (float) (damage * 1.2);
            }
            if (Q.IsReady())
            {
                damage += Damage.GetSpellDamage(player, hero, SpellSlot.Q);
            }
            if (E.IsReady())
            {
                damage += Damage.GetSpellDamage(player, hero, SpellSlot.E);
            }
            if (R.IsReady())
            {
                damage += Damage.GetSpellDamage(player, hero, SpellSlot.R);
            }
            damage += ItemHandler.GetItemsDamage(hero);
            var ignitedmg = player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
            if (player.Spellbook.CanUseSpell(player.GetSpellSlot("summonerdot")) == SpellState.Ready &&
                hero.Health < damage + ignitedmg)
            {
                damage += ignitedmg;
            }
            return (float) damage;
        }

        private Geometry.Polygon GetPoly(Vector3 castPos)
        {
            var eWidth = 200;
            var startPos = player.ServerPosition.Extend(castPos, player.Distance(castPos) - 100);
            var endPos = player.ServerPosition.Extend(castPos, player.Distance(castPos) + 400);
            var direction = (endPos.To2D() - startPos.To2D()).Normalized();

            var pos1 = (startPos.To2D() - direction.Perpendicular() * eWidth / 2f).To3D();

            var pos2 =
                (endPos.To2D() + (endPos.To2D() - startPos.To2D()).Normalized() +
                 direction.Perpendicular() * eWidth / 2f).To3D();

            var pos3 = (startPos.To2D() + direction.Perpendicular() * eWidth / 2f).To3D();

            var pos4 =
                (endPos.To2D() + (endPos.To2D() - startPos.To2D()).Normalized() -
                 direction.Perpendicular() * eWidth / 2f).To3D();
            var poly = new Geometry.Polygon();
            poly.Add(pos1);
            poly.Add(pos3);
            poly.Add(pos2);
            poly.Add(pos4);
            return poly;
        }

        private void InitYorick()
        {
            Q = new Spell(SpellSlot.Q, player.AttackRange);
            W = new Spell(SpellSlot.W, 600);
            W.SetSkillshot(0.75f, W.Instance.SData.LineWidth, 1200, false, SkillshotType.SkillshotCircle);
            E = new Spell(SpellSlot.E, 1000);
            E.SetSkillshot(0.5f, 200, 1200, false, SkillshotType.SkillshotCircle);
            R = new Spell(SpellSlot.R, 600);
            R.SetSkillshot(0.5f, 200, 1800, false, SkillshotType.SkillshotCircle);
        }

        private void InitMenu()
        {
            config = new Menu("Yorick", "Yorick", true);
            // Target Selector
            Menu menuTS = new Menu("Selector", "tselect");
            TargetSelector.AddToMenu(menuTS);
            config.AddSubMenu(menuTS);

            // Orbwalker
            Menu menuOrb = new Menu("Orbwalker", "orbwalker");
            orbwalker = new Orbwalking.Orbwalker(menuOrb);
            config.AddSubMenu(menuOrb);

            // Draw settings
            Menu menuD = new Menu("Drawings ", "dsettings");
            menuD.AddItem(new MenuItem("drawww", "Draw W range", true))
                .SetValue(new Circle(false, Color.FromArgb(180, 116, 99, 45)));
            menuD.AddItem(new MenuItem("drawee", "Draw E range", true))
                .SetValue(new Circle(false, Color.FromArgb(180, 116, 99, 45)));
            menuD.AddItem(new MenuItem("drawrr", "Draw R range", true))
                .SetValue(new Circle(false, Color.FromArgb(180, 116, 99, 45)));
            menuD.AddItem(new MenuItem("drawcombo", "Draw combo damage")).SetValue(true);
            config.AddSubMenu(menuD);
            // Combo Settings
            Menu menuC = new Menu("Combo ", "csettings");
            menuC.AddItem(new MenuItem("useq", "Use Q", true)).SetValue(true);
            menuC.AddItem(new MenuItem("useq2", "Use Q2", true)).SetValue(true);
            menuC.AddItem(new MenuItem("usew", "Use W", true)).SetValue(true);
            menuC.AddItem(new MenuItem("usee", "Use E", true)).SetValue(true);
            menuC.AddItem(new MenuItem("user", "Use R", true)).SetValue(true);
            menuC.AddItem(new MenuItem("userHp", "Under enemy health%", true)).SetValue(new Slider(50, 1, 100));
            menuC.AddItem(new MenuItem("useronlyMelee", "Only in melee range", true)).SetValue(true);
            menuC.AddItem(new MenuItem("useIgnite", "Use Ignite")).SetValue(true);
            menuC = ItemHandler.addItemOptons(menuC);
            config.AddSubMenu(menuC);
            // Harass Settings
            Menu menuH = new Menu("Harass ", "Hsettings");
            menuH.AddItem(new MenuItem("useqH", "Use Q", true)).SetValue(true);
            menuH.AddItem(new MenuItem("useq2H", "Use Q2", true)).SetValue(true);
            menuH.AddItem(new MenuItem("useeH", "Use E", true)).SetValue(false);
            config.AddSubMenu(menuH);
            // LaneClear Settings
            Menu menuLC = new Menu("LaneClear ", "Lcsettings");
            menuLC.AddItem(new MenuItem("useqLC", "Use Q", true)).SetValue(true);
            menuLC.AddItem(new MenuItem("useeLC", "Use E", true)).SetValue(true);
            menuLC.AddItem(new MenuItem("useeMin", "Min hit", true)).SetValue(new Slider(3, 1, 10));
            menuLC.AddItem(new MenuItem("minmana", "Keep X% mana", true)).SetValue(new Slider(1, 1, 100));
            config.AddSubMenu(menuLC);
            // Misc Settings
            Menu menuM = new Menu("Misc ", "Msettings");
            menuM = DrawHelper.AddMisc(menuM);
            menuM.AddItem(new MenuItem("autoWStun", "Auto W on stun", true)).SetValue(true);
            menuM.AddItem(new MenuItem("useqLH", "Use Q Lasthit", true)).SetValue(true);
            config.AddSubMenu(menuM);
            config.AddItem(new MenuItem("UnderratedAIO", "by Soresu v" + Program.version.ToString().Replace(",", ".")));
            config.AddToMainMenu();
        }
    }
}