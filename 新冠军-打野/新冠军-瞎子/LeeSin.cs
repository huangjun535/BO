namespace YuLeLeeSin_Rework.Plugin
{
    using LeagueSharp;
    using LeagueSharp.Data.Enumerations;
    using LeagueSharp.SDK;
    using LeagueSharp.SDK.Enumerations;
    using LeagueSharp.SDK.TSModes;
    using LeagueSharp.SDK.UI;
    using LeagueSharp.SDK.Utils;
    using SharpDX;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;
    using YuLeLeeSin_Rework.Core;
    using Color = System.Drawing.Color;
    using Menu = LeagueSharp.SDK.UI.Menu;


    internal class LeeSin : Program
    {
        private const int RKickRange = 750;
        private static readonly List<string> SpecialPet = new List<string> { "jarvanivstandard", "teemomushroom", "illaoiminion" };
        private static int cPassive;
        private static bool isDashing;
        private static int lastBubbaKush;
        private static int lastW, lastW2, lastE2, lastR;
        private static Obj_AI_Base objQ;
        private static Vector3 posBubbaKushFlash, posBubbaKushJump;
        private static string[] AutoEnableList =
        {
             "Annie", "Ahri", "Akali", "Anivia", "Annie", "Brand", "Cassiopeia", "Diana", "Evelynn", "FiddleSticks", "Fizz", "Gragas", "Heimerdinger", "Karthus",
             "Kassadin", "Katarina", "Kayle", "Kennen", "Leblanc", "Lissandra", "Lux", "Malzahar", "Mordekaiser", "Morgana", "Nidalee", "Orianna",
             "Ryze", "Sion", "Swain", "Syndra", "Teemo", "TwistedFate", "Veigar", "Viktor", "Vladimir", "Xerath", "Ziggs", "Zyra", "Velkoz", "Azir", "Ekko",
             "Ashe", "Caitlyn", "Corki", "Draven", "Ezreal", "Graves", "Jayce", "Jinx", "KogMaw", "Lucian", "MasterYi", "MissFortune", "Quinn", "Shaco", "Sivir",
             "Talon", "Tristana", "Twitch", "Urgot", "Varus", "Vayne", "Yasuo", "Zed", "Kindred", "AurelionSol"
        };

        public LeeSin()
        {
            Q = new Spell(SpellSlot.Q, 1100).SetSkillshot(0.25f, 60, 1800, true, SkillshotType.SkillshotLine);
            Q2 = new Spell(Q.Slot, 1300);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 425).SetTargetted(0.25f, float.MaxValue);
            E2 = new Spell(E.Slot, 570);
            R = new Spell(SpellSlot.R, 375).SetTargetted(0.25f, float.MaxValue);
            R2 = new Spell(R.Slot, RKickRange).SetSkillshot(R.Delay, 0, 1000, false, SkillshotType.SkillshotLine);
            Q.DamageType = Q2.DamageType = W.DamageType = R.DamageType = DamageType.Physical;
            E.DamageType = DamageType.Magical;
            Q.MinHitChance = HitChance.VeryHigh;

            WardManager.Init();
            Insec.Init();

            var QMenu = MainMenu.Add(new Menu("QMenu", "Q 设置"));
            {
                QMenu.Separator("连招设置");
                QMenu.Bool("CBQ", "使用Q1");
                QMenu.Bool("CBQ2", "使用Q2");
                QMenu.Bool("CBQ2Obj", "当差不多Q2要消失", false);
                QMenu.Bool("CBQCol", "惩戒Q");
                QMenu.Separator("清线清野");
                QMenu.Bool("LCQ", "使用Q");
                QMenu.Bool("LCQBig", "清野时候只Q大野怪");
                QMenu.Separator("补刀设置");
                QMenu.Bool("LHQ", "使用Q1");
                QMenu.Separator("击杀设置");
                QMenu.Bool("KSQ", "使用Q1");
                QMenu.Bool("KSQ2", "使用Q2");
                QMenu.Separator("回旋踢设置");
                QMenu.Bool("InsecQ", "使用Q");
                QMenu.Bool("InsecQCol", "惩戒Q");
                QMenu.Bool("InsecQObj", "Q目标附近的东西靠近");
            }

            var WMenu = MainMenu.Add(new Menu("WMenu", "W 设置"));
            {
                WMenu.Separator("连招设置");
                WMenu.Bool("CBW", "使用W1", false);
                WMenu.Bool("CBW2", "使用W2", false);
                WMenu.Separator("清线清野");
                WMenu.Bool("LCW", "使用W", false);
                WMenu.Separator("摸眼逃生");
                WMenu.KeyBind("FleeW", "摸眼按键", Keys.X);
            }
            
            var EMenu = MainMenu.Add(new Menu("EMenu", "E 设置"));
            {
                EMenu.Separator("连招设置");
                EMenu.Bool("CBE", "使用E1");
                EMenu.Bool("CBE2", "使用E2");
                EMenu.Separator("清线清野");
                EMenu.Bool("LCE", "使用E");
                EMenu.Separator("击杀设置");
                EMenu.Bool("KSE", "使用E1");
            }

            var RMenu = MainMenu.Add(new Menu("RMenu", "R 设置"));
            {
                RMenu.Separator("回旋踢设置");
                RMenu.KeyBind("InsecR", "回旋踢按键", Keys.T).ValueChanged += (sender, args) =>
                {
                    var keybind = sender as MenuKeyBind;
                    if (keybind != null)
                    {
                        Variables.Orbwalker.AttackState = !keybind.Active;
                    }
                };
                RMenu.Bool("InsecTargetSelect", "只踢左键选择的目标", false);
                RMenu.List("InsecMode", "Mode", new[] { "Tower/Hero/Current", "Mouse Position", "Current Position" });
                RMenu.Bool("InsecFlash", "Use Flash");
                RMenu.List("InsecFlashMode", "Flash Mode", new[] { "R-Flash", "Flash-R", "Both" });
                RMenu.Bool("InsecFlashJump", "Use WardJump To Gap For Flash");
                RMenu.Separator("北美第一瞎回旋踢");
                RMenu.KeyBind("BKR", "按键（R-闪）", Keys.XButton2);
                RMenu.List("BKRMode", "Mode", new[] { "闪现", "插眼", "闪现或插眼" });
                RMenu.Bool("BKRKill", "优先击杀敌人");
                RMenu.Slider("BKRCountA", "或者R飞数量 >=", 1, 1, 4);
                RMenu.Separator("自动击飞");
                RMenu.KeyBind("KUR", "开关", Keys.L, KeyBindType.Toggle);
                RMenu.Bool("KURKill", "If Kill Enemy Behind");
                RMenu.Slider("KURCountA", "Or Hit Enemy Behind >=", 1, 1, 4);
                RMenu.Separator("R闪按键");
                RMenu.KeyBind("RFlash", "R闪到鼠标位置", Keys.A);
                RMenu.Separator("击杀设置");
                RMenu.Bool("KSR", "Use R");
                if (GameObjects.EnemyHeroes.Any())
                {
                    GameObjects.EnemyHeroes.ForEach(i => RMenu.Bool("KSRCast" + i.ChampionName, i.ChampionName, AutoEnableList.Contains(i.ChampionName)));
                }
            }

            var StarMenu = MainMenu.Add(new Menu("Star", "明星连招"));
            {
                StarMenu.KeyBind("Star", "启动按键", Keys.X);
                StarMenu.Bool("StarKill", "当能击杀的时候自动启动", false);
                StarMenu.Bool("StarKillWJ", "明星连招时自动摸眼装逼", false);
            }

            SkinChance.Inject();

            var DrawMenu = MainMenu.Add(new Menu("Draw", "显示设置"));
            {
                DrawMenu.Bool("Q", "Q 范围", false);
                DrawMenu.Bool("W", "W 范围", false);
                DrawMenu.Bool("E", "E 范围", false);
                DrawMenu.Bool("R", "R 范围", false);
                DrawMenu.Bool("Damage", "连招伤害");
                DrawMenu.Bool("KnockUp", "自动击飞状态");
                DrawMenu.Bool("DLine", "显示回旋踢直线");
                DrawMenu.Bool("DWardFlash", "显示回旋踢摸眼闪现范围");
            }

            Game.OnUpdate += OnUpdate;
            Drawing.OnEndScene += OnEndScene;
            Drawing.OnDraw += OnDraw;
            Obj_AI_Base.OnBuffAdd += (sender, args) =>
                {
                    if (sender.IsMe)
                    {
                        switch (args.Buff.DisplayName)
                        {
                            case "BlindMonkFlurry":
                                cPassive = 2;
                                break;
                            case "BlindMonkQTwoDash":
                                isDashing = true;
                                break;
                        }
                    }
                    else if (sender.IsEnemy)
                    {
                        if (args.Buff.DisplayName == "BlindMonkSonicWave")
                        {
                            objQ = sender;
                        }
                        else if (args.Buff.Name == "blindmonkrroot" && Common.CanFlash)
                        {
                            CastRFlash((Obj_AI_Hero)sender);
                        }
                    }
                };
            Obj_AI_Base.OnBuffRemove += (sender, args) =>
                {
                    if (sender.IsMe)
                    {
                        switch (args.Buff.DisplayName)
                        {
                            case "BlindMonkFlurry":
                                cPassive = 0;
                                break;
                            case "BlindMonkQTwoDash":
                                isDashing = false;
                                break;
                        }
                    }
                    else if (sender.IsEnemy && args.Buff.DisplayName == "BlindMonkSonicWave")
                    {
                        objQ = null;
                    }
                };
            Obj_AI_Base.OnBuffUpdateCount += (sender, args) =>
                {
                    if (!sender.IsMe || args.Buff.DisplayName != "BlindMonkFlurry")
                    {
                        return;
                    }
                    cPassive = args.Buff.Count;
                };
            Obj_AI_Base.OnProcessSpellCast += (sender, args) =>
                {
                    if (!sender.IsMe)
                    {
                        return;
                    }
                    switch (args.Slot)
                    {
                        case SpellSlot.Q:
                            if (!args.SData.Name.ToLower().Contains("one"))
                            {
                                isDashing = true;
                            }
                            break;
                        case SpellSlot.W:
                            if (args.SData.Name.ToLower().Contains("one"))
                            {
                                lastW = Variables.TickCount;
                            }
                            else
                            {
                                lastW2 = Variables.TickCount;
                            }
                            break;
                        case SpellSlot.E:
                            if (!args.SData.Name.ToLower().Contains("one"))
                            {
                                lastE2 = Variables.TickCount;
                            }
                            break;
                        case SpellSlot.R:
                            lastR = Variables.TickCount;
                            break;
                    }
                    if (args.SData.Name == "SummonerFlash" && posBubbaKushFlash.IsValid())
                    {
                        posBubbaKushFlash = new Vector3();
                    }
                };
        }
        private static bool IsDashing => Variables.TickCount - lastW <= 100 || Player.IsDashing();

        private static bool IsEOne => E.Instance.SData.Name.ToLower().Contains("one");

        private static bool IsQOne => Q.Instance.SData.Name.ToLower().Contains("one");

        private static bool IsRecentR => Variables.TickCount - lastR < 2500;

        private static bool IsWOne => W.Instance.SData.Name.ToLower().Contains("one");

        private static void AutoKnockUp()
        {
            if (!R.IsReady())
            {
                return;
            }
            var multi = GetMultiHit(MainMenu["RMenu"]["KURKill"], MainMenu["RMenu"]["KURCountA"], 0);
            if (multi.Item1 != null && multi.Item2 != 0 && !multi.Item3.IsValid())
            {
                R.CastOnUnit(multi.Item1);
            }
        }

        private static void BubbaKush()
        {
            if (!R.IsReady())
            {
                return;
            }
            var isKill = MainMenu["RMenu"]["BKRKill"].GetValue<MenuBool>().Value;
            var minHit = MainMenu["RMenu"]["BKRCountA"].GetValue<MenuSlider>().Value;
            var multi = GetMultiHit(isKill, minHit, 0);
            if (multi.Item1 != null && multi.Item2 != 0 && !multi.Item3.IsValid() && R.CastOnUnit(multi.Item1))
            {
                return;
            }
            if (Variables.TickCount - lastBubbaKush <= 1000)
            {
                if (posBubbaKushJump.IsValid() && posBubbaKushJump.DistanceToPlayer() < 100)
                {
                    var targetSelect = Variables.TargetSelector.GetSelectedTarget();
                    if (targetSelect.IsValidTarget())
                    {
                        R.CastOnUnit(targetSelect);
                    }
                }
                return;
            }
            posBubbaKushFlash = posBubbaKushJump = new Vector3();
            Variables.TargetSelector.SetTarget(null);
            var mode = MainMenu["RMenu"]["BKRMode"].GetValue<MenuList>().Index;
            if (mode != 0 && WardManager.CanWardJump)
            {
                multi = GetMultiHit(isKill, minHit, 2);
                if (multi.Item1 != null && multi.Item2 != 0 && multi.Item3.IsValid())
                {
                    posBubbaKushJump = multi.Item3;
                    lastBubbaKush = Variables.TickCount;
                    Variables.TargetSelector.SetTarget(multi.Item1);
                    WardManager.Place(posBubbaKushJump);
                    return;
                }
            }
            if (mode != 1 && Common.CanFlash)
            {
                multi = GetMultiHit(isKill, minHit, 1);
                if (multi.Item1 != null && multi.Item2 != 0 && multi.Item3.IsValid() && R.CastOnUnit(multi.Item1))
                {
                    posBubbaKushFlash = multi.Item3;
                    lastBubbaKush = Variables.TickCount;
                    Variables.TargetSelector.SetTarget(multi.Item1);
                }
            }
        }

        private static bool CanE2(Obj_AI_Base target)
        {
            var buff = target.GetBuff("BlindMonkTempest");
            return buff != null && buff.EndTime - Game.Time < 0.25 * (buff.EndTime - buff.StartTime);
        }

        private static bool CanQ2(Obj_AI_Base target)
        {
            var buff = target.GetBuff("BlindMonkSonicWave");
            return buff != null && buff.EndTime - Game.Time < 0.25 * (buff.EndTime - buff.StartTime);
        }

        private static bool CanR(Obj_AI_Hero target)
        {
            var buff = target.GetBuff("BlindMonkDragonsRage");
            return buff != null && buff.EndTime - Game.Time <= 0.75 * (buff.EndTime - buff.StartTime);
        }

        private static void CastE(List<Obj_AI_Minion> minions = null)
        {
            if (!E.IsReady() || isDashing || IsDashing || Variables.TickCount - lastW <= 200
                || Variables.TickCount - lastW2 <= 250 || Player.Spellbook.IsCastingSpell)
            {
                return;
            }
            if (minions == null)
            {
                CastECombo();
            }
            else
            {
                CastELaneClear(minions);
            }
        }

        private static void CastECombo()
        {
            if (IsEOne)
            {
                var target =
                    Variables.TargetSelector.GetTargets(E.Range + 20, E.DamageType)
                        .Where(i => E.CanHitCircle(i))
                        .ToList();
                if (target.Count == 0)
                {
                    return;
                }
                if ((cPassive == 0 && Player.Mana >= 70) || target.Count > 2
                    || (Variables.Orbwalker.GetTarget() == null
                            ? target.Any(i => i.DistanceToPlayer() > Player.GetRealAutoAttackRange() + 100)
                            : cPassive < 2))
                {
                    E.Cast();
                }
            }
            else if (MainMenu["EMenu"]["CBE2"])
            {
                var target = GameObjects.EnemyHeroes.Where(i => i.IsValidTarget(E2.Range) && HaveE(i)).ToList();
                if (target.Count == 0)
                {
                    return;
                }
                if (cPassive == 0 || target.Count > 2
                    || target.Any(i => CanE2(i) || i.DistanceToPlayer() > i.GetRealAutoAttackRange() + 50))
                {
                    E2.Cast();
                }
            }
        }

        /// <summary>
        /// Get Damage
        /// </summary>
        /// <param name="Target">Target</param>
        /// <param name="CalCulateAttackDamage">CalCulate Attack Damage</param>
        /// <param name="CalCulateQDamage">CalCulate Q Damage</param>
        /// <param name="CalCulateWDamage">CalCulate W Damage</param>
        /// <param name="CalCulateEDamage">CalCulate E Damage</param>
        /// <param name="CalCulateRDamage">CalCulate R Damage</param>
        /// <returns></returns>
        public static double GetDamage(Obj_AI_Hero Target, bool CalCulateAttackDamage = true,
            bool CalCulateQDamage = true, bool CalCulateWDamage = true,
            bool CalCulateEDamage = true, bool CalCulateRDamage = true)
        {
            if (Target != null)
            {
                double Damage = 0d;

                if (CalCulateAttackDamage)
                {
                    Damage += GameObjects.Player.GetAutoAttackDamage(Target);
                }

                if (CalCulateQDamage)
                {
                    Damage += GameObjects.Player.Spellbook.GetSpell(SpellSlot.Q).IsReady() ? GameObjects.Player.GetSpellDamage(Target, SpellSlot.Q) : 0d;
                }

                if (CalCulateWDamage)
                {
                    Damage += GameObjects.Player.Spellbook.GetSpell(SpellSlot.W).IsReady() ? GameObjects.Player.GetSpellDamage(Target, SpellSlot.W) : 0d;
                }

                if (CalCulateEDamage)
                {
                    Damage += GameObjects.Player.Spellbook.GetSpell(SpellSlot.E).IsReady() ? GameObjects.Player.GetSpellDamage(Target, SpellSlot.E) : 0d;
                }

                if (CalCulateRDamage)
                {
                    Damage += GameObjects.Player.Spellbook.GetSpell(SpellSlot.R).IsReady() ? GameObjects.Player.GetSpellDamage(Target, SpellSlot.R) : 0d;
                }

                return Damage;
            }
            else
            {
                return 0d;
            }
        }


        private static void CastELaneClear(List<Obj_AI_Minion> minions)
        {
            if (IsEOne)
            {
                if (cPassive > 0)
                {
                    return;
                }
                var count = minions.Count(i => i.IsValidTarget(E.Range));
                if (count > 0 && (Player.Mana >= 70 || count > 2))
                {
                    E.Cast();
                }
            }
            else
            {
                var minion = minions.Where(i => i.IsValidTarget(E2.Range) && HaveE(i)).ToList();
                if (minion.Count > 0 && (cPassive == 0 || minion.Any(CanE2)))
                {
                    E2.Cast();
                }
            }
        }

        private static void CastRFlash(Obj_AI_Hero target)
        {
            var targetSelect = Variables.TargetSelector.GetSelectedTarget();
            if (!targetSelect.IsValidTarget() || !targetSelect.Compare(target)
                || target.Health + target.PhysicalShield <= R.GetDamage(target))
            {
                return;
            }
            var pos = new Vector3();
            if (MainMenu["RMenu"]["RFlash"].GetValue<MenuKeyBind>().Active)
            {
                pos = Game.CursorPos;
            }
            else if (MainMenu["RMenu"]["BKR"].GetValue<MenuKeyBind>().Active && posBubbaKushFlash.IsValid())
            {
                var multi = GetMultiHit(MainMenu["RMenu"]["BKRKill"], MainMenu["RMenu"]["BKRCountA"], 1);
                if (multi.Item1 != null && multi.Item1.Compare(target) && multi.Item2 != 0 && multi.Item3.IsValid())
                {
                    posBubbaKushFlash = multi.Item3;
                }
                pos = posBubbaKushFlash;
            }
            else if (MainMenu["RMenu"]["InsecR"].GetValue<MenuKeyBind>().Active && Insec.IsRecentRFlash)
            {
                pos = Insec.GetPositionKickTo(target);
            }
            if (pos.IsValid())
            {
                Player.Spellbook.CastSpell(
                    Flash,
                    target.ServerPosition.Extend(pos, -(Player.BoundingRadius / 2 + target.BoundingRadius + 50)));
            }
        }

        private static void CastW(List<Obj_AI_Minion> minions = null)
        {
            if (!W.IsReady() || Variables.TickCount - lastW <= 300 || isDashing || IsDashing
                || Variables.TickCount - lastE2 <= 250 || Player.Spellbook.IsCastingSpell)
            {
                return;
            }
            var hero = Variables.Orbwalker.GetTarget() as Obj_AI_Hero;
            Obj_AI_Minion minion = null;
            if (minions != null && minions.Count > 0)
            {
                minion = minions.FirstOrDefault(i => i.InAutoAttackRange());
            }
            if (hero == null && minion == null)
            {
                return;
            }
            if (hero != null && !IsWOne && !MainMenu["WMenu"]["CBW2"])
            {
                return;
            }
            if (hero != null && Player.HealthPercent < hero.HealthPercent && Player.HealthPercent < 30 && W.Cast())
            {
                return;
            }
            if (Player.HealthPercent < (minions == null ? 8 : 5) || (!IsWOne && Variables.TickCount - lastW > 2600)
                || cPassive == 0
                || (minion != null && minion.Team == GameObjectTeam.Neutral
                    && minion.GetJungleType() != JungleType.Small && Player.HealthPercent < 40 && IsWOne))
            {
                W.Cast();
            }
        }

        private static void Combo()
        {
            if (R.IsReady() && MainMenu["Star"]["StarKill"] && Q.IsReady() && !IsQOne && MainMenu["QMenu"]["CBQ"]
                && MainMenu["QMenu"]["CBQ2"])
            {
                var target = Variables.TargetSelector.GetTargets(Q2.Range, Q2.DamageType).FirstOrDefault(HaveQ);
                if (target != null
                    && target.Health + target.PhysicalShield
                    > Q.GetDamage(target, DamageStage.SecondCast) + Player.GetAutoAttackDamage(target)
                    && target.Health + target.PhysicalShield
                    <= GetQ2Dmg(target, R.GetDamage(target)) + Player.GetAutoAttackDamage(target))
                {
                    if (R.CastOnUnit(target))
                    {
                        return;
                    }
                    if (MainMenu["Star"]["StarKillWJ"] && !R.IsInRange(target)
                        && target.DistanceToPlayer() < WardManager.WardRange + R.Range - 50 && Player.Mana >= 80
                        && !isDashing)
                    {
                        Flee(target.ServerPosition, true);
                    }
                }
            }
            if (MainMenu["QMenu"]["CBQ"] && Q.IsReady())
            {
                if (IsQOne)
                {
                    var target = Q.GetTarget(Q.Width / 2);
                    if (!R.IsReady() && Variables.TickCount - lastR < 5000)
                    {
                        var targetR =
                            Variables.TargetSelector.GetTargets(Q.Range, Q.DamageType)
                                .FirstOrDefault(i => i.HasBuff("BlindMonkDragonsRage"));
                        if (targetR != null)
                        {
                            target = targetR;
                        }
                    }
                    if (target != null)
                    {
                        Q.CastSpellSmite(target, MainMenu["QMenu"]["CBQCol"]);
                    }
                }
                else if (MainMenu["QMenu"]["CBQ2"] && !IsDashing && objQ.IsValidTarget(Q2.Range))
                {
                    var target = objQ as Obj_AI_Hero;
                    if (target != null)
                    {
                        if ((CanQ2(target) || (!R.IsReady() && IsRecentR && CanR(target))
                             || target.Health + target.PhysicalShield
                             <= Q.GetDamage(target, DamageStage.SecondCast) + Player.GetAutoAttackDamage(target)
                             || ((R.IsReady()
                                  || (!target.HasBuff("BlindMonkDragonsRage") && Variables.TickCount - lastR > 1000))
                                 && target.DistanceToPlayer() > target.GetRealAutoAttackRange() + 100) || cPassive == 0)
                            && Q2.Cast())
                        {
                            return;
                        }
                    }
                    else if (MainMenu["QMenu"]["CBQ2Obj"])
                    {
                        var targetQ2 = Q2.GetTarget(200);
                        if (targetQ2 != null && objQ.Distance(targetQ2) < targetQ2.DistanceToPlayer()
                            && !targetQ2.InAutoAttackRange() && Q2.Cast())
                        {
                            return;
                        }
                    }
                }
            }
            if (MainMenu["EMenu"]["CBE"])
            {
                CastE();
            }
            if (MainMenu["WMenu"]["CBW"])
            {
                CastW();
            }
            var subTarget = W.GetTarget();

            UseItem(subTarget);

            if (subTarget != null && Common.CanIgnite && subTarget.HealthPercent < 30 && subTarget.DistanceToPlayer() <= IgniteRange)
            {
                Player.Spellbook.CastSpell(Ignite, subTarget);
            }
        }

        private static void Flee(Vector3 pos, bool isStar = false)
        {
            if (!W.IsReady() || !IsWOne || Variables.TickCount - lastW <= 500)
            {
                return;
            }
            var posPlayer = Player.ServerPosition;
            var posJump = pos.Distance(posPlayer) < W.Range ? pos : posPlayer.Extend(pos, W.Range);
            var objJumps = new List<Obj_AI_Base>();
            objJumps.AddRange(GameObjects.AllyHeroes.Where(i => !i.IsMe));
            objJumps.AddRange(GameObjects.AllyWards.Where(i => i.IsWard()));
            objJumps.AddRange(
                GameObjects.AllyMinions.Where(
                    i => i.IsMinion() || i.IsPet() || SpecialPet.Contains(i.CharData.BaseSkinName.ToLower())));
            var objJump =
                objJumps.Where(
                    i => i.IsValidTarget(W.Range, false) && i.Distance(posJump) < (isStar ? R.Range - 50 : 200))
                    .MinOrDefault(i => i.Distance(posJump));
            if (objJump != null)
            {
                W.CastOnUnit(objJump);
            }
            else
            {
                WardManager.Place(posJump);
            }
        }

        private static Tuple<Obj_AI_Hero, int, Vector3> GetMultiHit(bool checkKill, int minHit, int mode)
        {
            var bestHit = 0;
            Obj_AI_Hero bestTarget = null;
            var bestPos = new Vector3();
            var targetKicks =
                Variables.TargetSelector.GetTargets(R.Range + (mode == 2 ? 500 : 0), R.DamageType)
                    .Where(
                        i =>
                        (mode != 2
                         || i.DistanceToPlayer() < R.Range + (WardManager.WardRange - Insec.GetDistance(i) - 80))
                        && i.Health + i.PhysicalShield > R.GetDamage(i) && !i.HasBuffOfType(BuffType.SpellShield)
                        && !i.HasBuffOfType(BuffType.SpellImmunity))
                    .OrderByDescending(i => i.BonusHealth)
                    .ToList();
            foreach (var targetKick in targetKicks)
            {
                var posTarget = mode == 1 ? targetKick.ServerPosition : R.GetPredPosition(targetKick, true);
                R2.UpdateSourcePosition(posTarget, posTarget);
                R2.Width = targetKick.BoundingRadius - 10;
                var targetHits =
                    GameObjects.EnemyHeroes.Where(
                        i => i.IsValidTarget(R2.Range + R2.Width / 2, true, R2.From) && !i.Compare(targetKick))
                        .OrderByDescending(i => new Priority().GetDefaultPriority(i))
                        .ToList();
                var posEnd = new Vector3();
                if (mode == 0)
                {
                    var pos = R2.From.Extend(Player.ServerPosition, -R2.Range);
                    targetHits = targetHits.Where(i => R2.WillHit(i, pos)).ToList();
                    if (checkKill && targetHits.Any(i => i.Health + i.PhysicalShield <= GetRColDmg(targetKick, i)))
                    {
                        return new Tuple<Obj_AI_Hero, int, Vector3>(targetKick, -1, posEnd);
                    }
                }
                else
                {
                    R2.Delay = R.Delay + (mode == 2 ? 0.05f : 0);
                    var hits = new List<Obj_AI_Hero>();
                    foreach (var targetHit in targetHits)
                    {
                        var list = new List<Obj_AI_Hero>();
                        var pred = R2.GetPrediction(targetHit);
                        var pos = new Vector3();
                        if (pred.Hitchance >= HitChance.High)
                        {
                            list.Add(targetHit);
                            list.AddRange(
                                targetHits.Where(i => !i.Compare(targetHit) && R2.WillHit(i, pred.CastPosition)));
                            pos = mode == 1
                                      ? pred.CastPosition
                                      : pred.Input.From.Extend(pred.CastPosition, -Insec.GetDistance(targetKick));
                        }
                        if (!pos.IsValid())
                        {
                            continue;
                        }
                        if (checkKill && list.Any(i => i.Health + i.PhysicalShield <= GetRColDmg(targetKick, i)))
                        {
                            return new Tuple<Obj_AI_Hero, int, Vector3>(targetKick, -1, pos);
                        }
                        if (list.Count > hits.Count)
                        {
                            hits = list;
                            posEnd = pos;
                        }
                    }
                    targetHits = hits;
                }
                if (targetHits.Count > bestHit)
                {
                    bestTarget = targetKick;
                    bestHit = targetHits.Count;
                    bestPos = posEnd;
                }
            }
            return new Tuple<Obj_AI_Hero, int, Vector3>(bestTarget, bestHit >= minHit ? 1 : 0, bestPos);
        }

        private static double GetQ2Dmg(Obj_AI_Base target, double subHp)
        {
            var dmg = new[] { 50, 80, 110, 140, 170 }[Q.Level - 1] + 0.9 * Player.FlatPhysicalDamageMod
                      + 0.08 * (target.MaxHealth - (target.Health - subHp));
            return Player.CalculateDamage(
                target,
                DamageType.Physical,
                target is Obj_AI_Minion ? Math.Min(dmg, 400) : dmg) + subHp;
        }

        private static float GetRColDmg(Obj_AI_Hero kickTarget, Obj_AI_Hero hitTarget)
        {
            return R.GetDamage(hitTarget)
                   + (float)
                     Player.CalculateDamage(
                         hitTarget,
                         DamageType.Physical,
                         new[] { 0.12, 0.15, 0.18 }[R.Level - 1] * kickTarget.BonusHealth);
        }

        private static bool HaveE(Obj_AI_Base target)
        {
            return target.HasBuff("BlindMonkTempest");
        }

        private static bool HaveQ(Obj_AI_Base target)
        {
            return target.HasBuff("BlindMonkSonicWave");
        }

        private static void KillSteal()
        {
            if (MainMenu["QMenu"]["KSQ"] && Q.IsReady())
            {
                if (IsQOne)
                {
                    var target = Q.GetTarget(Q.Width / 2);
                    if (target != null
                        && (target.Health + target.PhysicalShield <= Q.GetDamage(target)
                            || (MainMenu["QMenu"]["KSQ2"]
                                && target.Health + target.PhysicalShield
                                <= GetQ2Dmg(target, Q.GetDamage(target)) + Player.GetAutoAttackDamage(target)
                                && Player.Mana - Q.Instance.ManaCost >= 30))
                        && Q.Casting(
                            target,
                            false,
                            CollisionableObjects.Heroes | CollisionableObjects.Minions | CollisionableObjects.YasuoWall)
                               .IsCasted())
                    {
                        return;
                    }
                }
                else if (MainMenu["QMenu"]["KSQ2"] && !IsDashing)
                {
                    var target = objQ as Obj_AI_Hero;
                    if (target != null
                        && target.Health + target.PhysicalShield
                        <= Q.GetDamage(target, DamageStage.SecondCast) + Player.GetAutoAttackDamage(target) && Q2.Cast())
                    {
                        return;
                    }
                }
            }
            if (MainMenu["EMenu"]["KSE"] && E.IsReady() && IsEOne
                && Variables.TargetSelector.GetTargets(E.Range, E.DamageType)
                       .Any(i => E.CanHitCircle(i) && i.Health + i.MagicalShield <= E.GetDamage(i)) && E.Cast())
            {
                return;
            }
            if (MainMenu["RMenu"]["KSR"] && R.IsReady())
            {
                var targets =
                    Variables.TargetSelector.GetTargets(R.Range, R.DamageType, false)
                        .Where(i => MainMenu["RMenu"]["KSRCast" + i.ChampionName])
                        .ToList();
                if (targets.Count > 0)
                {
                    var targetR = targets.FirstOrDefault(i => i.Health + i.PhysicalShield <= R.GetDamage(i));
                    if (targetR != null)
                    {
                        R.CastOnUnit(targetR);
                    }
                    else if (MainMenu["QMenu"]["KSQ"] && MainMenu["QMenu"]["KSQ2"] && Q.IsReady() && !IsQOne)
                    {
                        var targetQ2R =
                            targets.FirstOrDefault(
                                i =>
                                HaveQ(i)
                                && i.Health + i.PhysicalShield
                                <= GetQ2Dmg(i, R.GetDamage(i)) + Player.GetAutoAttackDamage(i));
                        if (targetQ2R != null)
                        {
                            R.CastOnUnit(targetQ2R);
                        }
                    }
                }
            }
        }

        private static void LaneClear()
        {
            var minions =
                Common.ListMinions().Where(i => i.IsValidTarget(Q2.Range)).OrderByDescending(i => i.MaxHealth).ToList();
            if (minions.Count == 0)
            {
                return;
            }
            if (MainMenu["EMenu"]["LCE"])
            {
                CastE(minions);
            }
            if (MainMenu["WMenu"]["LCW"])
            {
                CastW(minions);
            }
            if (MainMenu["QMenu"]["LCQ"] && Q.IsReady())
            {
                if (IsQOne)
                {
                    if (cPassive < 2)
                    {
                        var minionQ = minions.Where(i => i.DistanceToPlayer() < Q.Range - 10).ToList();
                        if (minionQ.Count > 0)
                        {
                            var minionJungle =
                                minionQ.Where(i => i.Team == GameObjectTeam.Neutral)
                                    .OrderByDescending(i => i.MaxHealth)
                                    .ThenBy(i => i.DistanceToPlayer())
                                    .ToList();
                            if (MainMenu["QMenu"]["LCQBig"] && minionJungle.Count > 0 && Player.Health > 100)
                            {
                                minionJungle =
                                    minionJungle.Where(
                                        i =>
                                        i.GetJungleType() == JungleType.Legendary
                                        || i.GetJungleType() == JungleType.Large || i.Name.Contains("Crab")).ToList();
                            }
                            if (minionJungle.Count > 0)
                            {
                                minionJungle.ForEach(i => Q.Casting(i));
                            }
                            else
                            {
                                var minionLane =
                                    minionQ.Where(i => i.Team != GameObjectTeam.Neutral)
                                        .OrderByDescending(i => i.GetMinionType().HasFlag(MinionTypes.Siege))
                                        .ThenBy(i => i.GetMinionType().HasFlag(MinionTypes.Super))
                                        .ThenBy(i => i.Health)
                                        .ThenByDescending(i => i.MaxHealth)
                                        .ToList();
                                if (minionLane.Count == 0)
                                {
                                    return;
                                }
                                foreach (var minion in minionLane)
                                {
                                    if (minion.InAutoAttackRange())
                                    {
                                        if (Q.GetHealthPrediction(minion) > Q.GetDamage(minion)
                                            && Q.Casting(minion).IsCasted())
                                        {
                                            return;
                                        }
                                    }
                                    else if ((Variables.Orbwalker.GetTarget() != null
                                                  ? Q.CanLastHit(minion, Q.GetDamage(minion))
                                                  : Q.GetHealthPrediction(minion) > Q.GetDamage(minion))
                                             && Q.Casting(minion).IsCasted())
                                    {
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (!IsDashing)
                {
                    var q2Minion = objQ;
                    if (q2Minion.IsValidTarget(Q2.Range)
                        && (CanQ2(q2Minion) || q2Minion.Health <= Q.GetDamage(q2Minion, DamageStage.SecondCast)
                            || q2Minion.DistanceToPlayer() > q2Minion.GetRealAutoAttackRange() + 100 || cPassive == 0))
                    {
                        Q2.Cast();
                    }
                }
            }
        }

        private static void LastHit()
        {
            if (!MainMenu["QMenu"]["LHQ"] || !Q.IsReady() || !IsQOne || Player.Spellbook.IsAutoAttacking)
            {
                return;
            }
            var minions =
                GameObjects.EnemyMinions.Where(
                    i => (i.IsMinion() || i.IsPet(false)) && i.IsValidTarget(Q.Range) && Q.CanLastHit(i, Q.GetDamage(i)))
                    .OrderByDescending(i => i.MaxHealth)
                    .ToList();
            if (minions.Count == 0)
            {
                return;
            }
            minions.ForEach(
                i =>
                Q.Casting(
                    i,
                    false,
                    CollisionableObjects.Heroes | CollisionableObjects.Minions | CollisionableObjects.YasuoWall));
        }

        private static void OnDraw(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }
            if (MainMenu["Draw"]["KnockUp"] && R.Level > 0)
            {
                var menu = MainMenu["RMenu"]["KUR"].GetValue<MenuKeyBind>();
                var text =
                    $"Auto Knock Up: {(menu.Active ? "On" : "Off")} <{MainMenu["RMenu"]["KURCountA"].GetValue<MenuSlider>().Value}> [{menu.Key}]";
                var pos = Drawing.WorldToScreen(Player.Position);
                Drawing.DrawText(
                    pos.X - (float)Drawing.GetTextExtent(text).Width / 2,
                    pos.Y + 20,
                    menu.Active ? Color.White : Color.Gray,
                    text);
            }

            if(MainMenu["Draw"]["Damage"])
            {
                foreach (var e in ObjectManager.Get<Obj_AI_Hero>().Where(e => e.IsValidTarget() && e.IsValid && !e.IsDead && !e.IsZombie))
                {
                    HpBarDraw.Unit = e;
                    HpBarDraw.DrawDmg((int)GetDamage(e), new SharpDX.ColorBGRA(255, 204, 0, 170));
                }
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
                    (IsQOne ? Q : Q2).Range,
                    Q.IsReady() ? Color.LimeGreen : Color.IndianRed);
            }
            if (MainMenu["Draw"]["W"] && W.Level > 0 && IsWOne)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, W.IsReady() ? Color.LimeGreen : Color.IndianRed);
            }
            if (MainMenu["Draw"]["E"] && E.Level > 0)
            {
                Render.Circle.DrawCircle(
                    Player.Position,
                    (IsEOne ? E : E2).Range,
                    E.IsReady() ? Color.LimeGreen : Color.IndianRed);
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

            if (MainMenu["WMenu"]["FleeW"].GetValue<MenuKeyBind>().Active)
            {
                Variables.Orbwalker.Move(Game.CursorPos);
                Flee(Game.CursorPos);
            }

            KillSteal();
            switch (Variables.Orbwalker.ActiveMode)
            {
                case OrbwalkingMode.Combo:
                    Combo();
                    break;
                case OrbwalkingMode.LaneClear:
                    LaneClear();
                    break;
                case OrbwalkingMode.LastHit:
                    LastHit();
                    break;
                case OrbwalkingMode.None:
                    if (MainMenu["RMenu"]["RFlash"].GetValue<MenuKeyBind>().Active)
                    {
                        Variables.Orbwalker.Move(Game.CursorPos);
                        if (R.IsReady() && Common.CanFlash)
                        {
                            var target =
                                Variables.TargetSelector.GetTargets(R.Range, R.DamageType)
                                    .Where(i => i.Health + i.PhysicalShield > R.GetDamage(i))
                                    .MaxOrDefault(i => new Priority().GetPriority(i));
                            if (target != null && R.CastOnUnit(target))
                            {
                                Variables.TargetSelector.SetTarget(target);
                            }
                        }
                    }
                    else if (MainMenu["RMenu"]["BKR"].GetValue<MenuKeyBind>().Active)
                    {
                        Variables.Orbwalker.Move(Game.CursorPos);
                        BubbaKush();
                    }
                    else if (MainMenu["Star"]["Star"].GetValue<MenuKeyBind>().Active)
                    {
                        StarCombo();
                    }
                    else if (MainMenu["RMenu"]["InsecR"].GetValue<MenuKeyBind>().Active)
                    {
                        Insec.Start();
                    }
                    break;
            }
            if (MainMenu["RMenu"]["KUR"].GetValue<MenuKeyBind>().Active
                && !MainMenu["RMenu"]["BKR"].GetValue<MenuKeyBind>().Active
                && !MainMenu["RMenu"]["InsecR"].GetValue<MenuKeyBind>().Active)
            {
                AutoKnockUp();
            }
        }

        private static void StarCombo()
        {
            var target = Q.GetTarget(Q.Width / 2);
            if (!IsQOne)
            {
                target = objQ as Obj_AI_Hero;
            }
            if (!Q.IsReady())
            {
                target = W.GetTarget();
            }
            Variables.Orbwalker.Orbwalk(target);
            if (target == null)
            {
                return;
            }
            if (Q.IsReady())
            {
                if (IsQOne)
                {
                    Q.CastSpellSmite(target, false);
                }
                else if (!IsDashing && HaveQ(target)
                         && (target.Health + target.PhysicalShield
                             <= Q.GetDamage(target, DamageStage.SecondCast) + Player.GetAutoAttackDamage(target)
                             || (!R.IsReady() && IsRecentR && CanR(target))) && Q2.Cast())
                {
                    return;
                }
            }
            if (E.IsReady() && IsEOne && E.CanHitCircle(target) && (!HaveQ(target) || Player.Mana >= 70) && E.Cast())
            {
                return;
            }
            if (!R.IsReady() || !Q.IsReady() || IsQOne || !HaveQ(target))
            {
                return;
            }
            if (R.IsInRange(target))
            {
                R.CastOnUnit(target);
            }
            else if (target.DistanceToPlayer() < WardManager.WardRange + R.Range - 50 && Player.Mana >= 70 && !isDashing)
            {
                Flee(target.ServerPosition, true);
            }
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
            if (Youmuu.IsReady && Player.CountEnemyHeroesInRange(W.Range + E.Range) > 0)
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

        private static class Insec
        {
            internal static bool IsWardFlash;

            internal static int LastWardTime, LastJumpTme;

            private static Vector3 lastEndPos, lastFlashPos;

            private static int lastInsecTime, lastMoveTime, lastRFlashTime, lastFlashRTime;

            private static Obj_AI_Base lastObjQ;

            internal static bool IsRecentRFlash => Variables.TickCount - lastRFlashTime < 5000;

            private static bool CanInsec
                =>
                    (WardManager.CanWardJump || (MainMenu["RMenu"]["InsecFlash"] && Common.CanFlash) || IsRecent)
                    && R.IsReady();

            private static bool CanWardFlash
                =>
                    MainMenu["RMenu"]["InsecFlash"] && MainMenu["RMenu"]["InsecFlashJump"] && WardManager.CanWardJump
                    && Common.CanFlash;

            private static Obj_AI_Hero GetTarget
            {
                get
                {
                    Obj_AI_Hero target;
                    if (MainMenu["RMenu"]["InsecTargetSelect"])
                    {
                        var sub = Variables.TargetSelector.GetSelectedTarget();
                        target = sub.IsValidTarget() ? sub : null;
                    }
                    else
                    {
                        var extraRange = 100
                                         + (CanWardFlash
                                                ? GetRange(null, true)
                                                : (WardManager.CanWardJump ? WardManager.WardRange : FlashRange));
                        if (MainMenu["QMenu"]["InsecQ"] && Q.IsReady() && IsQOne)
                        {
                            target = Q.GetTarget(extraRange);
                        }
                        else if (objQ.IsValidTarget(Q2.Range))
                        {
                            target = Q2.GetTarget(extraRange);
                        }
                        else
                        {
                            target = Variables.TargetSelector.GetTarget(extraRange, R.DamageType);
                        }
                    }
                    return target;
                }
            }

            private static bool IsRecent
                => IsRecentWardJump || (MainMenu["RMenu"]["InsecFlash"] && Variables.TickCount - lastFlashRTime < 5000);

            private static bool IsRecentWardJump
                => Variables.TickCount - LastWardTime < 5000 || Variables.TickCount - LastJumpTme < 5000;

            internal static float GetDistance(Obj_AI_Hero target)
            {
                return Math.Min((Player.BoundingRadius + target.BoundingRadius + 50) * 1.4f, 300);
            }

            internal static Vector3 GetPositionKickTo(Obj_AI_Hero target)
            {
                if (lastEndPos.IsValid())
                {
                    return lastEndPos;
                }
                var pos = Player.ServerPosition;
                switch (MainMenu["RMenu"]["InsecMode"].GetValue<MenuList>().Index)
                {
                    case 0:
                        var turret =
                            GameObjects.AllyTurrets.Where(
                                i =>
                                target.Distance(i) <= 1400 && i.Distance(target) - RKickRange <= 950
                                && i.Distance(target) > 225).MinOrDefault(i => i.DistanceToPlayer());
                        if (turret != null)
                        {
                            pos = turret.ServerPosition;
                        }
                        else
                        {
                            var hero =
                                GameObjects.AllyHeroes.Where(
                                    i =>
                                    i.IsValidTarget(1600, false, target.ServerPosition) && !i.IsMe
                                    && i.HealthPercent > 10 && i.Distance(target) > 325)
                                    .MaxOrDefault(i => i.CountAllyHeroesInRange(600));
                            if (hero != null)
                            {
                                pos = hero.ServerPosition;
                            }
                        }
                        break;
                    case 1:
                        pos = Game.CursorPos;
                        break;
                }
                return pos;
            }

            internal static void Init()
            {
                Game.OnUpdate += args =>
                    {
                        if (lastInsecTime > 0 && Variables.TickCount - lastInsecTime > 5000)
                        {
                            CleanData();
                        }
                        if (lastMoveTime > 0 && Variables.TickCount - lastMoveTime > 1000 && !R.IsReady())
                        {
                            lastMoveTime = 0;
                        }
                    };
                Drawing.OnDraw += args =>
                    {
                        if (Player.IsDead || R.Level == 0 || !CanInsec)
                        {
                            return;
                        }
                        if (MainMenu["Draw"]["DLine"])
                        {
                            var target = GetTarget;
                            if (target != null)
                            {
                                var posTarget = target.Position;
                                var posEnd = GetPositionKickTo(target);
                                var radius = target.BoundingRadius * 1.35f;
                                Render.Circle.DrawCircle(posTarget, radius, Color.BlueViolet);
                                Render.Circle.DrawCircle(
                                    GetPositionBehind(target, posEnd, posTarget),
                                    radius,
                                    Color.BlueViolet);
                                Drawing.DrawLine(
                                    Drawing.WorldToScreen(posTarget),
                                    Drawing.WorldToScreen(posEnd),
                                    1,
                                    Color.BlueViolet);
                            }
                        }
                        if (MainMenu["Draw"]["DWardFlash"] && CanWardFlash)
                        {
                            Render.Circle.DrawCircle(Player.Position, GetRange(null, true), Color.Orange);
                        }
                    };
                Obj_AI_Base.OnBuffAdd += (sender, args) =>
                    {
                        if (!sender.IsEnemy || args.Buff.DisplayName != "BlindMonkSonicWave")
                        {
                            return;
                        }
                        lastObjQ = sender;
                    };
                Obj_AI_Base.OnProcessSpellCast += (sender, args) =>
                    {
                        if (!lastFlashPos.IsValid() || !sender.IsMe
                            || !MainMenu["RMenu"]["InsecR"].GetValue<MenuKeyBind>().Active
                            || args.SData.Name != "SummonerFlash" || !MainMenu["RMenu"]["InsecFlash"]
                            || Variables.TickCount - lastFlashRTime > 1250 || args.End.Distance(lastFlashPos) > 100)
                        {
                            return;
                        }
                        lastFlashRTime = Variables.TickCount;
                        var target = Variables.TargetSelector.GetSelectedTarget();
                        if (target.IsValidTarget())
                        {
                            DelayAction.Add(5, () => R.CastOnUnit(target));
                        }
                    };
                Obj_AI_Base.OnDoCast += (sender, args) =>
                    {
                        if (!sender.IsMe || args.Slot != SpellSlot.R)
                        {
                            return;
                        }
                        CleanData();
                    };
            }

            internal static void Start()
            {
                var target = GetTarget;
                if (Variables.Orbwalker.CanMove && Variables.TickCount - lastMoveTime > 250)
                {
                    var posMove = Game.CursorPos;
                    if (target != null && lastMoveTime > 0 && CanInsec)
                    {
                        var posEnd = GetPositionKickTo(target);
                        if (posEnd.DistanceToPlayer() > target.Distance(posEnd))
                        {
                            posMove = GetPositionBehind(target, posEnd);
                        }
                    }
                    Variables.Orbwalker.Move(posMove);
                }
                if (target == null || !CanInsec)
                {
                    return;
                }
                if (!IsRecent)
                {
                    if (!IsWardFlash)
                    {
                        var checkJump = GapCheck(target);
                        if (checkJump.Item2)
                        {
                            GapByWardJump(target, checkJump.Item1);
                        }
                        else
                        {
                            var checkFlash = GapCheck(target, true);
                            if (checkFlash.Item2)
                            {
                                GapByFlash(target, checkFlash.Item1);
                            }
                            else if (CanWardFlash)
                            {
                                var posTarget = target.ServerPosition;
                                if (posTarget.DistanceToPlayer() < GetRange(target, true)
                                    && (!isDashing
                                        || (!lastObjQ.Compare(target) && lastObjQ.Distance(posTarget) > GetRange(target))))
                                {
                                    IsWardFlash = true;
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        Variables.TargetSelector.SetTarget(target);
                        WardManager.Place(target.ServerPosition);
                        return;
                    }
                }
                if (R.IsInRange(target))
                {
                    var posEnd = GetPositionKickTo(target);
                    var posTarget = target.ServerPosition;
                    var posPlayer = Player.ServerPosition;
                    if (posPlayer.Distance(posEnd) > posTarget.Distance(posEnd))
                    {
                        var segment = posTarget.Extend(posPlayer, -RKickRange)
                            .ProjectOn(posTarget, posEnd.Extend(posTarget, -(RKickRange * 0.5f)));
                        if (segment.IsOnSegment && segment.SegmentPoint.Distance(posEnd) <= RKickRange * 0.5f
                            && R.CastOnUnit(target))
                        {
                            return;
                        }
                    }
                }
                GapByQ(target);
            }

            private static void CleanData()
            {
                lastEndPos = lastFlashPos = new Vector3();
                lastInsecTime = 0;
                IsWardFlash = false;
                Variables.TargetSelector.SetTarget(null);
            }

            private static void GapByFlash(Obj_AI_Hero target, Vector3 posBehind)
            {
                switch (MainMenu["RMenu"]["InsecFlashMode"].GetValue<MenuList>().Index)
                {
                    case 0:
                        GapByRFlash(target);
                        break;
                    case 1:
                        GapByFlashR(target, posBehind);
                        break;
                    case 2:
                        if (!posBehind.IsValid())
                        {
                            GapByRFlash(target);
                        }
                        else
                        {
                            GapByFlashR(target, posBehind);
                        }
                        break;
                }
            }

            private static void GapByFlashR(Obj_AI_Hero target, Vector3 posBehind)
            {
                if (Variables.Orbwalker.CanMove)
                {
                    lastMoveTime = Variables.TickCount;
                }
                lastFlashPos = posBehind;
                lastEndPos = GetPositionAfterKick(target);
                lastInsecTime = lastFlashRTime = Variables.TickCount;
                Variables.TargetSelector.SetTarget(target);
                Player.Spellbook.CastSpell(Flash, posBehind);
            }

            private static void GapByQ(Obj_AI_Hero target)
            {
                if (!MainMenu["QMenu"]["InsecQ"] || !Q.IsReady() || IsDashing)
                {
                    return;
                }
                if (CanWardFlash && (IsWardFlash || (IsQOne && Player.Mana < 50 + 80)))
                {
                    return;
                }
                var minDist = GetRange(target, CanWardFlash);
                if (IsQOne)
                {
                    Q.CastSpellSmite(target, MainMenu["QMenu"]["InsecQCol"]);
                    if (!MainMenu["QMenu"]["InsecQObj"])
                    {
                        return;
                    }
                    var nearObj =
                        Common.ListEnemies(true)
                            .Where(
                                i =>
                                i.IsValidTarget(Q.Range) && !i.Compare(target)
                                && Q.GetHealthPrediction(i) > Q.GetDamage(i)
                                && i.Distance(target) < target.DistanceToPlayer() && i.Distance(target) < minDist - 80)
                            .OrderBy(i => i.Distance(target))
                            .ThenByDescending(i => i.Health)
                            .ToList();
                    if (nearObj.Count == 0)
                    {
                        return;
                    }
                    nearObj.ForEach(i => Q.Casting(i));
                }
                else if (target.DistanceToPlayer() > minDist
                         && (HaveQ(target) || (objQ.IsValidTarget(Q2.Range) && target.Distance(objQ) < minDist - 80))
                         && ((WardManager.CanWardJump && Player.Mana >= 80)
                             || (MainMenu["RMenu"]["InsecFlash"] && Common.CanFlash)) && Q2.Cast())
                {
                    Variables.TargetSelector.SetTarget(target);
                }
            }

            private static void GapByRFlash(Obj_AI_Hero target)
            {
                if (!R.CastOnUnit(target))
                {
                    return;
                }
                lastEndPos = GetPositionAfterKick(target);
                lastInsecTime = lastRFlashTime = Variables.TickCount;
                Variables.TargetSelector.SetTarget(target);
            }

            private static void GapByWardJump(Obj_AI_Hero target, Vector3 posBehind)
            {
                if (Variables.Orbwalker.CanMove)
                {
                    lastMoveTime = Variables.TickCount;
                    Variables.Orbwalker.Move(
                        posBehind.Extend(GetPositionKickTo(target), -(GetDistance(target) + Player.BoundingRadius / 2)));
                }
                lastEndPos = GetPositionAfterKick(target);
                lastInsecTime = LastWardTime = LastJumpTme = Variables.TickCount;
                Variables.TargetSelector.SetTarget(target);
                WardManager.Place(posBehind, 1);
            }

            private static Tuple<Vector3, bool> GapCheck(Obj_AI_Hero target, bool useFlash = false)
            {
                if (!useFlash ? !WardManager.CanWardJump : !MainMenu["RMenu"]["InsecFlash"] || !Common.CanFlash)
                {
                    return new Tuple<Vector3, bool>(new Vector3(), false);
                }
                var posEnd = GetPositionKickTo(target);
                var posPlayer = Player.ServerPosition;
                var posTarget = target.ServerPosition;
                if (!useFlash)
                {
                    var posBehind = posTarget.Extend(posEnd, -GetDistance(target));
                    if (posBehind.Distance(posPlayer) < WardManager.WardRange
                        && posTarget.Distance(posBehind) < posEnd.Distance(posBehind))
                    {
                        return new Tuple<Vector3, bool>(posBehind, true);
                    }
                }
                else
                {
                    var flashMode = MainMenu["RMenu"]["InsecFlashMode"].GetValue<MenuList>().Index;
                    if (flashMode != 1 && posTarget.Distance(posPlayer) < R.Range)
                    {
                        return new Tuple<Vector3, bool>(new Vector3(), true);
                    }
                    if (flashMode != 0)
                    {
                        var posBehind = posTarget.Extend(posEnd, -GetDistance(target));
                        var posFlash = posPlayer.Extend(posBehind, FlashRange);
                        if (posBehind.Distance(posPlayer) < FlashRange
                            && posTarget.Distance(posBehind) < posEnd.Distance(posBehind)
                            && posFlash.Distance(posTarget) > 50
                            && posFlash.Distance(posTarget) < posFlash.Distance(posEnd))
                        {
                            return new Tuple<Vector3, bool>(posBehind, true);
                        }
                    }
                }
                return new Tuple<Vector3, bool>(new Vector3(), false);
            }

            private static Vector3 GetPositionAfterKick(Obj_AI_Hero target)
            {
                return target.ServerPosition.Extend(GetPositionKickTo(target), RKickRange);
            }

            private static Vector3 GetPositionBehind(Obj_AI_Hero target, Vector3 to, Vector3 from = default(Vector3))
            {
                return (from.IsValid() ? from : target.ServerPosition).Extend(to, -GetDistance(target));
            }

            private static float GetRange(Obj_AI_Hero target, bool isWardFlash = false)
            {
                return !isWardFlash
                           ? (WardManager.CanWardJump ? WardManager.WardRange : FlashRange) - GetDistance(target)
                           : WardManager.WardRange + R.Range
                             - ((target ?? Player).BoundingRadius - 30
                                + (MainMenu["RMenu"]["InsecFlashMode"].GetValue<MenuList>().Index == 1 ? 50 : 0));
            }
        }

        private static class WardManager
        {
            internal const int WardRange = 600;
            private static Vector3 lastPlacePos;
            private static int lastPlaceTime;

            internal static bool CanWardJump => CanCastWard && W.IsReady() && IsWOne;

            private static bool CanCastWard => Variables.TickCount - lastPlaceTime > 1250 && Items.GetWardSlot() != null;

            private static bool IsTryingToJump => lastPlacePos.IsValid() && Variables.TickCount - lastPlaceTime < 1250;
            internal static void Init()
            {
                Game.OnUpdate += args =>
                    {
                        if (lastPlacePos.IsValid() && Variables.TickCount - lastPlaceTime > 1500)
                        {
                            lastPlacePos = new Vector3();
                        }
                        if (Player.IsDead)
                        {
                            return;
                        }
                        if (IsTryingToJump)
                        {
                            Jump(lastPlacePos);
                        }
                    };
                Obj_AI_Base.OnProcessSpellCast += (sender, args) =>
                    {
                        if (!lastPlacePos.IsValid() || !sender.IsMe || args.Slot != SpellSlot.W
                            || !args.SData.Name.ToLower().Contains("one"))
                        {
                            return;
                        }
                        var ward = args.Target as Obj_AI_Minion;
                        if (ward == null || !ward.IsValid() || !ward.IsWard() || ward.Distance(lastPlacePos) > 100)
                        {
                            return;
                        }
                        if (Variables.TickCount - Insec.LastJumpTme < 1250)
                        {
                            Insec.LastJumpTme = Variables.TickCount;
                        }
                        Insec.IsWardFlash = false;
                        lastPlacePos = new Vector3();
                    };
                GameObjectNotifier<Obj_AI_Minion>.OnCreate += (sender, minion) =>
                    {
                        if (!lastPlacePos.IsValid() || minion.Distance(lastPlacePos) > 100 || !minion.IsAlly
                            || !minion.IsWard() || !W.IsInRange(minion))
                        {
                            return;
                        }
                        if (Variables.TickCount - Insec.LastWardTime < 1250)
                        {
                            Insec.LastWardTime = Variables.TickCount;
                        }
                        if (Variables.TickCount - lastPlaceTime < 1250 && W.IsReady() && IsWOne && W.CastOnUnit(minion))
                        {
                            lastW = Variables.TickCount;
                        }
                    };
            }

            internal static void Place(Vector3 pos, int mode = 0)
            {
                if (!CanWardJump)
                {
                    return;
                }
                lastPlacePos = pos.DistanceToPlayer() < WardRange ? pos : Player.ServerPosition.Extend(pos, WardRange);
                switch (mode)
                {
                    case 0:
                        lastPlaceTime = Variables.TickCount + 1100;
                        break;
                    case 1:
                        lastPlaceTime = Insec.LastWardTime = Insec.LastJumpTme = Variables.TickCount;
                        break;
                }
                Player.Spellbook.CastSpell(Items.GetWardSlot().SpellSlot, lastPlacePos);
            }

            private static void Jump(Vector3 pos)
            {
                if (!W.IsReady() || !IsWOne || Variables.TickCount - lastW <= 500)
                {
                    return;
                }
                var wardObj =
                    GameObjects.AllyWards.Where(
                        i => i.IsValidTarget(W.Range, false) && i.IsWard() && i.Distance(pos) < 100)
                        .MinOrDefault(i => i.Distance(pos));
                if (wardObj != null)
                {
                    W.CastOnUnit(wardObj);
                }
            }
        }
    }
}