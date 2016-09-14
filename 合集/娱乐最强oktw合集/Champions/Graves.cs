﻿ using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SebbyLib;

namespace OneKeyToWin_AIO_Sebby
{
    class Graves
    {
        private Menu Config = Program.Config;
        public static SebbyLib.Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Spell E, Q, Q1, R, W , R1;
        private float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;

        public bool Esmart = false;
        public float OverKill = 0;
        public Obj_AI_Hero Player { get { return ObjectManager.Player; }}
        public static Core.OKTWdash Dash;

        public void LoadOKTW()
        {
            Q = new Spell(SpellSlot.Q, 900);
            W = new Spell(SpellSlot.W, 950f);
            E = new Spell(SpellSlot.E, 450f);
            R = new Spell(SpellSlot.R, 1000f);
            R1 = new Spell(SpellSlot.R, 1700f);

            Q.SetSkillshot(0.25f, 100f, 2100f, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.25f, 120f, 1500f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.25f, 100f, 2100f, false, SkillshotType.SkillshotLine);
            R1.SetSkillshot(0.25f, 100f, 2100f, false, SkillshotType.SkillshotLine);

            LoadMenuOKTW();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            SebbyLib.Orbwalking.AfterAttack += Orbwalker_AfterAttack;
        }

        private void LoadMenuOKTW()
        {
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("qRange", "Q range", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("wRange", "W range", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("eRange", "E range", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("rRange", "R range", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("onlyRdy", "Draw only ready spells", true).SetValue(true));

            foreach (var enemy in HeroManager.Enemies)
                Config.SubMenu(Player.ChampionName).SubMenu("Haras").AddItem(new MenuItem("haras" + enemy.ChampionName, enemy.ChampionName).SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("Q Config").AddItem(new MenuItem("autoQ", "Auto Q", true).SetValue(true));
            foreach (var enemy in HeroManager.Enemies)
                Config.SubMenu(Player.ChampionName).SubMenu("Q Config").SubMenu("Harras").AddItem(new MenuItem("haras" + enemy.ChampionName, enemy.ChampionName).SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("W Config").AddItem(new MenuItem("autoW", "Auto W", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("W Config").AddItem(new MenuItem("AGCW", "AntiGapcloser W", true).SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("E Config").AddItem(new MenuItem("autoE", "Auto E", true).SetValue(true));
            Dash = new Core.OKTWdash(E);

            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("autoR", "Auto R", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("fastR", "Fast R ks Combo", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("overkillR", "Overkill protection", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("useR", "Semi-manual cast R key", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press))); //32 == space

            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("farmQ", "Lane clear Q", true).SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("Mana", "LaneClear Mana", true).SetValue(new Slider(80, 100, 0)));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("jungleQ", "Jungle clear Q", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("jungleW", "Jungle clear W", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("jungleE", "Jungle clear E", true).SetValue(true));

            Config.SubMenu(Player.ChampionName).AddItem(new MenuItem("QWlogic", "Use Q and W only if don't have ammo", true).SetValue(false));
        }

        public void Orbwalker_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            
            if ( E.IsReady() && Config.Item("autoE", true).GetValue<bool>())
                LogicE();

            if(Config.Item("jungleE", true).GetValue<bool>() && Program.LaneClear)
            {
                if (E.IsReady() && Cache.GetMinions(Player.ServerPosition, 700, MinionTeam.Neutral).Any(x => x.NetworkId == target.NetworkId))
                    E.Cast(Game.CursorPos);
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Player.Mana > RMANA + EMANA )
            {
                var t = gapcloser.Sender;
                if (t.IsValidTarget(E.Range) )
                {
                    if (W.IsReady() && Config.Item("AGCW", true).GetValue<bool>())
                    {
                        W.Cast(gapcloser.End);
                        Program.debug("W AGC");
                    }
                }
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("useR", true).GetValue<KeyBind>().Active && R.IsReady())
            {
                var t = TargetSelector.GetTarget(1800, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget())
                    R1.Cast(t, true);
            }

            if (Program.LagFree(0))
            {
                SetMana();
                Jungle();
            }
            
            if (!Config.Item("QWlogic", true).GetValue<bool>() || !Player.HasBuff("gravesbasicattackammo1"))
            {
                if (Program.LagFree(2) && Q.IsReady() && !Player.IsWindingUp && Config.Item("autoQ", true).GetValue<bool>())
                    LogicQ();
                if (Program.LagFree(3) && W.IsReady() && !Player.IsWindingUp && Config.Item("autoW", true).GetValue<bool>())
                    LogicW();
            }
            if (Program.LagFree(4) && R.IsReady() && Config.Item("autoR", true).GetValue<bool>())
                LogicR();
        }

        private void Jungle()
        {
            if (Program.LaneClear)
            {
                var mobs = Cache.GetMinions(Player.ServerPosition, 600, MinionTeam.Neutral);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    if (Q.IsReady() && Config.Item("jungleQ", true).GetValue<bool>() )
                    {
                        Q.Cast(mob.Position);
                        return;
                    }
                    if (W.IsReady() && Config.Item("jungleW", true).GetValue<bool>())
                    {
                        W.Cast(mob.Position);
                        return;
                    }
                }
            }
        }

        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget())
            {
                var step = t.Distance(Player) / 20;
                for (var i = 0; i < 20; i++)
                {
                    var p = Player.Position.Extend(t.Position, step * i);
                    if (p.IsWall())
                    {
                        return;
                    }
                }

                if (Program.Combo && Player.Mana > RMANA + QMANA)
                    Program.CastSpell(Q, t);
                else if (Program.Farm && Config.Item("haras" + t.ChampionName).GetValue<bool>() && Player.Mana > RMANA + EMANA + WMANA + QMANA + QMANA)
                    Program.CastSpell(Q, t);
                else
                {
                    var qDmg = OktwCommon.GetKsDamage(t, Q);
                    var rDmg = R.GetDamage(t);
                    if (qDmg > t.Health)
                    {
                        Q.Cast(t, true);
                        OverKill = Game.Time;
                        Program.debug("Q ks");
                    }
                    else if (qDmg + rDmg > t.Health && R.IsReady() && Player.Mana > RMANA + QMANA)
                    {
                        Program.CastSpell(Q, t);
                        if (Config.Item("fastR", true).GetValue<bool>() && rDmg < t.Health)
                            Program.CastSpell(R, t);
                        Program.debug("Q + R ks");
                    }
                }

                if (!Program.None && Player.Mana > RMANA + QMANA + EMANA)
                {
                    foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range) && !OktwCommon.CanMove(enemy)))
                        Q.Cast(enemy);
                }
            }
            else if (Program.LaneClear && Player.ManaPercent > Config.Item("Mana", true).GetValue<Slider>().Value && Config.Item("farmQ", true).GetValue<bool>() && Player.Mana > RMANA + QMANA)
            {
                var allMinionsQ = Cache.GetMinions(Player.ServerPosition, Q.Range);
                var Qfarm = Q.GetLineFarmLocation(allMinionsQ, Q.Width);
                if (Qfarm.MinionsHit > 2)
                    Q.Cast(Qfarm.Position);
            }
        }

        private void LogicW()
        {
            var t = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            if (t.IsValidTarget())
            {
                var wDmg = OktwCommon.GetKsDamage(t, W);
                if (wDmg > t.Health)
                {
                    W.Cast(t, true, true);
                    return;
                }
                else if (wDmg + Q.GetDamage(t) > t.Health && Player.Mana > QMANA + WMANA + RMANA)
                {
                    W.Cast(t, true, true);
                }
                else if (Program.Combo && Player.Mana > RMANA + WMANA + QMANA)
                {
                    if (!SebbyLib.Orbwalking.InAutoAttackRange(t) || Player.CountEnemiesInRange(300) > 0 || t.CountEnemiesInRange(250) > 1 || Player.HealthPercent < 50)
                        W.Cast(t, true, true);
                    else if (Player.Mana > RMANA + WMANA + QMANA + EMANA)
                    {
                        foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(W.Range) && !OktwCommon.CanMove(enemy)))
                            W.Cast(enemy, true, true);
                    }
                }
            }
        }

        private void LogicE()
        {
            if (HeroManager.Enemies.Any(target => target.IsValidTarget(270) && target.IsMelee))
            {
                var dashPos = Dash.CastDash(true);
                if (!dashPos.IsZero)
                {
                    E.Cast(dashPos);
                }
            }
            if (Program.Combo && Player.Mana > RMANA + EMANA && !Player.HasBuff("gravesbasicattackammo2"))
            {
                var dashPos = Dash.CastDash();
                if (!dashPos.IsZero)
                {
                    E.Cast(dashPos);
                }
            }
        }

        private void LogicR()
        {
            foreach (var target in HeroManager.Enemies.Where(target => target.IsValidTarget(R1.Range) && OktwCommon.ValidUlt(target)))
            {
                double rDmg = OktwCommon.GetKsDamage(target,R);

                if (rDmg < target.Health)
                    continue;

                if (Config.Item("overkillR", true).GetValue<bool>() && target.Health < Player.Health)
                {
                    if(SebbyLib.Orbwalking.InAutoAttackRange(target))
                        continue;
                    if (target.CountAlliesInRange(400) > 0)
                        continue;
                }

                double rDmg2 = rDmg * 0.8;
                
                if(target.IsValidTarget(R.Range) && !OktwCommon.IsSpellHeroCollision(target, R) && rDmg > target.Health)
                {
                    Program.CastSpell(R, target);
                    Program.debug("Rdmg");
                }
                else if (rDmg2 > target.Health )
                {
                    if (!OktwCommon.IsSpellHeroCollision(target, R1))
                    {
                        Program.CastSpell(R1, target);
                        Program.debug("Rdmg2");
                    }
                    else if (target.IsValidTarget(1200))
                    {
                        Program.CastSpell(R1, target);
                        Program.debug("Rdmg2 collision");
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
