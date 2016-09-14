﻿using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using YuLeLibrary;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using System.Reflection;
using HealthPrediction = LeagueSharp.Common.HealthPrediction;

namespace SephKhazix
{
    class Khazix : Helper
    {
        private float distt;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        public Khazix()
        {
            GameOnOnGameLoad();
        }

        void GameOnOnGameLoad()
        {
            if (ObjectManager.Player.ChampionName != "Khazix")
            {
                return;
            }
            Init();
            GenerateMenu();
            Game.OnUpdate += OnUpdate;
            Game.OnUpdate += DoubleJump;
            Drawing.OnDraw += OnDraw;
            Orbwalking.BeforeAttack += BeforeAttack;
            Game.OnWndProc += Game_OnWndProc;
        }

        private void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg == 0x20a)
                KhazixMenu.menu.Item("ClearEnable").SetValue(!KhazixMenu.menu.Item("ClearEnable").GetValue<bool>());
        }

        void Init()
        {
            InitSkills();
            Khazix = ObjectManager.Player;

            foreach (var t in ObjectManager.Get<Obj_AI_Turret>().Where(t => t.IsEnemy))
            {
                EnemyTurretPositions.Add(t.ServerPosition);
            }

            var shop = ObjectManager.Get<Obj_Shop>().FirstOrDefault(o => o.IsAlly);
            if (shop != null)
            {
                NexusPosition = shop.Position;
            }

            HeroList = HeroManager.AllHeroes;
        }


        void OnUpdate(EventArgs args)
        {
            if (Khazix.IsDead || Khazix.IsRecalling())
            {
                return;
            }

            EvolutionCheck();

            if (Config.GetBool("Kson"))
            {
                KillSteal();
            }

            if (Config.GetKeyBind("Harass.Key"))
            {
                Harass();
            }

            switch (Config.Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Mixed();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    if (Config.GetBool("ClearEnable"))
                        Waveclear();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    if (Config.GetBool("ClearEnable"))
                        LH();
                    break;
            }
        }


        void Mixed()
        {
            LH();
        }

        void Harass()
        {
            if (Config.GetBool("UseQHarass") && Q.IsReady())
            {
                var enemy = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                if (enemy.IsValidEnemy())
                {
                    Q.Cast(enemy);
                }
            }
            if (Config.GetBool("UseWHarass") && W.IsReady())
            {
                Obj_AI_Hero target = TargetSelector.GetTarget(950, TargetSelector.DamageType.Physical);
                var autoWI = Config.GetBool("Harass.AutoWI");
                var autoWD = Config.GetBool("Harass.AutoWD");
                if (target != null && W.IsReady())
                {
                    if (!EvolvedW && Khazix.Distance(target) <= W.Range)
                    {
                        PredictionOutput predw = W.GetPrediction(target);
                        if (predw.Hitchance == HitChance.Medium)
                        {
                            W.Cast(predw.CastPosition);
                            return;
                        }
                    }
                    else if (EvolvedW && target.IsValidTarget(W.Range + 200))
                    {
                        PredictionOutput pred = W.GetPrediction(target);
                        if ((pred.Hitchance == HitChance.Immobile && autoWI) || (pred.Hitchance == HitChance.Dashing && autoWD) || pred.Hitchance >= HitChance.Medium)
                        {
                            CastWE(target, pred.UnitPosition.To2D(), 0, HitChance.High);
                        }
                    }
                }
            }
        }


        void LH()
        {
            List<Obj_AI_Base> allMinions = MinionManager.GetMinions(Khazix.ServerPosition, Q.Range);
            if (Config.GetBool("UseQFarm") && Q.IsReady())
            {
                foreach (Obj_AI_Base minion in
                    allMinions.Where(
                        minion =>
                            minion.IsValidTarget() &&
                            HealthPrediction.GetHealthPrediction(
                                minion, (int)(Khazix.Distance(minion) * 1000 / 1400)) <
                            0.75 * Khazix.GetSpellDamage(minion, SpellSlot.Q)))
                {
                    if (Vector3.Distance(minion.ServerPosition, Khazix.ServerPosition) >
                        Orbwalking.GetRealAutoAttackRange(Khazix) && Khazix.Distance(minion) <= Q.Range)
                    {
                        Q.CastOnUnit(minion);
                        return;
                    }
                }

            }
            if (Config.GetBool("UseWFarm") && W.IsReady())
            {
                MinionManager.FarmLocation farmLocation = MinionManager.GetBestCircularFarmLocation(
                  MinionManager.GetMinions(Khazix.ServerPosition, W.Range).Where(minion => HealthPrediction.GetHealthPrediction(
                                minion, (int)(Khazix.Distance(minion) * 1000 / 1400)) <
                            0.75 * Khazix.GetSpellDamage(minion, SpellSlot.W))
                      .Select(minion => minion.ServerPosition.To2D())
                      .ToList(), W.Width, W.Range);
                if (farmLocation.MinionsHit >= 1)
                {
                    if (!EvolvedW)
                    {
                        if (Khazix.Distance(farmLocation.Position) <= W.Range)
                        {
                            W.Cast(farmLocation.Position);
                        }
                    }

                    if (EvolvedW)
                    {
                        if (Khazix.Distance(farmLocation.Position) <= W.Range)
                        {
                            W.Cast(farmLocation.Position);
                        }
                    }
                }
            }

            if (Config.GetBool("UseEFarm") && E.IsReady())
            {

                MinionManager.FarmLocation farmLocation =
                    MinionManager.GetBestCircularFarmLocation(
                        MinionManager.GetMinions(Khazix.ServerPosition, E.Range).Where(minion => HealthPrediction.GetHealthPrediction(
                                minion, (int)(Khazix.Distance(minion) * 1000 / 1400)) <
                            0.75 * Khazix.GetSpellDamage(minion, SpellSlot.W))
                            .Select(minion => minion.ServerPosition.To2D())
                            .ToList(), E.Width, E.Range);

                if (farmLocation.MinionsHit >= 1)
                {
                    if (Khazix.Distance(farmLocation.Position) <= E.Range)
                        E.Cast(farmLocation.Position);
                }
            }


            if (Config.GetBool("UseItemsFarm"))
            {
                MinionManager.FarmLocation farmLocation =
                    MinionManager.GetBestCircularFarmLocation(
                        MinionManager.GetMinions(Khazix.ServerPosition, Hydra.Range)
                            .Select(minion => minion.ServerPosition.To2D())
                            .ToList(), Hydra.Range, Hydra.Range);

                if (Hydra.IsReady() && Khazix.Distance(farmLocation.Position) <= Hydra.Range && farmLocation.MinionsHit >= 2)
                {
                    Items.UseItem(3074, Khazix);
                }
                if (Tiamat.IsReady() && Khazix.Distance(farmLocation.Position) <= Tiamat.Range && farmLocation.MinionsHit >= 2)
                {
                    Items.UseItem(3077, Khazix);
                }
            }
        }

        void Waveclear()
        {
            List<Obj_AI_Minion> allMinions = ObjectManager.Get<Obj_AI_Minion>().Where(x => x.IsValidTarget(W.Range)).ToList();

            if (Config.GetBool("UseQFarm") && Q.IsReady())
            {
                var minion = Orbwalker.GetTarget() as Obj_AI_Minion;
                if (minion != null && HealthPrediction.GetHealthPrediction(
                                minion, (int)(Khazix.Distance(minion) * 1000 / 1400)) >
                            0.35f * Khazix.GetSpellDamage(minion, SpellSlot.Q) && Khazix.Distance(minion) <= Q.Range)
                {
                    Q.Cast(minion);
                }
                else if (minion == null || !minion.IsValid)
                {
                    foreach (var min in allMinions.Where(x => x.IsValidTarget(Q.Range)))
                    {
                        if (HealthPrediction.GetHealthPrediction(
                                min, (int)(Khazix.Distance(min) * 1000 / 1400)) >
                            3 * Khazix.GetSpellDamage(min, SpellSlot.Q) && Khazix.Distance(min) <= Q.Range)
                        {
                            Q.Cast(min);
                            break;
                        }
                    }
                }
            }

            if (Config.GetBool("UseWFarm") && W.IsReady() && Khazix.HealthPercent <= Config.GetSlider("Farm.WHealth"))
            {
                var wmins = EvolvedW ? allMinions.Where(x => x.IsValidTarget(WE.Range)) : allMinions.Where(x => x.IsValidTarget(W.Range));
                MinionManager.FarmLocation farmLocation = MinionManager.GetBestCircularFarmLocation(wmins
                      .Select(minion => minion.ServerPosition.To2D())
                      .ToList(), EvolvedW ? WE.Width : W.Width, EvolvedW ? WE.Range : W.Range);
                var distcheck = EvolvedW ? Khazix.Distance(farmLocation.Position) <= WE.Range : Khazix.Distance(farmLocation.Position) <= W.Range;
                if (distcheck)
                {
                    W.Cast(farmLocation.Position);
                }
            }

            if (Config.GetBool("UseEFarm") && E.IsReady())
            {
                MinionManager.FarmLocation farmLocation =
                    MinionManager.GetBestCircularFarmLocation(
                        MinionManager.GetMinions(Khazix.ServerPosition, E.Range)
                            .Select(minion => minion.ServerPosition.To2D())
                            .ToList(), E.Width, E.Range);
                if (Khazix.Distance(farmLocation.Position) <= E.Range)
                {
                    E.Cast(farmLocation.Position);
                }
            }


            if (Config.GetBool("UseItemsFarm"))
            {
                MinionManager.FarmLocation farmLocation =
                    MinionManager.GetBestCircularFarmLocation(
                        MinionManager.GetMinions(Khazix.ServerPosition, Hydra.Range)
                            .Select(minion => minion.ServerPosition.To2D())
                            .ToList(), Hydra.Range, Hydra.Range);

                if (Hydra.IsReady() && Khazix.Distance(farmLocation.Position) <= Hydra.Range && farmLocation.MinionsHit >= 2)
                {
                    Items.UseItem(3074, Khazix);
                }
                if (Tiamat.IsReady() && Khazix.Distance(farmLocation.Position) <= Tiamat.Range && farmLocation.MinionsHit >= 2)
                {
                    Items.UseItem(3077, Khazix);
                }
                if (Titanic.IsReady() && Khazix.Distance(farmLocation.Position) <= Titanic.Range && farmLocation.MinionsHit >= 2)
                {
                    Items.UseItem(3748, Khazix);
                }
            }
        }


        void Combo()
        {
            var target = HeroManager.Enemies.FindAll(x => x.IsValidEnemy(W.Range) && !x.IsZombie).MaxOrDefault(x => TargetSelector.GetPriority(x));

            if ((target != null))
            {
                if (Khazix.Spellbook.GetSpell(SpellSlot.E).Name.ToLower() != "khazixqlong")
                {
                    distt = Khazix.Distance(target) + 60;
                }
                else if (Khazix.Spellbook.GetSpell(SpellSlot.E).Name.ToLower() == "khazixqlong")
                {
                    distt = Khazix.Distance(target);
                }

                var dist = distt;

                // Normal abilities
                if (Q.IsReady() && target.IsValidTarget(Q.Range) && Config.GetBool("UseQCombo"))
                {
                    Q.Cast(target);
                }

                if (W.IsReady() && target.IsValidTarget(W.Range) && Config.GetBool("UseWCombo"))
                {
                    W.Cast(target);
                }

                if (E.IsReady() && dist <= E.Range && Config.GetBool("UseECombo") && dist > Q.Range + (0.7 * Khazix.MoveSpeed))
                {
                    PredictionOutput pred = E.GetPrediction(target);
                    if (target.IsValid && !target.IsDead && ShouldJump(pred.CastPosition))
                    {
                        E.Cast(pred.CastPosition);
                    }
                }

                // Use EQ AND EW Synergy
                if ((dist <= E.Range + Q.Range + (0.7 * Khazix.MoveSpeed) && dist > Q.Range && E.IsReady() &&
                    Config.GetBool("UseEGapclose")) || (dist <= E.Range + W.Range && dist > Q.Range && E.IsReady() && W.IsReady() &&
                    Config.GetBool("UseEGapcloseW")))
                {
                    PredictionOutput pred = E.GetPrediction(target);
                    if (target.IsValid && !target.IsDead && ShouldJump(pred.CastPosition))
                    {
                        E.Cast(pred.CastPosition);
                    }
                    if (Config.GetBool("UseRGapcloseW") && R.IsReady())
                    {
                        R.CastOnUnit(Khazix);
                    }
                }


                // Ult Usage
                if (R.IsReady() && !Q.IsReady() && !W.IsReady() && !E.IsReady() &&
                    Config.GetBool("UseRCombo"))
                {
                    R.Cast();
                }
                // Evolved

                if (W.IsReady() && EvolvedW && dist <= WE.Range && Config.GetBool("UseWCombo"))
                {
                    PredictionOutput pred = WE.GetPrediction(target);
                    if (pred.Hitchance >= HitChance.High)
                    {
                        CastWE(target, pred.UnitPosition.To2D(), 0, HitChance.High);
                    }
                    if (pred.Hitchance >= HitChance.Collision)
                    {
                        List<Obj_AI_Base> PCollision = pred.CollisionObjects;
                        var x = PCollision.Where(PredCollisionChar => PredCollisionChar.Distance(target) <= 30).FirstOrDefault();
                        if (x != null)
                        {
                            W.Cast(x.Position);
                        }
                    }
                }

                if (dist <= E.Range + (0.7 * Khazix.MoveSpeed) && dist > Q.Range &&
                    Config.GetBool("UseECombo") && E.IsReady())
                {
                    PredictionOutput pred = E.GetPrediction(target);
                    if (target.IsValid && !target.IsDead && ShouldJump(pred.CastPosition))
                    {
                        E.Cast(pred.CastPosition);
                    }
                }

                if (Config.GetBool("UseItems"))
                {
                    UseItems(target);
                }
            }
        }


        void KillSteal()
        {
            Obj_AI_Hero target = HeroList
                .Where(x => x.IsValidTarget() && x.Distance(Khazix.Position) < 1375f && !x.IsZombie)
                .MinOrDefault(x => x.Health);

            if (target != null && target.IsInRange(600))
            {
                if (Config.GetBool("UseIgnite") && IgniteSlot != SpellSlot.Unknown &&
                    Khazix.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                {
                    double igniteDmg = Khazix.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
                    if (igniteDmg > target.Health)
                    {
                        Khazix.Spellbook.CastSpell(IgniteSlot, target);
                        return;
                    }
                }

                if (Config.GetBool("UseQKs") && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    double QDmg = GetQDamage(target);

                    if (!Jumping && target.Health <= QDmg)
                    {
                        Q.Cast(target);
                    }
                }

                if (Config.GetBool("UseEKs") && E.IsReady() &&
                    Vector3.Distance(Khazix.ServerPosition, target.ServerPosition) <= E.Range && Vector3.Distance(Khazix.ServerPosition, target.ServerPosition) > Q.Range)
                {
                    double EDmg = Khazix.GetSpellDamage(target, SpellSlot.E);
                    if (!Jumping && target.Health < EDmg)
                    {
                        Utility.DelayAction.Add(
                            Game.Ping + Config.GetSlider("EDelay"), delegate
                            {
                                PredictionOutput pred = E.GetPrediction(target);
                                if (target.IsValid && !target.IsDead)
                                {
                                    if (Config.GetBool("Ksbypass") || ShouldJump(pred.CastPosition))
                                    {
                                        E.Cast(pred.CastPosition);
                                    }
                                }
                            });
                    }
                }

                if (W.IsReady() && !EvolvedW && Vector3.Distance(Khazix.ServerPosition, target.ServerPosition) <= W.Range &&
                    Config.GetBool("UseWKs"))
                {
                    double WDmg = Khazix.GetSpellDamage(target, SpellSlot.W);
                    if (target.Health <= WDmg)
                    {
                        if (target.IsValidTarget(W.Range))
                            W.Cast(target);
                    }
                }

                if (W.IsReady() && EvolvedW &&
                        Vector3.Distance(Khazix.ServerPosition, target.ServerPosition) <= W.Range &&
                        Config.GetBool("UseWKs"))
                {
                    double WDmg = Khazix.GetSpellDamage(target, SpellSlot.W);
                    PredictionOutput pred = W.GetPrediction(target);
                    if (target.Health <= WDmg && pred.Hitchance > HitChance.Medium)
                    {
                        CastWE(target, pred.UnitPosition.To2D(), 0, HitChance.High);
                        return;
                    }

                    if (pred.Hitchance >= HitChance.Collision)
                    {
                        List<Obj_AI_Base> PCollision = pred.CollisionObjects;
                        var x =
                            PCollision
                                .FirstOrDefault(PredCollisionChar => Vector3.Distance(PredCollisionChar.ServerPosition, target.ServerPosition) <= 30);
                        if (x != null)
                        {
                            W.Cast(x.Position);
                            return;
                        }
                    }
                }


                // Mixed's EQ KS
                if (Q.IsReady() && E.IsReady() &&
                    Vector3.Distance(Khazix.ServerPosition, target.ServerPosition) <= E.Range + Q.Range
                    && Config.GetBool("UseEQKs"))
                {
                    double QDmg = GetQDamage(target);
                    double EDmg = Khazix.GetSpellDamage(target, SpellSlot.E);
                    if ((target.Health <= QDmg + EDmg))
                    {
                        Utility.DelayAction.Add(Config.GetSlider("EDelay"), delegate
                        {
                            PredictionOutput pred = E.GetPrediction(target);
                            if (target.IsValidTarget() && !target.IsZombie && ShouldJump(pred.CastPosition))
                            {
                                if (Config.GetBool("Ksbypass") || ShouldJump(pred.CastPosition))
                                {
                                    E.Cast(pred.CastPosition);
                                }
                            }
                        });
                    }
                }

                // MIXED EW KS
                if (W.IsReady() && E.IsReady() && !EvolvedW &&
                    Vector3.Distance(Khazix.ServerPosition, target.ServerPosition) <= W.Range + E.Range
                    && Config.GetBool("UseEWKs"))
                {
                    double WDmg = Khazix.GetSpellDamage(target, SpellSlot.W);
                    if (target.Health <= WDmg)
                    {

                        Utility.DelayAction.Add(Config.GetSlider("EDelay"), delegate
                        {
                            PredictionOutput pred = E.GetPrediction(target);
                            if (target.IsValid && !target.IsDead && ShouldJump(pred.CastPosition))
                            {
                                if (Config.GetBool("Ksbypass") || ShouldJump(pred.CastPosition))
                                {
                                    E.Cast(pred.CastPosition);
                                }
                            }
                        });
                    }
                }

                if (Tiamat.IsReady() &&
                    Vector2.Distance(Khazix.ServerPosition.To2D(), target.ServerPosition.To2D()) <= Tiamat.Range &&
                    Config.GetBool("UseTiamatKs"))
                {
                    double Tiamatdmg = Khazix.GetItemDamage(target, Damage.DamageItems.Tiamat);
                    if (target.Health <= Tiamatdmg)
                    {
                        Tiamat.Cast();
                        return;
                    }
                }
                if (Hydra.IsReady() &&
                    Vector2.Distance(Khazix.ServerPosition.To2D(), target.ServerPosition.To2D()) <= Hydra.Range &&
                    Config.GetBool("UseTiamatKs"))
                {
                    double hydradmg = Khazix.GetItemDamage(target, Damage.DamageItems.Hydra);
                    if (target.Health <= hydradmg)
                    {
                        Hydra.Cast();
                    }
                }
            }
        }

        public static void GameOnOnGameLoad(EventArgs args)
        {
            Task.Factory.StartNew(
                () =>
                {Khazix K6 = new Khazix();
                    try
                    {
                        
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            );
        }

        internal bool ShouldJump(Vector3 position)
        {
            if (!Config.GetBool("Safety.Enabled"))
            {
                return true;
            }
            if (Config.GetBool("Safety.TowerJump") && position.PointUnderEnemyTurret())
            {
                return false;
            }
            else if (Config.GetBool("Safety.Enabled"))
            {
                if (Khazix.HealthPercent < Config.GetSlider("Safety.MinHealth"))
                {
                    return false;
                }

                if (Config.GetBool("Safety.CountCheck"))
                {
                    var enemies = position.GetEnemiesInRange(400);
                    var allies = position.GetAlliesInRange(400);

                    var ec = enemies.Count;
                    var ac = allies.Count;
                    float setratio = Config.GetSlider("Safety.Ratio") / 5;


                    if (ec != 0 && !(ac / ec >= setratio))
                    {
                        return false;
                    }
                }
                return true;
            }
            return true;
        }



        internal void CastWE(Obj_AI_Base unit, Vector2 unitPosition, int minTargets = 0, HitChance hc = HitChance.Medium)
        {
            var points = new List<Vector2>();
            var hitBoxes = new List<int>();

            Vector2 startPoint = Khazix.ServerPosition.To2D();
            Vector2 originalDirection = W.Range * (unitPosition - startPoint).Normalized();

            foreach (Obj_AI_Hero enemy in HeroManager.Enemies)
            {
                if (enemy.IsValidTarget() && enemy.NetworkId != unit.NetworkId)
                {
                    PredictionOutput pos = W.GetPrediction(enemy);
                    if (pos.Hitchance >= HitChance.Medium)
                    {
                        points.Add(pos.UnitPosition.To2D());
                        hitBoxes.Add((int)enemy.BoundingRadius + 275);
                    }
                }
            }

            var posiblePositions = new List<Vector2>();

            for (int i = 0; i < 3; i++)
            {
                if (i == 0)
                    posiblePositions.Add(unitPosition + originalDirection.Rotated(0));
                if (i == 1)
                    posiblePositions.Add(startPoint + originalDirection.Rotated(Wangle));
                if (i == 2)
                    posiblePositions.Add(startPoint + originalDirection.Rotated(-Wangle));
            }


            if (startPoint.Distance(unitPosition) < 900)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 pos = posiblePositions[i];
                    Vector2 direction = (pos - startPoint).Normalized().Perpendicular();
                    float k = (2 / 3 * (unit.BoundingRadius + W.Width));
                    posiblePositions.Add(startPoint - k * direction);
                    posiblePositions.Add(startPoint + k * direction);
                }
            }

            var bestPosition = new Vector2();
            int bestHit = -1;

            foreach (Vector2 position in posiblePositions)
            {
                int hits = CountHits(position, points, hitBoxes);
                if (hits > bestHit)
                {
                    bestPosition = position;
                    bestHit = hits;
                }
            }

            if (bestHit <= minTargets)
                return;

            W.Cast(bestPosition.To3D(), false);
        }

        int CountHits(Vector2 position, List<Vector2> points, List<int> hitBoxes)
        {
            int result = 0;

            Vector2 startPoint = Khazix.ServerPosition.To2D();
            Vector2 originalDirection = W.Range * (position - startPoint).Normalized();
            Vector2 originalEndPoint = startPoint + originalDirection;

            for (int i = 0; i < points.Count; i++)
            {
                Vector2 point = points[i];

                for (int k = 0; k < 3; k++)
                {
                    var endPoint = new Vector2();
                    if (k == 0)
                        endPoint = originalEndPoint;
                    if (k == 1)
                        endPoint = startPoint + originalDirection.Rotated(Wangle);
                    if (k == 2)
                        endPoint = startPoint + originalDirection.Rotated(-Wangle);

                    if (point.Distance(startPoint, endPoint, true, true) <
                        (W.Width + hitBoxes[i]) * (W.Width + hitBoxes[i]))
                    {
                        result++;
                        break;
                    }
                }
            }
            return result;
        }


        void DoubleJump(EventArgs args)
        {
            if (!E.IsReady() || !EvolvedE || !Config.GetBool("djumpenabled") || Khazix.IsDead || Khazix.IsRecalling())
            {
                return;
            }

            var Targets = HeroList.Where(x => x.IsValidTarget() && !x.IsInvulnerable && !x.IsZombie);

            if (Q.IsReady() && E.IsReady())
            {
                var CheckQKillable = Targets.FirstOrDefault(x => Vector3.Distance(Khazix.ServerPosition, x.ServerPosition) < Q.Range - 25 && GetQDamage(x) > x.Health);

                if (CheckQKillable != null)
                {
                    Jumping = true;
                    Jumppoint1 = GetJumpPoint(CheckQKillable);
                    E.Cast(Jumppoint1);
                    Q.Cast(CheckQKillable);
                    var oldpos = Khazix.ServerPosition;
                    Utility.DelayAction.Add(Config.GetSlider("JEDelay") + Game.Ping, () =>
                    {
                        if (E.IsReady())
                        {
                            Jumppoint2 = GetJumpPoint(CheckQKillable, false);
                            E.Cast(Jumppoint2);
                        }
                        Jumping = false;
                    });
                }
            }
        }

        Vector3 GetJumpPoint(Obj_AI_Hero Qtarget, bool firstjump = true)
        {
            if (Khazix.ServerPosition.PointUnderEnemyTurret())
            {
                return Khazix.ServerPosition.Extend(NexusPosition, E.Range);
            }

            if (Config.GetSL("jumpmode") == 0)
            {
                return Khazix.ServerPosition.Extend(NexusPosition, E.Range);
            }

            if (firstjump && Config.GetBool("jcursor"))
            {
                return Game.CursorPos;
            }

            if (!firstjump && Config.GetBool("jcursor2"))
            {
                return Game.CursorPos;
            }

            Vector3 Position = new Vector3();
            var jumptarget = HeroList
                      .FirstOrDefault(x => x.IsValidTarget() && !x.IsZombie && x != Qtarget &&
                              Vector3.Distance(Khazix.ServerPosition, x.ServerPosition) < E.Range);

            if (jumptarget != null)
            {
                Position = jumptarget.ServerPosition;
            }
            if (jumptarget == null)
            {
                return Khazix.ServerPosition.Extend(NexusPosition, E.Range);
            }
            return Position;
        }


        void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Target.Type == GameObjectType.obj_AI_Hero)
            {
                if (Config.GetBool("Safety.noaainult") && IsInvisible)
                {
                    args.Process = false;
                    return;
                }
                //if (Config.GetBool("djumpenabled") && Config.GetBool("noauto"))
                //{
                //    if (args.Target.Health < GetQDamage((Obj_AI_Hero)args.Target) && Khazix.ManaPercent > 15)
                //    {
                //        args.Process = false;
                //    }
                //}
            }
        }

        void OnDraw(EventArgs args)
        {
            if (Config.GetBool("Drawings.Disable") || Khazix.IsDead || Khazix.IsRecalling())
            {
                return;
            }
            if (Config.GetBool("Debugon"))
            {
                var isolatedtargs = GetIsolatedTargets();
                foreach (var x in isolatedtargs)
                {
                    var heroposwts = Drawing.WorldToScreen(x.Position);
                    Drawing.DrawText(heroposwts.X, heroposwts.Y, System.Drawing.Color.White, "Isolated");
                }
            }

            if (Config.GetBool("jumpdrawings") && Jumping)
            {
                var PlayerPosition = Drawing.WorldToScreen(Khazix.Position);
                var Jump1 = Drawing.WorldToScreen(Jumppoint1).To3D();
                var Jump2 = Drawing.WorldToScreen(Jumppoint2).To3D();
                Render.Circle.DrawCircle(Jump1, 250, System.Drawing.Color.White);
                Render.Circle.DrawCircle(Jump2, 250, System.Drawing.Color.White);
                Drawing.DrawLine(PlayerPosition.X, PlayerPosition.Y, Jump1.X, Jump1.Y, 10, System.Drawing.Color.DarkCyan);
                Drawing.DrawLine(Jump1.X, Jump1.Y, Jump2.X, Jump2.Y, 10, System.Drawing.Color.DarkCyan);
            }

            var drawq = Config.GetCircle("DrawQ");
            var draww = Config.GetCircle("DrawW");
            var drawe = Config.GetCircle("DrawE");

            if (drawq.Active)
            {
                Render.Circle.DrawCircle(Khazix.Position, Q.Range, drawq.Color);
            }
            if (draww.Active)
            {
                Render.Circle.DrawCircle(Khazix.Position, W.Range, drawq.Color);
            }

            if (drawe.Active)
            {
                Render.Circle.DrawCircle(Khazix.Position, E.Range, drawq.Color);
            }

        }
    }
}