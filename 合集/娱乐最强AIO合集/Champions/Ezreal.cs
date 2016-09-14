﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Text;
using System.Linq;
using Color = System.Drawing.Color;
using LeagueSharp;
using LeagueSharp.Common;
using SPrediction;
using SharpDX;
using UnderratedAIO.Helpers;
using UnderratedAIO.Helpers.SkillShot;
using Environment = UnderratedAIO.Helpers.Environment;
using Orbwalking = UnderratedAIO.Helpers.Orbwalking;

namespace UnderratedAIO.Champions
{
    internal class Ezreal
    {
        public static Menu config;
        public static Orbwalking.Orbwalker orbwalker;
        public static AutoLeveler autoLeveler;
        public static Spell Q, W, E, R;
        public static readonly Obj_AI_Hero player = ObjectManager.Player;
        public static bool justJumped;
        public static bool justQ;
        public static bool justW;
        public static Obj_AI_Minion LastAttackedminiMinion;
        public static float LastAttackedminiMinionTime;

        public Ezreal()
        {
            InitEzreal();
            InitMenu();
            //Game.PrintChat("<font color='#9933FF'>Soresu </font><font color='#FFFFFF'>- Ezreal</font>");
            Drawing.OnDraw += Game_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast += Game_ProcessSpell;
            Helpers.Jungle.setSmiteSlot();
            HpBarDamageIndicator.DamageToUnit = ComboDamage;
            Orbwalking.OnAttack += Orbwalking_OnAttack;
        }


        private void Orbwalking_OnAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe && target is Obj_AI_Minion)
            {
                LastAttackedminiMinion = (Obj_AI_Minion) target;
                LastAttackedminiMinionTime = Utils.GameTimeTickCount;
            }
        }

        private void Game_ProcessSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.SData.Name == "EzrealArcaneShift")
                {
                    if (!justJumped)
                    {
                        justJumped = true;
                        Utility.DelayAction.Add(400, () => justJumped = false);
                    }
                }
                if (args.SData.Name == "EzrealMysticShot")
                {
                    if (!justQ)
                    {
                        justQ = true;
                        Utility.DelayAction.Add(500, () => justQ = false);
                    }
                }
                if (args.SData.Name == "EzrealEssenceFlux")
                {
                    if (!justW)
                    {
                        justW = true;
                        Utility.DelayAction.Add(500, () => justW = false);
                    }
                }
            }
        }

        private void InitEzreal()
        {
            Q = new Spell(SpellSlot.Q, 1150);
            Q.SetSkillshot(0.25f, 60f, 2000f, true, SkillshotType.SkillshotLine);
            W = new Spell(SpellSlot.W, 1000);
            W.SetSkillshot(0.25f, 80f, 1600f, false, SkillshotType.SkillshotLine);
            E = new Spell(SpellSlot.E, 450);
            R = new Spell(SpellSlot.R, 2000);
            R.SetSkillshot(1.2f, 160f, 2000f, false, SkillshotType.SkillshotLine);
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
                    Lasthit();
                    break;
                default:
                    break;
            }
            if (config.Item("EzAutoQ", true).GetValue<KeyBind>().Active && Q.IsReady() &&
                config.Item("EzminmanaaQ", true).GetValue<Slider>().Value < player.ManaPercent &&
                orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo && Orbwalking.CanMove(100))
            {
                Obj_AI_Hero target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                if (target != null && Q.CanCast(target) && target.IsValidTarget())
                {
                    Q.CastIfHitchanceEquals(
                        target, CombatHelper.GetHitChance(config.Item("qHit", true).GetValue<Slider>().Value));
                }
            }
        }

        private void Harass()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            float perc = config.Item("minmanaH", true).GetValue<Slider>().Value / 100f;
            if (player.Mana < player.MaxMana * perc)
            {
                return;
            }
            if (target == null)
            {
                return;
            }
            if (config.Item("useqH", true).GetValue<bool>() && Q.IsReady())
            {
                var miniPred =
                    MinionManager.GetMinions(
                        Orbwalking.GetRealAutoAttackRange(player), MinionTypes.All, MinionTeam.NotAlly)
                        .FirstOrDefault(
                            minion =>
                                minion.Health > 5 &&
                                HealthPrediction.GetHealthPrediction(
                                    minion,
                                    (int) (player.AttackCastDelay * 1000) - 100 + Game.Ping / 2 +
                                    1000 * (int) player.Distance(minion) / (int) Orbwalking.GetMyProjectileSpeed()) < 0);
                var priortarg = orbwalker.GetTarget();
                var canHArass = priortarg != null && !(priortarg is Obj_AI_Hero);
                if (canHArass || (!canHArass && miniPred == null))
                {
                    if (Program.IsSPrediction)
                    {
                        Q.SPredictionCast(target, HitChance.High);
                    }
                    else
                    {
                        var targQ = Q.GetPrediction(target);
                        if (Q.Range - 100 > targQ.CastPosition.Distance(player.Position) &&
                            targQ.Hitchance >= HitChance.High)
                        {
                            Q.Cast(targQ.CastPosition);
                        }
                    }
                }
            }
            if (config.Item("usewH", true).GetValue<bool>() && W.IsReady())
            {
                if (Program.IsSPrediction)
                {
                    W.SPredictionCast(target, HitChance.High);
                }
                else
                {
                    var tarPered = W.GetPrediction(target);
                    if (W.Range - 80 > tarPered.CastPosition.Distance(player.Position) &&
                        tarPered.Hitchance >= HitChance.High)
                    {
                        W.Cast(tarPered.CastPosition);
                    }
                }
            }
        }

        private void Clear()
        {
            float perc = config.Item("minmana", true).GetValue<Slider>().Value / 100f;
            if (player.Mana < player.MaxMana * perc)
            {
                return;
            }
            LastHitQ();
        }

        private void Lasthit()
        {
            float perc = config.Item("minmanaLH", true).GetValue<Slider>().Value / 100f;
            if (player.Mana < player.MaxMana * perc)
            {
                return;
            }
            LastHitQ();
        }

        private void Combo()
        {
            Obj_AI_Hero target = getTarget();
            if (target == null)
            {
                return;
            }
            if (config.Item("selected").GetValue<bool>())
            {
                target = CombatHelper.SetTarget(target, TargetSelector.GetSelectedTarget());
                orbwalker.ForceTarget(target);
            }
            var cmbDmg = GetComboDamage(target);
            if (config.Item("useItems").GetValue<bool>())
            {
                ItemHandler.UseItems(target, config, cmbDmg);
            }
            if (config.Item("useq", true).GetValue<bool>() && Q.IsReady() && Orbwalking.CanMove(100) &&
                target.IsValidTarget() && !justJumped)
            {
                if (Program.IsSPrediction)
                {
                    Q.SPredictionCast(target, HitChance.High);
                    return;
                }
                else
                {
                    var targQ = Q.GetPrediction(target);
                    if (Q.Range - 100 > targQ.CastPosition.Distance(player.Position) &&
                        targQ.Hitchance >= HitChance.High)
                    {
                        Q.Cast(targQ.CastPosition);
                        return;
                    }
                }
            }
            if (config.Item("usew", true).GetValue<bool>() && W.IsReady() && Orbwalking.CanMove(100) && !justJumped &&
                (cmbDmg + player.GetAutoAttackDamage(target) > target.Health || player.Mana > Q.Instance.ManaCost * 2))
            {
                if (Program.IsSPrediction)
                {
                    W.SPredictionCast(target, HitChance.High);
                    return;
                }
                else
                {
                    var tarPered = W.GetPrediction(target);
                    if (W.Range - 80 > tarPered.CastPosition.Distance(player.Position))
                    {
                        W.CastIfHitchanceEquals(target, HitChance.High);
                        return;
                    }
                }
            }
            if (R.IsReady() && !justJumped)
            {
                var dist = player.Distance(target);
                if (config.Item("user", true).GetValue<bool>() && !justQ && !Q.CanCast(target) && !justW &&
                    !W.CanCast(target) && !CombatHelper.CheckCriticalBuffs(target) &&
                    config.Item("usermin", true).GetValue<Slider>().Value < dist && 3000 > dist &&
                    target.Health < R.GetDamage(target) * 0.7 && target.CountAlliesInRange(600) < 1)
                {
                    R.CastIfHitchanceEquals(target, HitChance.High);
                }
                if (target.CountAlliesInRange(700) > 0)
                {
                    R.CastIfWillHit(
                        target, config.Item("usertf", true).GetValue<Slider>().Value);
                }
            }
            bool canKill = cmbDmg > target.Health;
            if (config.Item("usee", true).GetValue<bool>() && E.IsReady() &&
                ((config.Item("useekill", true).GetValue<bool>() && canKill) ||
                 (!config.Item("useekill", true).GetValue<bool>() &&
                  (target.CountEnemiesInRange(1200) <= target.CountAlliesInRange(1200) && player.Health > target.Health &&
                   TargetSelector.GetPriority(target) >= 2f) || canKill)))
            {
                var bestPositons =
                    (from pos in
                        CombatHelper.PointsAroundTheTarget(target.Position, 750)
                            .Where(
                                p =>
                                    !p.IsWall() && p.IsValid() && p.Distance(player.Position) < E.Range &&
                                    p.Distance(target.Position) < 680 && !p.UnderTurret(true))
                        let mob =
                            ObjectManager.Get<Obj_AI_Base>()
                                .Where(
                                    m =>
                                        m.IsEnemy && m.IsValidTarget() && m.Distance(target.Position) < 750 &&
                                        m.SkinName != target.SkinName)
                                .OrderBy(m => m.Distance(pos))
                                .FirstOrDefault()
                        where (mob != null && mob.Distance(pos) > pos.Distance(target.Position) + 80) || (mob == null)
                        select pos).ToList();

                CastE(bestPositons, target);
            }
            var ignitedmg = (float) player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            bool hasIgnite = player.Spellbook.CanUseSpell(player.GetSpellSlot("SummonerDot")) == SpellState.Ready;
            if (config.Item("useIgnite", true).GetValue<bool>() && ignitedmg > target.Health && hasIgnite &&
                !player.IsChannelingImportantSpell() && !justQ && !Q.CanCast(target) && !justW && !W.CanCast(target) &&
                !justJumped)
            {
                player.Spellbook.CastSpell(player.GetSpellSlot("SummonerDot"), target);
            }
        }

        private Obj_AI_Hero getTarget()
        {
            switch (config.Item("DmgType", true).GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return TargetSelector.GetTarget(1500, TargetSelector.DamageType.Magical);
                    break;
                case 1:
                    return TargetSelector.GetTarget(1500, TargetSelector.DamageType.Physical);
                    break;
                default:
                    return TargetSelector.GetTarget(1500, TargetSelector.DamageType.Magical);
                    break;
            }
        }

        private void CastE(IEnumerable<Vector3> bestPositons, Obj_AI_Hero target)
        {
            var pos = bestPositons.OrderBy(p => target.Distance(p)).FirstOrDefault();
            if (pos != null && pos.IsValid())
            {
                E.Cast(pos);
            }
        }

        private void LastHitQ()
        {
            if (!Q.IsReady())
            {
                return;
            }
            if (config.Item("useqLC", true).GetValue<bool>() || config.Item("useqLH", true).GetValue<bool>())
            {
                var minions =
                    MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly)
                        .Where(
                            m =>
                                m.Health > 5 &&
                                m.Health <
                                (orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LastHit
                                    ? Q.GetDamage(m)
                                    : Q.GetDamage(m) * config.Item("qLHDamage", true).GetValue<Slider>().Value / 100) &&
                                Q.CanCast(m) &&
                                HealthPrediction.GetHealthPrediction(m, (int) (m.Distance(player) / Q.Speed * 1000)) > 0);
                if (minions != null && LastAttackedminiMinion != null)
                {
                    foreach (var minion in
                        minions.Where(
                            m =>
                                m.NetworkId != LastAttackedminiMinion.NetworkId ||
                                (m.NetworkId == LastAttackedminiMinion.NetworkId &&
                                 Utils.GameTimeTickCount - LastAttackedminiMinionTime > 700)))
                    {
                        if (minion.Team == GameObjectTeam.Neutral && minion.CountAlliesInRange(500) > 0 &&
                            minion.NetworkId != LastAttackedminiMinion.NetworkId)
                        {
                            continue;
                        }

                        if (minion.Distance(player) <= player.AttackRange && !Orbwalking.CanAttack() &&
                            Orbwalking.CanMove(100))
                        {
                            if (Q.Cast(minion).IsCasted())
                            {
                                Orbwalking.Orbwalker.AddToBlackList(minion.NetworkId);
                            }
                        }
                        else if (minion.Distance(player) > player.AttackRange)
                        {
                            if (Q.Cast(minion).IsCasted())
                            {
                                Orbwalking.Orbwalker.AddToBlackList(minion.NetworkId);
                            }
                        }
                    }
                }
            }
        }

        private void Game_OnDraw(EventArgs args)
        {
            DrawHelper.DrawCircle(config.Item("drawqq", true).GetValue<Circle>(), Q.Range);
            DrawHelper.DrawCircle(config.Item("drawww", true).GetValue<Circle>(), W.Range);
            DrawHelper.DrawCircle(config.Item("drawee", true).GetValue<Circle>(), E.Range);
            HpBarDamageIndicator.Enabled = config.Item("drawcombo", true).GetValue<bool>();
            if (config.Item("ShowState", true).GetValue<bool>())
            {
                config.Item("EzAutoQ", true).Permashow(true, "Auto Q");
            }
            else
            {
                config.Item("EzAutoQ", true).Permashow(false, "Auto Q");
            }
        }

        private static float ComboDamage(Obj_AI_Hero hero)
        {
            double damage = 0;
            if (Q.IsReady() && config.Item("Calcq", true).GetValue<bool>())
            {
                damage += Damage.GetSpellDamage(player, hero, SpellSlot.Q);
            }
            if (W.IsReady() && config.Item("Calcw", true).GetValue<bool>())
            {
                damage += Damage.GetSpellDamage(player, hero, SpellSlot.W);
            }
            if (E.IsReady() && config.Item("Calce", true).GetValue<bool>())
            {
                damage += Damage.GetSpellDamage(player, hero, SpellSlot.E);
            }
            if (R.IsReady() && config.Item("Calcr", true).GetValue<bool>())
            {
                damage += Damage.GetSpellDamage(player, hero, SpellSlot.R);
            }
            //damage += ItemHandler.GetItemsDamage(hero);
            var ignitedmg = player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
            if (player.Spellbook.CanUseSpell(player.GetSpellSlot("summonerdot")) == SpellState.Ready &&
                hero.Health < damage + ignitedmg)
            {
                damage += ignitedmg;
            }
            return (float) damage;
        }

        private static float GetComboDamage(Obj_AI_Hero hero)
        {
            double damage = 0;
            if (Q.IsReady())
            {
                damage += Damage.GetSpellDamage(player, hero, SpellSlot.Q);
            }
            if (W.IsReady())
            {
                damage += Damage.GetSpellDamage(player, hero, SpellSlot.W);
            }
            if (E.IsReady())
            {
                damage += Damage.GetSpellDamage(player, hero, SpellSlot.E);
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

        private void InitMenu()
        {
            config = new Menu("Ezreal ", "Ezreal", true);
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
            menuD.AddItem(new MenuItem("drawqq", "Draw Q range", true))
                .SetValue(new Circle(false, Color.FromArgb(180, 100, 146, 166)));
            menuD.AddItem(new MenuItem("drawww", "Draw W range", true))
                .SetValue(new Circle(false, Color.FromArgb(180, 100, 146, 166)));
            menuD.AddItem(new MenuItem("drawee", "Draw E range", true))
                .SetValue(new Circle(false, Color.FromArgb(180, 100, 146, 166)));
            menuD.AddItem(new MenuItem("drawrr", "Draw R range", true))
                .SetValue(new Circle(false, Color.FromArgb(180, 100, 146, 166)));
            menuD.AddItem(new MenuItem("drawcombo", "Draw combo damage", true)).SetValue(true);
            menuD.AddItem(new MenuItem("Calcq", "   Calc Q", true)).SetValue(true);
            menuD.AddItem(new MenuItem("Calcw", "   Calc W", true)).SetValue(true);
            menuD.AddItem(new MenuItem("Calce", "   Calc E", true)).SetValue(true);
            menuD.AddItem(new MenuItem("Calcr", "   Calc R", true)).SetValue(true);
            config.AddSubMenu(menuD);
            // Combo Settings
            Menu menuC = new Menu("Combo ", "csettings");
            menuC.AddItem(new MenuItem("useq", "Use Q", true)).SetValue(true);
            menuC.AddItem(new MenuItem("usew", "Use W", true)).SetValue(true);
            menuC.AddItem(new MenuItem("usee", "Use E", true)).SetValue(true);
            menuC.AddItem(new MenuItem("useekill", "   Only for kill", true)).SetValue(true);
            menuC.AddItem(new MenuItem("user", "Use R in 1v1", true)).SetValue(true);
            menuC.AddItem(new MenuItem("usermin", "   Min range", true)).SetValue(new Slider(800, 0, 1500));
            menuC.AddItem(new MenuItem("usertf", "R min enemy in teamfight", true)).SetValue(new Slider(3, 1, 5));
            menuC.AddItem(new MenuItem("selected", "Focus Selected target")).SetValue(true);
            menuC.AddItem(new MenuItem("useIgnite", "Use Ignite", true)).SetValue(true);
            menuC = ItemHandler.addItemOptons(menuC);
            config.AddSubMenu(menuC);
            // Harass Settings
            Menu menuH = new Menu("Harass ", "Hsettings");
            menuH.AddItem(new MenuItem("useqH", "Use Q", true)).SetValue(true);
            menuH.AddItem(new MenuItem("usewH", "Use W", true)).SetValue(true);
            menuH.AddItem(new MenuItem("minmanaH", "Keep X% mana", true)).SetValue(new Slider(1, 1, 100));
            config.AddSubMenu(menuH);
            // LaneClear Settings
            Menu menuLC = new Menu("LaneClear ", "Lcsettings");
            menuLC.AddItem(new MenuItem("useqLC", "Use Q", true)).SetValue(true);
            menuLC.AddItem(new MenuItem("minmana", "Keep X% mana", true)).SetValue(new Slider(1, 1, 100));
            config.AddSubMenu(menuLC);
            // Lasthit Settings
            Menu menuLH = new Menu("Lasthit ", "Lasthcsettings");
            menuLH.AddItem(new MenuItem("useqLH", "Use Q", true)).SetValue(true);
            menuLH.AddItem(new MenuItem("qLHDamage", "   Q lasthit damage percent", true))
                .SetValue(new Slider(1, 1, 100));
            menuLH.AddItem(new MenuItem("minmanaLH", "Keep X% mana", true)).SetValue(new Slider(1, 1, 100));
            config.AddSubMenu(menuLH);
            //Misc
            Menu menuM = new Menu("Misc ", "Msettings");
            menuM.AddItem(new MenuItem("DmgType", "Damage Type", true))
                .SetValue(new StringList(new[] { "AP", "AD" }, 0));
            //Auto Harass
            Menu autoQ = new Menu("Auto Harass", "autoQ");
            autoQ.AddItem(
                new MenuItem("EzAutoQ", "Auto Q toggle", true).SetShared()
                    .SetValue(new KeyBind('H', KeyBindType.Toggle)))
                .SetFontStyle(System.Drawing.FontStyle.Bold, SharpDX.Color.Orange);
            autoQ.AddItem(new MenuItem("EzminmanaaQ", "Keep X% mana", true)).SetValue(new Slider(40, 1, 100));
            autoQ.AddItem(new MenuItem("qHit", "Q hitChance", true).SetValue(new Slider(4, 1, 4)));
            autoQ.AddItem(new MenuItem("ShowState", "Show always", true)).SetValue(true);
            menuM.AddSubMenu(autoQ);
            menuM = DrawHelper.AddMisc(menuM);
            config.AddSubMenu(menuM);
            config.Item("EzAutoQ", true).Permashow(true, "Auto Q");
            
            config.AddItem(new MenuItem("UnderratedAIO", "by Soresu v" + Program.version.ToString().Replace(",", ".")));
            config.AddToMainMenu();
        }
    }
}