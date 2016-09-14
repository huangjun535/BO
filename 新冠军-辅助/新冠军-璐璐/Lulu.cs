using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using TreeLib.Core;
using TreeLib.Core.Extensions;
using TreeLib.Extensions;
using TreeLib.Managers;
using TreeLib.Objects;
using ActiveGapcloser = TreeLib.Core.ActiveGapcloser;
using Color = SharpDX.Color;
using YuLeLibrary;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using System.Reflection;
namespace YuLeLuLu
{
    internal class Lulu : Champion
    {
        private const int RRadius = 350;
        private static int LastWCast;
        private static Obj_AI_Base QAfterETarget;

        private static readonly Dictionary<SpellSlot, int[]> ManaCostDictionary = new Dictionary<SpellSlot, int[]>
        {
            { SpellSlot.Q, new[] { 0, 60, 65, 70, 75, 80 } },
            { SpellSlot.W, new[] { 0, 65, 65, 65, 65, 65 } },
            { SpellSlot.E, new[] { 0, 60, 70, 80, 90, 100 } },
            { SpellSlot.R, new[] { 0, 0, 0, 0, 0, 0 } }
        };

        public Lulu()
        {
            Q = SpellManager.Q;
            W = SpellManager.W;
            E = SpellManager.E;
            R = SpellManager.R;

            Menu = new Menu("QQ群438230879", "QQ群438230879", true);
            Menu.SetFontStyle(FontStyle.Regular, Color.Pink);
            Orbwalker = Menu.AddOrbwalker();

            var QMenu = Menu.AddMenu("Q", "Q 设置");
            {
                QMenu.AddBool("QCombo", "连招中使用");
                QMenu.AddBool("QHarass", "骚扰中使用");
                QMenu.AddKeyBind("QFarm", "打钱中使用", 'K', KeyBindType.Toggle, true);
                QMenu.AddBool("QLC", "清线中使用");
                QMenu.AddBool("QLH", "补刀中使用", false);
                QMenu.AddBool("QGapcloser", "反突进中使用");
                QMenu.AddBool("QImpaired", "自动Q减速敌人", false);
                QMenu.AddInfo("PixQ", "-- Pix设置", Color.Purple);
                QMenu.AddBool("QPixCombo", "连招中使用PixQ", false);
                QMenu.AddBool("QPixHarass", "骚扰中使用PixQ", false);
            }

            var WMenu = Menu.AddMenu("W", "W 设置");
            {
                var wEnemies = WMenu.AddMenu("WEnemies", "优先级设置");
                foreach (var enemy in Enemies)
                {
                    wEnemies.AddSlider(enemy.ChampionName + "WPriority", enemy.ChampionName, 1, 0, 5);
                }

                wEnemies.AddInfo("WEnemiesInfo", "0是最低级, 5是最高级", Color.DeepSkyBlue);
                wEnemies.AddBool("WPriority", "启动优先设置", false);

                WMenu.AddBool("WCombo", "连招中使用");
                WMenu.AddBool("WHarass", "骚扰中使用");
                WMenu.AddBool("WGapcloser", "反突进中使用");
                WMenu.AddBool("WInterrupter", "打断技能中使用");
                WMenu.AddBool("FleeW", "逃跑中使用");
            }

            var EMenu = Menu.AddMenu("E", "E 设置");
            {
                var eAllies = EMenu.AddMenu("EAllies", "保护友军");
                foreach (var ally in Allies)
                    eAllies.AddSlider(ally.ChampionName + "EPriority", ally.ChampionName, 20);
                eAllies.AddInfo("EAlliesInfo", "这是血量触发 谁的血量刚好到就保护谁", Color.DeepSkyBlue);
                eAllies.AddBool("EAuto", "启动!");
                EMenu.AddBool("ECombo", "连招中使用");
                EMenu.AddBool("EHarass", "骚扰中使用");
                EMenu.AddBool("EJG", "清野中使用");
                EMenu.AddInfo("PixEQ", "-- Pix设置", Color.Purple);
                EMenu.AddBool("EQPixCombo", "连招中使用PixE");
                EMenu.AddBool("EQPixHarass", "骚扰中使用PixE");
            }

            var RMenu = Menu.AddMenu("R", "R 设置");
            {
                var saver = RMenu.AddMenu("Saver", "保护友军");
                foreach (var ally in Allies)
                    saver.AddSlider(ally.ChampionName + "RPriority", ally.ChampionName, 15);
                saver.AddInfo("RAlliesInfo", "这是血量触发 谁的血量刚好到就保护谁", Color.DeepSkyBlue);
                saver.AddBool("RAuto", "启动!");
                RMenu.AddKeyBind("RForce", "大招按键(给附近最低血量的友军)!", 'K');
                RMenu.AddBool("RInterrupter", "打断技能中使用");
                RMenu.AddBool("RKnockup", "自动击飞中使用");
                RMenu.AddSlider("RKnockupEnemies", "最小自动击飞敌人数", 2, 1, 5);
            }

            var wEnemies1 = Menu.AddMenu("WEnemie1s", "WE 加速护盾设置");
            foreach (var ally in Allies.Where(a => !a.IsMe))
            {
                wEnemies1.AddSlider(ally.ChampionName + "WEPriority", ally.ChampionName, 1, 0, 5);
            }
            wEnemies1.AddInfo("SupermanInfo2", "0是最低级, 5是最高级", Color.Red);
            wEnemies1.AddKeyBind("Superman", "加速护盾按键!", 'A');

            var ks = Menu.AddMenu("Killsteal", "击杀设置");
            ks.AddBool("KSQ", "使用 Q");
            ks.AddBool("KSE", "使用 E");
            ks.AddBool("KSEQ", "使用 E->Q");

            ManaManager.Initialize(Menu);
            Q.SetManaCondition(ManaManager.ManaMode.Combo, 5);
            Q.SetManaCondition(ManaManager.ManaMode.Harass, 5);
            Q.SetManaCondition(ManaManager.ManaMode.Farm, 30);
            W.SetManaCondition(ManaManager.ManaMode.Combo, 15);
            W.SetManaCondition(ManaManager.ManaMode.Harass, 15);
            E.SetManaCondition(ManaManager.ManaMode.Combo, 10);
            E.SetManaCondition(ManaManager.ManaMode.Harass, 10);
            E.SetManaCondition(ManaManager.ManaMode.Farm, 30);

            Menu.SubMenu("自动眼位").AddItem(new MenuItem("AutoWard", "启动", true).SetValue(true));
            Menu.SubMenu("自动眼位").AddItem(new MenuItem("AutoBuy", "lv9自动买灯泡", true).SetValue(false));
            Menu.SubMenu("自动眼位").AddItem(new MenuItem("AutoPink", "自动真眼扫描", true).SetValue(true));
            Menu.SubMenu("自动眼位").AddItem(new MenuItem("AutoWardCombo", "仅连招模式启动 ", true).SetValue(true));
            new AutoWard().Load();
            new Tracker().Load();

            var SkinMenu = Menu.AddSubMenu(new Menu("换肤设置", "换肤设置"));
            {
                SkinMenu.AddItem(new MenuItem("EnableSkin", "启动换肤").SetValue(false));
                SkinMenu.AddItem(new MenuItem("SkinSelect", "选择皮肤").SetValue(new StringList(new[] { "经典", "邪恶女巫", "驯龙女巫", "泳池派对", "寒冬精灵" })));
            }

            var misc = Menu.AddMenu("Misc", "杂项设置");
            CustomAntiGapcloser.Initialize(misc);
            CustomInterrupter.Initialize(misc);
            misc.AddBool("Support", "辅助模式", false);
            misc.AddKeyBind("Flee", "逃跑按键", 'Z');
            misc.AddBool("FleeMove", "逃跑时移动到鼠标位置");

            var draw = Menu.AddMenu("Drawings", "Drawings");
            draw.AddCircle("DrawQ", "Draw Q", System.Drawing.Color.Purple, Q.Range);
            draw.AddCircle("DrawW", "Draw W/E", System.Drawing.Color.Purple, W.Range);
            draw.AddCircle("DrawR", "Draw R", System.Drawing.Color.Purple, R.Range);
            draw.AddBool("DrawPix", "Draw Pix");
            draw.AddBool("FarmPermashow", "Permashow Farm Enabled");

            Menu.AddToMainMenu();

            var dmg = draw.AddMenu("DamageIndicator", "Damage Indicator");
            dmg.AddBool("DmgEnabled", "Draw Damage Indicator");
            dmg.AddCircle("HPColor", "Predicted Health Color", System.Drawing.Color.White);
            dmg.AddCircle("FillColor", "Damage Color", System.Drawing.Color.MediumPurple);
            dmg.AddBool("Killable", "Killable Text");
            DamageIndicator.Initialize(dmg, Utility.GetComboDamage);

            ManaBarIndicator.Initialize(draw, ManaCostDictionary);
            Pix.Initialize(Menu.Item("DrawPix"));
            SpellManager.Initialize(Menu, Orbwalker);

            if (draw.Item("FarmPermashow").IsActive())
            {
                QMenu.Item("QFarm").Permashow();
            }

            draw.Item("FarmPermashow").ValueChanged += (sender, eventArgs) => { QMenu.Item("QFarm").Permashow(eventArgs.GetNewValue<bool>()); };


            CustomAntiGapcloser.OnEnemyGapcloser += CustomAntiGapcloser_OnEnemyGapcloser;
            CustomInterrupter.OnInterruptableTarget += CustomInterrupter_OnInterruptableTarget;
        }

        private static void setbool()
        {
            AutoWard.Enable = Menu.GetBool("AutoWard");
            AutoWard.AutoBuy = Menu.GetBool("AutoBuy");
            AutoWard.AutoPink = Menu.GetBool("AutoPink");
            AutoWard.OnlyCombo = Menu.GetBool("AutoWardCombo");
            AutoWard.InComboMode = Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo;
        }

        private static void Skin()
        {
            if (Menu.Item("EnableSkin").GetValue<bool>())
            {
                ObjectManager.Player.SetSkin(ObjectManager.Player.CharData.BaseSkinName, Menu.Item("SkinSelect").GetValue<StringList>().SelectedIndex);
            }
            else if (!Menu.Item("EnableSkin").GetValue<bool>())
            {
                ObjectManager.Player.SetSkin(ObjectManager.Player.CharData.BaseSkinName, 0);
            }
        }

        public override void OnCombo(Orbwalking.OrbwalkingMode mode)
        {
            if (W.IsActive() && !W.HasManaCondition() && W.IsReady() && Menu.Item("WPriority").IsActive())
            {
                var wTarg = Utility.GetBestWTarget();
                if (wTarg != null && W.CanCast(wTarg) && W.CastOnUnit(wTarg))
                {
                    Console.WriteLine("[AUTO] Cast W");
                    return;
                }
            }

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical) ?? Pix.GetTarget();

            if (!target.IsValidTarget() || !SpellManager.Q.IsInRange(target))
            {
                PixCombo();
                return;
            }

            if (PixCombo())
            {
                return;
            }

            if (E.IsActive() && !E.HasManaCondition() && E.CanCast(target) && E.CastOnUnit(target))
            {
                Console.WriteLine("[Combo] Cast E");
                return;
            }

            if (Q.IsReady() && W.IsActive() && !W.HasManaCondition() && W.CanCast(target) && W.CastOnUnit(target))
            {
                Console.WriteLine("[Combo] Cast W");
                return;
            }

            if (!Q.IsActive() || !Q.IsReady() || Q.HasManaCondition())
            {
                return;
            }

            if (Q.Cast(target).IsCasted())
            {
                Console.WriteLine("[Combo] Cast Q");
            }
        }

        public static void GameOnOnGameLoad(EventArgs args)
        {
            Task.Factory.StartNew(
                () =>
                {Program.GameOnOnGameLoad();
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

        private static bool PixCombo(Obj_AI_Hero target, bool useQ, bool useE, bool killSteal = false)
        {
            if (!target.IsValidTarget() || !Pix.IsValid())
            {
                return false;
            }

            useQ &= Q.IsReady() && (killSteal || !Q.HasManaCondition());
            useE &= useQ && E.IsReady() && (killSteal || !E.HasManaCondition()) &&
                    Player.Mana > ManaCostDictionary[Q.Slot][Q.Level] + ManaCostDictionary[E.Slot][E.Level];

            if (useQ && SpellManager.PixQ.IsInRange(target) && SpellManager.PixQ.Cast(target).IsCasted())
            {
                Console.WriteLine("[Pix] Cast Q");
                return true;
            }

            if (!useE)
            {
                return false;
            }

            var eqTarget = Pix.GetETarget(target);
            if (eqTarget == null || !E.CastOnUnit(eqTarget))
            {
                return false;
            }

            Console.WriteLine("[Pix] Cast E");
            return true;
        }

        private static bool PixCombo()
        {
            var mode = Orbwalker.ActiveMode.GetModeString();
            var target = Pix.GetTarget(Q.Range + E.Range);
            return PixCombo(target, Menu.Item("QPix" + mode).IsActive(), Menu.Item("EQPix" + mode).IsActive());
        }

        public override void OnJungle(Orbwalking.OrbwalkingMode activeMode)
        {
            if (!Menu.Item("EJG").IsActive() || !E.IsReady() || E.HasManaCondition())
            {
                return;
            }

            var mobs = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (mobs.Count > 0)
            {
                var mob = mobs[0];

                if (E.IsReady())
                {
                    E.Cast(mob);
                }
            }
        }

        public override void OnFarm(Orbwalking.OrbwalkingMode mode)
        {
            if (!Menu.Item("QFarm").IsActive() || !Q.IsReady() || Q.HasManaCondition())
            {
                return;
            }

            var condition = mode == Orbwalking.OrbwalkingMode.LaneClear ? Menu.Item("QLC") : Menu.Item("QLH");

            if (condition == null || !condition.IsActive())
            {
                return;
            }

            var qMinions = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly);
            var killable = qMinions.FirstOrDefault(o => o.Health < Q.GetDamage(o));

            if (killable != null && !killable.CanAAKill() && Q.Cast(killable).IsCasted())
            {
                return;
            }

            var pixMinions = Pix.GetMinions();
            killable = pixMinions.FirstOrDefault(o => o.Health < Q.GetDamage(o));

            if (Pix.IsValid() && killable != null && !killable.CanAAKill() &&
                SpellManager.PixQ.Cast(killable).IsCasted())
            {
                return;
            }

            if (mode == Orbwalking.OrbwalkingMode.LastHit)
            {
                return;
            }

            var pos = Q.GetLineFarmLocation(qMinions);
            var spell = Q;

            var pixPos = Pix.GetFarmLocation();

            if (Pix.IsValid() && pixPos.MinionsHit > pos.MinionsHit)
            {
                pos = pixPos;
                spell = SpellManager.PixQ;
            }

            if (pos.MinionsHit > 2 && spell.Cast(pos.Position)) {}
        }

        public override void OnUpdate()
        {
            if (Player.IsDead)
            {
                return;
            }

            if (Player.IsRecalling())
            {
                return;
            }

            setbool();
            Skin();

            if (Saver())
            {
                return;
            }

            if (Flee())
            {
                return;
            }

            if (AutoQ())
            {
                return;
            }

            if (Superman())
            {
                return;
            }

            if (AutoR())
            {
                return;
            }

            if (Killsteal()) {}
        }

        private static bool Killsteal()
        {
            var mana = Player.Mana;
            var useQ = Menu.Item("KSQ").IsActive() && Q.IsReady();
            var useE = Menu.Item("KSE").IsActive() && E.IsReady();
            var useEQ = Menu.Item("KSEQ").IsActive() && Player.Mana > Q.ManaCost + E.ManaCost;

            if (!useQ && !useE)
            {
                return false;
            }

            foreach (var enemy in
                Enemies.Where(e => e.IsValidTarget(E.Range + Q.Range) && !e.IsZombie).OrderBy(e => e.Health))
            {
                var qDmg = Q.GetDamage(enemy);
                var eDmg = E.GetDamage(enemy);

                if (useE && E.IsInRange(enemy))
                {
                    if (eDmg > enemy.Health && E.CastOnUnit(enemy))
                    {
                        return true;
                    }

                    if (useQ && qDmg + eDmg > enemy.Health && useEQ && E.CastOnUnit(enemy))
                    {
                        QAfterETarget = enemy;
                        return true;
                    }
                }


                if (useQ && qDmg > enemy.Health && Q.IsInRange(enemy) && Q.Cast(enemy).IsCasted())
                {
                    return true;
                }

                if (useQ && useE && useEQ && qDmg > enemy.Health && PixCombo(enemy, true, true, true))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool Saver()
        {
            if (Player.InFountain())
            {
                return false;
            }

            var useE = Menu.Item("EAuto").IsActive() && E.IsReady();
            var useR = Menu.Item("RAuto").IsActive() && R.IsReady();

            if (!useE && !useR)
            {
                return false;
            }

            foreach (var ally in Allies.Where(h => h.IsValidTarget(R.Range, false) && h.CountEnemiesInRange(300) > 0))
            {
                var hp = ally.GetPredictedHealthPercent();

                if (useE && E.IsInRange(ally) &&
                    hp <= Menu.Item(ally.ChampionName + "EPriority").GetValue<Slider>().Value)
                {
                    E.CastOnUnit(ally);
                }

                if (useR && hp <= Menu.Item(ally.ChampionName + "RPriority").GetValue<Slider>().Value &&
                    R.CastOnUnit(ally))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool AutoQ()
        {
            return Menu.Item("QImpaired").IsActive() && Q.IsReady() &&
                   Enemies.Any(e => e.IsValidTarget(Q.Range) && e.IsMovementImpaired() && Q.Cast(e).IsCasted());
        }

        private static bool Superman()
        {
            if (!Menu.Item("Superman").IsActive() || !(W.IsReady() || E.IsReady()))
            {
                return false;
            }

            var target = Utility.GetBestWETarget();

            if (target == null)
            {
                Console.WriteLine("TARG");
                return false;
            }

            if (W.IsReady() && W.IsInRange(target) && W.CastOnUnit(target)) {}

            return E.IsReady() && E.IsInRange(target) && E.CastOnUnit(target);
        }

        private static bool AutoR()
        {
            if (!R.IsReady() || Player.InFountain())
            {
                return false;
            }

            if (Menu.Item("RForce").IsActive() &&
                Allies.Where(h => h.IsValidTarget(R.Range, false)).OrderBy(o => o.Health).Any(o => R.CastOnUnit(o)))
            {
                return true;
            }


            if (!Menu.Item("RKnockup").IsActive())
            {
                return false;
            }

            var count = 0;
            var bestAlly = Player;
            foreach (var ally in Allies.Where(a => a.IsValidTarget(R.Range, false)))
            {
                var c = ally.CountEnemiesInRange(RRadius);

                if (c <= count)
                {
                    continue;
                }

                count = c;
                bestAlly = ally;
            }

            return count >= Menu.Item("RKnockupEnemies").GetValue<Slider>().Value && R.CastOnUnit(bestAlly);
        }

        private static bool Flee()
        {
            if (!Menu.Item("Flee").IsActive())
            {
                return false;
            }

            if (Player.IsDashing())
            {
                return true;
            }

            Orbwalker.ActiveMode = Orbwalking.OrbwalkingMode.None;

            if (Menu.Item("FleeW").IsActive() && W.IsReady() && W.CastOnUnit(Player))
            {
                return true;
            }

            if (!Menu.Item("FleeMove").IsActive() || Player.GetWaypoints().Last().Distance(Game.CursorPos) < 100)
            {
                return true;
            }

            return Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
        }

        public override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.Slot == SpellSlot.W)
                {
                    LastWCast = Utils.TickCount;
                }

                if (args.Slot == SpellSlot.E && QAfterETarget != null)
                {
                    LeagueSharp.Common.Utility.DelayAction.Add(
                        100, () =>
                        {
                            SpellManager.PixQ.Cast(QAfterETarget);
                            QAfterETarget = null;
                        });
                }

                return;
            }

            if (!(Menu.Item("EAuto").IsActive() && E.IsReady()) || !(Menu.Item("RAuto").IsActive() && R.IsReady()))
            {
                return;
            }

            var caster = sender as Obj_AI_Hero;
            var target = args.Target as Obj_AI_Hero;

            if (caster == null || !caster.IsValid || !caster.IsEnemy || target == null || !target.IsValid ||
                !target.IsAlly)
            {
                return;
            }

            var damage = 0d;
            try
            {
                damage = caster.GetSpellDamage(target, args.SData.Name);
            }
            catch {}

            var hp = (target.Health - damage) / target.MaxHealth * 100;

            if (E.CanCast(target) && hp <= Menu.Item(target.ChampionName + "EPriority").GetValue<Slider>().Value)
            {
                E.CastOnUnit(target);
            }

            if (R.CanCast(target) && hp <= Menu.Item(target.ChampionName + "RPriority").GetValue<Slider>().Value)
            {
                R.CastOnUnit(target);
            }
        }

        public override void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            foreach (var spell in new[] { "Q", "W", "R" })
            {
                var circle = Menu.Item("Draw" + spell).GetValue<Circle>();
                if (circle.Active)
                {
                    Render.Circle.DrawCircle(Player.Position, circle.Radius, circle.Color);
                }
            }
        }

        public override void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (!Menu.Item("Support").GetValue<bool>() ||
                !HeroManager.Allies.Any(x => x.IsValidTarget(1000, false) && !x.IsMe))
            {
                return;
            }

            if (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Mixed &&
                Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LastHit)
            {
                return;
            }

            var minion = args.Target as Obj_AI_Base;
            if (minion != null && minion.IsMinion && minion.IsValidTarget())
            {
                args.Process = false;
            }
        }

        public void CustomInterrupter_OnInterruptableTarget(Obj_AI_Hero sender,
            CustomInterrupter.InterruptableTargetEventArgs args)
        {
            if (sender == null || !sender.IsValidTarget())
            {
                return;
            }

            if (Utils.TickCount - LastWCast < 2000)
            {
                return;
            }

            if (Menu.Item("WInterrupter").IsActive() && W.CanCast(sender) && W.CastOnUnit(sender))
            {
                return;
            }

            if (!Menu.Item("RInterrupter").IsActive() || !R.IsReady())
            {
                return;
            }

            if (
                Allies.OrderBy(h => h.Distance(sender))
                    .Any(h => h.IsValidTarget(R.Range, false) && h.Distance(sender) < RRadius && R.CastOnUnit(h))) {}
        }

        private static void CustomAntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!gapcloser.Sender.IsValidTarget())
            {
                return;
            }

            if (Menu.Item("QGapcloser").IsActive() && Q.CanCast(gapcloser.Sender))
            {
                Q.Cast(gapcloser.Sender);
            }

            if (Menu.Item("WGapcloser").IsActive() && W.CanCast(gapcloser.Sender) && W.CastOnUnit(gapcloser.Sender)) {}
        }
    }
}