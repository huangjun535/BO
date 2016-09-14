namespace YuLeRiven
{
    using System;
    using System.Linq;
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;
    using ItemData = LeagueSharp.Common.Data.ItemData;
    using SharpDX.Direct3D9;
    using YuLeLibrary;
    using System.Collections.Generic;
    using YuLeLibrary;
    using System.Threading.Tasks;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Reflection;


    public class Program
    {
        public static Menu Menu;
        private static Orbwalking.Orbwalker Orbwalker;
        private static Obj_AI_Hero Player;
        private static HpBarIndicator Indicator = new HpBarIndicator();
        private const string IsFirstR = "RivenFengShuiEngine";
        private const string IsSecondR = "RivenIzunaBlade";
        private static SpellSlot Flash;
        private static Spell Q, Q1, W, E, R;
        private static int QStack = 1;
        private static bool forceQ;
        private static bool forceW;
        private static bool forceR;
        private static bool forceR2;
        private static bool forceItem;
        private static float LastQ;
        private static float LastR;
        private static AttackableUnit QTarget;
        public static readonly Dictionary<string, Vector3> JumpPos = new Dictionary<string, Vector3>()
        {
            { "mid_Dragon" , new Vector3 (9122f, 4058f, 53.95995f) },
            { "left_dragon" , new Vector3 (9088f, 4544f, 52.24316f) },
            { "baron" , new Vector3 (5774f, 10706f, 55.77578F) },
            { "red_wolves" , new Vector3 (11772f, 8856f, 50.30728f) },
            { "blue_wolves" , new Vector3 (3046f, 6132f, 57.04655f) },
        };

        private static void Main() => CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;

        private static void GameOnOnGameLoad()
        {
            Player = ObjectManager.Player;

            if (Player.ChampionName != "Riven")
                return;

            Q = new Spell(SpellSlot.Q, 260f);
            W = new Spell(SpellSlot.W, 250f);
            E = new Spell(SpellSlot.E, 300);
            R = new Spell(SpellSlot.R, 900);
            R.SetSkillshot(0.25f, 45, 1600, false, SkillshotType.SkillshotCone);
            Flash = ObjectManager.Player.GetSpellSlot("SummonerFlash");

            OnMenuLoad();

            Game.OnUpdate += OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Obj_AI_Base.OnProcessSpellCast += OnCast;
            Obj_AI_Base.OnDoCast += OnDoCast;
            Obj_AI_Base.OnDoCast += OnDoCastLC;
            Obj_AI_Base.OnPlayAnimation += OnPlay;
            Obj_AI_Base.OnProcessSpellCast += OnCasting;
            Interrupter2.OnInterruptableTarget += Interrupt;
        }

        private static void OnMenuLoad()
        {
            Menu = new Menu("QQ群438230879", "YuLeRiven", true).SetFontStyle(System.Drawing.FontStyle.Regular, SharpDX.Color.Pink);

            var orbwalker = new Menu("走砍设置", "rorb");
            Orbwalker = new Orbwalking.Orbwalker(orbwalker);
            Menu.AddSubMenu(orbwalker);

            var QMenu = new Menu("Q设置", "QMenu");
            QMenu.AddItem(new MenuItem("QD", "Q1 Q2 延迟").SetValue(new Slider(29, 23, 43)));
            QMenu.AddItem(new MenuItem("QLD", "Q3 延迟").SetValue(new Slider(39, 36, 53)));
            QMenu.AddItem(new MenuItem("Qstrange", "QA中使用动作取消后摇(不建议开启)").SetValue(false));
            QMenu.AddItem(new MenuItem("KeepQ", "保留 Q被动").SetValue(true));
            QMenu.AddItem(new MenuItem("LaneQ", "智能清线逻辑").SetValue(true));
            QMenu.AddItem(new MenuItem("Q3Wall", "逃跑时Q3翻墙").SetValue(true));
            Menu.AddSubMenu(QMenu);

            var WMenu = new Menu("W 设置", "WMEnu");
            WMenu.AddItem(new MenuItem("ComboW", "连招W").SetValue(true));
            WMenu.AddItem(new MenuItem("LaneW", "清线W最小命中小兵数(0为关闭)").SetValue(new Slider(5, 0, 5)));
            WMenu.AddItem(new MenuItem("killstealw", "击杀W").SetValue(true));
            WMenu.AddItem(new MenuItem("FirstHydra", "爆发模式先使用W").SetValue(false));
            WMenu.AddItem(new MenuItem("Winterrupt", "打断技能W").SetValue(true));
            WMenu.AddItem(new MenuItem("AutoW", "自动W最小命中敌人数").SetValue(new Slider(5, 0, 5)));
            Menu.AddSubMenu(WMenu);

            var EMenu = new Menu("E 设置", "EMenu");
            EMenu.AddItem(new MenuItem("NOTE", "敌人在AA范围禁止E").SetValue(true));
            EMenu.AddItem(new MenuItem("LaneE", "自动清线E").SetValue(true));
            EMenu.AddItem(new MenuItem("youmu", "自动E后接幽梦").SetValue(false));
            EMenu.AddItem(new MenuItem("AutoShield", "自动E挡技能").SetValue(true));
            EMenu.AddItem(new MenuItem("Shield", "自动E接能补到的刀").SetValue(true));
            Menu.AddSubMenu(EMenu);


            var RMenu = new Menu("R 设置", "RMenu");
            RMenu.AddItem(new MenuItem("RBURST", "左键选择爆发目标(关闭为自动选择)").SetValue(true));
            RMenu.AddItem(new MenuItem("AlwaysR", "见人直接开R").SetValue(new KeyBind('G', KeyBindType.Toggle))).Permashow();
            RMenu.AddItem(new MenuItem("UseHoola", "定制连招逻辑（少一段伤害，更快的QA）").SetValue(new KeyBind('L', KeyBindType.Toggle))).Permashow();
            RMenu.AddItem(new MenuItem("RKillable", "当目标能被击杀自动R").SetValue(true));
            RMenu.AddItem(new MenuItem("RMaxDam", "二段R打最大伤害").SetValue(true));
            RMenu.AddItem(new MenuItem("killstealr", "击杀R").SetValue(true));
            Menu.AddSubMenu(RMenu);

            var wardMenu = new Menu("杂项设置", "wardMenu");
            wardMenu.AddItem(new MenuItem("FleeKey", "逃跑+翻墙按键", true).SetValue(new KeyBind(Menu.Item("Flee").GetValue<KeyBind>().Key, KeyBindType.Press)));
            wardMenu.AddItem(new MenuItem("AutoWard1", "         自动眼位", true));
            wardMenu.AddItem(new MenuItem("AutoWard", "启动", true).SetValue(true));
            wardMenu.AddItem(new MenuItem("AutoBuy", "lv9自动买灯泡", true).SetValue(false));
            wardMenu.AddItem(new MenuItem("AutoPink", "自动真眼扫描", true).SetValue(true));
            wardMenu.AddItem(new MenuItem("AutoWardCombo", "仅连招模式启动 ", true).SetValue(true));
            new AutoWard().Load();
            new Tracker().Load();
            Menu.AddSubMenu(wardMenu);

            var Draw = new Menu("显示设置", "Draw");
            Draw.AddItem(new MenuItem("DrawCB", "显示连招最大范围").SetValue(false));
            Draw.AddItem(new MenuItem("DrawHS", "显示骚扰最大范围").SetValue(false));
            Draw.AddItem(new MenuItem("DrawFH", "显示快速骚扰最大范围").SetValue(false));
            Draw.AddItem(new MenuItem("DrawBT", "显示爆发最大范围").SetValue(false));
            Draw.AddItem(new MenuItem("Dind", "显示连招伤害").SetValue(true));
            Draw.AddItem(new MenuItem("FleeSpot", "显示翻墙点").SetValue(true));
            Menu.AddSubMenu(Draw);

            Menu.AddToMainMenu();
        }

        private static bool WallJump => Menu.Item("Q3Wall").GetValue<bool>();
        private static bool NotE => Menu.Item("NOTE").GetValue<bool>();
        private static bool Dind => Menu.Item("Dind").GetValue<bool>();
        private static bool DrawCB => Menu.Item("DrawCB").GetValue<bool>();
        private static bool KillstealW => Menu.Item("killstealw").GetValue<bool>();
        private static bool KillstealR => Menu.Item("killstealr").GetValue<bool>();
        private static bool DrawFH => Menu.Item("DrawFH").GetValue<bool>();
        private static bool DrawHS => Menu.Item("DrawHS").GetValue<bool>();
        private static bool DrawBT => Menu.Item("DrawBT").GetValue<bool>();
        private static bool UseHoola => Menu.Item("UseHoola").GetValue<KeyBind>().Active;
        private static bool AlwaysR => Menu.Item("AlwaysR").GetValue<KeyBind>().Active;
        private static bool AutoShield => Menu.Item("AutoShield").GetValue<bool>();
        private static bool Shield => Menu.Item("Shield").GetValue<bool>();
        private static bool KeepQ => Menu.Item("KeepQ").GetValue<bool>();
        private static int QD => Menu.Item("QD").GetValue<Slider>().Value;
        private static int QLD => Menu.Item("QLD").GetValue<Slider>().Value;
        private static int AutoW => Menu.Item("AutoW").GetValue<Slider>().Value;
        private static bool ComboW => Menu.Item("ComboW").GetValue<bool>();
        private static bool RMaxDam => Menu.Item("RMaxDam").GetValue<bool>();
        private static bool RKillable => Menu.Item("RKillable").GetValue<bool>();
        private static int LaneW => Menu.Item("LaneW").GetValue<Slider>().Value;
        private static bool LaneE => Menu.Item("LaneE").GetValue<bool>();
        private static bool WInterrupt => Menu.Item("WInterrupt").GetValue<bool>();
        private static bool Qstrange => Menu.Item("Qstrange").GetValue<bool>();
        private static bool FirstHydra => Menu.Item("FirstHydra").GetValue<bool>();
        private static bool LaneQ => Menu.Item("LaneQ").GetValue<bool>();
        private static bool Youmu => Menu.Item("youmu").GetValue<bool>();

        private static bool HasTitan() => (Items.HasItem(3748) && Items.CanUseItem(3748));

        private static void CastTitan()
        {
            if (Items.HasItem(3748) && Items.CanUseItem(3748))
            {
                Items.UseItem(3748);
                Orbwalking.LastAATick = 0;
            }
        }

        private static void Drawing_OnEndScene(EventArgs args)
        {
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(ene => ene.IsValidTarget() && !ene.IsZombie))
            {
                if (Dind)
                {
                    Indicator.unit = enemy;
                    Indicator.drawDmg(getComboDamage(enemy), new ColorBGRA(255, 204, 0, 170));
                }
            }
        }

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

        private static void OnDoCastLC(Obj_AI_Base Sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!Sender.IsMe || !Orbwalking.IsAutoAttack((args.SData.Name)))
                return;

            QTarget = (Obj_AI_Base)args.Target;

            if (args.Target is Obj_AI_Minion)
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                {
                    var Minions = MinionManager.GetMinions(70 + 120 + Player.BoundingRadius);
                    if (HasTitan())
                    {
                        CastTitan();
                        return;
                    }
                    if (Q.IsReady() && LaneQ)
                    {
                        ForceItem();
                        Utility.DelayAction.Add(1, () => ForceCastQ(Minions[0]));
                    }
                    if ((!Q.IsReady() || (Q.IsReady() && !LaneQ)) && W.IsReady() && LaneW != 0 &&
                        Minions.Count >= LaneW)
                    {
                        ForceItem();
                        Utility.DelayAction.Add(1, ForceW);
                    }
                    if ((!Q.IsReady() || (Q.IsReady() && !LaneQ)) && (!W.IsReady() || (W.IsReady() && LaneW == 0) || Minions.Count < LaneW) &&
                        E.IsReady() && LaneE)
                    {
                        E.Cast(Minions[0].Position);
                        Utility.DelayAction.Add(1, ForceItem);
                    }
                }
            }
        }
        private static int Item => Items.CanUseItem(3077) && Items.HasItem(3077) ? 3077 : Items.CanUseItem(3074) && Items.HasItem(3074) ? 3074 : 0;
        private static void OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var spellName = args.SData.Name;
            if (!sender.IsMe || !Orbwalking.IsAutoAttack(spellName)) return;
            QTarget = (Obj_AI_Base)args.Target;

            if (args.Target is Obj_AI_Minion)
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                {
                    var Mobs = MinionManager.GetMinions(120 + 70 + Player.BoundingRadius, MinionTypes.All,
                        MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                    if (Mobs.Count != 0)
                    {
                        if (HasTitan())
                        {
                            CastTitan();
                            return;
                        }
                        if (Q.IsReady())
                        {
                            ForceItem();
                            Utility.DelayAction.Add(1, () => ForceCastQ(Mobs[0]));
                        }
                        else if (W.IsReady())
                        {
                            ForceItem();
                            Utility.DelayAction.Add(1, ForceW);
                        }
                        else if (E.IsReady())
                        {
                            E.Cast(Mobs[0].Position);
                        }
                    }
                }
            }
            if (args.Target is Obj_AI_Turret || args.Target is Obj_Barracks || args.Target is Obj_BarracksDampener || args.Target is Obj_Building) if (args.Target.IsValid && args.Target != null && Q.IsReady() && LaneQ && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear) ForceCastQ((Obj_AI_Base)args.Target);
            if (args.Target is Obj_AI_Hero)
            {
                var target = (Obj_AI_Hero)args.Target;
                if (KillstealR && R.IsReady() && R.Instance.Name == IsSecondR) if (target.Health < (Rdame(target, target.Health) + Player.GetAutoAttackDamage(target)) && target.Health > Player.GetAutoAttackDamage(target)) R.Cast(target.Position);
                if (KillstealW && W.IsReady()) if (target.Health < (W.GetDamage(target) + Player.GetAutoAttackDamage(target)) && target.Health > Player.GetAutoAttackDamage(target)) W.Cast();
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                {
                    if (HasTitan())
                    {
                        CastTitan();
                        return;
                    }
                    if (Q.IsReady())
                    {
                        ForceItem();
                        Utility.DelayAction.Add(1, () => ForceCastQ(target));
                    }
                    else if (W.IsReady() && InWRange(target))
                    {
                        ForceItem();
                        Utility.DelayAction.Add(1, ForceW);
                    }
                    else if (E.IsReady() && !Orbwalking.InAutoAttackRange(target)) E.Cast(target.Position);
                }
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.FastHarass)
                {
                    if (HasTitan())
                    {
                        CastTitan();
                        return;
                    }
                    if (W.IsReady() && InWRange(target))
                    {
                        ForceItem();
                        Utility.DelayAction.Add(1, ForceW);
                        Utility.DelayAction.Add(2, () => ForceCastQ(target));
                    }
                    else if (Q.IsReady())
                    {
                        ForceItem();
                        Utility.DelayAction.Add(1, () => ForceCastQ(target));
                    }
                    else if (E.IsReady() && !Orbwalking.InAutoAttackRange(target) && !InWRange(target))
                    {
                        E.Cast(target.Position);
                    }
                }

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                {
                    if (HasTitan())
                    {
                        CastTitan();
                        return;
                    }
                    if (QStack == 2 && Q.IsReady())
                    {
                        ForceItem();
                        Utility.DelayAction.Add(1, () => ForceCastQ(target));
                    }
                }

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Burst)
                {
                    if (HasTitan())
                    {
                        CastTitan();
                        return;
                    }
                    if (R.IsReady() && R.Instance.Name == IsSecondR)
                    {
                        ForceItem();
                        Utility.DelayAction.Add(1, ForceR2);
                    }
                    else if (Q.IsReady())
                    {
                        ForceItem();
                        Utility.DelayAction.Add(1, () => ForceCastQ(target));
                    }
                }
            }
        }

        private static void Interrupt(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (sender.IsEnemy && W.IsReady() && sender.IsValidTarget() && !sender.IsZombie && WInterrupt)
            {
                if (sender.IsValidTarget(125 + Player.BoundingRadius + sender.BoundingRadius)) W.Cast();
            }
        }

        private static int GetWRange => Player.HasBuff("RivenFengShuiEngine") ? 330 : 265;

        private static void AutoUseW()
        {
            if (AutoW > 0)
            {
                if (Player.CountEnemiesInRange(GetWRange) >= AutoW)
                {
                    ForceW();
                }
            }
        }

        private static void OnTick(EventArgs args)
        {
            ForceSkill();


            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case Orbwalking.OrbwalkingMode.FastHarass:
                    FastHarass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Jungleclear();
                    break;
                case Orbwalking.OrbwalkingMode.Burst:
                    Burst();
                    break;
                case Orbwalking.OrbwalkingMode.Flee:
                    Flee();
                    break;
            }


            UseRMaxDam();
            AutoUseW();
            Killsteal();

            if (Utils.GameTimeTickCount - LastQ >= 3650 && QStack != 1 && !Player.IsRecalling() && KeepQ && Q.IsReady())
                Q.Cast(Game.CursorPos);

            AutoWard.Enable = Menu.Item("AutoWard", true).GetValue<bool>();
            AutoWard.AutoBuy = Menu.Item("AutoBuy", true).GetValue<bool>();
            AutoWard.AutoPink = Menu.Item("AutoPink", true).GetValue<bool>();
            AutoWard.OnlyCombo = Menu.Item("AutoWardCombo", true).GetValue<bool>();
            AutoWard.InComboMode = Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo;

        }

        private static void Killsteal()
        {
            if (KillstealW && W.IsReady())
            {
                var targets = HeroManager.Enemies.Where(x => x.IsValidTarget(R.Range) && !x.IsZombie);
                foreach (var target in targets)
                {
                    if (target.Health < W.GetDamage(target) && InWRange(target))
                        W.Cast();
                }
            }
            if (KillstealR && R.IsReady() && R.Instance.Name == IsSecondR)
            {
                var targets = HeroManager.Enemies.Where(x => x.IsValidTarget(R.Range) && !x.IsZombie);
                foreach (var target in targets)
                {
                    if (target.Health < Rdame(target, target.Health) && (!target.HasBuff("kindrednodeathbuff") && !target.HasBuff("Undying Rage") && !target.HasBuff("JudicatorIntervention")))
                        R.Cast(target.Position);
                }
            }
        }
        private static void UseRMaxDam()
        {
            if (RMaxDam && R.IsReady() && R.Instance.Name == IsSecondR)
            {
                var targets = HeroManager.Enemies.Where(x => x.IsValidTarget(R.Range) && !x.IsZombie);
                foreach (var target in targets)
                {
                    if (target.Health / target.MaxHealth <= 0.25 && (!target.HasBuff("kindrednodeathbuff") || !target.HasBuff("Undying Rage") || !target.HasBuff("JudicatorIntervention")))
                        R.Cast(target.Position);
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (DrawCB)
                Render.Circle.DrawCircle(Player.Position, 250 + Player.AttackRange + 70, System.Drawing.Color.Orange);
            if (DrawBT && Flash != SpellSlot.Unknown)
                Render.Circle.DrawCircle(Player.Position, 800, System.Drawing.Color.LightBlue);
            if (DrawFH)
                Render.Circle.DrawCircle(Player.Position, 450 + Player.AttackRange + 70, System.Drawing.Color.Red);
            if (DrawHS)
                Render.Circle.DrawCircle(Player.Position, 400, System.Drawing.Color.Green);

            if (IsWallDash(Player.ServerPosition.Extend(Game.CursorPos, Q.Range), Q.Range) && Menu.Item("FleeSpot").GetValue<bool>())
            {
                var Eend = Player.ServerPosition.Extend(Game.CursorPos, E.Range);
                var WallE = GetFirstWallPoint(Player.Position, Eend);
                var WallPoint = GetFirstWallPoint(Player.Position, Player.Position.Extend(Game.CursorPos, Q.Range));

                if (WallPoint.Distance(Player.ServerPosition) <= 600)
                {
                    Render.Circle.DrawCircle(WallPoint, 60, System.Drawing.Color.White);
                    Render.Circle.DrawCircle(Player.Position.Extend(Game.CursorPos, Q.Range), 60, System.Drawing.Color.Green);
                }
            }
        }

        private static void Jungleclear()
        {

            var Mobs = MinionManager.GetMinions(250 + Player.AttackRange + 70, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (W.IsReady() && E.IsReady() && !Orbwalking.InAutoAttackRange(Mobs[0]))
            {
                E.Cast(Mobs[0].Position);
                Utility.DelayAction.Add(1, ForceItem);
                Utility.DelayAction.Add(200, ForceW);
            }
        }

        private static void Combo()
        {
            var targetR = TargetSelector.GetTarget(250 + Player.AttackRange + 70, TargetSelector.DamageType.Physical);
            if (R.IsReady() && R.Instance.Name == IsFirstR && Orbwalker.InAutoAttackRange(targetR) && AlwaysR && targetR != null) ForceR();
            if (R.IsReady() && R.Instance.Name == IsFirstR && W.IsReady() && InWRange(targetR) && ComboW && AlwaysR && targetR != null)
            {
                ForceR();
                Utility.DelayAction.Add(1, ForceW);
            }
            if (W.IsReady() && InWRange(targetR) && ComboW && targetR != null) W.Cast();
            if (UseHoola && R.IsReady() && R.Instance.Name == IsFirstR && W.IsReady() && targetR != null && E.IsReady() && targetR.IsValidTarget() && !targetR.IsZombie && (IsKillableR(targetR) || AlwaysR))
            {
                if (!InWRange(targetR))
                {
                    if (NotE)
                    {
                        if (!Orbwalker.InAutoAttackRange(targetR))
                            E.Cast(targetR.Position);
                    }
                    else
                    {
                        E.Cast(targetR.Position);
                    }
                    ForceR();
                    Utility.DelayAction.Add(200, ForceW);
                    Utility.DelayAction.Add(305, () => ForceCastQ(targetR));
                }
            }
            else if (!UseHoola && R.IsReady() && R.Instance.Name == IsFirstR && W.IsReady() && targetR != null && E.IsReady() && targetR.IsValidTarget() && !targetR.IsZombie && (IsKillableR(targetR) || AlwaysR))
            {
                if (!InWRange(targetR))
                {
                    if (NotE)
                    {
                        if (!Orbwalker.InAutoAttackRange(targetR))
                            E.Cast(targetR.Position);
                    }
                    else
                    {
                        E.Cast(targetR.Position);
                    }
                    ForceR();
                    Utility.DelayAction.Add(200, ForceW);
                }
            }
            else if (UseHoola && W.IsReady() && E.IsReady())
            {
                if (targetR.IsValidTarget() && targetR != null && !targetR.IsZombie && !InWRange(targetR))
                {
                    if (NotE)
                    {
                        if (!Orbwalker.InAutoAttackRange(targetR))
                            E.Cast(targetR.Position);
                    }
                    else
                    {
                        E.Cast(targetR.Position);
                    }
                    Utility.DelayAction.Add(10, ForceItem);
                    Utility.DelayAction.Add(200, ForceW);
                    Utility.DelayAction.Add(305, () => ForceCastQ(targetR));
                }
            }
            else if (!UseHoola && W.IsReady() && targetR != null && E.IsReady())
            {
                if (targetR.IsValidTarget() && targetR != null && !targetR.IsZombie && !InWRange(targetR))
                {
                    if (NotE)
                    {
                        if (!Orbwalker.InAutoAttackRange(targetR))
                            E.Cast(targetR.Position);
                    }
                    else
                    {
                        E.Cast(targetR.Position);
                    }
                    Utility.DelayAction.Add(10, ForceItem);
                    Utility.DelayAction.Add(240, ForceW);
                }
            }
            else if (E.IsReady())
            {
                if (targetR.IsValidTarget() && !targetR.IsZombie && !InWRange(targetR))
                {
                    if (NotE)
                    {
                        if (!Orbwalker.InAutoAttackRange(targetR))
                            E.Cast(targetR.Position);
                    }
                    else
                    {
                        E.Cast(targetR.Position);
                    }
                }
            }
        }

        private static void Burst()
        {
            Obj_AI_Hero target = null;

            if (Menu.Item("RBURST").GetValue<bool>())
            {
                target = TargetSelector.GetSelectedTarget();
            }
            else if (!Menu.Item("RBURST").GetValue<bool>())
            {
                target = TargetSelector.GetTarget(1500, TargetSelector.DamageType.Physical);
                TargetSelector.SetTarget(target);
            }

            if (target != null && target.IsValidTarget() && !target.IsZombie)
            {


                if (R.IsReady() && R.Instance.Name == IsFirstR && W.IsReady() && E.IsReady() && Player.Distance(target.Position) <= 250 + 70 + Player.AttackRange)
                {
                    E.Cast(target.Position);
                    CastYoumoo();
                    ForceR();
                    Utility.DelayAction.Add(100, ForceW);
                }
                else if (R.IsReady() && R.Instance.Name == IsFirstR && E.IsReady() && W.IsReady() && Q.IsReady() &&
                         Player.Distance(target.Position) <= 400 + 70 + Player.AttackRange)
                {
                    E.Cast(target.Position);
                    CastYoumoo();
                    ForceR();
                    Utility.DelayAction.Add(150, () => ForceCastQ(target));
                    Utility.DelayAction.Add(160, ForceW);
                }
                else if (Flash.IsReady()
                    && R.IsReady() && R.Instance.Name == IsFirstR && (Player.Distance(target.Position) <= 800) && (!FirstHydra || (FirstHydra && !HasItem())))
                {
                    E.Cast(target.Position);
                    CastYoumoo();
                    ForceR();
                    Utility.DelayAction.Add(180, FlashW);
                }
                else if (Flash.IsReady()
                    && R.IsReady() && E.IsReady() && W.IsReady() && R.Instance.Name == IsFirstR && (Player.Distance(target.Position) <= 800) && FirstHydra && HasItem())
                {
                    if (NotE)
                    {
                        if (!Orbwalker.InAutoAttackRange(target))
                            E.Cast(target.Position);
                    }
                    else
                    {
                        E.Cast(target.Position);
                    }
                    ForceR();
                    Utility.DelayAction.Add(100, ForceItem);
                    Utility.DelayAction.Add(210, FlashW);
                }
            }
        }

        private static void FastHarass()
        {
            if (Q.IsReady() && E.IsReady())
            {
                var target = TargetSelector.GetTarget(450 + Player.AttackRange + 70, TargetSelector.DamageType.Physical);
                if (target.IsValidTarget() && !target.IsZombie)
                {
                    if (!Orbwalking.InAutoAttackRange(target) && !InWRange(target)) E.Cast(target.Position);
                    Utility.DelayAction.Add(10, ForceItem);
                    Utility.DelayAction.Add(170, () => ForceCastQ(target));
                }
            }
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(400, TargetSelector.DamageType.Physical);
            if (Q.IsReady() && W.IsReady() && E.IsReady() && QStack == 1)
            {
                if (target.IsValidTarget() && !target.IsZombie)
                {
                    ForceCastQ(target);
                    Utility.DelayAction.Add(1, ForceW);
                }
            }
            if (Q.IsReady() && E.IsReady() && QStack == 3 && !Orbwalking.CanAttack() && Orbwalking.CanMove(5))
            {
                var epos = Player.ServerPosition +
                          (Player.ServerPosition - target.ServerPosition).Normalized() * 300;
                E.Cast(epos);
                Utility.DelayAction.Add(190, () => Q.Cast(epos));
            }
        }

        private static void Flee()
        {
            if (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Flee)
                return;

            var end = Player.Position.Extend(Game.CursorPos, Q.Range);
            var IsWallDash1 = IsWallDash(end, Q.Range);
            var Eend = Player.Position.Extend(Game.CursorPos, E.Range);
            var WallE = GetFirstWallPoint(Player.ServerPosition, Eend);
            var WallPoint = GetFirstWallPoint(Player.ServerPosition, end);

            if (Q.IsReady() && QStack < 3)
            {
                Q.Cast(Game.CursorPos);
            }

            if (IsWallDash1 && QStack == 3 && WallPoint.Distance(Player.ServerPosition) <= 800 && WallJump)
            {
                ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, WallPoint);
                if (WallPoint.Distance(Player.ServerPosition) <= 600)
                {
                    ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, WallPoint);
                    if (WallPoint.Distance(Player.ServerPosition) < 55)
                    {
                        if (E.IsReady())
                        {
                            E.Cast(WallE);
                        }
                        if (QStack == 3)
                        {
                            Player.IssueOrder(GameObjectOrder.MoveTo, WallPoint);
                            Q.Cast(WallPoint);
                        }
                    }
                }
            }

            if (!IsWallDash1 && !WallJump)
            {
                var enemy = HeroManager.Enemies.Where(hero => hero.IsValidTarget(Player.HasBuff("RivenFengShuiEngine") ? 70 + 195 + Player.BoundingRadius : 70 + 120 + Player.BoundingRadius) && W.IsReady());
                var x = Player.Position.Extend(Game.CursorPos, 300);
                var objAiHeroes = enemy as Obj_AI_Hero[] ?? enemy.ToArray();

                if (Q.IsReady() && !Player.IsDashing())
                    Q.Cast(Game.CursorPos);

                if (W.IsReady() && objAiHeroes.Any())
                    foreach (var target in objAiHeroes)
                        if (InWRange(target))
                            W.Cast();

                if (E.IsReady() && !Player.IsDashing())
                    E.Cast(x);
            }
        }

        private static void OnPlay(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
        {
            if (!sender.IsMe) return;

            switch (args.Animation)
            {
                case "Spell1a":
                    LastQ = Utils.GameTimeTickCount;
                    if (Qstrange && Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.None) Game.Say("/d");
                    QStack = 2;
                    if (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.None && Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LastHit && Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Flee) Utility.DelayAction.Add((QD * 10) + 1, Reset);
                    break;
                case "Spell1b":
                    LastQ = Utils.GameTimeTickCount;
                    if (Qstrange && Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.None) Game.Say("/d");
                    QStack = 3;
                    if (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.None && Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LastHit && Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Flee) Utility.DelayAction.Add((QD * 10) + 1, Reset);
                    break;
                case "Spell1c":
                    LastQ = Utils.GameTimeTickCount;
                    if (Qstrange && Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.None) Game.Say("/d");
                    QStack = 1;
                    if (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.None && Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LastHit && Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Flee) Utility.DelayAction.Add((QLD * 10) + 3, Reset);
                    break;
                case "Spell3":
                    if ((Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Burst ||
                        Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo ||
                        Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.FastHarass ||
                        Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Flee) && Youmu) CastYoumoo();
                    break;
                case "Spell4a":
                    LastR = Utils.GameTimeTickCount;
                    break;
                case "Spell4b":
                    var target = TargetSelector.GetSelectedTarget();
                    if (Q.IsReady() && target.IsValidTarget()) ForceCastQ(target);
                    break;
            }
        }

        private static void OnCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe) return;

            if (args.SData.Name.Contains("ItemTiamatCleave")) forceItem = false;
            if (args.SData.Name.Contains("RivenTriCleave")) forceQ = false;
            if (args.SData.Name.Contains("RivenMartyr")) forceW = false;
            if (args.SData.Name == IsFirstR) forceR = false;
            if (args.SData.Name == IsSecondR) forceR2 = false;
        }

        private static void Reset()
        {
            Game.Say("/d");
            Orbwalking.LastAATick = 0;
            Player.IssueOrder(GameObjectOrder.MoveTo, Player.Position.Extend(Game.CursorPos, Player.Distance(Game.CursorPos) + 10));
        }

        private static bool InWRange(AttackableUnit target) => target != null && target.IsValidTarget(Player.HasBuff("RivenFengShuiEngine") ? 330 : 265);

        private static void ForceSkill()
        {
            if (forceQ && QTarget != null && QTarget.IsValidTarget(E.Range + Player.BoundingRadius + 70) && Q.IsReady()) Q.Cast(QTarget.Position);
            if (forceW) W.Cast();
            if (forceR && R.Instance.Name == IsFirstR) R.Cast();
            if (forceItem && Items.CanUseItem(Item) && Items.HasItem(Item) && Item != 0) Items.UseItem(Item);
            if (forceR2 && R.Instance.Name == IsSecondR)
            {
                var target = TargetSelector.GetSelectedTarget();
                if (target != null) R.Cast(target.Position);
            }
        }

        private static void ForceItem()
        {
            if (Items.CanUseItem(Item) && Items.HasItem(Item) && Item != 0) forceItem = true;
            Utility.DelayAction.Add(500, () => forceItem = false);
        }
        private static void ForceR()
        {
            forceR = (R.IsReady() && R.Instance.Name == IsFirstR);
            Utility.DelayAction.Add(500, () => forceR = false);
        }
        private static void ForceR2()
        {
            forceR2 = R.IsReady() && R.Instance.Name == IsSecondR;
            Utility.DelayAction.Add(500, () => forceR2 = false);
        }
        private static void ForceW()
        {
            forceW = W.IsReady();
            Utility.DelayAction.Add(500, () => forceW = false);
        }

        private static void ForceCastQ(AttackableUnit target)
        {
            forceQ = true;
            QTarget = target;
        }


        private static void FlashW()
        {
            var target = TargetSelector.GetSelectedTarget();
            if (target != null && target.IsValidTarget() && !target.IsZombie)
            {
                W.Cast();
                Utility.DelayAction.Add(10, () => Player.Spellbook.CastSpell(Flash, target.Position));
            }
        }

        private static bool HasItem() => ItemData.Tiamat_Melee_Only.GetItem().IsReady() || ItemData.Ravenous_Hydra_Melee_Only.GetItem().IsReady();

        private static void CastYoumoo() { if (ItemData.Youmuus_Ghostblade.GetItem().IsReady()) ItemData.Youmuus_Ghostblade.GetItem().Cast(); }
        private static void OnCasting(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsEnemy && sender.Type == Player.Type && (AutoShield || (Shield && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit)))
            {
                var epos = Player.ServerPosition +
                          (Player.ServerPosition - sender.ServerPosition).Normalized() * 300;

                if (Player.Distance(sender.ServerPosition) <= args.SData.CastRange)
                {
                    switch (args.SData.TargettingType)
                    {
                        case SpellDataTargetType.Unit:

                            if (args.Target.NetworkId == Player.NetworkId)
                            {
                                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit && !args.SData.Name.Contains("NasusW"))
                                {
                                    if (E.IsReady()) E.Cast(epos);
                                }
                            }

                            break;
                        case SpellDataTargetType.SelfAoe:

                            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit)
                            {
                                if (E.IsReady()) E.Cast(epos);
                            }

                            break;
                    }
                    if (args.SData.Name.Contains("IreliaEquilibriumStrike"))
                    {
                        if (args.Target.NetworkId == Player.NetworkId)
                        {
                            if (W.IsReady() && InWRange(sender)) W.Cast();
                            else if (E.IsReady()) E.Cast(epos);
                        }
                    }
                    if (args.SData.Name.Contains("TalonCutthroat"))
                    {
                        if (args.Target.NetworkId == Player.NetworkId)
                        {
                            if (W.IsReady()) W.Cast();
                        }
                    }
                    if (args.SData.Name.Contains("RenektonPreExecute"))
                    {
                        if (args.Target.NetworkId == Player.NetworkId)
                        {
                            if (W.IsReady()) W.Cast();
                        }
                    }
                    if (args.SData.Name.Contains("GarenRPreCast"))
                    {
                        if (args.Target.NetworkId == Player.NetworkId)
                        {
                            if (E.IsReady()) E.Cast(epos);
                        }
                    }
                    if (args.SData.Name.Contains("GarenQAttack"))
                    {
                        if (args.Target.NetworkId == Player.NetworkId)
                        {
                            if (E.IsReady()) E.Cast();
                        }
                    }
                    if (args.SData.Name.Contains("XenZhaoThrust3"))
                    {
                        if (args.Target.NetworkId == Player.NetworkId)
                        {
                            if (W.IsReady()) W.Cast();
                        }
                    }
                    if (args.SData.Name.Contains("RengarQ"))
                    {
                        if (args.Target.NetworkId == Player.NetworkId)
                        {
                            if (E.IsReady()) E.Cast();
                        }
                    }
                    if (args.SData.Name.Contains("RengarPassiveBuffDash"))
                    {
                        if (args.Target.NetworkId == Player.NetworkId)
                        {
                            if (E.IsReady()) E.Cast();
                        }
                    }
                    if (args.SData.Name.Contains("RengarPassiveBuffDashAADummy"))
                    {
                        if (args.Target.NetworkId == Player.NetworkId)
                        {
                            if (E.IsReady()) E.Cast();
                        }
                    }
                    if (args.SData.Name.Contains("TwitchEParticle"))
                    {
                        if (args.Target.NetworkId == Player.NetworkId)
                        {
                            if (E.IsReady()) E.Cast();
                        }
                    }
                    if (args.SData.Name.Contains("FizzPiercingStrike"))
                    {
                        if (args.Target.NetworkId == Player.NetworkId)
                        {
                            if (E.IsReady()) E.Cast();
                        }
                    }
                    if (args.SData.Name.Contains("HungeringStrike"))
                    {
                        if (args.Target.NetworkId == Player.NetworkId)
                        {
                            if (E.IsReady()) E.Cast();
                        }
                    }
                    if (args.SData.Name.Contains("YasuoDash"))
                    {
                        if (args.Target.NetworkId == Player.NetworkId)
                        {
                            if (E.IsReady()) E.Cast();
                        }
                    }
                    if (args.SData.Name.Contains("KatarinaRTrigger"))
                    {
                        if (args.Target.NetworkId == Player.NetworkId)
                        {
                            if (W.IsReady() && InWRange(sender)) W.Cast();
                            else if (E.IsReady()) E.Cast();
                        }
                    }
                    if (args.SData.Name.Contains("YasuoDash"))
                    {
                        if (args.Target.NetworkId == Player.NetworkId)
                        {
                            if (E.IsReady()) E.Cast();
                        }
                    }
                    if (args.SData.Name.Contains("KatarinaE"))
                    {
                        if (args.Target.NetworkId == Player.NetworkId)
                        {
                            if (W.IsReady()) W.Cast();
                        }
                    }
                    if (args.SData.Name.Contains("MonkeyKingQAttack"))
                    {
                        if (args.Target.NetworkId == Player.NetworkId)
                        {
                            if (E.IsReady()) E.Cast();
                        }
                    }
                    if (args.SData.Name.Contains("MonkeyKingSpinToWin"))
                    {
                        if (args.Target.NetworkId == Player.NetworkId)
                        {
                            if (E.IsReady()) E.Cast();
                            else if (W.IsReady()) W.Cast();
                        }
                    }
                    if (args.SData.Name.Contains("MonkeyKingQAttack"))
                    {
                        if (args.Target.NetworkId == Player.NetworkId)
                        {
                            if (E.IsReady()) E.Cast();
                        }
                    }
                    if (args.SData.Name.Contains("MonkeyKingQAttack"))
                    {
                        if (args.Target.NetworkId == Player.NetworkId)
                        {
                            if (E.IsReady()) E.Cast();
                        }
                    }
                    if (args.SData.Name.Contains("MonkeyKingQAttack"))
                    {
                        if (args.Target.NetworkId == Player.NetworkId)
                        {
                            if (E.IsReady()) E.Cast();
                        }
                    }
                }
            }
        }

        private static double basicdmg(Obj_AI_Base target)
        {
            if (target != null)
            {
                double dmg = 0;
                double passivenhan = 0;
                if (Player.Level >= 18) { passivenhan = 0.5; }
                else if (Player.Level >= 15) { passivenhan = 0.45; }
                else if (Player.Level >= 12) { passivenhan = 0.4; }
                else if (Player.Level >= 9) { passivenhan = 0.35; }
                else if (Player.Level >= 6) { passivenhan = 0.3; }
                else if (Player.Level >= 3) { passivenhan = 0.25; }
                else { passivenhan = 0.2; }
                if (HasItem()) dmg = dmg + Player.GetAutoAttackDamage(target) * 0.7;
                if (W.IsReady()) dmg = dmg + W.GetDamage(target);
                if (Q.IsReady())
                {
                    var qnhan = 4 - QStack;
                    dmg = dmg + Q.GetDamage(target) * qnhan + Player.GetAutoAttackDamage(target) * qnhan * (1 + passivenhan);
                }
                dmg = dmg + Player.GetAutoAttackDamage(target) * (1 + passivenhan);
                return dmg;
            }
            return 0;
        }


        private static float getComboDamage(Obj_AI_Base enemy)
        {
            if (enemy != null)
            {
                float damage = 0;
                float passivenhan = 0;
                if (Player.Level >= 18) { passivenhan = 0.5f; }
                else if (Player.Level >= 15) { passivenhan = 0.45f; }
                else if (Player.Level >= 12) { passivenhan = 0.4f; }
                else if (Player.Level >= 9) { passivenhan = 0.35f; }
                else if (Player.Level >= 6) { passivenhan = 0.3f; }
                else if (Player.Level >= 3) { passivenhan = 0.25f; }
                else { passivenhan = 0.2f; }
                if (HasItem()) damage = damage + (float)Player.GetAutoAttackDamage(enemy) * 0.7f;
                if (W.IsReady()) damage = damage + W.GetDamage(enemy);
                if (Q.IsReady())
                {
                    var qnhan = 4 - QStack;
                    damage = damage + Q.GetDamage(enemy) * qnhan + (float)Player.GetAutoAttackDamage(enemy) * qnhan * (1 + passivenhan);
                }
                damage = damage + (float)Player.GetAutoAttackDamage(enemy) * (1 + passivenhan);
                if (R.IsReady())
                {
                    return damage * 1.2f + R.GetDamage(enemy);
                }

                return damage;
            }
            return 0;
        }

        public static bool IsKillableR(Obj_AI_Hero target)
        {
            if (RKillable && target.IsValidTarget() && (totaldame(target) >= target.Health
                 && basicdmg(target) <= target.Health) || Player.CountEnemiesInRange(900) >= 2 && (!target.HasBuff("kindrednodeathbuff") && !target.HasBuff("Undying Rage") && !target.HasBuff("JudicatorIntervention")))
            {
                return true;
            }
            return false;
        }

        private static double totaldame(Obj_AI_Base target)
        {
            if (target != null)
            {
                double dmg = 0;
                double passivenhan = 0;
                if (Player.Level >= 18) { passivenhan = 0.5; }
                else if (Player.Level >= 15) { passivenhan = 0.45; }
                else if (Player.Level >= 12) { passivenhan = 0.4; }
                else if (Player.Level >= 9) { passivenhan = 0.35; }
                else if (Player.Level >= 6) { passivenhan = 0.3; }
                else if (Player.Level >= 3) { passivenhan = 0.25; }
                else { passivenhan = 0.2; }
                if (HasItem()) dmg = dmg + Player.GetAutoAttackDamage(target) * 0.7;
                if (W.IsReady()) dmg = dmg + W.GetDamage(target);
                if (Q.IsReady())
                {
                    var qnhan = 4 - QStack;
                    dmg = dmg + Q.GetDamage(target) * qnhan + Player.GetAutoAttackDamage(target) * qnhan * (1 + passivenhan);
                }
                dmg = dmg + Player.GetAutoAttackDamage(target) * (1 + passivenhan);
                if (R.IsReady())
                {
                    var rdmg = Rdame(target, target.Health - dmg * 1.2);
                    return dmg * 1.2 + rdmg;
                }
                return dmg;
            }
            return 0;
        }

        private static double Rdame(Obj_AI_Base target, double health)
        {
            if (target != null)
            {
                var missinghealth = (target.MaxHealth - health) / target.MaxHealth > 0.75 ? 0.75 : (target.MaxHealth - health) / target.MaxHealth;
                var pluspercent = missinghealth * (8 / 3);
                var rawdmg = new double[] { 80, 120, 160 }[R.Level - 1] + 0.6 * Player.FlatPhysicalDamageMod;
                return Player.CalcDamage(target, Damage.DamageType.Physical, rawdmg * (1 + pluspercent));
            }
            return 0;
        }

        public static Vector3 GetFirstWallPoint(Vector3 start, Vector3 end, int step = 1)
        {
            if (start.IsValid() && end.IsValid())
            {
                var distance = start.Distance(end);
                for (var i = 0; i < distance; i = i + step)
                {
                    var newPoint = start.Extend(end, i);

                    if (NavMesh.GetCollisionFlags(newPoint) == CollisionFlags.Wall || newPoint.IsWall())
                    {
                        return newPoint;
                    }
                }
            }
            return Vector3.Zero;
        }
        public static float GetWallWidth(Vector3 start, Vector3 direction, int maxWallWidth = 1000, int step = 1)
        {
            var thickness = 0f;

            if (!start.IsValid() || !direction.IsValid())
            {
                return thickness;
            }

            for (var i = 0; i < maxWallWidth; i = i + step)
            {
                if (NavMesh.GetCollisionFlags(start.Extend(direction, i)) == CollisionFlags.Wall
                    || start.Extend(direction, i).IsWall())
                {
                    thickness += step;
                }
                else
                {
                    return thickness;
                }
            }

            return thickness;
        }

        public static bool IsWallDash(Vector3 position, float dashRange, float minWallWidth = 50)
        {
            var dashEndPos = ObjectManager.Player.Position.Extend(position, dashRange);
            var firstWallPoint = GetFirstWallPoint(ObjectManager.Player.Position, dashEndPos);

            if (firstWallPoint.Equals(Vector3.Zero))
            {
                // No Wall
                return false;
            }

            if (dashEndPos.IsWall())
            // End Position is in Wall
            {
                var wallWidth = GetWallWidth(firstWallPoint, dashEndPos);

                if (wallWidth > minWallWidth && wallWidth - firstWallPoint.Distance(dashEndPos) < wallWidth * 0.83)
                {
                    return true;
                }
            }
            else
            // End Position is not a Wall
            {
                return true;
            }

            return false;
        }
    }

    internal class HpBarIndicator
    {
        public static Device dxDevice = Drawing.Direct3DDevice;
        public static Line dxLine;
        public float hight = 9;
        public float width = 104;


        public HpBarIndicator()
        {
            dxLine = new Line(dxDevice) { Width = 9 };

            Drawing.OnPreReset += DrawingOnOnPreReset;
            Drawing.OnPostReset += DrawingOnOnPostReset;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomainOnDomainUnload;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnDomainUnload;
        }

        public Obj_AI_Hero unit { get; set; }

        private Vector2 Offset
        {
            get
            {
                if (unit != null)
                {
                    return unit.IsAlly ? new Vector2(34, 9) : new Vector2(10, 20);
                }

                return new Vector2();
            }
        }

        public Vector2 startPosition
        {
            get { return new Vector2(unit.HPBarPosition.X + Offset.X, unit.HPBarPosition.Y + Offset.Y); }
        }


        private static void CurrentDomainOnDomainUnload(object sender, EventArgs eventArgs)
        {
            dxLine.Dispose();
        }

        private static void DrawingOnOnPostReset(EventArgs args)
        {
            dxLine.OnResetDevice();
        }

        private static void DrawingOnOnPreReset(EventArgs args)
        {
            dxLine.OnLostDevice();
        }


        private float getHpProc(float dmg = 0)
        {
            float health = ((unit.Health - dmg) > 0) ? (unit.Health - dmg) : 0;
            return (health / unit.MaxHealth);
        }

        private Vector2 getHpPosAfterDmg(float dmg)
        {
            float w = getHpProc(dmg) * width;
            return new Vector2(startPosition.X + w, startPosition.Y);
        }

        public void drawDmg(float dmg, ColorBGRA color)
        {
            Vector2 hpPosNow = getHpPosAfterDmg(0);
            Vector2 hpPosAfter = getHpPosAfterDmg(dmg);

            fillHPBar(hpPosNow, hpPosAfter, color);
        }

        private void fillHPBar(int to, int from, System.Drawing.Color color)
        {
            var sPos = startPosition;
            for (var i = from; i < to; i++)
            {
                Drawing.DrawLine(sPos.X + i, sPos.Y, sPos.X + i, sPos.Y + 9, 1, color);
            }
        }

        private void fillHPBar(Vector2 from, Vector2 to, ColorBGRA color)
        {
            dxLine.Begin();

            dxLine.Draw(new[] {
                new Vector2((int) from.X, (int) from.Y + 4f),
                new Vector2((int) to.X, (int) to.Y + 4f) }, color);

            dxLine.End();
        }
    }
}