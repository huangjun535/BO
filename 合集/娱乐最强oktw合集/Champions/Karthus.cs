﻿using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SebbyLib;

namespace OneKeyToWin_AIO_Sebby.Champions
{
    class Karthus
    {
        private Menu Config = Program.Config;
        public static SebbyLib.Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Spell E, Q, R, W;
        private float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;
        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        public void LoadOKTW()
        {
            Q = new Spell(SpellSlot.Q, 890);
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 520);
            R = new Spell(SpellSlot.R, 20000);

            Q.SetSkillshot(0.95f, 140f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(0.5f, 50f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            R.DamageType = TargetSelector.DamageType.Magical;

            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("noti", "Show R notification", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("qRange", "Q range", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("wRange", "W range", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("eRange", "E range", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("rRange", "R range", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("onlyRdy", "Draw when skill rdy", true).SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("Q Config").AddItem(new MenuItem("autoQ", "Auto Q", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Q Config").AddItem(new MenuItem("harrasQ", "Harass Q", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Q Config").AddItem(new MenuItem("QHarassMana", "Harass Mana", true).SetValue(new Slider(30, 100, 0)));

            foreach (var enemy in HeroManager.Enemies)
                Config.SubMenu(Player.ChampionName).SubMenu("Q Config").SubMenu("Use on:").AddItem(new MenuItem("Qon" + enemy.ChampionName, enemy.ChampionName).SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("W Config").AddItem(new MenuItem("autoW", "Auto W", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("W Config").AddItem(new MenuItem("harrasW", "Harass W", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("W Config").AddItem(new MenuItem("WmodeCombo", "W combo mode", true).SetValue(new StringList(new[] { "always", "run - cheese" }, 1)));
            Config.SubMenu(Player.ChampionName).SubMenu("W Config").SubMenu("W Gap Closer").AddItem(new MenuItem("WmodeGC", "Gap Closer position mode", true).SetValue(new StringList(new[] { "Dash end position", "My hero position" }, 0)));
            foreach (var enemy in HeroManager.Enemies)
                Config.SubMenu(Player.ChampionName).SubMenu("W Config").SubMenu("W Gap Closer").SubMenu("Cast on enemy:").AddItem(new MenuItem("WGCchampion" + enemy.ChampionName, enemy.ChampionName, true).SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("E Config").AddItem(new MenuItem("autoE", "Auto E if enemy in range", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("E Config").AddItem(new MenuItem("Emana", "E % minimum mana", true).SetValue(new Slider(20, 100, 0)));

            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("autoR", "Auto R", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("autoRzombie", "Auto R upon dying if can help team", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("Renemy", "Don't R if enemy in x range", true).SetValue(new Slider(1500, 2000, 0)));
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("RenemyA", "Don't R if ally in x range near target", true).SetValue(new Slider(800, 2000, 0)));
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("Rturrent", "Don't R under turret", true).SetValue(true));

            foreach (var enemy in HeroManager.Enemies)
                Config.SubMenu(Player.ChampionName).SubMenu("Harras").AddItem(new MenuItem("harras" + enemy.ChampionName, enemy.ChampionName).SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("farmQout", "Last hit Q minion out range AA", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("farmQ", "Lane clear Q", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("farmE", "Lane clear E", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("Mana", "LaneClear Mana", true).SetValue(new Slider(80, 100, 0)));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("QLCminions", " QLaneClear minimum minions", true).SetValue(new Slider(2, 10, 0)));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("ELCminions", " ELaneClear minimum minions", true).SetValue(new Slider(5, 10, 0)));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("jungleQ", "Jungle clear Q", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("jungleE", "Jungle clear E", true).SetValue(true));

            Config.SubMenu(Player.ChampionName).AddItem(new MenuItem("autoZombie", "Auto zombie mode COMBO / LANECLEAR", true).SetValue(true));
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
           // Drawing.OnEndScene += Drawing_OnEndScene;
            //Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            //AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }
        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (W.IsReady() && Player.Mana > RMANA + WMANA)
            {
                var t = gapcloser.Sender;
                if (t.IsValidTarget(W.Range) && Config.Item("WGCchampion" + t.ChampionName, true).GetValue<bool>())
                {
                    if (Config.Item("WmodeGC", true).GetValue<StringList>().SelectedIndex == 0)
                        W.Cast(gapcloser.End);
                    else
                        W.Cast(Player.ServerPosition);
                }
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsRecalling())
                return;

            if (Player.IsZombie)
            {
                if (Config.Item("autoZombie", true).GetValue<bool>())
                {
                    if (Player.CountEnemiesInRange(Q.Range) > 0)
                        Orbwalker.ActiveMode = SebbyLib.Orbwalking.OrbwalkingMode.Combo;
                    else
                        Orbwalker.ActiveMode = SebbyLib.Orbwalking.OrbwalkingMode.LaneClear;
                }
                if (R.IsReady() && Config.Item("autoRzombie", true).GetValue<bool>())
                {
                    float timeDeadh = 8;
                    timeDeadh = OktwCommon.GetPassiveTime(Player, "KarthusDeathDefiedBuff");
                    Program.debug("Time " + timeDeadh);
                    if (timeDeadh < 4)
                    {
                        foreach (var target in HeroManager.Enemies.Where(target => target.IsValidTarget() && OktwCommon.ValidUlt(target)) )
                        {
                            var rDamage = R.GetDamage(target);
                            if (target.Health < 3 * rDamage && target.CountAlliesInRange(800) > 0)
                                R.Cast();
                            if (target.Health < rDamage * 1.5 && target.Distance(Player.Position) < 900)
                                R.Cast();
                            if (target.Health + target.HPRegenRate * 5 < rDamage)
                                R.Cast();
                        }
                    }
                }
            }
            else
            {
                Orbwalker.ActiveMode = SebbyLib.Orbwalking.OrbwalkingMode.None;
            }

            if (Program.LagFree(0))
            {
                SetMana();
                Jungle();
            }
            if (Program.LagFree(1) && Q.IsReady() && Config.Item("autoQ", true).GetValue<bool>())
                LogicQ();
            if (Program.LagFree(2) && E.IsReady() && Config.Item("autoE", true).GetValue<bool>())
                LogicE();
            if (Program.LagFree(3) && R.IsReady())
                LogicR();
            if (Program.LagFree(4) && W.IsReady() && Config.Item("autoW", true).GetValue<bool>())
                LogicW();
        }

        private void LogicR()
        {

            if (Config.Item("autoR", true).GetValue<bool>() && Player.CountEnemiesInRange(Config.Item("Renemy", true).GetValue<Slider>().Value) == 0)
            {
                if (Player.UnderTurret(true) && Config.Item("Rturrent", true).GetValue<bool>())
                    return;

                foreach (var target in HeroManager.Enemies.Where(target => target.IsValid && !target.IsDead))
                {
                    if (target.IsValidTarget() && target.CountAlliesInRange(Config.Item("RenemyA", true).GetValue<Slider>().Value) == 0)
                    {
                        float predictedHealth = target.Health + target.HPRegenRate * 4;
                        float Rdmg = OktwCommon.GetKsDamage(target, R);

                        if (target.HealthPercent > 30)
                        {
                            if (Items.HasItem(3155, target))
                            {
                                Rdmg = Rdmg - 250;
                            }

                            if (Items.HasItem(3156, target))
                            {
                                Rdmg = Rdmg - 400;
                            }
                        }

                        if (Rdmg > predictedHealth && OktwCommon.ValidUlt(target))
                        {
                            R.Cast();
                            Program.debug("R normal");
                        }
                    }
                    else if(!target.IsVisible)
                    {
                        var ChampionInfoOne = Core.OKTWtracker.ChampionInfoList.Find(x => x.NetworkId == target.NetworkId);
                        if (ChampionInfoOne != null )
                        {
                            var timeInvisible = Game.Time - ChampionInfoOne.LastVisableTime;
                            if (timeInvisible > 3 && timeInvisible < 10)
                            {
                                float predictedHealth = target.Health + target.HPRegenRate * (4+ timeInvisible);
                                if (R.GetDamage(target) > predictedHealth)
                                    R.Cast();
                            }   
                        }
                    }
                }
            }
        }

        private float GetQDamage(Obj_AI_Base t)
        {
            var minions = Cache.GetMinions(t.Position, Q.Width + 20);

            if(minions.Count > 1)
                return Q.GetDamage(t, 1);
            else
                return Q.GetDamage(t);
        }

        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (t.IsValidTarget() && Config.Item("Qon" + t.ChampionName).GetValue<bool>())
            {
                
                if (Program.Combo && Player.Mana > RMANA + QMANA + WMANA)
                    Program.CastSpell(Q, t);
                else if (Program.Farm && OktwCommon.CanHarras() && Config.Item("harrasQ", true).GetValue<bool>() 
                    && Config.Item("harras" + t.ChampionName).GetValue<bool>() && Player.ManaPercent > Config.Item("QHarassMana", true).GetValue<Slider>().Value)
                    Program.CastSpell(Q, t);
                else if (OktwCommon.GetKsDamage(t, Q) > t.Health)
                    Program.CastSpell(Q, t);

                foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range) && !OktwCommon.CanMove(enemy)))
                    Program.CastSpell(Q, t);
            }
            if (!OktwCommon.CanHarras())
                return;

            if (!Program.None && !Program.Combo && Player.Mana > RMANA + QMANA * 2)
            {
                var allMinions = Cache.GetMinions(Player.ServerPosition, Q.Range);

                if (Config.Item("farmQout", true).GetValue<bool>())
                {
                    foreach (var minion in allMinions.Where(minion => minion.IsValidTarget(Q.Range) && (!Orbwalker.InAutoAttackRange(minion) || (!minion.UnderTurret(true) && minion.UnderTurret()))))
                    {
                        var hpPred = SebbyLib.HealthPrediction.GetHealthPrediction(minion, 1100);
                        if (hpPred < GetQDamage(minion) * 0.9 && hpPred > minion.Health - hpPred * 2)
                        {
                            Q.Cast(minion);
                            return;
                        }
                    }
                }

                if (Program.LaneClear && Config.Item("farmQ", true).GetValue<bool>() && Player.ManaPercent > Config.Item("Mana", true).GetValue<Slider>().Value)
                {
                    foreach (var minion in allMinions.Where(minion => minion.IsValidTarget(Q.Range) && Orbwalker.InAutoAttackRange(minion)))
                    {    
                        var hpPred = SebbyLib.HealthPrediction.GetHealthPrediction(minion, 1100);
                        if (hpPred < GetQDamage(minion) * 0.9 && hpPred > minion.Health - hpPred * 2)
                        {
                            Q.Cast(minion);
                            return;
                        }
                    }
                }
                
                if (Program.LaneClear && Player.ManaPercent > Config.Item("Mana", true).GetValue<Slider>().Value && Config.Item("farmQ", true).GetValue<bool>())
                {
                    var farmPos = Q.GetCircularFarmLocation(allMinions, Q.Width);
                    if (farmPos.MinionsHit >= Config.Item("QLCminions", true).GetValue<Slider>().Value)
                        Q.Cast(farmPos.Position);
                }
            }
        }

        private void LogicW()
        {
            if ((Program.Combo || (Program.Farm && Config.Item("harrasW", true).GetValue<bool>())) && Player.Mana > RMANA + WMANA)
            {
                if (Config.Item("WmodeCombo", true).GetValue<StringList>().SelectedIndex == 1)
                {
                    var t = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
                    if (t.IsValidTarget(W.Range) && W.GetPrediction(t).CastPosition.Distance(t.Position) > 100)
                    {
                        if (Player.Position.Distance(t.ServerPosition) > Player.Position.Distance(t.Position))
                        {
                            if (t.Position.Distance(Player.ServerPosition) < t.Position.Distance(Player.Position))
                                Program.CastSpell(W, t);
                        }
                        else
                        {
                            if (t.Position.Distance(Player.ServerPosition) > t.Position.Distance(Player.Position))
                                Program.CastSpell(W, t);
                        }
                    }
                }
                else
                {
                    var t = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
                    if (t.IsValidTarget())
                    {
                        Program.CastSpell(W, t);
                    }
                }
            }
        }

        private void LogicE()
        {
            if (Program.None)
                return;

            if (Player.HasBuff("KarthusDefile"))
            {
                if (Program.LaneClear)
                {
                    if(OktwCommon.CountEnemyMinions(Player, E.Range) < Config.Item("ELCminions", true).GetValue<Slider>().Value || Player.ManaPercent < Config.Item("Mana", true).GetValue<Slider>().Value)
                    E.Cast();
                }
                else if (Config.Item("autoE", true).GetValue<bool>())
                {
                    if (Player.ManaPercent < Config.Item("Emana", true).GetValue<Slider>().Value || Player.CountEnemiesInRange(E.Range) == 0)
                        E.Cast();
                }
            }
            else 
            {
                if (Program.LaneClear)
                {
                    if (OktwCommon.CountEnemyMinions(Player, E.Range) >= Config.Item("ELCminions", true).GetValue<Slider>().Value && Player.ManaPercent > Config.Item("Mana", true).GetValue<Slider>().Value)
                    E.Cast();
                }
                else if (Config.Item("autoE", true).GetValue<bool>() && Player.ManaPercent > Config.Item("Emana", true).GetValue<Slider>().Value && Player.CountEnemiesInRange(E.Range) > 0)
                {
                    E.Cast();
                }
            }
        }

        private void Jungle()
        {
            if (Program.LaneClear && Player.Mana > RMANA + QMANA )
            {
                var mobs = Cache.GetMinions(Player.ServerPosition, Q.Range, MinionTeam.Neutral);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    if (Q.IsReady() && Config.Item("jungleQ", true).GetValue<bool>())
                    {
                        Q.Cast(mob.ServerPosition);
                        return;
                    }
                    if (E.IsReady() && Config.Item("jungleE", true).GetValue<bool>() && mob.IsValidTarget(E.Range))
                    {
                        E.Cast(mob.ServerPosition);
                        return;
                    }
                }
            }
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
                RMANA = QMANA - Player.PARRegenRate * Q.Instance.Cooldown;
            else
                RMANA = R.Instance.ManaCost;
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
            if (R.IsReady() && Config.Item("noti", true).GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

                if (t.IsValidTarget() && OktwCommon.GetKsDamage(t, R) > t.Health)
                {
                    Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.5f, System.Drawing.Color.Red, "Ult can kill: " + t.ChampionName + " Heal - damage =  " + (t.Health - OktwCommon.GetKsDamage(t, R)) + " hp");
                }
            }
        }
    }
}
