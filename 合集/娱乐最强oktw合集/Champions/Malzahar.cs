﻿using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SebbyLib;

namespace OneKeyToWin_AIO_Sebby.Champions
{
    class Malzahar
    {
        private Menu Config = Program.Config;
        public static SebbyLib.Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Spell Q, Qr, W, E, R;
        private float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;
        private float Rtime = 0;
        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        public void LoadOKTW()
        {
            Q = new Spell(SpellSlot.Q, 900);
            Qr = new Spell(SpellSlot.Q, 900);
            W = new Spell(SpellSlot.W, 750);
            E = new Spell(SpellSlot.E, 650);
            R = new Spell(SpellSlot.R, 700);

            Qr.SetSkillshot(0.25f, 100, float.MaxValue, false, SkillshotType.SkillshotCircle);
            Q.SetSkillshot(0.75f, 80, float.MaxValue, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(1.2f, 230, float.MaxValue, false, SkillshotType.SkillshotCircle);

            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("noti", "Show notification & line", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("onlyRdy", "Draw only ready spells", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("qRange", "Q range", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("wRange", "W range", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("eRange", "E range", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("rRange", "R range", true).SetValue(false));

            Config.SubMenu(Player.ChampionName).SubMenu("Q Config").AddItem(new MenuItem("autoQ", "Auto Q", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Q Config").AddItem(new MenuItem("harrasQ", "Harass Q", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Q Config").AddItem(new MenuItem("intQ", "Interrupt spells Q", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Q Config").AddItem(new MenuItem("gapQ", "Gapcloser Q", true).SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("W Config").AddItem(new MenuItem("autoW", "Auto W", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("W Config").AddItem(new MenuItem("harrasW", "Harass W", true).SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("E Config").AddItem(new MenuItem("autoE", "Auto E", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("E Config").AddItem(new MenuItem("harrasE", "Harras E", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("E Config").AddItem(new MenuItem("harrasEminion", "Try harras E on minion", true).SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("autoR", "Auto R", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("useR", "Fast combo key", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press))); //32 == space

            foreach (var enemy in HeroManager.Enemies)
                Config.SubMenu(Player.ChampionName).SubMenu("R Config").SubMenu("Gapcloser").AddItem(new MenuItem("gapcloser" + enemy.ChampionName, enemy.ChampionName).SetValue(false));

            foreach (var enemy in HeroManager.Enemies)
                Config.SubMenu(Player.ChampionName).SubMenu("R Config").SubMenu("Fast combo key use on:").AddItem(new MenuItem("Ron" + enemy.ChampionName, enemy.ChampionName).SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("Rturrent", "Don't R under turret", true).SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("farmQ", "Lane clear Q", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("farmW", "Lane clear W", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("farmE", "Lane clear E", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("Mana", "LaneClear Mana", true).SetValue(new Slider(80, 100, 0)));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("LCminions", "LaneClear minimum minions", true).SetValue(new Slider(2, 10, 0)));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("jungleE", "Jungle clear E", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("jungleQ", "Jungle clear Q", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("jungleW", "Jungle clear W", true).SetValue(true));

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
        }

        private void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.R)
            {
                var t = args.Target as Obj_AI_Hero;
                if (t != null && t.Health - OktwCommon.GetIncomingDamage(t) > R.GetDamage(t) * 2.5)
                {
                    if (E.IsReady() && Player.Mana > RMANA + EMANA)
                    {
                        E.CastOnUnit(t);
                        args.Process = false;
                        return;
                    }

                    if (W.IsReady() && Player.Mana > RMANA + WMANA)
                    {
                        W.Cast(t.Position);
                        args.Process = false;
                        return;
                    }

                    if (Q.IsReady() && t.IsValidTarget(Q.Range) && Player.Mana > RMANA + QMANA)
                    {
                        Qr.Cast(t);
                        args.Process = false;
                        return;
                    }

                }
                if(R.IsReady() && t.IsValidTarget())
                     Rtime = Game.Time;
                
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {

            var t = gapcloser.Sender;

            if (Q.IsReady() && Config.Item("gapQ", true).GetValue<bool>() && t.IsValidTarget(Q.Range))
            {
                Q.Cast(gapcloser.End);
            }
            else if (R.IsReady() && Config.Item("gapcloser" + gapcloser.Sender.ChampionName).GetValue<bool>() && t.IsValidTarget(R.Range))
            {
                R.CastOnUnit(t);
            }
        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero t, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!Config.Item("intQ", true).GetValue<bool>() || !Q.IsReady())
                return;

            if (t.IsValidTarget(Q.Range))
            {
                 Q.Cast(t);
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsChannelingImportantSpell() || Game.Time - Rtime < 2.5 || Player.HasBuff("malzaharrsound"))
            {
                Program.debug("R chaneling");
                OktwCommon.blockMove = true;
                OktwCommon.blockAttack = true;
                OktwCommon.blockSpells = true;
                SebbyLib.Orbwalking.Attack = false;
                SebbyLib.Orbwalking.Move = false;
                return;
            }
            else
            {
                OktwCommon.blockSpells = false;
                OktwCommon.blockMove = false;
                OktwCommon.blockAttack = false;
                SebbyLib.Orbwalking.Attack = true;
                SebbyLib.Orbwalking.Move = true;
            }

            if (R.IsReady() && Config.Item("useR", true).GetValue<KeyBind>().Active)
            {
                var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
                if (t.IsValidTarget(R.Range) && Config.Item("Ron" + t.ChampionName).GetValue<bool>() && OktwCommon.ValidUlt(t))
                {
                    R.CastOnUnit(t);
                    return;
                }
            }

            if (Program.LagFree(0))
            {
                SetMana();
                Jungle();
            }

            if (Program.LagFree(1) && E.IsReady() && Config.Item("autoE", true).GetValue<bool>())
                LogicE();
            if (Program.LagFree(2) && Q.IsReady() && Config.Item("autoQ", true).GetValue<bool>())
                LogicQ();
            if (Program.LagFree(3) && W.IsReady() && Config.Item("autoW", true).GetValue<bool>())
                LogicW();
            if (Program.LagFree(4) && R.IsReady() && Config.Item("autoR", true).GetValue<bool>())
                LogicR();
        }

        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (t.IsValidTarget())
            {
                var qDmg = OktwCommon.GetKsDamage(t, Q) + BonusDmg(t);

                if (qDmg > t.Health)
                    Program.CastSpell(Q, t);

                if (R.IsReady() && t.IsValidTarget(R.Range))
                {
                    return;
                }
                if (Program.Combo && Player.Mana > RMANA + QMANA)
                    Program.CastSpell(Q, t);
                else if (Program.Farm && Config.Item("harrasQ", true).GetValue<bool>() && Player.Mana > RMANA + EMANA + WMANA + EMANA)
                    Program.CastSpell(Q, t);

                if (Player.Mana > RMANA + QMANA)
                {
                    foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range) && !OktwCommon.CanMove(enemy)))
                        Q.Cast(enemy);
                }
            }
            else if (Program.LaneClear && Player.ManaPercent > Config.Item("Mana", true).GetValue<Slider>().Value && Config.Item("farmQ", true).GetValue<bool>() )
            {
                var allMinions = Cache.GetMinions(Player.ServerPosition, Q.Range);
                var farmPos = Q.GetCircularFarmLocation(allMinions, 150);
                if (farmPos.MinionsHit > Config.Item("LCminions", true).GetValue<Slider>().Value)
                    Q.Cast(farmPos.Position);
            }
        }

        private void LogicW()
        {
            var t = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            if (t.IsValidTarget())
            {
                var qDmg = Q.GetDamage(t);
                var wDmg = OktwCommon.GetKsDamage(t, W) + BonusDmg(t) ;
                if (wDmg > t.Health)
                {
                    W.Cast(Player.Position.Extend(t.Position,450));
                }
                else if (wDmg + qDmg > t.Health && Player.Mana > QMANA + EMANA)
                    W.Cast(Player.Position.Extend(t.Position, 450));
                else if (Program.Combo && Player.Mana > RMANA + WMANA)
                    W.Cast(Player.Position.Extend(t.Position, 450));
                else if (Program.Farm && Config.Item("harrasW", true).GetValue<bool>() && !Player.UnderTurret(true) && Player.Mana > RMANA + WMANA + EMANA + QMANA + WMANA && OktwCommon.CanHarras())
                    W.Cast(Player.Position.Extend(t.Position, 450));
            }
            else if (Program.LaneClear && Player.ManaPercent > Config.Item("Mana", true).GetValue<Slider>().Value && Config.Item("farmW", true).GetValue<bool>() )
            {
                var allMinions = Cache.GetMinions(Player.ServerPosition, W.Range);
                var farmPos = W.GetCircularFarmLocation(allMinions, W.Width);
                if (farmPos.MinionsHit >= Config.Item("LCminions", true).GetValue<Slider>().Value)
                    W.Cast(farmPos.Position);
            }
        }

        private void LogicE()
        {
            var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (t.IsValidTarget())
            {
                var eDmg = OktwCommon.GetKsDamage(t, E) + BonusDmg(t);
                var wDmg = W.GetDamage(t);

                if (eDmg > t.Health)
                    E.CastOnUnit(t);
                else if (W.IsReady() && wDmg + eDmg > t.Health && Player.Mana > WMANA + EMANA)
                    E.CastOnUnit(t);
                else if (R.IsReady() && W.IsReady() && wDmg + eDmg + R.GetDamage(t) > t.Health && Player.Mana > WMANA + EMANA + RMANA)
                    E.CastOnUnit(t);
                if (Program.Combo && Player.Mana > RMANA + EMANA)
                    E.CastOnUnit(t);
                else if (Program.Farm && Config.Item("harrasE", true).GetValue<bool>() && Player.Mana > RMANA + EMANA + WMANA + EMANA)
                    E.CastOnUnit(t);
            }
            else if (Program.LaneClear && Player.ManaPercent > Config.Item("Mana", true).GetValue<Slider>().Value && Config.Item("farmE", true).GetValue<bool>())
            {
                var allMinions = Cache.GetMinions(Player.ServerPosition, E.Range);
                if (allMinions.Count >= Config.Item("LCminions", true).GetValue<Slider>().Value)
                {
                    foreach (var minion in allMinions.Where(minion => minion.IsValidTarget(E.Range) && minion.Health < E.GetDamage(minion) && !minion.HasBuff("AlZaharMaleficVisions")))
                    {
                        E.CastOnUnit(minion);
                    }
                }
            }
            else if (Program.Farm && Player.Mana > RMANA + EMANA + WMANA + EMANA && Config.Item("harrasEminion", true).GetValue<bool>())
            {
                var te = TargetSelector.GetTarget(E.Range + 400, TargetSelector.DamageType.Magical);
                if (te.IsValidTarget())
                {
                    var allMinions = Cache.GetMinions(Player.ServerPosition, E.Range);
                    foreach (var minion in allMinions.Where(minion => minion.IsValidTarget(E.Range) && minion.Health < E.GetDamage(minion) && te.Distance(minion.Position) < 500 && !minion.HasBuff("AlZaharMaleficVisions")))
                    {
                        E.CastOnUnit(minion);
                    }
                }
            }
        }

        private void LogicR()
        {
            if (Player.UnderTurret(true) && Config.Item("Rturrent", true).GetValue<bool>())
                return;
            if (Player.CountEnemiesInRange(800) < 3)
                return;

            foreach (var t in HeroManager.Enemies.Where(t => t.IsValidTarget(R.Range)))
            { 
                var totalComboDamage = R.GetDamage(t) * 2.5;

                totalComboDamage += E.GetDamage(t);

                if (W.IsReady() && Player.Mana > RMANA + WMANA)
                {
                    totalComboDamage += Q.GetDamage(t);
                }

                if (Player.Mana > RMANA + QMANA)
                    totalComboDamage += Q.GetDamage(t);

                if (totalComboDamage > t.Health - OktwCommon.GetIncomingDamage(t) && OktwCommon.ValidUlt(t))
                {
                    R.CastOnUnit(t);
                }
            }
        }

        private void Jungle()
        {
            if (Program.LaneClear && Player.Mana > RMANA + EMANA)
            {
                var mobs = Cache.GetMinions(Player.ServerPosition, 600, MinionTeam.Neutral);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    if (W.IsReady() && Config.Item("jungleW", true).GetValue<bool>())
                    {
                        W.Cast(mob.ServerPosition);
                        return;
                    }

                    if (Q.IsReady() && Config.Item("jungleQ", true).GetValue<bool>())
                    {
                        Q.Cast(mob.ServerPosition);
                        return;
                    }

                    if (E.IsReady() && Config.Item("jungleE", true).GetValue<bool>() && mob.HasBuff("brandablaze"))
                    {
                        E.Cast(mob);
                        return;
                    }
                }
            }
        }

        private int CountMinionsInRange(float range, Vector3 pos)
        {
            var minions = MinionManager.GetMinions(pos, range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);
            int count = 0;
            foreach (var minion in minions)
            {
                count++;
            }
            return count;
        }

        private float BonusDmg(Obj_AI_Hero target)
        {
            return (float)Player.CalcDamage(target, Damage.DamageType.Magical, (target.MaxHealth * 0.08) - (target.HPRegenRate * 5));
        }

        private void SetMana()
        {
            if ((Config.Item("manaDisable", true).GetValue<bool>() && Program.Combo) || Player.HealthPercent < 20)
            {
                QMANA = 0;
                WMANA = 0;
                EMANA = 0;
                RMANA = 0;
                return;
            }

            QMANA = Q.Instance.ManaCost;
            WMANA = W.Instance.ManaCost;
            EMANA = E.Instance.ManaCost;

            if (!R.IsReady())
                RMANA = WMANA - Player.PARRegenRate * W.Instance.Cooldown;
            else
                RMANA = R.Instance.ManaCost;
        }

        public static void drawLine(Vector3 pos1, Vector3 pos2, int bold, System.Drawing.Color color)
        {
            var wts1 = Drawing.WorldToScreen(pos1);
            var wts2 = Drawing.WorldToScreen(pos2);

            Drawing.DrawLine(wts1[0], wts1[1], wts2[0], wts2[1], bold, color);
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("qRange", true).GetValue<bool>())
            {
                if (Config.Item("onlyRdy", true).GetValue<bool>())
                {
                    if (Q.IsReady())
                        Utility.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Cyan, 1, 1);
                }
                else
                    Utility.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Cyan, 1, 1);
            }

            if (Config.Item("wRange", true).GetValue<bool>())
            {
                if (Config.Item("onlyRdy", true).GetValue<bool>())
                {
                    if (W.IsReady())
                        Utility.DrawCircle(Player.Position, W.Range, System.Drawing.Color.Orange, 1, 1);
                }
                else
                    Utility.DrawCircle(Player.Position, W.Range, System.Drawing.Color.Orange, 1, 1);
            }

            if (Config.Item("eRange", true).GetValue<bool>())
            {
                if (Config.Item("onlyRdy", true).GetValue<bool>())
                {
                    if (E.IsReady())
                        Utility.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Yellow, 1, 1);
                }
                else
                    Utility.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Yellow, 1, 1);
            }

            if (Config.Item("rRange", true).GetValue<bool>())
            {
                if (Config.Item("onlyRdy", true).GetValue<bool>())
                {
                    if (R.IsReady())
                        Utility.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Gray, 1, 1);
                }
                else
                    Utility.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Gray, 1, 1);
            }

            if (Config.Item("noti", true).GetValue<bool>() && R.IsReady())
            {
              
            }
        }
    }
}
