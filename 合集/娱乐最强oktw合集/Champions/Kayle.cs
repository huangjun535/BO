﻿using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SebbyLib;

namespace OneKeyToWin_AIO_Sebby.Champions
{
    class Kayle
    {
        private Menu Config = Program.Config;
        public static SebbyLib.Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Spell E, Q, R, W;
        private float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;
        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public void LoadOKTW()
        {
            Q = new Spell(SpellSlot.Q, 670);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, 660);
            R = new Spell(SpellSlot.R, 900);

            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("noti", "Show notification & line", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("onlyRdy", "Draw only ready spells", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("qRange", "Q range", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("wRange", "W range", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("eRange", "E range", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("rRange", "R range", true).SetValue(false));

            Config.SubMenu(Player.ChampionName).SubMenu("Q Config").AddItem(new MenuItem("autoQ", "Auto Q", true).SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("W Config").AddItem(new MenuItem("autoW", "Auto W", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("W Config").AddItem(new MenuItem("autoWspeed", "W speed-up", true).SetValue(true));
            foreach (var enemy in HeroManager.Allies)
                Config.SubMenu(Player.ChampionName).SubMenu("W Config").SubMenu("W ally:").AddItem(new MenuItem("Wally" + enemy.ChampionName, enemy.ChampionName).SetValue(true));


            Config.SubMenu(Player.ChampionName).SubMenu("E Config").AddItem(new MenuItem("autoE", "Auto E", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("E Config").AddItem(new MenuItem("harrasE", "Harras E", true).SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("autoR", "Auto R", true).SetValue(true));
            foreach (var enemy in HeroManager.Allies)
                Config.SubMenu(Player.ChampionName).SubMenu("R Config").SubMenu("R ally:").AddItem(new MenuItem("Rally" + enemy.ChampionName, enemy.ChampionName).SetValue(true));


            foreach (var enemy in HeroManager.Enemies)
            {
                for (int i = 0; i < 4; i++)
                {
                    var spell = enemy.Spellbook.Spells[i];
                    if (spell.SData.TargettingType != SpellDataTargetType.Self && spell.SData.TargettingType != SpellDataTargetType.SelfAndUnit)
                    {
                        Config.SubMenu(Player.ChampionName).SubMenu("R Config").SubMenu("Spell Manager").SubMenu(enemy.ChampionName).AddItem(new MenuItem("spell" + spell.SData.Name, spell.Name,true).SetValue(false));
                    }
                }
            }


            foreach (var enemy in HeroManager.Enemies)
                Config.SubMenu(Player.ChampionName).SubMenu("Harras").AddItem(new MenuItem("harras" + enemy.ChampionName, enemy.ChampionName).SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("farmE", "Lane clear E", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("jungleQ", "Jungle clear Q", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("jungleE", "Jungle clear E", true).SetValue(true));

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }
        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!R.IsReady() || sender.IsMinion || !sender.IsEnemy || args.SData.IsAutoAttack() || !Config.Item("autoR", true).GetValue<bool>()
                 || !sender.IsValid<Obj_AI_Hero>() || args.SData.Name.ToLower() == "tormentedsoil")
                return;

            if (Config.Item("spell" + args.SData.Name,true) == null || !Config.Item("spell" + args.SData.Name,true).GetValue<bool>())
                return;

            if (args.Target != null)
            {
                if (args.Target.IsAlly )
                {
                    var ally = args.Target as Obj_AI_Hero;
                    if(ally != null && Config.Item("Rally" + ally.ChampionName).GetValue<bool>())
                        R.CastOnUnit(ally);
                }
            }
            else
            {
                foreach (var ally in HeroManager.Allies.Where(ally => ally.IsValid && !ally.IsDead && ally.HealthPercent < 70 && Player.ServerPosition.Distance(ally.ServerPosition) < R.Range && Config.Item("Rally" + ally.ChampionName).GetValue<bool>()))
                {
                    if(OktwCommon.CanHitSkillShot(ally, args))
                        R.CastOnUnit(ally);
                }
            }
        }
        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Program.LagFree(1))
            {
                SetMana();
                Jungle();
            }

            if (R.IsReady() && Config.Item("autoR", true).GetValue<bool>())
                LogicR();

            if (Program.LagFree(2) && W.IsReady() && !Player.IsWindingUp && Config.Item("autoW", true).GetValue<bool>())
                LogicW();
            
            if (Program.LagFree(3) && E.IsReady() && Config.Item("autoE", true).GetValue<bool>())
                LogicE();
            if (Program.LagFree(4) && Q.IsReady() && !Player.IsWindingUp && Config.Item("autoQ", true).GetValue<bool>())
                LogicQ();
        }

        private void LogicR()
        {
            foreach (var ally in HeroManager.Allies.Where(ally => ally.IsValid && !ally.IsDead && ally.HealthPercent < 70 && Player.ServerPosition.Distance(ally.ServerPosition) < R.Range && Config.Item("Rally" + ally.ChampionName).GetValue<bool>() ))
            {
                double dmg = OktwCommon.GetIncomingDamage(ally, 1);
                var enemys = ally.CountEnemiesInRange(800);
               
                if (dmg == 0 && enemys == 0)
                    continue;

                enemys = (enemys == 0) ? 1 : enemys;

                if (ally.Health - dmg < enemys * ally.Level * 20)
                    R.CastOnUnit(ally);
            }
        }

        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (t.IsValidTarget())
            {
                if (Program.Combo)
                    Q.Cast(t);
                else if (Program.Farm && Config.Item("harras" + t.ChampionName).GetValue<bool>() && Player.Mana > RMANA + WMANA + QMANA + QMANA)
                    Q.Cast(t);
                else if (Player.Health < Player.Level * 40 && !W.IsReady() && !R.IsReady())
                    Q.Cast(t);
                else if (OktwCommon.GetKsDamage(t, Q) > t.Health)
                    Q.Cast(t);
            }
        }

        private void LogicW()
        {
            if (!Player.InFountain() && !Player.HasBuff("Recall") && !Player.IsRecalling())
            {
                Obj_AI_Hero lowest = Player;

                foreach (var ally in HeroManager.Allies.Where(ally => ally.IsValid && !ally.IsDead && Config.Item("Wally" + ally.ChampionName).GetValue<bool>() && Player.Distance(ally.Position) < W.Range))
                {
                    if (ally.Health < lowest.Health)
                        lowest = ally;
                }
                
                if (Player.Mana > WMANA + QMANA && lowest.Health < lowest.Level * 40)
                    W.CastOnUnit(lowest);
                else if (Player.Mana > WMANA + EMANA + QMANA && lowest.Health < lowest.MaxHealth * 0.4 && lowest.Health < 1500)
                    W.CastOnUnit(lowest);
                else if (Player.Mana > Player.MaxMana * 0.5 && lowest.Health < lowest.MaxHealth * 0.7 && lowest.Health < 2000)
                    W.CastOnUnit(lowest);
                else if (Player.Mana > Player.MaxMana * 0.9 && lowest.Health < lowest.MaxHealth * 0.9)
                    W.CastOnUnit(lowest);
                else if (Player.Mana == Player.MaxMana && lowest.Health < lowest.MaxHealth * 0.9)
                    W.CastOnUnit(lowest);
                if (Config.Item("autoWspeed", true).GetValue<bool>())
                {
                    var t = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Magical);
                    if (t.IsValidTarget())
                    {
                        if (Program.Combo && Player.Mana > WMANA + QMANA + EMANA && Player.Distance(t.Position) > Q.Range)
                            W.CastOnUnit(Player);
                    }
                }
            }
        }

        private void LogicE()
        {
            if(Program.Combo && Player.Mana > WMANA + EMANA && Player.CountEnemiesInRange(700) > 0)
                E.Cast();
            else if (Program.Farm && Config.Item("harrasE", true).GetValue<bool>() && Player.Mana > WMANA + EMANA + QMANA && Player.CountEnemiesInRange(500) > 0)
                E.Cast();
            else if (Program.LaneClear && Config.Item("farmE", true).GetValue<bool>() && Player.Mana > WMANA + EMANA + QMANA && FarmE())
                E.Cast();
        }

        private void Jungle()
        {
            if (Program.LaneClear && Player.Mana > RMANA + WMANA + RMANA + WMANA)
            {
                var mobs = Cache.GetMinions(Player.ServerPosition, 600, MinionTeam.Neutral);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    if (E.IsReady() && Config.Item("jungleE", true).GetValue<bool>())
                    {
                        E.Cast();
                        return;
                    }
                    if (Q.IsReady() && Config.Item("jungleQ", true).GetValue<bool>())
                    {
                        Q.Cast(mob);
                        return;
                    }
                }
            }
        }

        private bool FarmE()
        {
            return (Cache.GetMinions(Player.ServerPosition, 600).Count > 0);
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
            RMANA = 0;

            if (!Q.IsReady())
                QMANA = QMANA - Player.PARRegenRate * Q.Instance.Cooldown;

        }
        private void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("qRange", true).GetValue<bool>())
            {
                if (Config.Item("onlyRdy", true).GetValue<bool>())
                {
                    if (Q.IsReady())
                        Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan, 1, 1);
                }
                else
                    Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan, 1, 1);
            }
            if (Config.Item("wRange", true).GetValue<bool>())
            {
                if (Config.Item("onlyRdy", true).GetValue<bool>())
                {
                    if (W.IsReady())
                        Utility.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.Orange, 1, 1);
                }
                else
                    Utility.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.Orange, 1, 1);
            }
            if (Config.Item("eRange", true).GetValue<bool>())
            {
                if (Config.Item("onlyRdy", true).GetValue<bool>())
                {
                    if (E.IsReady())
                        Utility.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Yellow, 1, 1);
                }
                else
                    Utility.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Yellow, 1, 1);
            }
            if (Config.Item("rRange", true).GetValue<bool>())
            {
                if (Config.Item("onlyRdy", true).GetValue<bool>())
                {
                    if (R.IsReady())
                        Utility.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.Gray, 1, 1);
                }
                else
                    Utility.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.Gray, 1, 1);
            }
        }
    }
}
