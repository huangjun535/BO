namespace YuLeVladimir_Rework.Plugin
{
    using LeagueSharp;
    using LeagueSharp.Data.Enumerations;
    using LeagueSharp.SDK;
    using LeagueSharp.SDK.Enumerations;
    using LeagueSharp.SDK.UI;
    using LeagueSharp.SDK.Utils;
    using SharpDX;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;
    using YuLeVladimir_Rework.Core;
    using YuLeVladimir_Rework.Evade;
    using Color = System.Drawing.Color;
    using Menu = LeagueSharp.SDK.UI.Menu;

    internal class Vladimir : Program
    {
        private static BuffInstance buffE;

        private static bool haveQ, haveQEmp, haveW, haveE;

        private static int lastECharge;

        public Vladimir()
        {
            Q = new Spell(SpellSlot.Q, 600).SetTargetted(0.25f, float.MaxValue);
            W = new Spell(SpellSlot.W, 350);
            E = new Spell(SpellSlot.E, 630).SetSkillshot(0, 40, 4000, false, SkillshotType.SkillshotLine);
            R = new Spell(SpellSlot.R, 700).SetSkillshot(
                0.001f,
                375,
                float.MaxValue,
                false,
                SkillshotType.SkillshotCircle);
            Q.DamageType = W.DamageType = E.DamageType = R.DamageType = DamageType.Magical;
            R.MinHitChance = HitChance.VeryHigh;

            var QMenu = MainMenu.Add(new Menu("QMenu", "Q 设置"));
            {
                QMenu.Separator("连招设置");
                QMenu.Separator("连招总是使用");
                QMenu.Separator("骚扰设置");
                QMenu.Separator("骚扰总是使用");
                QMenu.Bool("HybridQLastHit", "使用Q 补刀(被动 < 2)");
                QMenu.Separator("清线清野");
                QMenu.Separator("清线清野总是使用");
                QMenu.Separator("补刀设置");
                QMenu.Bool("LastHitQ", "使用Q");
                QMenu.Separator("击杀设置");
                QMenu.Bool("KillStealQ", "使用Q");
            }

            var WMenu = MainMenu.Add(new Menu("WMenu", "W 设置"));
            {
                WMenu.KeyBind("FleeW", "使用W逃跑", Keys.Z);
            }

            var EMenu = MainMenu.Add(new Menu("EMenu", "E 设置"));
            {
                EMenu.Separator("连招设置");
                EMenu.Bool("CBE", "使用E");
                EMenu.Separator("骚扰设置");
                EMenu.Bool("HybridE", "使用E", false);
                EMenu.Separator("清线清野");
                EMenu.Bool("LCE", "使用E");
                EMenu.Separator("击杀设置");
                EMenu.Bool("KillStealE", "使用E");
            }

            var RMenu = MainMenu.Add(new Menu("RMenu", "R 设置"));
            {
                RMenu.Separator("连招设置");
                RMenu.Bool("R", "使用R");
                RMenu.Slider("RHpU", "敌人血量 < (%) 和最少命中数 >= 2", 60);
                RMenu.Slider("RCountA", "或者最少命中敌人数 >=", 3, 1, 5);
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
            }

            var DrawMenu = MainMenu.Add(new Menu("Draw", "显示设置"));
            {
                DrawMenu.Bool("Q", "Q 范围", false);
                DrawMenu.Bool("E", "E 范围", false);
                DrawMenu.Bool("R", "R 范围", false);
            }

            Game.OnUpdate += OnUpdate;
            Drawing.OnEndScene += OnEndScene;
            Variables.Orbwalker.OnAction += (sender, args) =>
                {
                    if (!Q.IsReady() || args.Type != OrbwalkingType.BeforeAttack)
                    {
                        return;
                    }
                    var mode = Variables.Orbwalker.ActiveMode;
                    var hero = args.Target as Obj_AI_Hero;
                    if (hero == null || (mode != OrbwalkingMode.Combo && mode != OrbwalkingMode.Hybrid))
                    {
                        return;
                    }
                    args.Process = !Q.IsInRange(hero);
                };
            Obj_AI_Base.OnBuffAdd += (sender, args) =>
                {
                    if (!sender.IsMe)
                    {
                        return;
                    }
                    switch (args.Buff.DisplayName)
                    {
                        case "VladimirQFrenzy":
                            haveQEmp = true;
                            break;
                        case "VladimirSanguinePool":
                            haveW = true;
                            break;
                        case "VladimirE":
                            haveE = true;
                            buffE = args.Buff;
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
                        case "VladimirQFrenzy":
                            haveQEmp = false;
                            break;
                        case "VladimirSanguinePool":
                            haveW = false;
                            break;
                        case "VladimirE":
                            haveE = false;
                            buffE = null;
                            break;
                    }
                };
            GameObjectNotifier<MissileClient>.OnCreate += (sender, args) =>
                {
                    var spellCaster = args.SpellCaster as Obj_AI_Hero;
                    if (spellCaster == null || !spellCaster.IsMe || args.SData.Name != "VladimirEMissile" || !haveE)
                    {
                        return;
                    }
                    haveE = false;
                    buffE = null;
                };
            Obj_AI_Base.OnProcessSpellCast += (sender, args) =>
                {
                    if (!sender.IsMe)
                    {
                        return;
                    }
                    switch (args.SData.Name)
                    {
                        case "VladimirQ":
                            haveQ = true;
                            break;
                        case "VladimirE":
                            if (buffE == null && !haveE)
                            {
                                haveE = true;
                            }
                            break;
                    }
                };
            Spellbook.OnUpdateChargedSpell += (sender, args) =>
                {
                    if (!sender.Owner.IsMe || Variables.TickCount - lastECharge >= 3000 || !args.ReleaseCast)
                    {
                        return;
                    }
                    args.Process = false;
                };
            Spellbook.OnCastSpell += (sender, args) =>
                {
                    if (args.Slot != E.Slot || Variables.TickCount - lastECharge <= 500 || IsChargeE)
                    {
                        return;
                    }
                    ERelease();
                };
            Obj_AI_Base.OnDoCast += (sender, args) =>
                {
                    if (!sender.IsMe || args.Slot != Q.Slot || !haveQ)
                    {
                        return;
                    }
                    haveQ = false;
                };
        }

        private static bool CanCastE => E.IsReady() || haveE;

        private static List<Tuple<Obj_AI_Hero, Vector3>> GetETarget
            =>
                Variables.TargetSelector.GetTargets(E.Range + 20, E.DamageType)
                    .Select(i => new Tuple<Obj_AI_Hero, Vector3>(i, E.GetPredPosition(i)))
                    .Where(i => CanHitE(i.Item2))
                    .ToList();

        private static bool IsChargeE => buffE != null || haveE || Variables.TickCount - lastECharge <= 100;

        private static bool IsEmpQ => Player.Mana >= (haveQEmp ? 0.2f : 1.9f);

        private static bool CanHitE(Vector3 point)
        {
            for (var i = 0; i < 360; i += 18)
            {
                var pos = E.From.ToVector2() + E.Range * new Vector2(1, 0).Rotated((float)(Math.PI * i / 180.0));
                if (E.WillHit(point, pos.ToVector3()))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool CanReleaseE(double time)
        {
            return haveE && buffE != null && Game.Time - buffE.StartTime >= time - 0.02;
        }

        private static void Combo()
        {
            if (MainMenu["RMenu"]["R"] && R.IsReady())
            {
                var targets = Variables.TargetSelector.GetTargets(R.Range + 50, R.DamageType);
                if (targets.Count > 0)
                {
                    var hit = 0;
                    var posCast = new Vector3();
                    foreach (var target in targets)
                    {
                        var pred = R.GetPrediction(target);
                        if (pred.Hitchance < R.MinHitChance)
                        {
                            continue;
                        }
                        var hits =
                            GameObjects.EnemyHeroes.Where(
                                i =>
                                i.IsValidTarget() && !i.Compare(target)
                                && R.WillHit(R.GetPredPosition(i), pred.CastPosition)).ToList();
                        hits.Add(target);
                        if (hits.Count <= hit)
                        {
                            continue;
                        }
                        if ((hits.Count > 1
                             && (hits.Any(i => i.Health + i.MagicalShield <= R.GetDamage(i))
                                 || hits.Sum(i => i.HealthPercent) / hits.Count < MainMenu["RMenu"]["RHpU"]))
                            || hits.Count >= MainMenu["RMenu"]["RCountA"] || Player.HealthPercent <= 30)
                        {
                            hit = hits.Count;
                            posCast = pred.CastPosition;
                        }
                    }
                    if (posCast.IsValid() && R.Cast(posCast))
                    {
                        return;
                    }
                }
            }
            if (MainMenu["EMenu"]["CBE"] && CanCastE)
            {
                var canE = GetETarget.Count > 0;
                if (E.IsReady())
                {
                    ECharge(canE);
                }
                else if (CanReleaseE(1) && (canE || Player.CountAllyHeroesInRange(900) == 0))
                {
                    ERelease();
                }
            }
            if ((!IsChargeE || (IsEmpQ && (CanReleaseE(0.7) || Player.HealthPercent <= 15)))
                && Q.CastOnBestTarget().IsCasted())
            {
                return;
            }
            var subTarget = Q.GetTarget(100);
            if (subTarget != null && MainMenu["MiscMenu"]["Ignite"] && Common.CanIgnite && subTarget.HealthPercent < 25
                && subTarget.DistanceToPlayer() <= IgniteRange)
            {
                Player.Spellbook.CastSpell(Ignite, subTarget);
            }
        }

        private static void ECharge(bool canCast)
        {
            if (!canCast || haveQ || IsChargeE || Variables.TickCount - lastECharge <= 400 + Game.Ping)
            {
                return;
            }
            Player.Spellbook.CastSpell(E.Slot, Player.Position);
            lastECharge = Variables.TickCount;
        }

        private static void ERelease()
        {
            if (haveQ)
            {
                return;
            }
            Player.Spellbook.UpdateChargedSpell(E.Slot, Player.Position, true, false);
            Player.Spellbook.CastSpell(E.Slot, Player.Position, false);
        }

        private static float GetEDmg(Obj_AI_Base target)
        {
            float minDmg = E.GetDamage(target), maxDmg = E.GetDamage(target, DamageStage.Empowered);
            return buffE != null
                       ? (Game.Time - buffE.StartTime >= 1
                              ? maxDmg
                              : minDmg + (Game.Time - buffE.StartTime) * (maxDmg - minDmg))
                       : (target.DistanceToPlayer() < 200 ? maxDmg : minDmg);
        }

        private static void Hybrid()
        {
            if (MainMenu["EMenu"]["HybridE"] && CanCastE)
            {
                var canE = GetETarget.Count > 0;
                if (E.IsReady())
                {
                    ECharge(canE);
                }
                else if (CanReleaseE(1) && (canE || Player.CountAllyHeroesInRange(750) == 0))
                {
                    ERelease();
                }
            }
            if ((!IsChargeE || IsEmpQ) && Q.CastOnBestTarget().IsCasted())
            {
                return;
            }
            if (MainMenu["QMenu"]["HybridQLastHit"] && Q.IsReady() && !IsEmpQ && (!MainMenu["EMenu"]["HybridE"] || !IsChargeE))
            {
                var minion =
                    GameObjects.EnemyMinions.Where(
                        i =>
                        (i.IsMinion() || i.IsPet(false)) && i.IsValidTarget(Q.Range) && Q.CanLastHit(i, Q.GetDamage(i)))
                        .MaxOrDefault(i => i.MaxHealth);
                if (minion != null)
                {
                    Q.CastOnUnit(minion);
                }
            }
        }

        private static void KillSteal()
        {
            if (MainMenu["QMenu"]["KillStealQ"] && Q.IsReady())
            {
                var target =
                    Variables.TargetSelector.GetTargets(Q.Range, Q.DamageType)
                        .FirstOrDefault(
                            i =>
                            i.Health + i.MagicalShield
                            <= Q.GetDamage(i, !IsEmpQ ? DamageStage.Default : DamageStage.Empowered));
                if (target != null && Q.CastOnUnit(target))
                {
                    return;
                }
            }
            if (MainMenu["EMenu"]["KillStealE"] && CanCastE
                && GetETarget.Any(i => i.Item1.Health + i.Item1.MagicalShield <= GetEDmg(i.Item1)))
            {
                if (E.IsReady())
                {
                    ECharge(true);
                }
                else if (IsChargeE)
                {
                    ERelease();
                }
            }
        }

        private static void LaneClear()
        {
            if (MainMenu["EMenu"]["LCE"] && CanCastE)
            {
                var canE =
                    Common.ListMinions()
                        .Where(
                            i =>
                            i.IsValidTarget(E.Range)
                            && !Q.CanLastHit(i, Q.GetDamage(i, !IsEmpQ ? DamageStage.Default : DamageStage.Empowered)))
                        .ToList()
                        .Count > 0;
                if (E.IsReady())
                {
                    ECharge(canE);
                }
                else if (CanReleaseE(1) && (canE || Common.ListMinions().Count(i => i.IsValidTarget(750)) == 0))
                {
                    ERelease();
                }
            }
            if ((!IsChargeE || (IsEmpQ && (CanReleaseE(1) || Player.HealthPercent <= 10))) && Q.IsReady())
            {
                var minions =
                    Common.ListMinions()
                        .Where(i => i.IsValidTarget(Q.Range))
                        .OrderByDescending(i => i.MaxHealth)
                        .ToList();
                if (minions.Count > 0)
                {
                    var minion =
                        minions.FirstOrDefault(
                            i => Q.CanLastHit(i, Q.GetDamage(i, !IsEmpQ ? DamageStage.Default : DamageStage.Empowered)))
                        ?? minions.FirstOrDefault();
                    if (minion != null)
                    {
                        Q.CastOnUnit(minion);
                    }
                }
            }
        }

        private static void LastHit()
        {
            if (!MainMenu["QMenu"]["LastHitQ"] || !Q.IsReady() || Player.Spellbook.IsAutoAttacking)
            {
                return;
            }
            var minion =
                GameObjects.EnemyMinions.Where(
                    i =>
                    (i.IsMinion() || i.IsPet(false)) && i.IsValidTarget(Q.Range)
                    && Q.CanLastHit(i, Q.GetDamage(i, !IsEmpQ ? DamageStage.Default : DamageStage.Empowered)))
                    .MaxOrDefault(i => i.MaxHealth);
            if (minion == null)
            {
                return;
            }
            Q.CastOnUnit(minion);
        }

        private static void OnEndScene(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }
            if (MainMenu["Draw"]["Q"] && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.LimeGreen : Color.IndianRed);
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
            if (Player.HasBuff("VladimirE") || Player.HasBuff("VladimirSanguinePool"))
            {
                Variables.Orbwalker.Move(Game.CursorPos);
                Variables.Orbwalker.AttackState = false;
            }
            else
            {
                Variables.Orbwalker.AttackState = true;
            }

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
                    if (MainMenu["WMenu"]["FleeW"].GetValue<MenuKeyBind>().Active)
                    {
                        Variables.Orbwalker.Move(Game.CursorPos);
                        if (W.IsReady() && !haveW)
                        {
                            W.Cast();
                        }
                    }
                    break;
            }
        }

        private static class EvadeTarget
        {
            private static readonly List<MissileClient> DetectedTargets = new List<MissileClient>();

            private static readonly List<SpellData> Spells = new List<SpellData>();

            internal static void Init()
            {
                LoadSpellData();
                var evadeMenu = MainMenu.Add(new Menu("EvadeTarget", "Evade Target"));
                {
                    evadeMenu.Bool("W", "Use W");
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
                if (spellData == null)
                {
                    return;
                }
                DetectedTargets.Add(missile);
            }

            private static void EvadeTargetOnDelete(object sender, MissileClient missile)
            {
                var caster = missile.SpellCaster as Obj_AI_Hero;
                if (caster == null || !caster.IsValid || caster.Team == Player.Team)
                {
                    return;
                }
                DetectedTargets.RemoveAll(i => i.Compare(missile));
            }

            private static void LoadSpellData()
            {
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
                        {
                            ChampionName = "FiddleSticks",
                            SpellNames = new[] { "fiddlesticksdarkwind", "fiddlesticksdarkwindmissile" },
                            Slot = SpellSlot.E
                        });
                Spells.Add(new SpellData { ChampionName = "Lulu", SpellNames = new[] { "luluw" }, Slot = SpellSlot.W });
                Spells.Add(
                    new SpellData { ChampionName = "Shaco", SpellNames = new[] { "twoshivpoison" }, Slot = SpellSlot.E });
                Spells.Add(
                    new SpellData { ChampionName = "Syndra", SpellNames = new[] { "syndrar" }, Slot = SpellSlot.R });
                Spells.Add(
                    new SpellData { ChampionName = "Teemo", SpellNames = new[] { "blindingdart" }, Slot = SpellSlot.Q });
                Spells.Add(
                    new SpellData
                        { ChampionName = "TwistedFate", SpellNames = new[] { "goldcardattack" }, Slot = SpellSlot.W });
                Spells.Add(
                    new SpellData
                        { ChampionName = "Vayne", SpellNames = new[] { "vaynecondemnmissile" }, Slot = SpellSlot.E });
                Spells.Add(
                    new SpellData
                        { ChampionName = "Veigar", SpellNames = new[] { "veigarprimordialburst" }, Slot = SpellSlot.R });
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
                foreach (var missile in DetectedTargets.OrderBy(i => i.Distance(Player)))
                {
                    if (Player.Distance(missile) < 150)
                    {
                        W.Cast();
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
        }
    }
}