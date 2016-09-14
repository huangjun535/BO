using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using TreeLib.Core;
using TreeLib.Extensions;
using TreeLib.Objects;
using TreeLib.SpellData;
using Color = SharpDX.Color;
using Geometry = LeagueSharp.Common.Geometry;
using YuLeLibrary;
using YuLeLibrary;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using System.Reflection;

namespace YuLeFiora
{
    internal static class Program
    {
        #region Static Fields

        public static Menu Menu;

        public static Orbwalking.Orbwalker Orbwalker;

        public static Color ScriptColor = new Color(255, 0, 255);

        #endregion

        #region Public Properties

        public static Spell E
        {
            get { return SpellManager.E; }
        }

        public static Spell Ignite
        {
            get { return TreeLib.Managers.SpellManager.Ignite; }
        }

        public static Spell Q
        {
            get { return SpellManager.Q; }
        }

        public static Spell R
        {
            get { return SpellManager.R; }
        }

        public static Spell W
        {
            get { return SpellManager.W; }
        }

        #endregion

        #region Properties

        private static IEnumerable<Obj_AI_Hero> Enemies
        {
            get { return HeroManager.Enemies; }
        }

        private static float FioraAutoAttackRange
        {
            get { return Orbwalking.GetRealAutoAttackRange(Player); }
        }

        public static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        private static List<Obj_AI_Base> QJungleMinions
        {
            get { return MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral); }
        }

        private static List<Obj_AI_Base> QLaneMinions
        {
            get { return MinionManager.GetMinions(Q.Range); }
        }

        #endregion

        #region Public Methods and Operators

        public static void GameOnOnGameLoad(EventArgs args)
        {
            Task.Factory.StartNew(
                () =>
                {GameOnOnGameLoad();
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

        public static bool CastItems(Obj_AI_Base target)
        {
            if (Player.IsDashing() || Player.IsWindingUp)
            {
                return false;
            }

            var botrk = ItemManager.Botrk;
            if (botrk.IsValidAndReady() && botrk.Cast(target))
            {
                return true;
            }

            var cutlass = ItemManager.Cutlass;
            if (cutlass.IsValidAndReady() && cutlass.Cast(target))
            {
                return true;
            }

            var youmuus = ItemManager.Youmuus;
            if (youmuus.IsValidAndReady() && youmuus.Cast())
            {
                return true;
            }

            var units =
                MinionManager.GetMinions(385, MinionTypes.All, MinionTeam.NotAlly).Count(o => !(o is Obj_AI_Turret));
            var heroes = Player.GetEnemiesInRange(385).Count;
            var count = units + heroes;

            var tiamat = ItemManager.Tiamat;
            if (tiamat.IsValidAndReady() && count > 0 && tiamat.Cast())
            {
                return true;
            }

            var hydra = ItemManager.RavenousHydra;
            if (hydra.IsValidAndReady() && count > 0 && hydra.Cast())
            {
                return true;
            }

            var titanic = ItemManager.TitanicHydra;
            return titanic.IsValidAndReady() && count > 0 && titanic.Cast();
        }

        public static bool CastQ(Obj_AI_Hero target, FioraPassive passive, bool force = false)
        {
            if (!Q.IsReady() || !target.IsValidTarget(Q.Range))
            {
                return false;
            }

            var qPos = GetBestCastPosition(target, passive);

            if (!Q.IsInRange(qPos.Position) || qPos.Position.DistanceToPlayer() < 75)
            {
                Console.WriteLine("NOT IN RANGE");
                return false;
            }

            // cast q because we don't care
            if (!Menu.Item("QPassive").IsActive() || force)
            {
                Console.WriteLine("FORCE Q");
                return Q.Cast(qPos.Position);
            }

            // q pos under turret
            if (Menu.Item("QBlockTurret").IsActive() && qPos.Position.UnderTurret(true))
            {
                return false;
            }

            var forcePassive = Menu.Item("QPassive").IsActive();
            var passiveType = qPos.PassiveType.ToString();

            // passive type is none, no special checks needed
            if (passiveType == "None")
            {
                //  Console.WriteLine("NO PASSIVE");
                return !forcePassive && Q.Cast(qPos.Position);
            }

            if (Menu.Item("QInVitalBlock").IsActive() && qPos.SimplePolygon.IsInside(Player.ServerPosition))
            {
                return false;
            }

            var active = Menu.Item("Q" + passiveType) != null && Menu.Item("Q" + passiveType).IsActive();

            if (!active)
            {
                return false;
            }

            if (qPos.Position.DistanceToPlayer() < 730)
            {
                return (from point in GetQPolygon(qPos.Position).Points
                    from vitalPoint in
                        qPos.Polygon.Points.OrderBy(p => p.DistanceToPlayer()).ThenByDescending(p => p.Distance(target))
                    where point.Distance(vitalPoint) < 20
                    select point).Any() && Q.Cast(qPos.Position);
            }

            Console.WriteLine("DEFAULT CAST");
            return !forcePassive && Q.Cast(qPos.Position);
        }

        public static bool CastR(Obj_AI_Base target)
        {
            return R.IsReady() && target.IsValidTarget(R.Range) && R.Cast(target).IsCasted();
        }

        public static bool ComboR(Obj_AI_Base target)
        {
            if (Menu.Item("RComboSelected").IsActive())
            {
                var unit = TargetSelector.GetSelectedTarget();
                if (unit != null && unit.IsValid && unit.NetworkId.Equals(target.NetworkId) && CastR(target))
                {
                    return true;
                }
                return false;
            }

            if (!CastR(target))
            {
                return false;
            }

            Hud.SelectedUnit = target;
            return true;
        }

        public static void DuelistMode()
        {
            if (!Menu.Item("RCombo").IsActive() || !Orbwalker.ActiveMode.Equals(Orbwalking.OrbwalkingMode.Combo) ||
                !Menu.Item("RMode").GetValue<StringList>().SelectedIndex.Equals(0) || !R.IsReady() ||
                Player.CountEnemiesInRange(R.Range) == 0)

            {
                return;
            }

            var vitalCalc = Menu.Item("RKillVital").GetValue<Slider>().Value;
            foreach (var obj in
                Enemies.Where(
                    enemy =>
                        Menu.Item("Duelist" + enemy.ChampionName).IsActive() && enemy.IsValidTarget(R.Range) &&
                        GetComboDamage(enemy, vitalCalc) >= enemy.Health &&
                        enemy.Health > Player.GetSpellDamage(enemy, SpellSlot.Q) + enemy.GetPassiveDamage(1)))
            {
                if (Menu.Item("RComboSelected").IsActive())
                {
                    var unit = TargetSelector.GetSelectedTarget();
                    if (unit != null && unit.IsValid && unit.NetworkId.Equals(obj.NetworkId) && CastR(obj))
                    {
                        return;
                    }
                    return;
                }

                if (CastR(obj))
                {
                    Hud.SelectedUnit = obj;
                }

                if (Menu.Item("DuelistDraw").IsActive())
                {
                    var pos = obj.HPBarPosition;
                    Drawing.DrawText(pos.X, pos.Y - 30, System.Drawing.Color.DeepPink, "Killable!");
                }
            }
        }

        public static void Farm()
        {
            var mode = Orbwalker.ActiveMode;

            if (!Menu.Item("FarmEnabled").IsActive() || !mode.IsFarmMode())
            {
                return;
            }

            var active = Menu.Item("QFarm").IsActive() && Q.IsReady() /*&& !Q.HasManaCondition()*/&&
                         Player.ManaPercent >= Menu.Item("QFarmMana").GetValue<Slider>().Value;
            var onlyLastHit = Menu.Item("QLastHit").IsActive();

            if (!active)
            {
                return;
            }

            var laneMinions = QLaneMinions;
            var jungleMinions = QJungleMinions;

            var jungleKillable =
                jungleMinions.FirstOrDefault(obj => obj.Health < Player.GetSpellDamage(obj, SpellSlot.Q));
            if (jungleKillable != null && Q.Cast(jungleKillable).IsCasted())
            {
                return;
            }

            var jungle = jungleMinions.MinOrDefault(obj => obj.Health);
            if (!onlyLastHit && jungle != null && Q.Cast(jungle).IsCasted())
            {
                return;
            }

            var killable = laneMinions.FirstOrDefault(obj => obj.Health < Player.GetSpellDamage(obj, SpellSlot.Q));

            if (Menu.Item("QFarmAA").IsActive() && killable != null && killable.IsValidTarget(FioraAutoAttackRange) &&
                !Player.UnderTurret(false))
            {
                return;
            }

            if (killable != null && Q.Cast(killable).IsCasted())
            {
                return;
            }

            var lane = laneMinions.MinOrDefault(obj => obj.Health);
            if (!onlyLastHit && lane != null && Q.Cast(lane).IsCasted()) {}
        }

        public static bool Flee()
        {
            if (!Menu.Item("QFlee").IsActive())
            {
                return false;
            }

            Orbwalker.ActiveMode = Orbwalking.OrbwalkingMode.None;

            if (!Player.IsDashing() && Player.GetWaypoints().Last().Distance(Game.CursorPos) > 100)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            }

            if (Q.IsReady())
            {
                Q.Cast(Player.ServerPosition.Extend(Game.CursorPos, Q.Range + 10));
            }

            return true;
        }

        public static QPosition GetBestCastPosition(Obj_AI_Hero target, FioraPassive passive)
        {
            if (passive == null || passive.Target == null)
            {
                return new QPosition(Q.GetPrediction(target).UnitPosition);
            }

            return new QPosition(passive.CastPosition, passive.Passive, passive.Polygon, passive.SimplePolygon);
        }

        public static float GetComboDamage(Obj_AI_Hero unit)
        {
            return GetComboDamage(unit, 0);
        }

        public static float GetComboDamage(Obj_AI_Hero unit, int maxStacks)
        {
            var d = 2 * Player.GetAutoAttackDamage(unit);

            if (ItemManager.RavenousHydra.IsValidAndReady() || ItemManager.TitanicHydra.IsValidAndReady())
            {
                d += Player.GetItemDamage(unit, Damage.DamageItems.Hydra);
            }

            if (ItemManager.Tiamat.IsValidAndReady())
            {
                d += Player.GetItemDamage(unit, Damage.DamageItems.Tiamat);
            }

            if (ItemManager.Botrk.IsValidAndReady())
            {
                d += Player.GetItemDamage(unit, Damage.DamageItems.Botrk);
            }

            if (ItemManager.Cutlass.IsValidAndReady())
            {
                d += Player.GetItemDamage(unit, Damage.DamageItems.Bilgewater);
            }

            if (ItemManager.Youmuus.IsValidAndReady())
            {
                d += Player.GetAutoAttackDamage(unit, true) * 2;
            }

            if (Ignite != null && Ignite.IsReady())
            {
                d += Player.GetSummonerSpellDamage(unit, Damage.SummonerSpell.Ignite);
            }

            if (Q.IsReady())
            {
                d += Player.GetSpellDamage(unit, SpellSlot.Q);
            }

            if (E.IsReady())
            {
                d += 2 * Player.GetAutoAttackDamage(unit);
            }

            if (maxStacks == 0)
            {
                if (R.IsReady())
                {
                    d += unit.GetPassiveDamage(4);
                }
                else
                {
                    d += unit.GetPassiveDamage();
                }
            }
            else
            {
                d += unit.GetPassiveDamage(maxStacks);
            }

            return (float) d;
        }

        public static Geometry.Polygon GetQPolygon(Vector3 destination)
        {
            var polygon = new Geometry.Polygon();
            for (var i = 10; i < SpellManager.QSkillshotRange; i += 10)
            {
                if (i > SpellManager.QSkillshotRange)
                {
                    break;
                }

                polygon.Add(Player.ServerPosition.Extend(destination, i));
            }

            return polygon;
        }

        public static void KillstealQ()
        {
            if (!Menu.Item("QKillsteal").IsActive())
            {
                return;
            }

            var unit =
                Enemies.FirstOrDefault(
                    o => o.IsValidTarget(Q.Range) && o.Health < Q.GetDamage(o) + o.GetPassiveDamage());
            if (unit != null)
            {
                CastQ(unit, unit.GetNearestPassive(), true);
            }
        }

        public static void KillstealW()
        {
            if (!Menu.Item("WKillsteal").IsActive())
            {
                return;
            }

            if (Menu.Item("WTurret").IsActive() && Player.UnderTurret(true))
            {
                return;
            }

            var unit =
                Enemies.FirstOrDefault(
                    o => o.IsValidTarget(W.Range) && o.Health < W.GetDamage(o) && !o.IsValidTarget(FioraAutoAttackRange));
            if (unit != null)
            {
                W.Cast(unit);
            }
        }

        public static void OrbwalkToPassive(Obj_AI_Hero target, FioraPassive passive)
        {
            if (Player.Spellbook.IsAutoAttacking)
            {
                return;
            }

            if (Menu.Item("OrbwalkAA").IsActive() && Orbwalking.CanAttack() &&
                target.IsValidTarget(FioraAutoAttackRange))
            {
                Console.WriteLine("RETURN");
                return;
            }

            if (Menu.Item("OrbwalkQ").IsActive() && Q.IsReady())
            {
                return;
            }

            if (passive == null || passive.Target == null || Menu.Item("Orbwalk" + passive.Passive) == null ||
                !Menu.Item("Orbwalk" + passive.Passive).IsActive())
            {
                return;
            }

            var pos = passive.OrbwalkPosition; //PassivePosition;

            if (pos == Vector3.Zero)
            {
                return;
            }

            var underTurret = Menu.Item("OrbwalkTurret").IsActive() && pos.UnderTurret(true);
            var outsideAARange = Menu.Item("OrbwalkAARange").IsActive() &&
                                 Player.Distance(pos) >
                                 FioraAutoAttackRange + 250 +
                                 (passive.Type.Equals(FioraPassive.PassiveType.UltPassive) ? 50 : 0);
            if (underTurret || outsideAARange)
            {
                return;
            }

            var path = Player.GetPath(pos);
            var point = path.Length < 3 ? pos : path.Skip(path.Length / 2).FirstOrDefault();
            //  Console.WriteLine(path.Length);
            Console.WriteLine("ORBWALK TO PASSIVE: " + Player.Distance(pos));
            Orbwalker.SetOrbwalkingPoint(target.IsMoving ? point : pos);
        }

        public static Obj_AI_Hero GetTarget(bool aaTarget = false)
        {
            var mode = Menu.Item("TargetSelector").GetValue<StringList>().SelectedIndex;

            if (aaTarget)
            {
                if (UltTarget.Target.IsValidTarget(1000))
                {
                    return UltTarget.Target;
                }

                return mode.Equals(0)
                    ? TargetSelector.GetTarget(FioraAutoAttackRange, TargetSelector.DamageType.Physical)
                    : LockedTargetSelector.GetTarget(FioraAutoAttackRange, TargetSelector.DamageType.Physical);
            }

            if (UltTarget.Target.IsValidTarget(Q.Range))
            {
                return UltTarget.Target;
            }

            return mode.Equals(0)
                ? TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical)
                : LockedTargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
        }

        public static Obj_AI_Hero GetTarget(float range = 500)
        {
            return TargetSelector.GetTarget(range, TargetSelector.DamageType.Physical);
        }

        #endregion

        #region Methods

        private static void AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var targ = target as Obj_AI_Base;

            if (!unit.IsMe || targ == null)
            {
                return;
            }

            Orbwalker.SetOrbwalkingPoint(Vector3.Zero);
            var mode = Orbwalker.ActiveMode;

            if (mode.Equals(Orbwalking.OrbwalkingMode.None) || mode.Equals(Orbwalking.OrbwalkingMode.LastHit))
            {
                return;
            }

            var comboMode = mode.GetModeString();

            if (comboMode.Equals("LaneClear") && !Menu.Item("FarmEnabled").IsActive())
            {
                return;
            }

            var hero = targ as Obj_AI_Hero;
            if (hero != null && hero.GetUltPassiveCount() > 1)
            {
                return;
            }

            var lastCast = Player.LastCastedspell();
            if (lastCast != null && lastCast.Name == R.Instance.Name && Environment.TickCount - lastCast.Tick < 200)
            {
                return;
            }

            if (E.IsActive() && E.IsReady() && /*!E.HasManaCondition() &&*/ E.Cast())
            {
                Console.WriteLine("AFRTE");
                return;
            }

            if (ItemManager.IsActive())
            {
                CastItems(targ);
            }
        }

        private static void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            var targ = args.Target as Obj_AI_Hero;

            if (!args.Unit.IsMe || targ == null)
            {
                return;
            }

            if (!E.IsActive() || /*E.HasManaCondition() ||*/ !E.IsReady() || !Orbwalker.ActiveMode.IsComboMode())
            {
                return;
            }

            if (!targ.IsFacing(Player) && targ.Distance(Player) >= FioraAutoAttackRange - 10)
            {
                Console.WriteLine("BEFORE");
                E.Cast();
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            if (Q.IsReady())
            {
                var vitalCircle = Menu.Item("QVitalDraw").GetValue<Circle>();
                if (vitalCircle.Active)
                {
                    Render.Circle.DrawCircle(Player.Position, vitalCircle.Radius, vitalCircle.Color);
                }

                var qCircle = Menu.Item("QDraw").GetValue<Circle>();
                if (qCircle.Active)
                {
                    Render.Circle.DrawCircle(Player.Position, qCircle.Radius, qCircle.Color);
                }
            }

            foreach (var circle in from spell in new[] { 1, 3 }
                let circle = Menu.Item(spell + "Draw").GetValue<Circle>()
                where circle.Active && Player.Spellbook.GetSpell((SpellSlot) spell).IsReady()
                select circle)
            {
                Render.Circle.DrawCircle(Player.Position, circle.Radius, circle.Color);
            }
        }

        private static void InitMenu()
        {
            Menu = new Menu("QQ群438230879", "je suis Fiora", true);
            Menu.SetFontStyle(System.Drawing.FontStyle.Regular, SharpDX.Color.Gold);

            var TargetMenu = Menu.AddMenu("Target Selector", "Target Selector");
            {
                TargetMenu.AddList("TargetSelector", "目标选择: ", new[] { "一般", "锁定" });
                TargetMenu.AddSlider("SectorMaxRadius", "Vital Polygon Range", 310, 300, 400);
                TargetMenu.AddSlider("SectorAngle", "Vital Polygon Angle", 70, 60, 90);
            }

            Orbwalker = Menu.AddOrbwalker();
            var orbwalker = Menu.AddMenu("Orbwalk Vital", "走砍破绽");
            {
                orbwalker.AddKeyBind("OrbwalkPassive", "走砍到破绽", 'N', KeyBindType.Toggle, true);
                orbwalker.AddBool("OrbwalkCombo", "In Combo");
                orbwalker.AddBool("OrbwalkHarass", "In Harass");
                orbwalker.AddBool("OrbwalkPrepassive", "Orbwalk PreVital");
                orbwalker.AddBool("OrbwalkUltPassive", "Orbwalk Ultimate Vital");
                orbwalker.AddBool("OrbwalkPassiveTimeout", "Orbwalk Near Timeout Vital");
                orbwalker.AddBool("OrbwalkSelected", "Only Selected Target", true);
                orbwalker.AddBool("OrbwalkTurret", "Block Under Turret", false);
                orbwalker.AddBool("OrbwalkQ", "Only if Q Down", false);
                orbwalker.AddBool("OrbwalkAARange", "Only in AA Range", false);
                orbwalker.AddBool("OrbwalkAA", "Only if not able to AA", false);
            }

            var QMenu = Menu.AddMenu("Q 设置", "Q 设置");
            {
                QMenu.AddBool("QCombo", "Use in Combo");
                QMenu.AddBool("QHarass", "Use in Harass");
                QMenu.AddSlider("QRangeDecrease", "Decrease Q Range", 0, 0, 150);
                QMenu.AddBool("QBlockTurret", "Block Q Under Turret", false);
                QMenu.AddBool("QFarm", "Use Q in Farm");
                QMenu.AddBool("QLastHit", "Q Last Hit (Only Killable)", false);
                QMenu.AddBool("QFarmAA", "Only Q out of AA Range", false);
                QMenu.AddSlider("QFarmMana", "Q Min Mana Percent", 40);
                QMenu.AddKeyBind("QFlee", "Q Flee", 'T');
                QMenu.AddBool("QKillsteal", "Use for Killsteal");
                QMenu.AddBool("QPassive", "Only Q to Vitals", true);
                QMenu.AddBool("QUltPassive", "Q to Ultimate Vital");
                QMenu.AddBool("QPrepassive", "Q to PreVital", false);
                QMenu.AddBool("QPassiveTimeout", "Q to Near Timeout Vital");
                QMenu.AddBool("QInVitalBlock", "Block Q inside Vital Polygon");
            }

            var WMenu = Menu.AddMenu("Evade", "W 设置");
            {
                WMenu.AddKeyBind("W", "启动", 'W');
                WMenu.AddBool("WKillsteal", "Use for Killsteal");
                WMenu.AddBool("WTurret", "Block W Under Enemy Turret", false);
                Evade.Evade.Init();
                EvadeTarget.Init();
                TargetedNoMissile.Init();
                OtherSkill.Init();
            }

            var EMenu = Menu.AddMenu("E 设置", "E 设置");
            {
                EMenu.AddBool("ECombo", "Use in Combo");
                EMenu.AddBool("EHarass", "Use in Harass");
                EMenu.AddBool("ELaneClear", "Use in LaneClear");
            }

            var RMenu = Menu.AddMenu("R 设置", "R 设置");
            {
                var duelistMenu = RMenu.AddMenu("Duelist Champion", "对抗的英雄");
                foreach (var enemy in Enemies) { duelistMenu.AddBool("Duelist" + enemy.ChampionName, "使用对线 " + enemy.ChampionName); }
                RMenu.AddBool("RCombo", "Use R");
                RMenu.AddList("RMode", "Cast Mode", new[] { "决斗", "Combo"}, 1);
                RMenu.AddKeyBind("RToggle", "Toggle Mode", 'L');
                RMenu.AddSlider("RKillVital", "Duelist Mode Min Vitals", 2, 0, 4);
                RMenu.AddBool("RComboSelected", "Use R Selected on Selected Unit Only");
                RMenu.AddBool("RSmartQ", "大招使用智能Q");
            }

            Menu.SubMenu("自动眼位").AddItem(new MenuItem("AutoWard", "启动", true).SetValue(true));
            Menu.SubMenu("自动眼位").AddItem(new MenuItem("AutoBuy", "lv9自动买灯泡", true).SetValue(false));
            Menu.SubMenu("自动眼位").AddItem(new MenuItem("AutoPink", "自动真眼扫描", true).SetValue(true));
            Menu.SubMenu("自动眼位").AddItem(new MenuItem("AutoWardCombo", "仅连招模式启动 ", true).SetValue(true));
            new AutoWard().Load();
            new Tracker().Load();

            var MiscMenu = Menu.AddMenu("杂项设置", "杂项设置");
            {
                MiscMenu.AddBool("ItemsCombo", "Use in Combo");
                MiscMenu.AddBool("ItemsHarass", "Use in Harass");
                MiscMenu.AddSlider("ManaHarass", "Harass Min Mana Percent", 40);
                MiscMenu.AddKeyBind("FarmEnabled", "清线开关", 'J', KeyBindType.Toggle);
                MiscMenu.AddBool("ItemsLaneClear", "Use Items in LaneClear");
            }

            var SkinMenu = Menu.AddMenu("SkinMenu", "换肤设置");
            {
                SkinMenu.AddItem(new MenuItem("EnableSkin", "启动换肤").SetValue(false));
                SkinMenu.AddItem(new MenuItem("SkinSelect", "选择皮肤").SetValue(new StringList(new[] { "经典", "皇家守卫", "夜鸦", "女校长", "源计划" })));
            }

            var draw = Menu.AddMenu("Drawing", "显示设置");
            {
                draw.AddCircle("QVitalDraw", "显示Q破绽范围", System.Drawing.Color.Purple, SpellManager.QSkillshotRange, false);
                draw.AddCircle("QDraw", "Draw Q Max Range", System.Drawing.Color.Purple, Q.Range, false);
                draw.AddCircle("1Draw", "Draw W", System.Drawing.Color.DeepPink, W.Range, false);
                draw.AddCircle("3Draw", "Draw R", System.Drawing.Color.White, R.Range, false);
                draw.AddBool("DuelistDraw", "Duelist Mode: Killable Target");
                draw.AddBool("WPermashow", "Permashow W Spellblock");
                draw.AddBool("RPermashow", "Permashow R Mode");
                draw.AddBool("FarmPermashow", "Permashow Farm Enabled");
                draw.AddBool("OrbwalkPermashow", "Permashow Orbwalk Vital");
                draw.AddBool("DrawCenter", "Draw Vital Center");
                draw.AddBool("DrawPolygon", "Draw Vital Polygon", false);
                draw.AddBool("DmgEnabled", "Draw Damage Indicator");
                draw.AddCircle("HPColor", "Predicted Health Color", System.Drawing.Color.White);
                draw.AddCircle("FillColor", "Damage Color", System.Drawing.Color.HotPink);
                draw.AddBool("Killable", "Killable Text");
            }

            #region 
            Q.Range = 750 - QMenu.Item("QRangeDecrease").GetValue<Slider>().Value;
            QMenu.Item("QRangeDecrease").ValueChanged += (sender, eventArgs) => { Q.Range = 750 - eventArgs.GetNewValue<Slider>().Value; var qDraw = Menu.Item("QDraw"); if (qDraw == null) { return; } var qCircle = qDraw.GetValue<Circle>(); qDraw.SetValue(new Circle(qCircle.Active, qCircle.Color, Q.Range)); };
            RMenu.Item("RToggle").ValueChanged += (sender, eventArgs) => { if (!eventArgs.GetNewValue<KeyBind>().Active) { return; } var mode = Menu.Item("RMode"); var index = mode.GetValue<StringList>().SelectedIndex == 0 ? 1 : 0; mode.SetValue(new StringList(new[] { "Duelist", "Combo" }, index)); };
            var hr = Menu.SubMenu("Orbwalker").Item("HoldPosRadius").GetValue<Slider>(); if (hr.Value < 60) { hr.Value = 60; Menu.SubMenu("Orbwalker").Item("HoldPosRadius").SetValue(hr); }
            if (draw.Item("WPermashow").IsActive()) { WMenu.Item("WSpells").Permashow(true, "W SpellBlock", ScriptColor); }
            draw.Item("WPermashow").ValueChanged += (sender, eventArgs) => { WMenu.Item("WSpells").Permashow(eventArgs.GetNewValue<bool>(), "W SpellBlock", ScriptColor); };
            if (draw.Item("RPermashow").IsActive()) { RMenu.Item("RMode").Permashow(true, null, ScriptColor); }
            draw.Item("RPermashow").ValueChanged += (sender, eventArgs) =>  { RMenu.Item("RMode").Permashow(eventArgs.GetNewValue<bool>(), null, ScriptColor); };
            if (draw.Item("FarmPermashow").IsActive()) { MiscMenu.Item("FarmEnabled").Permashow(true, null, ScriptColor); }
            draw.Item("FarmPermashow").ValueChanged += (sender, eventArgs) => { MiscMenu.Item("FarmEnabled").Permashow(eventArgs.GetNewValue<bool>(), null, ScriptColor); };
            if (draw.Item("OrbwalkPermashow").IsActive()) { orbwalker.Item("OrbwalkPassive").Permashow(true, null, ScriptColor); }
            draw.Item("OrbwalkPermashow").ValueChanged += (sender, eventArgs) => { orbwalker.Item("OrbwalkPassive").Permashow(eventArgs.GetNewValue<bool>(), null, ScriptColor); };
            #endregion

            Menu.AddToMainMenu();
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

        private static void Game_OnGameUpdate(EventArgs args)
        {
            Orbwalker.SetOrbwalkingPoint(Vector3.Zero);

            if (Player.IsDead || Flee())
            {
                return;
            }
            Skin();
            setbool();
            KillstealQ();
            KillstealW();
            DuelistMode();
            Farm();

            if (Player.IsDashing() || Player.IsWindingUp || Player.Spellbook.IsCastingSpell)
            {
                return;
            }

            if (!Orbwalker.ActiveMode.IsComboMode())
            {
                return;
            }

            var aaTarget = GetTarget(true);
            var passive = new FioraPassive();
            if (aaTarget.IsValidTarget())
            {
                passive = aaTarget.GetNearestPassive();
                if (Menu.Item("OrbwalkPassive").IsActive() &&
                    Menu.Item("Orbwalk" + Orbwalker.ActiveMode.GetModeString()).IsActive())
                {
                    var selTarget = TargetSelector.SelectedTarget;
                    //Console.WriteLine("START ORBWALK TO PASSIVE");
                    if (!Menu.Item("OrbwalkSelected").IsActive() ||
                        (selTarget != null && selTarget.NetworkId.Equals(aaTarget.NetworkId)))
                    {
                        OrbwalkToPassive(aaTarget, passive);
                    }
                }
                Orbwalker.ForceTarget(aaTarget);
            }

            var target = GetTarget(false);
            if (!target.IsValidTarget(W.Range))
            {
                return;
            }

            var vital = aaTarget != null && target.NetworkId.Equals(aaTarget.NetworkId)
                ? passive
                : target.GetNearestPassive();

            if (Orbwalker.ActiveMode.Equals(Orbwalking.OrbwalkingMode.Mixed) &&
                Player.ManaPercent < Menu.Item("ManaHarass").GetValue<Slider>().Value)
            {
                return;
            }

            if (R.IsActive() /*&& !R.HasManaCondition()*/&&
                Menu.Item("RMode").GetValue<StringList>().SelectedIndex.Equals(1) && ComboR(target))
            {
                return;
            }

            if (Q.IsActive()) // && !Q.HasManaCondition())
            {
                if (target.IsValidTarget(FioraAutoAttackRange) && !Orbwalking.IsAutoAttack(Player.LastCastedSpellName()))
                {
                    return;
                }

                if (target.ChampionName.Equals("Poppy") && target.HasBuff("poppywzone"))
                {
                    return;
                }

                var count = target.GetUltPassiveCount();

                if (!Menu.Item("RSmartQ").IsActive() || count == 0)
                {
                    CastQ(target, vital);
                    return;
                }
                if (count > 2)
                {
                    return;
                }

                CastQ(target, target.GetFurthestPassive());
            }
        }

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        private static void GameOnOnGameLoad()
        {
            if (ObjectManager.Player.ChampionName != "Fiora")
            {
                return;
            }
			Game.PrintChat("<font color='#FF9900'><b>濞涙▊鍗曠嫭鍒朵綔</b></font><font color='#FF0033'><b>-鏂扮増鍐犲啗鍓戝К</b></font><font color='#CCFF66'><b>-鍒囧嬁澶栦紶</b></font>");

            Bootstrap.Initialize();

            InitMenu();

            DamageIndicator.DamageToUnit = GetComboDamage;
            PassiveManager.Initialize();

            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += AfterAttack;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        #endregion
    }
}