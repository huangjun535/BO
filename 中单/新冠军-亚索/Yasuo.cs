namespace YuLeYasuo_Rework.Plugin
{
    using LeagueSharp;
    using LeagueSharp.Data.Enumerations;
    using LeagueSharp.SDK;
    using LeagueSharp.SDK.Enumerations;
    using LeagueSharp.SDK.Polygons;
    using LeagueSharp.SDK.TSModes;
    using LeagueSharp.SDK.UI;
    using LeagueSharp.SDK.Utils;
    using SharpDX;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;
    using YuLeYasuo_Rework.Core;
    using YuLeYasuo_Rework.Evade;
    using Color = System.Drawing.Color;
    using Menu = LeagueSharp.SDK.UI.Menu;

    internal class Yasuo : Program
    {
        private const float QDelay = 0.39f, Q2Delay = 0.35f, QDelays = 0.22f, Q2Delays = 0.315f;
        private const int RWidth = 400;
        private static int cDash;
        private static bool haveQ3, haveR;
        private static bool isDash, isBlockQ;
        private static int lastE;
        private static Vector3 posDash;
        private static MissileClient wallLeft, wallRight;
        private static RectanglePoly wallPoly;
        private static string[] AutoEnableList =
        {
             "Annie", "Ahri", "Akali", "Anivia", "Annie", "Brand", "Cassiopeia", "Diana", "Evelynn", "FiddleSticks", "Fizz", "Gragas", "Heimerdinger", "Karthus",
             "Kassadin", "Katarina", "Kayle", "Kennen", "Leblanc", "Lissandra", "Lux", "Malzahar", "Mordekaiser", "Morgana", "Nidalee", "Orianna",
             "Ryze", "Sion", "Swain", "Syndra", "Teemo", "TwistedFate", "Veigar", "Viktor", "Vladimir", "Xerath", "Ziggs", "Zyra", "Velkoz", "Azir", "Ekko",
             "Ashe", "Caitlyn", "Corki", "Draven", "Ezreal", "Graves", "Jayce", "Jinx", "KogMaw", "Lucian", "MasterYi", "MissFortune", "Quinn", "Shaco", "Sivir",
             "Talon", "Tristana", "Twitch", "Urgot", "Varus", "Vayne", "Yasuo", "Zed", "Kindred", "AurelionSol"
        };

        public Yasuo()
        {
            Q = new Spell(SpellSlot.Q, 505).SetSkillshot(QDelay, 20, float.MaxValue, false, SkillshotType.SkillshotLine);
            Q2 = new Spell(Q.Slot, 1100).SetSkillshot(Q2Delay, 90, 1200, true, Q.Type);
            Q3 = new Spell(Q.Slot, 220).SetSkillshot(0.001f, 220, float.MaxValue, false, SkillshotType.SkillshotCircle);
            W = new Spell(SpellSlot.W, 400).SetTargetted(0.25f, float.MaxValue);
            E = new Spell(SpellSlot.E, 475).SetTargetted(0, 1200);
            E2 = new Spell(E.Slot, E.Range).SetTargetted(Q3.Delay, E.Speed);
            R = new Spell(SpellSlot.R, 1200);
            Q.DamageType = Q2.DamageType = R.DamageType = DamageType.Physical;
            E.DamageType = DamageType.Magical;
            Q.MinHitChance = Q2.MinHitChance = HitChance.VeryHigh;
            Q.CastCondition += () => !isBlockQ;
            Q3.CastCondition += () => !isBlockQ;
            E.CastCondition += () => !posDash.IsValid();

            var QMenu = MainMenu.Add(new Menu("QMenu", "Q 设置"));
            {
                QMenu.Separator("连招设置");
                QMenu.Separator("连招总是使用");
                QMenu.Separator("骚扰设置");
                QMenu.Bool("HybridQ3", "使用Q3");
                QMenu.Bool("HybridQLastHit", "使用Q1/Q2补刀");
                QMenu.Separator("清线清野");
                QMenu.Bool("LCQ", "使用Q");
                QMenu.Bool("LCQ3", "使用Q3", false);
                QMenu.Separator("补刀设置");
                QMenu.Bool("LHQ", "使用Q");
                QMenu.Bool("LHQ3", "使用Q3", false);
                QMenu.Separator("击杀设置");
                QMenu.Bool("KSQ", "使用Q");
                QMenu.Separator("逃跑设置");
                QMenu.Bool("FleeQ", "使用Q");
                QMenu.Separator("自动设置");
                QMenu.KeyBind("HybridAutoQ", "自动Q骚扰按键", Keys.T, KeyBindType.Toggle);
                QMenu.Bool("HybridAutoQ3", "自动使用Q3", false);
                QMenu.KeyBind("StackQ", "自动堆叠Q", Keys.Z, KeyBindType.Toggle);
            }

            var WMenu = MainMenu.Add(new Menu("WMenu", "W 设置"));
            {
                WMenu.Separator("连招设置");
                WMenu.Bool("W", "使用W", false);
                WMenu.Separator("其他请查看躲避");
            }

            var EMenu = MainMenu.Add(new Menu("EMenu", "E 设置"));
            {
                EMenu.Separator("连招设置");
                EMenu.Bool("CBE", "浪比E", false);
                EMenu.Bool("CBEGap", "使用E突进");
                EMenu.List("CBEMode", "突进模式", new[] { "Enemy", "Mouse" });
                EMenu.Bool("CBETower", "塔下使用", false);
                EMenu.Bool("CBEStackQ", "突进时自动堆叠Q", false);
                EMenu.Separator("清线清野");
                EMenu.Bool("LCE", "使用E");
                EMenu.Bool("LCELastHit", "仅用来补刀", false);
                EMenu.Bool("LCETower", "塔下使用", false);
                EMenu.Separator("补刀设置");
                EMenu.Bool("LHE", "使用E");
                EMenu.Bool("LHETower", "塔下使用", false);
                EMenu.Separator("击杀设置");
                EMenu.Bool("KSE", "使用E");
                EMenu.Separator("逃跑设置");
                EMenu.KeyBind("FleeE", "使用E按键", Keys.Z);
            }

            var RMenu = MainMenu.Add(new Menu("RMenu", "R 设置"));
            {
                RMenu.Separator("连招设置");
                RMenu.KeyBind("R", "使用R", Keys.X, KeyBindType.Toggle);
                RMenu.Bool("RDelay", "延迟释放");
                RMenu.Slider("RHpU", "敌人血量 <= %", 60);
                RMenu.Slider("RCountA", "或者命中敌人数 >=", 2, 1, 5);
                RMenu.Separator("击杀设置");
                RMenu.Bool("KSR", "使用R");
                if (GameObjects.EnemyHeroes.Any())
                {
                    RMenu.Separator("击杀名单");
                    GameObjects.EnemyHeroes.ForEach(i => RMenu.Bool("RCast" + i.ChampionName, i.ChampionName, AutoEnableList.Contains(i.ChampionName)));
                }
            }

            if (GameObjects.EnemyHeroes.Any())
            {
                Evade.Init();
                EvadeTarget.Init();
            }

            new AutoWard(MainMenu);

            var MiscMenu = MainMenu.Add(new Menu("MiscMenu", "杂项设置"));
            {
                MiscMenu.Bool("Ignite", "使用点燃");
                MiscMenu.Bool("Item", "使用物品");
            }

            var DrawMenu = MainMenu.Add(new Menu("Draw", "显示设置"));
            {
                DrawMenu.Bool("Q", "Q 范围", false);
                DrawMenu.Bool("E", "E 范围", false);
                DrawMenu.Bool("R", "R 范围", false);
                DrawMenu.Bool("UseR", "连招R使用状态");
                DrawMenu.Bool("StackQ", "自动堆叠Q状态");
            }

            MainMenu.KeyBind("EQ3Flash", "EQ闪现按键", Keys.XButton2);

            Game.OnUpdate += OnUpdate;
            Drawing.OnEndScene += OnEndScene;
            Drawing.OnDraw += OnDraw;
            Game.OnUpdate += args =>
                {
                    if (Player.IsDead)
                    {
                        if (isDash)
                        {
                            isDash = false;
                            posDash = new Vector3();
                        }
                        return;
                    }
                    if (isDash && !Player.IsDashing())
                    {
                        isDash = false;
                        DelayAction.Add(
                            50,
                            () =>
                                {
                                    if (!isDash)
                                    {
                                        posDash = new Vector3();
                                    }
                                });
                    }
                    Q.Delay = GetQDelay(false);
                    Q2.Delay = GetQDelay(true);
                    E.Speed = E2.Speed = 1200 + (Player.MoveSpeed - 345);
                };
            Variables.Orbwalker.OnAction += (sender, args) =>
                {
                    if (args.Type != OrbwalkingType.AfterAttack
                        || Variables.Orbwalker.ActiveMode != OrbwalkingMode.LaneClear || !(args.Target is Obj_AI_Turret)
                        || !Q.IsReady() || haveQ3)
                    {
                        return;
                    }
                    if (Q.GetTarget(50) != null || Common.ListMinions().Count(i => i.IsValidTarget(Q.Range + 50)) > 0)
                    {
                        return;
                    }
                    if ((Items.HasItem((int)ItemId.Sheen) && Items.CanUseItem((int)ItemId.Sheen))
                        || (Items.HasItem((int)ItemId.Trinity_Force) && Items.CanUseItem((int)ItemId.Trinity_Force)))
                    {
                        Q.Cast(Game.CursorPos);
                    }
                };
            Obj_AI_Base.OnProcessSpellCast += (sender, args) =>
                {
                    if (!sender.IsMe)
                    {
                        return;
                    }
                    if (args.Slot == SpellSlot.E)
                    {
                        lastE = Variables.TickCount;
                    }
                };
            Events.OnDash += (sender, args) =>
                {
                    if (!args.Unit.IsMe)
                    {
                        return;
                    }
                    isDash = true;
                    posDash = args.EndPos.ToVector3();
                };
            Obj_AI_Base.OnBuffAdd += (sender, args) =>
                {
                    if (!sender.IsMe)
                    {
                        return;
                    }
                    switch (args.Buff.DisplayName)
                    {
                        case "YasuoQ3W":
                            haveQ3 = true;
                            break;
                        case "YasuoDashScalar":
                            cDash = 1;
                            break;
                        case "YasuoRArmorPen":
                            haveR = isBlockQ = true;
                            break;
                    }
                };
            Obj_AI_Base.OnBuffRemove += (sender, args) =>
                {
                    if (!sender.IsMe)
                    {
                        return;
                    }
                    switch (args.Buff.DisplayName)
                    {
                        case "YasuoQ3W":
                            haveQ3 = false;
                            break;
                        case "YasuoDashScalar":
                            cDash = 0;
                            break;
                    }
                };
            Obj_AI_Base.OnBuffUpdateCount += (sender, args) =>
                {
                    if (!sender.IsMe || args.Buff.DisplayName != "YasuoDashScalar")
                    {
                        return;
                    }
                    cDash = 2;
                };
            Obj_AI_Base.OnDoCast += (sender, args) =>
                {
                    if (!sender.IsMe)
                    {
                        return;
                    }
                    if (haveR && args.SData.Name == "TempYasuoRMissile")
                    {
                        haveR = false;
                        DelayAction.Add(40, Variables.Orbwalker.ResetSwingTimer);
                        DelayAction.Add(70, () => isBlockQ = false);
                    }
                };
            GameObjectNotifier<MissileClient>.OnCreate += (sender, args) =>
                {
                    var spellCaster = args.SpellCaster as Obj_AI_Hero;
                    if (spellCaster == null || !spellCaster.IsMe)
                    {
                        return;
                    }
                    switch (args.SData.Name)
                    {
                        case "YasuoWMovingWallMisL":
                            wallLeft = args;
                            break;
                        case "YasuoWMovingWallMisR":
                            wallRight = args;
                            break;
                    }
                };
            GameObjectNotifier<MissileClient>.OnDelete += (sender, args) =>
                {
                    if (args.Compare(wallLeft))
                    {
                        wallLeft = null;
                    }
                    else if (args.Compare(wallRight))
                    {
                        wallRight = null;
                    }
                };
        }

        private static bool CanCastQCir => posDash.IsValid() && posDash.DistanceToPlayer() < 150;

        private static List<Obj_AI_Base> GetQCirObj
            =>
                Common.ListEnemies(true)
                    .Where(i => i.IsValidTarget() && Q3.WillHit(Q3.GetPredPosition(i), posDash))
                    .ToList();

        private static List<Obj_AI_Base> GetQCirTarget
            =>
                Variables.TargetSelector.GetTargets(Q3.Width, Q.DamageType, true, posDash)
                    .Where(i => Q3.WillHit(Q3.GetPredPosition(i), posDash))
                    .Cast<Obj_AI_Base>()
                    .ToList();

        private static bool IsDashing => Variables.TickCount - lastE <= 100 || Player.IsDashing() || posDash.IsValid();

        private static Spell SpellQ => !haveQ3 ? Q : Q2;

        private static bool UseR => MainMenu["RMenu"]["R"].GetValue<MenuKeyBind>().Active && R.IsReady();


        private static void AutoQ()
        {
            if (!MainMenu["QMenu"]["HybridAutoQ"].GetValue<MenuKeyBind>().Active || !Q.IsReady() || IsDashing
                || (haveQ3 && !MainMenu["QMenu"]["HybridAutoQ3"]))
            {
                return;
            }
            if (!haveQ3)
            {
                Q.CastingBestTarget(true);
            }
            else
            {
                CastQ3();
            }
        }

        private static void BeyBlade()
        {
            if (!Common.CanFlash || !Q.IsReady() || !haveQ3)
            {
                return;
            }
            if (Player.IsDashing())
            {
                var bestHit = 0;
                var bestPos = new Vector3();
                for (var i = 0; i < 360; i += 10)
                {
                    var pos =
                        (Player.ServerPosition.ToVector2()
                         + FlashRange * new Vector2(1, 0).Rotated((float)(Math.PI * i / 180.0))).ToVector3();
                    var hits = GameObjects.EnemyHeroes.Count(a => a.IsValidTarget(Q3.Width, true, pos));
                    if (hits > bestHit)
                    {
                        bestHit = hits;
                        bestPos = pos;
                    }
                }
                if (bestPos.IsValid() && Q3.Cast(bestPos))
                {
                    DelayAction.Add(5, () => Player.Spellbook.CastSpell(Flash, bestPos));
                }
            }
            if (!E.IsReady())
            {
                return;
            }
            var obj =
                Common.ListEnemies(true)
                    .Where(i => i.IsValidTarget(E.Range) && !HaveE(i))
                    .MaxOrDefault(
                        i =>
                        GameObjects.EnemyHeroes.Count(
                            a =>
                            a.IsValidTarget() && !a.Compare(i)
                            && (a.Distance(i) < Q3.Width + FlashRange - 50
                                || a.Distance(GetPosAfterDash(i)) < Q3.Width + FlashRange - 50)));
            if (obj != null)
            {
                E.CastOnUnit(obj);
            }
        }

        private static bool CanCastDelayR(Obj_AI_Hero target, bool isAirBlade = false)
        {
            if (target.HasBuffOfType(BuffType.Knockback))
            {
                return true;
            }
            var buff = target.Buffs.FirstOrDefault(i => i.Type == BuffType.Knockup);
            return buff != null
                   && Game.Time - buff.StartTime >= (!isAirBlade ? 0.89 : 0.85) * (buff.EndTime - buff.StartTime);
        }

        private static bool CanDash(
            Obj_AI_Base target,
            bool inQCir = false,
            bool underTower = true,
            Vector3 pos = new Vector3(),
            bool isAirBlade = false)
        {
            if (HaveE(target))
            {
                return false;
            }
            if (!pos.IsValid())
            {
                pos = E.GetPredPosition(target, true);
            }
            var posAfterE = GetPosAfterDash(target);
            return (underTower || !posAfterE.IsUnderEnemyTurret())
                   && (inQCir
                           ? Q3.WillHit(pos, posAfterE)
                           : posAfterE.Distance(pos) < (!isAirBlade ? pos.DistanceToPlayer() : R.Range))
                   && Evade.IsSafePoint(posAfterE.ToVector2()).IsSafe;
        }

        private static bool CastQ3()
        {
            var targets = Variables.TargetSelector.GetTargets(Q2.Range + Q2.Width / 2, Q2.DamageType);
            if (targets.Count == 0)
            {
                return false;
            }
            var posCast = new Vector3();
            foreach (var pred in
                targets.Select(i => Q2.GetPrediction(i, true, -1, CollisionableObjects.YasuoWall))
                    .Where(
                        i =>
                        i.Hitchance >= Q2.MinHitChance || (i.Hitchance >= HitChance.High && i.AoeTargetsHitCount > 1))
                    .OrderByDescending(i => i.AoeTargetsHitCount))
            {
                posCast = pred.CastPosition;
                break;
            }
            return posCast.IsValid() && Q2.Cast(posCast);
        }

        private static bool CastQCir(List<Obj_AI_Base> obj)
        {
            var target = obj.FirstOrDefault();
            return target != null && Q3.Cast(SpellQ.GetPredPosition(target, true));
        }

        private static void Combo()
        {
            if (MainMenu["EMenu"]["CBE"] && E.IsReady() && wallLeft != null && wallRight != null)
            {
                var target = Variables.TargetSelector.GetTarget(E.Range, DamageType.Physical);
                if (target != null && Math.Abs(target.GetProjectileSpeed() - float.MaxValue) > float.Epsilon
                    && !HaveE(target) && Evade.IsSafePoint(GetPosAfterDash(target).ToVector2()).IsSafe)
                {
                    var listPos =
                        Common.ListEnemies()
                            .Where(i => i.IsValidTarget(E.Range * 2) && !HaveE(i))
                            .Select(GetPosAfterDash)
                            .Where(
                                i =>
                                target.Distance(i) < target.DistanceToPlayer()
                                || target.Distance(i) < target.GetRealAutoAttackRange() + 100)
                            .ToList();
                    if (listPos.Any(i => IsThroughWall(target.ServerPosition, i)) && E.CastOnUnit(target))
                    {
                        return;
                    }
                }
            }
            var targetE = MainMenu["EMenu"]["CBEGap"] && E.Level > 0 ? GetBestDashObj(MainMenu["EMenu"]["CBETower"]) : null;
            if (targetE != null && E.CastOnUnit(targetE))
            {
                return;
            }
            if (Q.IsReady())
            {
                if (Player.IsDashing())
                {
                    var target = GetRTarget(true);
                    if (target != null && Q3.Cast(target.ServerPosition))
                    {
                        DelayAction.Add(5, () => R.CastOnUnit(target));
                    }
                }
                if (IsDashing)
                {
                    if (CanCastQCir)
                    {
                        if (CastQCir(GetQCirTarget))
                        {
                            return;
                        }
                        if (!haveQ3 && MainMenu["EMenu"]["CBEGap"] && MainMenu["EMenu"]["CBEStackQ"]
                            && Player.CountEnemyHeroesInRange(700) == 0 && CastQCir(GetQCirObj))
                        {
                            return;
                        }
                    }
                }
                else if (targetE == null && (!haveQ3 ? Q.CastingBestTarget(true).IsCasted() : CastQ3()))
                {
                    return;
                }
            }
            if (MainMenu["WMenu"]["W"] && W.IsReady())
            {
                var target = Variables.TargetSelector.GetTarget(E.Range, DamageType.Physical);
                if (target != null && Math.Abs(target.GetProjectileSpeed() - float.MaxValue) > float.Epsilon
                    && (target.HealthPercent > Player.HealthPercent
                            ? Player.CountAllyHeroesInRange(500) < target.CountEnemyHeroesInRange(700)
                            : Player.HealthPercent <= 30))
                {
                    var posPred = W.GetPredPosition(target, true);
                    if (posPred.DistanceToPlayer() > 100 && posPred.DistanceToPlayer() < 330 && W.Cast(posPred))
                    {
                        return;
                    }
                }
            }
            if (UseR)
            {
                var target = GetRTarget();
                if (target != null && R.CastOnUnit(target))
                {
                    return;
                }
            }
            var subTarget = Q.GetTarget(100) ?? Q2.GetTarget();
            if (MainMenu["MiscMenu"]["Item"])
            {
                UseItem(subTarget);
            }
            if (subTarget != null && MainMenu["MiscMenu"]["Ignite"] && Common.CanIgnite && subTarget.HealthPercent < 25
                && subTarget.DistanceToPlayer() <= IgniteRange)
            {
                Player.Spellbook.CastSpell(Ignite, subTarget);
            }
        }

        private static void Flee()
        {
            if (MainMenu["QMenu"]["FleeQ"] && Q.IsReady() && !haveQ3 && IsDashing && CanCastQCir && CastQCir(GetQCirObj))
            {
                return;
            }
            if (!E.IsReady())
            {
                return;
            }
            var obj = GetBestDashObjToMouse(true);
            if (obj != null)
            {
                E.CastOnUnit(obj);
            }
        }

        private static Obj_AI_Base GetBestDashObj(bool underTower)
        {
            if (UseR && Q.IsReady(50))
            {
                var target = GetRTarget(true);
                if (target != null)
                {
                    return E.IsInRange(target) && CanDash(target, true, underTower, target.ServerPosition, true)
                               ? target
                               : (GetBestDashObjToUnit(target, true, underTower, true)
                                  ?? GetBestDashObjToUnit(target, false, underTower, true));
                }
            }
            if (MainMenu["EMenu"]["CBEMode"].GetValue<MenuList>().Index == 0)
            {
                var target = E.GetTarget(Q3.Width);
                if (target != null && haveQ3 && Q.IsReady(50))
                {
                    var nearObj = GetBestDashObjToUnit(target, true, underTower);
                    if (nearObj != null
                        && (GetPosAfterDash(nearObj).CountEnemyHeroesInRange(Q3.Width) > 1
                            || Player.CountEnemyHeroesInRange(Q.Range + E.Range / 2) == 1))
                    {
                        return nearObj;
                    }
                }
                target = E.GetTarget();
                if (target != null
                    && ((cDash > 0 && CanDash(target, false, underTower))
                        || (haveQ3 && Q.IsReady(50) && CanDash(target, true, underTower))))
                {
                    return target;
                }
                target = Q.GetTarget(100) ?? Q2.GetTarget();
                if (target != null && (!Player.Spellbook.IsAutoAttacking || Player.HealthPercent < 40))
                {
                    var nearObj = GetBestDashObjToUnit(target, false, underTower);
                    var canDash = cDash == 0 && nearObj != null && !HaveE(target);
                    if (Q.IsReady(50))
                    {
                        var nearObjQ3 = GetBestDashObjToUnit(target, true, underTower);
                        if (nearObjQ3 != null)
                        {
                            nearObj = nearObjQ3;
                            canDash = true;
                        }
                    }
                    if (!canDash && target.DistanceToPlayer() > target.GetRealAutoAttackRange() * 0.7)
                    {
                        canDash = true;
                    }
                    if (canDash)
                    {
                        if (nearObj == null && E.IsInRange(target) && CanDash(target, false, underTower))
                        {
                            nearObj = target;
                        }
                        if (nearObj != null)
                        {
                            return nearObj;
                        }
                    }
                }
            }
            else
            {
                var target = Variables.Orbwalker.GetTarget();
                if (target == null || Player.Distance(target) > target.GetRealAutoAttackRange() * 0.7
                    || Player.Distance(Game.CursorPos) > E.Range * 1.5)
                {
                    var obj = GetBestDashObjToMouse(underTower);
                    if (obj != null)
                    {
                        return obj;
                    }
                }
            }
            return null;
        }

        private static Obj_AI_Base GetBestDashObjToMouse(bool underTower)
        {
            var pos = Game.CursorPos;
            return
                Common.ListEnemies()
                    .Where(i => i.IsValidTarget(E.Range) && CanDash(i, false, underTower, pos))
                    .MinOrDefault(i => GetPosAfterDash(i).Distance(pos));
        }

        private static Obj_AI_Base GetBestDashObjToUnit(
            Obj_AI_Hero target,
            bool inQCir,
            bool underTower,
            bool isAirBlade = false)
        {
            var pos = !isAirBlade ? E.GetPredPosition(target, true) : target.ServerPosition;
            return
                Common.ListEnemies()
                    .Where(
                        i =>
                        i.IsValidTarget(E.Range) && !i.Compare(target)
                        && CanDash(i, inQCir, underTower, pos, isAirBlade))
                    .MinOrDefault(i => GetPosAfterDash(i).Distance(pos));
        }

        private static double GetEDmg(Obj_AI_Base target)
        {
            return E.GetDamage(target) + E.GetDamage(target, DamageStage.Buff) - 3;
        }

        private static Vector3 GetPosAfterDash(Obj_AI_Base target)
        {
            return Player.ServerPosition.Extend(target.ServerPosition, E.Range);
        }

        private static float GetQDelay(bool isQ3)
        {
            var delayOri = !isQ3 ? QDelay : Q2Delay;
            var delayMax = !isQ3 ? QDelays : Q2Delays;
            var perReduce = 1 - delayMax / delayOri;
            var delayReal =
                Math.Max(
                    delayOri * (1 - Math.Min((Player.AttackSpeedMod - 1) * (perReduce / 1.1f), perReduce)),
                    delayMax);
            return (float)Math.Round((decimal)delayReal, 3, MidpointRounding.AwayFromZero);
        }

        private static double GetQDmg(Obj_AI_Base target)
        {
            var dmgItem = 0d;
            if (Items.HasItem((int)ItemId.Sheen) && (Items.CanUseItem((int)ItemId.Sheen) || Player.HasBuff("Sheen")))
            {
                dmgItem = Player.BaseAttackDamage;
            }
            if (Items.HasItem((int)ItemId.Trinity_Force)
                && (Items.CanUseItem((int)ItemId.Trinity_Force) || Player.HasBuff("Sheen")))
            {
                dmgItem = Player.BaseAttackDamage * 2;
            }
            if (dmgItem > 0)
            {
                dmgItem = Player.CalculateDamage(target, DamageType.Physical, dmgItem);
            }
            double dmgQ = Q.GetDamage(target);
            if (Math.Abs(Player.Crit - 1) < float.Epsilon)
            {
                dmgQ += Player.CalculateDamage(
                    target,
                    Q.DamageType,
                    (Items.HasItem((int)ItemId.Infinity_Edge) ? 0.875 : 0.5) * Player.TotalAttackDamage);
            }
            return dmgQ + dmgItem;
        }

        private static Obj_AI_Hero GetRTarget(bool isAirBlade = false)
        {
            var result = new Tuple<Obj_AI_Hero, List<Obj_AI_Hero>>(null, new List<Obj_AI_Hero>());
            var targets = Variables.TargetSelector.GetTargets(R.Range, R.DamageType).Where(HaveR);
            foreach (var target in targets)
            {
                var nears =
                    GameObjects.EnemyHeroes.Where(
                        i => i.IsValidTarget(RWidth, true, target.ServerPosition) && !i.Compare(target) && HaveR(i))
                        .ToList();
                nears.Add(target);
                if (nears.Count > result.Item2.Count
                    && ((nears.Count > 1 && nears.Any(i => i.Health + i.PhysicalShield <= R.GetDamage(i) + GetQDmg(i)))
                        || nears.Sum(i => i.HealthPercent) / nears.Count < MainMenu["RMenu"]["RHpU"]
                        || nears.Count >= MainMenu["RMenu"]["RCountA"]))
                {
                    result = new Tuple<Obj_AI_Hero, List<Obj_AI_Hero>>(target, nears);
                }
            }
            return MainMenu["RMenu"]["RDelay"]
                   && (Player.HealthPercent >= 15
                       || GameObjects.EnemyHeroes.Count(i => i.IsValidTarget(600) && !HaveR(i)) == 0)
                       ? (result.Item2.Any(i => CanCastDelayR(i, isAirBlade)) ? result.Item1 : null)
                       : result.Item1;
        }

        private static bool HaveE(Obj_AI_Base target)
        {
            return target.HasBuff("YasuoDashWrapper");
        }

        private static bool HaveR(Obj_AI_Hero target)
        {
            return target.HasBuffOfType(BuffType.Knockback) || target.HasBuffOfType(BuffType.Knockup);
        }

        private static void Hybrid()
        {
            if (!Q.IsReady() || IsDashing)
            {
                return;
            }
            if (!haveQ3)
            {
                var state = Q.CastingBestTarget(true);
                if (state.IsCasted())
                {
                    return;
                }
                if (state == CastStates.InvalidTarget && MainMenu["QMenu"]["HybridQLastHit"] && Q.GetTarget(50) == null
                    && !Player.Spellbook.IsAutoAttacking)
                {
                    var minion =
                        GameObjects.EnemyMinions.Where(
                            i => (i.IsMinion() || i.IsPet(false)) && IsInRangeQ(i) && Q.CanLastHit(i, GetQDmg(i)))
                            .MaxOrDefault(i => i.MaxHealth);
                    if (minion != null)
                    {
                        Q.Casting(minion);
                    }
                }
            }
            else if (MainMenu["QMenu"]["HybridQ3"])
            {
                CastQ3();
            }
        }

        private static bool IsInRangeQ(Obj_AI_Minion minion)
        {
            return minion.IsValidTarget(Math.Min(465 + minion.BoundingRadius / 3, 480));
        }

        private static bool IsThroughWall(Vector3 from, Vector3 to)
        {
            if (wallLeft == null || wallRight == null)
            {
                return false;
            }
            wallPoly = new RectanglePoly(wallLeft.Position, wallRight.Position, 75);
            for (var i = 0; i < wallPoly.Points.Count; i++)
            {
                var inter = wallPoly.Points[i].Intersection(
                    wallPoly.Points[i != wallPoly.Points.Count - 1 ? i + 1 : 0],
                    from.ToVector2(),
                    to.ToVector2());
                if (inter.Intersects)
                {
                    return true;
                }
            }
            return false;
        }

        private static void KillSteal()
        {
            if (MainMenu["QMenu"]["KSQ"] && Q.IsReady())
            {
                if (IsDashing)
                {
                    if (CanCastQCir)
                    {
                        var targets = GetQCirTarget.Where(i => i.Health + i.PhysicalShield <= GetQDmg(i)).ToList();
                        if (CastQCir(targets))
                        {
                            return;
                        }
                    }
                }
                else
                {
                    var target = SpellQ.GetTarget(SpellQ.Width / 2);
                    if (target != null && target.Health + target.PhysicalShield <= GetQDmg(target))
                    {
                        if (!haveQ3)
                        {
                            if (Q.Casting(target).IsCasted())
                            {
                                return;
                            }
                        }
                        else if (Q2.Casting(target, false, CollisionableObjects.YasuoWall).IsCasted())
                        {
                            return;
                        }
                    }
                }
            }
            if (MainMenu["EMenu"]["KSE"] && E.IsReady())
            {
                var canQ = MainMenu["QMenu"]["KSQ"] && Q.IsReady(50);
                var target =
                    Variables.TargetSelector.GetTargets(E.Range, E.DamageType)
                        .FirstOrDefault(
                            i =>
                            !HaveE(i)
                            && (canQ && Q3.WillHit(Q3.GetPredPosition(i), GetPosAfterDash(i))
                                    ? i.Health - Math.Max(GetEDmg(i) - i.MagicalShield, 0) + i.PhysicalShield
                                      <= GetQDmg(i)
                                    : i.Health + i.MagicalShield <= GetEDmg(i)));
                if (target != null && E.CastOnUnit(target))
                {
                    return;
                }
            }
            if (MainMenu["RMenu"]["KSR"] && R.IsReady())
            {
                var target =
                    Variables.TargetSelector.GetTargets(R.Range, R.DamageType)
                        .Where(
                            i =>
                            HaveR(i) && MainMenu["RMenu"]["RCast" + i.ChampionName]
                            && i.Health + i.PhysicalShield <= R.GetDamage(i) + (Q.IsReady(1000) ? GetQDmg(i) : 0))
                        .MaxOrDefault(i => new Priority().GetDefaultPriority(i));
                if (target != null)
                {
                    R.CastOnUnit(target);
                }
            }
        }

        private static void LaneClear()
        {
            if (MainMenu["EMenu"]["LHE"] && E.IsReady())
            {
                var minions =
                    Common.ListMinions()
                        .Where(
                            i =>
                            i.IsValidTarget(E.Range) && !HaveE(i)
                            && (MainMenu["EMenu"]["LCETower"] || !GetPosAfterDash(i).IsUnderEnemyTurret())
                            && Evade.IsSafePoint(GetPosAfterDash(i).ToVector2()).IsSafe)
                        .OrderByDescending(i => i.MaxHealth)
                        .ToList();
                if (minions.Count > 0)
                {
                    var minion = minions.FirstOrDefault(i => E.CanLastHit(i, GetEDmg(i)));
                    if (MainMenu["QMenu"]["LCQ"] && minion == null && Q.IsReady(50)
                        && (!haveQ3 || MainMenu["QMenu"]["LCQ3"]))
                    {
                        var sub = new List<Obj_AI_Minion>();
                        foreach (var mob in minions)
                        {
                            if ((E2.CanLastHit(mob, GetQDmg(mob), GetEDmg(mob)) || mob.Team == GameObjectTeam.Neutral)
                                && mob.Distance(GetPosAfterDash(mob)) < Q3.Width)
                            {
                                sub.Add(mob);
                            }
                            if (MainMenu["EMenu"]["LCELastHit"])
                            {
                                continue;
                            }
                            var nearMinion =
                                Common.ListMinions()
                                    .Where(i => i.IsValidTarget(Q3.Width, true, GetPosAfterDash(mob)))
                                    .ToList();
                            if (nearMinion.Count > 2 || nearMinion.Count(i => mob.Health <= GetQDmg(mob)) > 1)
                            {
                                sub.Add(mob);
                            }
                        }
                        minion = sub.FirstOrDefault();
                    }
                    if (minion != null && E.CastOnUnit(minion))
                    {
                        return;
                    }
                }
            }
            if (MainMenu["QMenu"]["LCQ"] && Q.IsReady() && (!haveQ3 || MainMenu["QMenu"]["LCQ3"]))
            {
                if (IsDashing)
                {
                    if (CanCastQCir)
                    {
                        var minions = GetQCirObj.Where(i => i is Obj_AI_Minion).ToList();
                        if (minions.Any(i => i.Health <= GetQDmg(i) || i.Team == GameObjectTeam.Neutral)
                            || minions.Count > 2)
                        {
                            CastQCir(minions);
                        }
                    }
                }
                else
                {
                    var minions =
                        Common.ListMinions()
                            .Where(i => !haveQ3 ? IsInRangeQ(i) : i.IsValidTarget(Q2.Range - i.BoundingRadius / 2))
                            .OrderByDescending(i => i.MaxHealth)
                            .ToList();
                    if (minions.Count == 0)
                    {
                        return;
                    }
                    if (!haveQ3)
                    {
                        var minion = minions.FirstOrDefault(i => Q.CanLastHit(i, GetQDmg(i)));
                        if (minion != null)
                        {
                            Q.Casting(minion);
                        }
                        else
                        {
                            var pos = Q.GetLineFarmLocation(minions);
                            if (pos.MinionsHit > 0)
                            {
                                Q.Cast(pos.Position);
                            }
                        }
                    }
                    else
                    {
                        var pos = Q2.GetLineFarmLocation(minions);
                        if (pos.MinionsHit > 0)
                        {
                            Q2.Cast(pos.Position);
                        }
                    }
                }
            }
        }

        private static void LastHit()
        {
            if (MainMenu["QMenu"]["LHQ"] && Q.IsReady() && !IsDashing && (!haveQ3 || MainMenu["QMenu"]["LHQ3"]))
            {
                if (!haveQ3)
                {
                    var minion =
                        GameObjects.EnemyMinions.Where(
                            i => (i.IsMinion() || i.IsPet(false)) && IsInRangeQ(i) && Q.CanLastHit(i, GetQDmg(i)))
                            .MaxOrDefault(i => i.MaxHealth);
                    if (minion != null && Q.Casting(minion).IsCasted())
                    {
                        return;
                    }
                }
                else
                {
                    var minion =
                        GameObjects.EnemyMinions.Where(
                            i =>
                            (i.IsMinion() || i.IsPet(false)) && i.IsValidTarget(Q2.Range - i.BoundingRadius / 2)
                            && Q2.CanLastHit(i, GetQDmg(i))).MaxOrDefault(i => i.MaxHealth);
                    if (minion != null && Q2.Casting(minion, false, CollisionableObjects.YasuoWall).IsCasted())
                    {
                        return;
                    }
                }
            }
            if (MainMenu["EMenu"]["LHE"] && E.IsReady() && !Player.Spellbook.IsAutoAttacking)
            {
                var minion =
                    GameObjects.EnemyMinions.Where(
                        i =>
                        (i.IsMinion() || i.IsPet(false)) && i.IsValidTarget(E.Range) && !HaveE(i)
                        && E.CanLastHit(i, GetEDmg(i)) && Evade.IsSafePoint(GetPosAfterDash(i).ToVector2()).IsSafe
                        && (MainMenu["EMenu"]["LHETower"] || !GetPosAfterDash(i).IsUnderEnemyTurret()))
                        .MaxOrDefault(i => i.MaxHealth);
                if (minion != null)
                {
                    E.CastOnUnit(minion);
                }
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }
            if (MainMenu["Draw"]["UseR"] && R.Level > 0)
            {
                var menu = MainMenu["RMenu"]["R"].GetValue<MenuKeyBind>();
                var pos = Drawing.WorldToScreen(Player.Position);
                var text = $"Use R In Combo: {(menu.Active ? "On" : "Off")} [{menu.Key}]";
                Drawing.DrawText(
                    pos.X - (float)Drawing.GetTextExtent(text).Width / 2,
                    pos.Y + 40,
                    menu.Active ? Color.White : Color.Gray,
                    text);
            }
            if (MainMenu["Draw"]["StackQ"] && Q.Level > 0)
            {
                var menu = MainMenu["QMenu"]["StackQ"].GetValue<MenuKeyBind>();
                var text =
                    $"Auto Stack Q: {(menu.Active ? (haveQ3 ? "Full" : (Q.IsReady() ? "Ready" : "Not Ready")) : "Off")} [{menu.Key}]";
                var pos = Drawing.WorldToScreen(Player.Position);
                Drawing.DrawText(
                    pos.X - (float)Drawing.GetTextExtent(text).Width / 2,
                    pos.Y + 20,
                    menu.Active ? Color.White : Color.Gray,
                    text);
            }
        }

        private static void OnEndScene(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }
            if (MainMenu["Draw"]["Q"] && Q.Level > 0)
            {
                Render.Circle.DrawCircle(
                    Player.Position,
                    IsDashing ? Q3.Width : SpellQ.Range,
                    Q.IsReady() ? Color.LimeGreen : Color.IndianRed);
            }
            if (MainMenu["Draw"]["E"] && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.LimeGreen : Color.IndianRed);
            }
            if (MainMenu["Draw"]["R"] && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.LimeGreen : Color.IndianRed);
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead || MenuGUI.IsChatOpen || MenuGUI.IsShopOpen || Player.IsRecalling())
            {
                return;
            }
            KillSteal();
            switch (Variables.Orbwalker.ActiveMode)
            {
                case OrbwalkingMode.Combo:
                    Combo();
                    break;
                case OrbwalkingMode.Hybrid:
                    Hybrid();
                    break;
                case OrbwalkingMode.LaneClear:
                    LaneClear();
                    break;
                case OrbwalkingMode.LastHit:
                    LastHit();
                    break;
                case OrbwalkingMode.None:
                    if (MainMenu["EMenu"]["FleeE"].GetValue<MenuKeyBind>().Active)
                    {
                        Variables.Orbwalker.Move(Game.CursorPos);
                        Flee();
                    }
                    else if (MainMenu["EQ3Flash"].GetValue<MenuKeyBind>().Active)
                    {
                        Variables.Orbwalker.Move(Game.CursorPos);
                        BeyBlade();
                    }
                    break;
            }
            if (Variables.Orbwalker.ActiveMode != OrbwalkingMode.Combo
                && Variables.Orbwalker.ActiveMode != OrbwalkingMode.Hybrid
                && !MainMenu["EQ3Flash"].GetValue<MenuKeyBind>().Active)
            {
                AutoQ();
            }
            if (MainMenu["QMenu"]["StackQ"].GetValue<MenuKeyBind>().Active
                && !MainMenu["EMenu"]["FleeE"].GetValue<MenuKeyBind>().Active)
            {
                StackQ();
            }
        }

        private static void StackQ()
        {
            if (!Q.IsReady() || haveQ3 || IsDashing)
            {
                return;
            }
            var state = Q.CastingBestTarget(true);
            if (state.IsCasted() || state != CastStates.InvalidTarget)
            {
                return;
            }
            var minions = Common.ListMinions().Where(IsInRangeQ).OrderByDescending(i => i.MaxHealth).ToList();
            if (minions.Count == 0)
            {
                return;
            }
            var minion = minions.FirstOrDefault(i => Q.CanLastHit(i, GetQDmg(i))) ?? minions.FirstOrDefault();
            if (minion == null)
            {
                return;
            }
            Q.Casting(minion);
        }

        private static void UseItem(Obj_AI_Hero target)
        {
            if (target != null && (target.HealthPercent < 40 || Player.HealthPercent < 50))
            {
                if (Bilgewater.IsReady)
                {
                    Bilgewater.Cast(target);
                }
                if (BotRuinedKing.IsReady)
                {
                    BotRuinedKing.Cast(target);
                }
            }
            if (Youmuu.IsReady && Player.CountEnemyHeroesInRange(Q.Range + E.Range) > 0)
            {
                Youmuu.Cast();
            }
            if (Tiamat.IsReady && Player.CountEnemyHeroesInRange(Tiamat.Range) > 0)
            {
                Tiamat.Cast();
            }
            if (Hydra.IsReady && Player.CountEnemyHeroesInRange(Hydra.Range) > 0)
            {
                Hydra.Cast();
            }
            if (Titanic.IsReady && !Player.Spellbook.IsAutoAttacking && Variables.Orbwalker.GetTarget() != null)
            {
                Titanic.Cast();
            }
        }

        private static class EvadeTarget
        {
            private static readonly List<Targets> DetectedTargets = new List<Targets>();
            private static readonly List<SpellData> Spells = new List<SpellData>();

            internal static void Init()
            {
                LoadSpellData();
                var evadeMenu = MainMenu.Add(new Menu("EvadeTarget", "Evade Target"));
                {
                    evadeMenu.Bool("W", "Use W");
                    var aaMenu = new Menu("AA", "Auto Attack");
                    {
                        aaMenu.Bool("B", "Basic Attack");
                        aaMenu.Slider("BHpU", "-> If Hp < (%)", 35);
                        aaMenu.Bool("C", "Crit Attack");
                        aaMenu.Slider("CHpU", "-> If Hp < (%)", 40);
                        evadeMenu.Add(aaMenu);
                    }
                    foreach (var hero in
                        GameObjects.EnemyHeroes.Where(
                            i =>
                            Spells.Any(
                                a =>
                                string.Equals(
                                    a.ChampionName,
                                    i.ChampionName,
                                    StringComparison.InvariantCultureIgnoreCase))))
                    {
                        evadeMenu.Add(new Menu(hero.ChampionName.ToLowerInvariant(), "-> " + hero.ChampionName));
                    }
                    foreach (var spell in
                        Spells.Where(
                            i =>
                            GameObjects.EnemyHeroes.Any(
                                a =>
                                string.Equals(
                                    a.ChampionName,
                                    i.ChampionName,
                                    StringComparison.InvariantCultureIgnoreCase))))
                    {
                        ((Menu)evadeMenu[spell.ChampionName.ToLowerInvariant()]).Bool(
                            spell.MissileName,
                            spell.MissileName + " (" + spell.Slot + ")",
                            false);
                    }
                }
                Game.OnUpdate += OnUpdateTarget;
                GameObjectNotifier<MissileClient>.OnCreate += EvadeTargetOnCreate;
                GameObjectNotifier<MissileClient>.OnDelete += EvadeTargetOnDelete;
            }

            private static void EvadeTargetOnCreate(object sender, MissileClient missile)
            {
                var caster = missile.SpellCaster as Obj_AI_Hero;
                if (caster == null || !caster.IsValid || caster.Team == Player.Team || !missile.Target.IsMe)
                {
                    return;
                }
                var spellData =
                    Spells.FirstOrDefault(
                        i =>
                        i.SpellNames.Contains(missile.SData.Name.ToLower())
                        && MainMenu["EvadeTarget"][i.ChampionName.ToLowerInvariant()][i.MissileName]);
                if (spellData == null && AutoAttack.IsAutoAttack(missile.SData.Name)
                    && (!missile.SData.Name.ToLower().Contains("crit")
                            ? MainMenu["EvadeTarget"]["AA"]["B"]
                              && Player.HealthPercent < MainMenu["EvadeTarget"]["AA"]["BHpU"]
                            : MainMenu["EvadeTarget"]["AA"]["C"]
                              && Player.HealthPercent < MainMenu["EvadeTarget"]["AA"]["CHpU"]))
                {
                    spellData = new SpellData
                                    { ChampionName = caster.ChampionName, SpellNames = new[] { missile.SData.Name } };
                }
                if (spellData == null)
                {
                    return;
                }
                DetectedTargets.Add(new Targets { Start = caster.ServerPosition, Obj = missile });
            }

            private static void EvadeTargetOnDelete(object sender, MissileClient missile)
            {
                var caster = missile.SpellCaster as Obj_AI_Hero;
                if (caster == null || !caster.IsValid || caster.Team == Player.Team)
                {
                    return;
                }
                DetectedTargets.RemoveAll(i => i.Obj.Compare(missile));
            }

            private static void LoadSpellData()
            {
                Spells.Add(
                    new SpellData { ChampionName = "Akali", SpellNames = new[] { "akalimota" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData { ChampionName = "Anivia", SpellNames = new[] { "frostbite" }, Slot = SpellSlot.E });
                Spells.Add(
                    new SpellData { ChampionName = "Annie", SpellNames = new[] { "disintegrate" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData
                        {
                            ChampionName = "Brand", SpellNames = new[] { "brandwildfire", "brandwildfiremissile" },
                            Slot = SpellSlot.R
                        });
                Spells.Add(
                    new SpellData
                        {
                            ChampionName = "Caitlyn", SpellNames = new[] { "caitlynaceintheholemissile" },
                            Slot = SpellSlot.R
                        });
                Spells.Add(
                    new SpellData
                        { ChampionName = "Cassiopeia", SpellNames = new[] { "cassiopeiatwinfang" }, Slot = SpellSlot.E });
                Spells.Add(
                    new SpellData { ChampionName = "Elise", SpellNames = new[] { "elisehumanq" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData
                        {
                            ChampionName = "FiddleSticks",
                            SpellNames = new[] { "fiddlesticksdarkwind", "fiddlesticksdarkwindmissile" },
                            Slot = SpellSlot.E
                        });
                Spells.Add(
                    new SpellData { ChampionName = "Gangplank", SpellNames = new[] { "parley" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData { ChampionName = "Janna", SpellNames = new[] { "sowthewind" }, Slot = SpellSlot.W });
                Spells.Add(
                    new SpellData { ChampionName = "Kassadin", SpellNames = new[] { "nulllance" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData
                        {
                            ChampionName = "Katarina", SpellNames = new[] { "katarinaq", "katarinaqmis" },
                            Slot = SpellSlot.Q
                        });
                Spells.Add(
                    new SpellData
                        { ChampionName = "Kayle", SpellNames = new[] { "judicatorreckoning" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData
                        {
                            ChampionName = "Leblanc", SpellNames = new[] { "leblancchaosorb", "leblancchaosorbm" },
                            Slot = SpellSlot.Q
                        });
                Spells.Add(new SpellData { ChampionName = "Lulu", SpellNames = new[] { "luluw" }, Slot = SpellSlot.W });
                Spells.Add(
                    new SpellData
                        { ChampionName = "Malphite", SpellNames = new[] { "seismicshard" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData
                        {
                            ChampionName = "MissFortune",
                            SpellNames = new[] { "missfortunericochetshot", "missFortunershotextra" }, Slot = SpellSlot.Q
                        });
                Spells.Add(
                    new SpellData
                        {
                            ChampionName = "Nami", SpellNames = new[] { "namiwenemy", "namiwmissileenemy" },
                            Slot = SpellSlot.W
                        });
                Spells.Add(
                    new SpellData { ChampionName = "Nunu", SpellNames = new[] { "iceblast" }, Slot = SpellSlot.E });
                Spells.Add(
                    new SpellData { ChampionName = "Pantheon", SpellNames = new[] { "pantheonq" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData
                        {
                            ChampionName = "Ryze", SpellNames = new[] { "spellflux", "spellfluxmissile" },
                            Slot = SpellSlot.E
                        });
                Spells.Add(
                    new SpellData { ChampionName = "Shaco", SpellNames = new[] { "twoshivpoison" }, Slot = SpellSlot.E });
                Spells.Add(
                    new SpellData { ChampionName = "Sona", SpellNames = new[] { "sonaqmissile" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData { ChampionName = "Swain", SpellNames = new[] { "swaintorment" }, Slot = SpellSlot.E });
                Spells.Add(
                    new SpellData { ChampionName = "Syndra", SpellNames = new[] { "syndrar" }, Slot = SpellSlot.R });
                Spells.Add(
                    new SpellData { ChampionName = "Teemo", SpellNames = new[] { "blindingdart" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData
                        { ChampionName = "Tristana", SpellNames = new[] { "detonatingshot" }, Slot = SpellSlot.E });
                Spells.Add(
                    new SpellData
                        { ChampionName = "TwistedFate", SpellNames = new[] { "bluecardattack" }, Slot = SpellSlot.W });
                Spells.Add(
                    new SpellData
                        { ChampionName = "TwistedFate", SpellNames = new[] { "goldcardattack" }, Slot = SpellSlot.W });
                Spells.Add(
                    new SpellData
                        { ChampionName = "TwistedFate", SpellNames = new[] { "redcardattack" }, Slot = SpellSlot.W });
                Spells.Add(
                    new SpellData
                        { ChampionName = "Vayne", SpellNames = new[] { "vaynecondemnmissile" }, Slot = SpellSlot.E });
                Spells.Add(
                    new SpellData
                        { ChampionName = "Veigar", SpellNames = new[] { "veigarprimordialburst" }, Slot = SpellSlot.R });
                Spells.Add(
                    new SpellData
                        { ChampionName = "Viktor", SpellNames = new[] { "viktorpowertransfer" }, Slot = SpellSlot.Q });
            }

            private static void OnUpdateTarget(EventArgs args)
            {
                if (Player.IsDead)
                {
                    return;
                }
                if (Player.HasBuffOfType(BuffType.SpellShield) || Player.HasBuffOfType(BuffType.SpellImmunity))
                {
                    return;
                }
                if (!MainMenu["EvadeTarget"]["W"] || !W.IsReady() || DetectedTargets.Count == 0)
                {
                    return;
                }
                foreach (var missile in DetectedTargets.OrderBy(i => i.Obj.Distance(Player)))
                {
                    if (Player.Distance(missile.Obj) < 450)
                    {
                        W.Cast(Player.ServerPosition.Extend(missile.Start, 100));
                    }
                }
            }


            private class SpellData
            {
                public string ChampionName;
                public SpellSlot Slot;
                public string[] SpellNames = { };

                public string MissileName => this.SpellNames.First();
            }

            private class Targets
            {
                public MissileClient Obj;
                public Vector3 Start;
            }
        }
    }
}