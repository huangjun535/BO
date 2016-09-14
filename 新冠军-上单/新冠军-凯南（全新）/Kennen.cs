namespace YuLeKennen.Plugin
{
    using Core;
    using LeagueSharp;
    using LeagueSharp.Data.Enumerations;
    using LeagueSharp.SDK;
    using LeagueSharp.SDK.Enumerations;
    using LeagueSharp.SDK.UI;
    using LeagueSharp.SDK.Utils;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using Menu = LeagueSharp.SDK.UI.Menu;

    internal class Kennen : Program
    {
        private static readonly Items.Item Wooglet = new Items.Item(3090, 0);
        private static bool isSkinReset = true;
        private static readonly Items.Item Zhonya = new Items.Item(3157, 0);
        private static bool haveE, haveR, castR = false;

        public Kennen()
        {
            Q = new Spell(SpellSlot.Q, 1050).SetSkillshot(0.19f, 50, 1700, true, SkillshotType.SkillshotLine);
            W = new Spell(SpellSlot.W, 800).SetTargetted(0.25f, float.MaxValue);
            E = new Spell(SpellSlot.E, 0);
            R = new Spell(SpellSlot.R, 550).SetTargetted(0.25f, float.MaxValue);
            Q.DamageType = W.DamageType = R.DamageType = DamageType.Magical;
            Q.MinHitChance = HitChance.VeryHigh;

            var QMenu = MainMenu.Add(new Menu("Q", "Q技能设置"));
            {
                QMenu.Bool("ComboQ", "连招中使用");
                QMenu.Bool("hybridQ", "骚扰中使用");
                QMenu.Bool("JCQ", "清野中使用");
                QMenu.Bool("lhQ", "补刀中使用");
                QMenu.Bool("ksQ", "击杀中使用");
                QMenu.KeyBind("AutoQ", "自动Q按键", Keys.N, KeyBindType.Toggle);
                QMenu.Slider("AutoQMpA", "自己Mp >=", 100, 0, 200);
            }

            var WMenu = MainMenu.Add(new Menu("W", "W技能设置"));
            {
                WMenu.Bool("comboW", "连招中使用");
                WMenu.Bool("hybridW", "骚扰中使用");
                WMenu.Slider("hybridWMpA", "骚扰中使用|自己Mp >= %", 40, 0, 200);
                WMenu.Add(new MenuSliderButton("JCW", "清野最低命中数", 3, 1, 4, true));
                WMenu.Bool("ksW", "W抢人头");
            }

            var EMenu = MainMenu.Add(new Menu("E", "E技能设置"));
            {
                EMenu.Bool("JCE", "清野E");
            }

            var RMenu = MainMenu.Add(new Menu("R", "R技能设置"));
            {
                RMenu.Bool("R", "连招中使用");
                RMenu.Slider("RHpU", "击中敌人数 >= 2 且 敌人Hp <= %", 60);
                RMenu.Slider("RCountA", "或者命中敌人数 >=", 3, 1, 5);
                RMenu.KeyBind("RFlash", "闪R按键", Keys.T);
                RMenu.Bool("RFlashAuto", "全自动闪R");
                RMenu.Slider("RFlashCount", "命中敌人数 >=", 3, 1, 5);
            }

            new AutoWard(MainMenu);

            var skinMenu = MainMenu.Add(new Menu("Skin", "换肤设置"));
            {
                skinMenu.Slider("Index", "皮肤设置: ", 0, 0, 6).ValueChanged += (sender, args) => { isSkinReset = true; };
                skinMenu.Bool("Own", "关闭后恢复自己本来的皮肤");
            }

            var MiscMenu = MainMenu.Add(new Menu("Misc", "杂项设置"));
            {
                MiscMenu.Bool("Ignite", "智能点燃");
                MiscMenu.Bool("Zhonya", "释放大招时自动中亚");
                MiscMenu.Slider("ZhonyaHpU", "自己的Hp <= %", 20);
            }

            var drawMenu = MainMenu.Add(new Menu("Draw", "显示设置"));
            {
                drawMenu.Bool("Q", "Q 范围", true);
                drawMenu.Bool("W", "W 范围", false);
                drawMenu.Bool("R", "R 范围", false);
            }

            MainMenu.KeyBind("FleeE", "逃跑按键", Keys.Z);

            Game.OnUpdate += OnUpdate;
            Drawing.OnEndScene += OnEndScene;
            Obj_AI_Base.OnBuffAdd += Obj_AI_Base_OnBuffAdd;
            Obj_AI_Base.OnBuffRemove += Obj_AI_Base_OnBuffRemove;
        }

        private static void SKins()
        {
            if (Player.IsDead)
            {
                if (!isSkinReset)
                {
                    isSkinReset = true;
                }
            }
            else if (isSkinReset)
            {
                isSkinReset = false;
                if (!MainMenu["Skin"]["Own"] || Player.BaseSkinId == 0)
                {
                    Player.SetSkin(Player.ChampionName, MainMenu["Skin"]["Index"]);
                }
            }
        }

        private void Obj_AI_Base_OnBuffAdd(Obj_AI_Base sender, Obj_AI_BaseBuffAddEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            switch (args.Buff.DisplayName)
            {
                case "KennenLightningRush":
                    haveE = true;
                    break;
                case "KennenShurikenStorm":
                    haveR = true;
                    break;
            }
        }

        private void Obj_AI_Base_OnBuffRemove(Obj_AI_Base sender, Obj_AI_BaseBuffRemoveEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            switch (args.Buff.DisplayName)
            {
                case "KennenLightningRush":
                    haveE = false;
                    break;
                case "KennenShurikenStorm":
                    haveR = false;
                    break;
            }
        }

        private static List<Obj_AI_Hero> GetWTarget => Variables.TargetSelector.GetTargets(W.Range, W.DamageType).Where(i => HaveW(i) && W.CanHitCircle(i)).ToList();

        private static void AutoQ()
        {
            if (!Q.IsReady() || !MainMenu["Q"]["AutoQ"].GetValue<MenuKeyBind>().Active || Player.Mana < MainMenu["Q"]["AutoQMpA"])
            {
                return;
            }

            Q.CastingBestTarget();
        }

        private static void Combo()
        {
            if (MainMenu["R"]["R"])
            {
                if (R.IsReady())
                {
                    var target = Variables.TargetSelector.GetTargets(R.Range + 50, R.DamageType).Where(i => R.CanHitCircle(i)).ToList();

                    if (target.Count > 0 && ((target.Count > 1 && target.Any(i => i.Health + i.MagicalShield <= R.GetDamage(i))) || (target.Count > 1 && target.Sum(i => i.HealthPercent) / target.Count <= MainMenu["R"]["RHpU"]) || target.Count >= MainMenu["R"]["RCountA"]) && R.Cast())
                    {
                        return;
                    }
                }
                else if (haveR && MainMenu["Misc"]["Zhonya"] && Player.HealthPercent < MainMenu["Misc"]["ZhonyaHpU"] && Player.CountEnemyHeroesInRange(W.Range) > 0)
                {
                    if (Zhonya.IsReady)
                    {
                        Zhonya.Cast();
                    }

                    if (Wooglet.IsReady)
                    {
                        Wooglet.Cast();
                    }
                }
            }

            if (MainMenu["Q"]["ComboQ"] && Q.CastingBestTarget().IsCasted())
            {
                return;
            }

            if (MainMenu["W"]["comboW"] && W.IsReady())
            {
                var target = GetWTarget;

                if (target.Count > 0)
                {
                    if (haveR)
                    {
                        if ((target.Count(i => HaveW(i, true)) > 1 || target.Any(i => i.Health + i.MagicalShield <= W.GetDamage(i, DamageStage.Empowered)) || target.Count > 2 || (target.Count(i => HaveW(i, true)) > 0 && target.Count > 1)) && W.Cast())
                        {
                            return;
                        }
                    }
                    else if (W.Cast())
                    {
                        return;
                    }
                }
            }

            var subTarget = W.GetTarget();

            if (subTarget != null && MainMenu["Misc"]["Ignite"] && Ignite.IsReady() && subTarget.HealthPercent < 30 && subTarget.DistanceToPlayer() <= IgniteRange)
            {
                Player.Spellbook.CastSpell(Ignite, subTarget);
            }
        }

        private static bool HaveW(Obj_AI_Base target, bool checkCanStun = false)
        {
            var buff = target.GetBuffCount("KennenMarkOfStorm");

            return buff > 0 && (!checkCanStun || buff == 2);
        }

        private static void Hybrid()
        {
            if (MainMenu["Q"]["hybridQ"] && Q.CastingBestTarget().IsCasted())
            {
                return;
            }

            if (MainMenu["W"]["hybridW"] && W.IsReady() && Player.Mana >= MainMenu["W"]["hybridWMpA"] && GetWTarget.Count > 0)
            {
                W.Cast();
            }
        }

        private static void KillSteal()
        {
            if (MainMenu["Q"]["ksQ"] && Q.IsReady())
            {
                var target = Variables.TargetSelector.GetTarget(Q);

                if (target != null && target.Health <= Q.GetDamage(target))
                {
                    Q.Cast(target);
                }
            }

            if (MainMenu["W"]["ksW"] && W.IsReady() && GetWTarget.Any(i => i.Health + i.MagicalShield <= W.GetDamage(i, DamageStage.Empowered)))
            {
                W.Cast();
            }
        }

        private static void LastHit()
        {
            if (!MainMenu["Q"]["lhQ"] || !Q.IsReady() || Player.Spellbook.IsAutoAttacking)
            {
                return;
            }

            var minions = GameObjects.EnemyMinions.Where(i => (i.IsMinion() || i.IsPet(false)) && i.IsValidTarget(Q.Range) && Q.CanLastHit(i, Q.GetDamage(i))).OrderByDescending(i => i.MaxHealth).ToList();

            if (minions.Count == 0)
            {
                return;
            }

            minions.ForEach(i => Q.Casting(i, false, CollisionableObjects.Heroes | CollisionableObjects.Minions | CollisionableObjects.YasuoWall));
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

            if (MainMenu["Draw"]["W"] && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, W.IsReady() ? Color.LimeGreen : Color.IndianRed);
            }

            if (MainMenu["Draw"]["R"] && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.LimeGreen : Color.IndianRed);
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            Variables.Orbwalker.AttackState = !haveE;

            if (Player.IsDead || MenuGUI.IsChatOpen || MenuGUI.IsShopOpen || Player.IsRecalling())
            {
                return;
            }

            SKins();

            KillSteal();

            Variables.Orbwalker.AttackState = !haveE;

            switch (Variables.Orbwalker.ActiveMode)
            {
                case OrbwalkingMode.Combo:
                    Combo();
                    break;
                case OrbwalkingMode.Hybrid:
                    Hybrid();
                    break;
                case OrbwalkingMode.LastHit:
                    LastHit();
                    break;
                case OrbwalkingMode.LaneClear:
                    LaneCLear();
                    break;
                case OrbwalkingMode.None:
                    if (MainMenu["FleeE"].GetValue<MenuKeyBind>().Active)
                    {
                        Variables.Orbwalker.Move(Game.CursorPos);
                        if (E.IsReady() && !haveE)
                        {
                            E.Cast();
                        }
                    }
                    break;
            }
            if (Variables.Orbwalker.ActiveMode != OrbwalkingMode.Combo && Variables.Orbwalker.ActiveMode != OrbwalkingMode.Hybrid)
            {
                AutoQ();
            }

            RFlash();
        }

        private static void RFlash()
        {
            if (MainMenu["R"]["RFlash"].GetValue<MenuKeyBind>().Active || (MainMenu["R"]["RFlashAuto"] && Variables.Orbwalker.ActiveMode == OrbwalkingMode.Combo))
            {
                if (MainMenu["R"]["RFlash"].GetValue<MenuKeyBind>().Active)
                {
                    Variables.Orbwalker.Move(Game.CursorPos);
                }

                var Count = MainMenu["R"]["RFlashCount"].GetValue<MenuSlider>().Value;
                var Pos = Player.ServerPosition.Extend(Game.CursorPos, FlashRange);

                if (Flash != SpellSlot.Unknown && !Pos.IsZero && !Pos.IsWall() && Pos.IsValid())
                {
                    if (Pos.CountEnemyHeroesInRange(R.Range - 50) >= Count)
                    {
                        if (Flash.IsReady() && R.IsReady())
                        {
                            castR = true;
                            Player.Spellbook.CastSpell(Flash, Pos);
                        }

                        if (R.IsReady() && !Flash.IsReady() && castR)
                        {
                            R.Cast();
                        }
                    }
                }
            }
        }

        private static void LaneCLear()
        {
            var Mobs = GameObjects.Jungle.Where(x => x.IsValidTarget(W.Range) && !x.HasBuff("kennenmarkofstorm")).ToList();
            var MobsW = GameObjects.Jungle.Where(m => m.IsValidTarget(W.Range) && m.HasBuff("kennenmarkofstorm")).ToList();
            var MobsQ = GameObjects.Jungle.Where(m => m.IsValidTarget(Q.Range)).ToList();

            if (MainMenu["E"]["JCE"] && E.IsReady() && ((!Player.HasBuff("KennenLightningRush") && Mobs.Count() > 1) || (Player.HasBuff("KennenLightningRush") && Mobs.FirstOrDefault() == null)))
            {
                E.Cast();
                return;
            }

            if (MainMenu["W"]["JCW"].GetValue<MenuSliderButton>().BValue && W.IsReady() && MobsW.Count() >= MainMenu["W"]["JCW"].GetValue<MenuSliderButton>().SValue && !Player.HasBuff("KennenLightningRush"))
            {
                W.Cast();
                return;
            }

            if (MainMenu["Q"]["JCQ"] && Q.IsReady() && MobsQ.Count() >= 1)
            {
                Q.Cast(MobsQ.FirstOrDefault());
                return;
            }
        }
    }
}