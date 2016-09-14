﻿using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SebbyLib;

namespace OneKeyToWin_AIO_Sebby
{

    class Jinx
    {
        private Menu Config = Program.Config;
        public static SebbyLib.Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        public Spell Q, W, E, R;
        public float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;

        public double lag = 0, WCastTime = 0, QCastTime = 0, DragonTime = 0, grabTime = 0;
        public float DragonDmg = 0;

        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        public void LoadOKTW()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 1500f);
            E = new Spell(SpellSlot.E, 920f);
            R = new Spell(SpellSlot.R, 3000f);

            W.SetSkillshot(0.6f, 60f, 3300f, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(1.2f, 100f, 1750f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.7f, 140f, 1500f, false, SkillshotType.SkillshotLine);

            LoadMenuOKTW();
            Game.OnUpdate += Game_OnUpdate;
            SebbyLib.Orbwalking.BeforeAttack += BeforeAttack;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private void LoadMenuOKTW()
        {
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("noti", "Show notification", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("semi", "Semi-manual R target", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("qRange", "Q range", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("wRange", "W range", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("eRange", "E range", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("rRange", "R range", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("onlyRdy", "Draw only ready spells", true).SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("W Config").AddItem(new MenuItem("autoW", "Auto W", true).SetValue(true));
            foreach (var enemy in HeroManager.Enemies)
                Config.SubMenu(Player.ChampionName).SubMenu("W Config").SubMenu("Harras").AddItem(new MenuItem("haras" + enemy.ChampionName, enemy.ChampionName).SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("Q Config").AddItem(new MenuItem("autoQ", "Auto Q", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Q Config").AddItem(new MenuItem("Qharras", "Harass Q", true).SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("E Config").AddItem(new MenuItem("autoE", "Auto E on CC", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("E Config").AddItem(new MenuItem("comboE", "Auto E in Combo BETA", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("E Config").AddItem(new MenuItem("AGC", "AntiGapcloserE", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("E Config").AddItem(new MenuItem("opsE", "OnProcessSpellCastE", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("E Config").AddItem(new MenuItem("telE", "Auto E teleport", true).SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("autoR", "Auto R", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").SubMenu("R Jungle stealer").AddItem(new MenuItem("Rjungle", "R Jungle stealer", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").SubMenu("R Jungle stealer").AddItem(new MenuItem("Rdragon", "Dragon", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").SubMenu("R Jungle stealer").AddItem(new MenuItem("Rbaron", "Baron", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("hitchanceR", "Hit Chance R", true).SetValue(new Slider(2, 3, 0)));
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("useR", "OneKeyToCast R", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press))); //32 == space
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("Rturrent", "Don't R under turret", true).SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("farmQout", "Q farm out range AA", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("farmQ", "Q LaneClear Q", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("Mana", "LaneClear Q Mana", true).SetValue(new Slider(80, 100, 30)));

        }

        private void BeforeAttack(SebbyLib.Orbwalking.BeforeAttackEventArgs args)
        {
            if (!Q.IsReady() || !Config.Item("autoQ", true).GetValue<bool>() || !FishBoneActive)
                return;

            var t = args.Target as Obj_AI_Hero;

            if (t != null)
            {
                var realDistance = GetRealDistance(t) - 50;
                if (Program.Combo && (realDistance < GetRealPowPowRange(t) || (Player.Mana < RMANA + 20 && Player.GetAutoAttackDamage(t) * 3 < t.Health)))
                    Q.Cast();
                else if (Program.Farm && Config.Item("Qharras", true).GetValue<bool>() && (realDistance > bonusRange() || realDistance < GetRealPowPowRange(t) || Player.Mana < RMANA + EMANA + WMANA + WMANA))
                    Q.Cast();
            }

            var minion = args.Target as Obj_AI_Minion;
            if (Program.Farm && minion != null)
            {
                var realDistance = GetRealDistance(minion);

                if(realDistance < GetRealPowPowRange(minion) || Player.ManaPercent < Config.Item("Mana", true).GetValue<Slider>().Value)
                {
                    Q.Cast();
                }
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Config.Item("AGC", true).GetValue<bool>() && E.IsReady() && Player.Mana > RMANA + EMANA)
            {
                var Target = gapcloser.Sender;
                if (Target.IsValidTarget(E.Range))
                    E.Cast(gapcloser.End);
            }
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            if (unit.IsMinion)
                return;

            if (unit.IsMe)
            {
                if (args.SData.Name == "JinxWMissile")
                    WCastTime = Game.Time;
            }
            if (E.IsReady())
            {
                if (unit.IsEnemy && Config.Item("opsE", true).GetValue<bool>() &&  unit.IsValidTarget(E.Range) && ShouldUseE(args.SData.Name))
                {
                    E.Cast(unit.ServerPosition, true);
                }
                if (unit.IsAlly && args.SData.Name == "RocketGrab" && Player.Distance(unit.Position) < E.Range)
                {
                    grabTime = Game.Time;
                }
            }

        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (R.IsReady())
            {
                if (Config.Item("useR", true).GetValue<KeyBind>().Active)
                {
                    var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                    if (t.IsValidTarget())
                        R.Cast(t);
                }
                if (Config.Item("Rjungle", true).GetValue<bool>())
                {
                    KsJungle();
                }
            }

            if (Program.LagFree(0))
            {
                SetMana();
            }

            if (E.IsReady())
                LogicE();

            if (Program.LagFree(2) && Q.IsReady() && Config.Item("autoQ", true).GetValue<bool>())
                LogicQ();

            if (Program.LagFree(3) && W.IsReady() && !Player.IsWindingUp && Config.Item("autoW", true).GetValue<bool>())
                LogicW();

            if (Program.LagFree(4) && R.IsReady())
                LogicR();
        }

        private void LogicQ()
        {
            if (Program.Farm && !FishBoneActive && !Player.IsWindingUp && Orbwalker.GetTarget() == null && SebbyLib.Orbwalking.CanAttack() && Config.Item("farmQout", true).GetValue<bool>() && Player.Mana > RMANA + WMANA + EMANA + 10)
            {
                foreach (var minion in Cache.GetMinions(Player.Position, bonusRange() + 30).Where(
                minion => !SebbyLib.Orbwalking.InAutoAttackRange(minion) && GetRealPowPowRange(minion) < GetRealDistance(minion) && bonusRange() < GetRealDistance(minion)))
                {
                    var hpPred = SebbyLib.HealthPrediction.GetHealthPrediction(minion, 400, 70);
                    if (hpPred < Player.GetAutoAttackDamage(minion) * 1.1 && hpPred > 5)
                    {
                        Orbwalker.ForceTarget(minion);
                        Q.Cast();
                        return;
                    }
                }
            }

            var t = TargetSelector.GetTarget(bonusRange() + 60, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget())
            {
                if (!FishBoneActive && (!SebbyLib.Orbwalking.InAutoAttackRange(t) || t.CountEnemiesInRange(250) > 2) && Orbwalker.GetTarget() == null)
                {
                    var distance = GetRealDistance(t);
                    if (Program.Combo && (Player.Mana > RMANA + WMANA + 10 || Player.GetAutoAttackDamage(t) * 3 > t.Health))
                        Q.Cast();
                    else if (Program.Farm && !Player.IsWindingUp && SebbyLib.Orbwalking.CanAttack() && Config.Item("Qharras", true).GetValue<bool>() && !ObjectManager.Player.UnderTurret(true) && Player.Mana > RMANA + WMANA + EMANA + 20 && distance < bonusRange() + t.BoundingRadius + Player.BoundingRadius)
                        Q.Cast();
                }
            }
            else if (!FishBoneActive && Program.Combo && Player.Mana > RMANA + WMANA + 20 && Player.CountEnemiesInRange(2000) > 0)
                Q.Cast();
            else if (FishBoneActive && Program.Combo && Player.Mana < RMANA + WMANA + 20)
                Q.Cast();
            else if (FishBoneActive && Program.Combo && Player.CountEnemiesInRange(2000) == 0)
                Q.Cast();
            else if (FishBoneActive && (Program.Farm || Orbwalker.ActiveMode == SebbyLib.Orbwalking.OrbwalkingMode.LastHit))
            {
                Q.Cast();
            }
        }

        private void LogicW()
        {
            var t = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget())
            {
 
                foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(W.Range) && enemy.Distance(Player) > bonusRange()))
                {
                    var comboDmg = OktwCommon.GetKsDamage(enemy, W);
                    if (R.IsReady() && Player.Mana > RMANA + WMANA + 20)
                    {
                        comboDmg += R.GetDamage(enemy, 1);
                    }
                    if (comboDmg > enemy.Health && OktwCommon.ValidUlt(enemy))
                    {
                        Program.CastSpell(W, enemy);
                        return;
                    }
                }
                

                if (Player.CountEnemiesInRange(bonusRange()) == 0)
                {
                    if (Program.Combo && Player.Mana > RMANA + WMANA + 10)
                    {
                        foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(W.Range) && GetRealDistance(enemy) > bonusRange() ).OrderBy(enemy => enemy.Health))
                            Program.CastSpell(W, enemy);
                    }
                    else if (Program.Farm && Player.Mana > RMANA + EMANA + WMANA + WMANA + 40 && OktwCommon.CanHarras())
                    {
                        foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(W.Range) && Config.Item("haras" + enemy.ChampionName).GetValue<bool>()))
                            Program.CastSpell(W, enemy);
                    }
                }
                if (!Program.None && Player.Mana > RMANA + WMANA && Player.CountEnemiesInRange(GetRealPowPowRange(t)) == 0)
                {
                    foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(W.Range) && !OktwCommon.CanMove(enemy)))
                        W.Cast(enemy, true);
                }
            }
        }

        private void LogicE()
        {
            if (Player.Mana > RMANA + EMANA && Config.Item("autoE", true).GetValue<bool>() && Game.Time - grabTime > 1)
            {
                foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(E.Range + 50) && !OktwCommon.CanMove(enemy)))
                {
                    E.Cast(enemy);
                    return;
                }
                if (!Program.LagFree(1))
                    return;

                if (Config.Item("telE", true).GetValue<bool>())
                {
                    var trapPos = OktwCommon.GetTrapPos(E.Range);
                    if(!trapPos.IsZero)
                        E.Cast(trapPos);
                }

                if (Program.Combo && Player.IsMoving && Config.Item("comboE", true).GetValue<bool>() && Player.Mana > RMANA + EMANA + WMANA)
                {
                    var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                    if (t.IsValidTarget(E.Range) && E.GetPrediction(t).CastPosition.Distance(t.Position) > 200 )
                    {
                        E.CastIfWillHit(t, 2);
                        if (t.HasBuffOfType(BuffType.Slow))
                        {
                            Program.CastSpell(E, t);
                        }
                        if (OktwCommon.IsMovingInSameDirection(Player, t))
                            Program.CastSpell(E, t);
                    }
                }
            }
        }

        private void LogicR()
        {
            if (Player.UnderTurret(true) && Config.Item("Rturrent", true).GetValue<bool>())
                return;
            if (Game.Time - WCastTime > 0.9 && Config.Item("autoR", true).GetValue<bool>())
            {
                foreach (var target in HeroManager.Enemies.Where(target => target.IsValidTarget(R.Range) && OktwCommon.ValidUlt(target)))
                {
                    var predictedHealth = target.Health - OktwCommon.GetIncomingDamage(target);
                    var Rdmg = R.GetDamage(target, 1);

                    if (Rdmg > predictedHealth && !OktwCommon.IsSpellHeroCollision(target, R) && GetRealDistance(target) > bonusRange() + 200)
                    {
                        if ( GetRealDistance(target) > bonusRange() + 300 + target.BoundingRadius && target.CountAlliesInRange(500) == 0 && Player.CountEnemiesInRange(400) == 0)
                        {
                            castR(target);
                        }
                        else if (target.CountEnemiesInRange(200) > 2)
                        {
                            R.Cast(target, true, true);
                        }
                    }
                }
            }
        }

        private void castR(Obj_AI_Hero target)
        {
            var inx = Config.Item("hitchanceR", true).GetValue<Slider>().Value;
            if (inx == 0)
            {
                R.Cast(R.GetPrediction(target).CastPosition);
            }
            else if (inx == 1)
            {
                R.Cast(target);
            }
            else if (inx == 2)
            {
                Program.CastSpell(R, target);
            }
            else if (inx == 3)
            {
                List<Vector2> waypoints = target.GetWaypoints();
                if ((Player.Distance(waypoints.Last<Vector2>().To3D()) - Player.Distance(target.Position)) > 400)
                {
                    Program.CastSpell(R, target);
                }
            }
        }

        private float bonusRange() { return 670f + Player.BoundingRadius + 25 * Player.Spellbook.GetSpell(SpellSlot.Q).Level; }

        private bool FishBoneActive { get { return Player.HasBuff("JinxQ"); } }

        private float GetRealPowPowRange(GameObject target)
        {
            return 620f + Player.BoundingRadius + target.BoundingRadius;

        }

        private float GetRealDistance(Obj_AI_Base target)
        {
            
            return Player.ServerPosition.Distance(Prediction.GetPrediction(target, 0.05f).CastPosition) + Player.BoundingRadius + target.BoundingRadius;
        }

        public bool ShouldUseE(string SpellName)
        {
            switch (SpellName)
            {
                case "ThreshQ":
                    return true;
                case "KatarinaR":
                    return true;
                case "AlZaharNetherGrasp":
                    return true;
                case "GalioIdolOfDurand":
                    return true;
                case "LuxMaliceCannon":
                    return true;
                case "MissFortuneBulletTime":
                    return true;
                case "RocketGrabMissile":
                    return true;
                case "CaitlynPiltoverPeacemaker":
                    return true;
                case "EzrealTrueshotBarrage":
                    return true;
                case "InfiniteDuress":
                    return true;
                case "VelkozR":
                    return true;
            }
            return false;
        }

        private void KsJungle()
        {
            var mobs = Cache.GetMinions(Player.ServerPosition, float.MaxValue, MinionTeam.Neutral);
            foreach (var mob in mobs)
            {
                //debug(mob.SkinName);
                if (mob.Health < mob.MaxHealth && ((mob.SkinName.ToLower().Contains("dragon") && Config.Item("Rdragon", true).GetValue<bool>())
                    || (mob.SkinName == "SRU_Baron" && Config.Item("Rbaron", true).GetValue<bool>()))
                    && mob.CountAlliesInRange(1000) == 0
                    && mob.Distance(Player.Position) > 1000)
                {
                    if (DragonDmg == 0)
                        DragonDmg = mob.Health;

                    if (Game.Time - DragonTime > 4)
                    {
                        if (DragonDmg - mob.Health > 0)
                        {
                            DragonDmg = mob.Health;
                        }
                        DragonTime = Game.Time;
                    }

                    else
                    {
                        var DmgSec = (DragonDmg - mob.Health) * (Math.Abs(DragonTime - Game.Time) / 4);
                        //debug("DS  " + DmgSec);
                        if (DragonDmg - mob.Health > 0)
                        {
                            var timeTravel = GetUltTravelTime(Player, R.Speed, R.Delay, mob.Position);
                            var timeR = (mob.Health - Player.CalcDamage(mob, Damage.DamageType.Physical, (250 + (100 * R.Level)) + Player.FlatPhysicalDamageMod + 300)) / (DmgSec / 4);
                            //debug("timeTravel " + timeTravel + "timeR " + timeR + "d " + ((150 + (100 * R.Level + 200) + Player.FlatPhysicalDamageMod)));
                            if (timeTravel > timeR)
                                R.Cast(mob.Position);
                        }
                        else
                        {
                            DragonDmg = mob.Health;
                        }
                        //debug("" + GetUltTravelTime(Player, R.Speed, R.Delay, mob.Position));
                    }
                }
            }
        }

        private float GetUltTravelTime(Obj_AI_Hero source, float speed, float delay, Vector3 targetpos)
        {
            float distance = Vector3.Distance(source.ServerPosition, targetpos);
            float missilespeed = speed;
            if (source.ChampionName == "Jinx" && distance > 1350)
            {
                const float accelerationrate = 0.3f; //= (1500f - 1350f) / (2200 - speed), 1 unit = 0.3units/second
                var acceldifference = distance - 1350f;
                if (acceldifference > 150f) //it only accelerates 150 units
                    acceldifference = 150f;
                var difference = distance - 1500f;
                missilespeed = (1350f * speed + acceldifference * (speed + accelerationrate * acceldifference) + difference * 2200f) / distance;
            }
            return (distance / missilespeed + delay);
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

            QMANA = 10;
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
                if (FishBoneActive)
                    Utility.DrawCircle(Player.Position, 590f + Player.BoundingRadius, System.Drawing.Color.DeepPink, 1, 1);
                else
                    Utility.DrawCircle(Player.Position, bonusRange() -29, System.Drawing.Color.DeepPink, 1, 1);
            }
            if (Config.Item("wRange", true).GetValue<bool>())
            {
                if (Config.Item("onlyRdy", true).GetValue<bool>())
                {
                    if (W.IsReady())
                        Utility.DrawCircle(Player.Position, W.Range, System.Drawing.Color.Cyan, 1, 1);
                }
                else
                    Utility.DrawCircle(Player.Position, W.Range, System.Drawing.Color.Cyan, 1, 1);
            }
            if (Config.Item("eRange", true).GetValue<bool>())
            {
                if (Config.Item("onlyRdy", true).GetValue<bool>())
                {
                    if (E.IsReady())
                        Utility.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Gray, 1, 1);
                }
                else
                    Utility.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Gray, 1, 1);
            }
            if (Config.Item("noti", true).GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);

                if (R.IsReady() && t.IsValidTarget() && R.GetDamage(t, 1) > t.Health)
                {
                    Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.5f, System.Drawing.Color.Red, "Ult can kill: " + t.ChampionName + " have: " + t.Health + "hp");
                    drawLine(t.Position, Player.Position, 5, System.Drawing.Color.Red);
                }
                else if (t.IsValidTarget(2000) && W.GetDamage(t) > t.Health)
                {
                    Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.5f, System.Drawing.Color.Red, "W can kill: " + t.ChampionName + " have: " + t.Health + "hp");
                    drawLine(t.Position, Player.Position, 3, System.Drawing.Color.Yellow);
                }
            }
        }
    }
}