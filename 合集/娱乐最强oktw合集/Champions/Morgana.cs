﻿using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SebbyLib;

namespace OneKeyToWin_AIO_Sebby
{
    class Morgana
    {
        private Menu Config = Program.Config;
        public static SebbyLib.Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Spell E, Q, R, W;
        private float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;
        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        public void LoadOKTW()
        {
            Q = new Spell(SpellSlot.Q, 1150);
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 800);
            R = new Spell(SpellSlot.R, 600);

            Q.SetSkillshot(0.25f, 70f, 1200f, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.50f, 200f, 2200f, false, SkillshotType.SkillshotCircle);

            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("qRange", "Q range", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("wRange", "W range", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("eRange", "E range", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("rRange", "R range", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("onlyRdy", "Draw when skill rdy", true).SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("Q config").AddItem(new MenuItem("ts", "Use common TargetSelector", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Q config").AddItem(new MenuItem("ts1", "ON - only one target"));
            Config.SubMenu(Player.ChampionName).SubMenu("Q config").AddItem(new MenuItem("ts2", "OFF - all targets"));
            Config.SubMenu(Player.ChampionName).SubMenu("Q config").AddItem(new MenuItem("qCC", "Auto Q cc & dash enemy", true).SetValue(true));
            foreach (var enemy in HeroManager.Enemies)
                Config.SubMenu(Player.ChampionName).SubMenu("Q config").SubMenu("Use on").AddItem(new MenuItem("grab" + enemy.ChampionName, enemy.ChampionName).SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("W config").AddItem(new MenuItem("autoW", "Auto W", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("W config").AddItem(new MenuItem("autoWcc", "Auto W only CC enemy", true).SetValue(false));

            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("farmW", "Lane clear W", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("Mana", "LaneClear Mana", true).SetValue(new Slider(80, 100, 0)));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("LCminions", "LaneClear minimum minions", true).SetValue(new Slider(2, 10, 0)));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("jungleQ", "Jungle clear Q", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("jungleW", "Jungle clear W", true).SetValue(true));

            foreach (var enemy in HeroManager.Enemies)
            {
                for (int i = 0; i < 4; i++)
                {
                    var spell2 = enemy.Spellbook.Spells[i];
                    if (spell2 != null && spell2.SData != null && spell2.SData.TargettingType != SpellDataTargetType.Self && spell2.SData.TargettingType != SpellDataTargetType.SelfAndUnit)
                    { 
                        var spell = Damage.Spells[enemy.ChampionName].FirstOrDefault(s => s.Slot == enemy.Spellbook.Spells[i].Slot);
                        if (spell != null)
                        {
                            if (spell.DamageType == Damage.DamageType.Physical || spell.DamageType == Damage.DamageType.True)
                                Config.SubMenu(Player.ChampionName).SubMenu("E Shield Config").SubMenu("Spell Manager").SubMenu(enemy.ChampionName).AddItem(new MenuItem("spell" + spell2.SData.Name, spell2.Name, true).SetValue(false));
                            else
                                Config.SubMenu(Player.ChampionName).SubMenu("E Shield Config").SubMenu("Spell Manager").SubMenu(enemy.ChampionName).AddItem(new MenuItem("spell" + spell2.SData.Name, spell2.Name, true).SetValue(true));
                        }
                        else
                            Config.SubMenu(Player.ChampionName).SubMenu("E Shield Config").SubMenu("Spell Manager").SubMenu(enemy.ChampionName).AddItem(new MenuItem("spell" + spell2.SData.Name, spell2.Name, true).SetValue(true));
                    }
                }
            }

            foreach (var ally in HeroManager.Allies)
            {
                Config.SubMenu(Player.ChampionName).SubMenu("E Shield Config").SubMenu("Shield ally").SubMenu(ally.ChampionName).AddItem(new MenuItem("skillshot" + ally.ChampionName, "skillshot", true).SetValue(true));
                Config.SubMenu(Player.ChampionName).SubMenu("E Shield Config").SubMenu("Shield ally").SubMenu(ally.ChampionName).AddItem(new MenuItem("targeted" + ally.ChampionName, "targeted", true).SetValue(true));
                Config.SubMenu(Player.ChampionName).SubMenu("E Shield Config").SubMenu("Shield ally").SubMenu(ally.ChampionName).AddItem(new MenuItem("HardCC" + ally.ChampionName, "Hard CC", true).SetValue(true));
                Config.SubMenu(Player.ChampionName).SubMenu("E Shield Config").SubMenu("Shield ally").SubMenu(ally.ChampionName).AddItem(new MenuItem("Poison" + ally.ChampionName, "Poison", true).SetValue(true));
            }

            Config.SubMenu(Player.ChampionName).SubMenu("R config").AddItem(new MenuItem("rCount", "Auto R if enemies in range", true).SetValue(new Slider(3, 0, 5)));
            Config.SubMenu(Player.ChampionName).SubMenu("R config").AddItem(new MenuItem("rKs", "R ks", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("R config").AddItem(new MenuItem("inter", "OnPossibleToInterrupt", true)).SetValue(true);
            Config.SubMenu(Player.ChampionName).SubMenu("R config").AddItem(new MenuItem("Gap", "OnEnemyGapcloser", true)).SetValue(true);    

            Game.OnUpdate += Game_OnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            
        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (R.IsReady() && Config.Item("inter", true).GetValue<bool>() && sender.IsValidTarget(R.Range))
                R.Cast();
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!E.IsReady() || !sender.IsEnemy || sender.IsMinion || args.SData.IsAutoAttack() || !sender.IsValid<Obj_AI_Hero>() || Player.Distance(sender.ServerPosition) > 2000)
                return;

            if (Config.Item("spell" + args.SData.Name, true) != null && !Config.Item("spell" + args.SData.Name, true).GetValue<bool>())
                return;
            
            foreach (var ally in HeroManager.Allies.Where(ally => ally.IsValid  && Player.Distance(ally.ServerPosition) < E.Range))
            {
                //double dmg = 0;

                if (Config.Item("targeted" + ally.ChampionName, true).GetValue<bool>() && args.Target != null && args.Target.NetworkId == ally.NetworkId)
                {
                    E.CastOnUnit(ally);
                    return;
                    //dmg = dmg + sender.GetSpellDamage(ally, args.SData.Name);
                }
                else if (Config.Item("skillshot" + ally.ChampionName ,true).GetValue<bool>())
                {
                    if (!OktwCommon.CanHitSkillShot(ally, args))
                        continue;
                    //dmg = dmg + sender.GetSpellDamage(ally, args.SData.Name);
                    E.CastOnUnit(ally);
                    return;
                }
            }   
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (R.IsReady() && Config.Item("Gap", true).GetValue<bool>() && gapcloser.Sender.IsValidTarget(R.Range))
                R.Cast();
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Program.LagFree(0))
            {
                SetMana();
                Jungle();
            }
            if (Program.LagFree(1) && Q.IsReady())
                LogicQ();
            if (Program.LagFree(2) && R.IsReady())
                LogicR();
            if (Program.LagFree(3) && W.IsReady() && Config.Item("autoW", true).GetValue<bool>())
                LogicW();
            if (Program.LagFree(4) && E.IsReady())
                LogicE();
        }

        private void LogicE()
        {
            foreach (var ally in HeroManager.Allies.Where(ally => ally.IsValid && ally.Distance(Player.Position) < E.Range))
            {
                if (Config.Item("Poison" + ally.ChampionName, true).GetValue<bool>() && ally.HasBuffOfType(BuffType.Poison))
                {
                    E.CastOnUnit(ally);
                }
            }
        }

        private void LogicQ()
        {
            if (Program.Combo && Config.Item("ts", true).GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

                if (t.IsValidTarget(Q.Range) && !t.HasBuffOfType(BuffType.SpellImmunity) && !t.HasBuffOfType(BuffType.SpellShield) && Config.Item("grab" + t.ChampionName).GetValue<bool>())
                    Program.CastSpell(Q, t);
            }
            foreach (var t in HeroManager.Enemies.Where(t => t.IsValidTarget(Q.Range) && Config.Item("grab" + t.ChampionName).GetValue<bool>()))
            {
                if (!t.HasBuffOfType(BuffType.SpellImmunity) && !t.HasBuffOfType(BuffType.SpellShield))
                {
                    if (Program.Combo && !Config.Item("ts", true).GetValue<bool>())
                        Program.CastSpell(Q, t);

                    if (Config.Item("qCC", true).GetValue<bool>())
                    {
                        if (!OktwCommon.CanMove(t))
                            Q.Cast(t, true);
                        Q.CastIfHitchanceEquals(t, HitChance.Dashing);
                        Q.CastIfHitchanceEquals(t, HitChance.Immobile);
                    }
                }
            }
        }

        private void LogicR()
        {
            bool rKs = Config.Item("rKs", true).GetValue<bool>();
            foreach (var target in HeroManager.Enemies.Where(target => target.IsValidTarget(R.Range) && target.HasBuff("rocketgrab2")))
            {
                if (rKs && R.GetDamage(target) > target.Health)
                    R.Cast();
            }
            if (Player.CountEnemiesInRange(R.Range) >= Config.Item("rCount", true).GetValue<Slider>().Value && Config.Item("rCount", true).GetValue<Slider>().Value > 0)
                R.Cast();
        }
        private void LogicW()
        {
            var t = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget() )
            {
                if (!Config.Item("autoWcc", true).GetValue<bool>() && !Q.IsReady())
                {
                    if (W.GetDamage(t) > t.Health)
                        Program.CastSpell(W, t);
                    else if (Program.Combo && Player.Mana > RMANA + WMANA + EMANA + QMANA)
                        Program.CastSpell(W, t);
                }

                foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(W.Range) && !OktwCommon.CanMove(enemy)))
                    W.Cast(enemy, true);
            }
            else if (Program.LaneClear && Player.ManaPercent > Config.Item("Mana", true).GetValue<Slider>().Value && Config.Item("farmW", true).GetValue<bool>() && Player.Mana > RMANA + WMANA)
            {
                var minionList = Cache.GetMinions(Player.ServerPosition, W.Range);
                var farmPosition = W.GetCircularFarmLocation(minionList, W.Width);

                if (farmPosition.MinionsHit > Config.Item("LCminions", true).GetValue<Slider>().Value)
                    W.Cast(farmPosition.Position);
            }
        }

        private void Jungle()
        {
            if (Program.LaneClear && Player.Mana > RMANA + WMANA + RMANA + WMANA)
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
                }
            }
        }

        private bool HardCC(Obj_AI_Hero target)
        {
            if (target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Knockup) ||
                target.HasBuffOfType(BuffType.Charm) || target.HasBuffOfType(BuffType.Fear) || target.HasBuffOfType(BuffType.Knockback) ||
                target.HasBuffOfType(BuffType.Taunt) || target.HasBuffOfType(BuffType.Suppression) ||
                target.IsStunned)
            {
                return true;

            }
            else
                return false;
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
        }
    }
}
