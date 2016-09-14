namespace YuLeXerath
{
    using CNLib;
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using YuLeLibrary;
    using Color = System.Drawing.Color;

    internal class Program
    {
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q, W, E, R;
        public static Menu Config;
        public static Obj_AI_Hero Player;
        public static Vector2 PingLocation;
        public static int LastPingT = 0;
        public static Vector3 Rtarget;
        public static float lastR = 0;
        public static Items.Item FarsightOrb = new Items.Item(3342, 4000f), ScryingOrb = new Items.Item(3363, 3500f);

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        public static SharpDX.Direct3D9.Font KillTextFont = new SharpDX.Direct3D9.Font(Drawing.Direct3DDevice, new SharpDX.Direct3D9.FontDescription
        {
            Height = 28,
            FaceName = "Microsoft YaHei",
        });
        public static List<Obj_AI_Hero> KillableList { get; set; } = new List<Obj_AI_Hero>();
        public static Dictionary<int, float> PingList { get; set; } = new Dictionary<int, float>();
        public static bool IsChinese { get; set; } = CNLib.MultiLanguage.IsCN;

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            if (Player.ChampionName != "Xerath")
                return;
			Game.PrintChat("<font color='#FF33CC'><b>QQQUN:438230879</b></font><font color='#CCFF66'><b>-QQQUN：438230879</b></font><font color='#FF9900'><b>-QQQUN：438230879</b></font>");

            LoadSpells();
            LoadMenu();
            LoadEvents();
        }

        private static void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 1550);
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 1150);
            R = new Spell(SpellSlot.R, 675);

            Q.SetSkillshot(0.6f, 95f, float.MaxValue, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.7f, 125f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 60f, 1400f, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.7f, 130f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            Q.SetCharged("XerathArcanopulseChargeUp", "XerathArcanopulseChargeUp", 750, 1550, 1.5f);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }

        private static void LoadMenu()
        {
            Config = new Menu("QQ群：438230879", "YuLeXerath", true).SetFontStyle(System.Drawing.FontStyle.Regular, SharpDX.Color.Pink);

            Config.AddSubMenu(new Menu("走砍设置", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.AddSubMenu(new Menu("按键设置", "KeyMenu"));
            Config.SubMenu("KeyMenu").AddItem(new MenuItem("ComboActive", "连招!").SetValue(new KeyBind(Config.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));
            Config.SubMenu("KeyMenu").AddItem(new MenuItem("HarassActive", "骚扰!").SetValue(new KeyBind(Config.Item("Farm").GetValue<KeyBind>().Key, KeyBindType.Press)));
            Config.SubMenu("KeyMenu").AddItem(new MenuItem("HarassActiveT", "自动骚扰!").SetValue(new KeyBind('Y',KeyBindType.Toggle)));
            Config.SubMenu("KeyMenu").AddItem(new MenuItem("LaneClearActive", "清线!").SetValue(new KeyBind(Config.Item("LaneClear").GetValue<KeyBind>().Key, KeyBindType.Press)));
            Config.SubMenu("KeyMenu").AddItem(new MenuItem("JungleFarmActive", "清野!").SetValue(new KeyBind(Config.Item("LaneClear").GetValue<KeyBind>().Key, KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Q 设置", "QMenu"));
            Config.SubMenu("QMenu").AddItem(new MenuItem("UseQCombo", "连招中使用").SetValue(true));
            Config.SubMenu("QMenu").AddItem(new MenuItem("UseQHarass", "骚扰中使用").SetValue(true));
            Config.SubMenu("QMenu").AddItem(new MenuItem("UseQFarm", "清线中使用").SetValue(true));
            Config.SubMenu("QMenu").AddItem(new MenuItem("UseQFarmMana", "清线最低蓝量").SetValue(new Slider(50)));
            Config.SubMenu("QMenu").AddItem(new MenuItem("UseQJFarm", "清野中使用").SetValue(true));
            Config.SubMenu("QMenu").AddItem(new MenuItem("UseQJFarmMana", "清线最低蓝量").SetValue(new Slider(50)));

            Config.AddSubMenu(new Menu("W 设置", "WMenu"));
            Config.SubMenu("WMenu").AddItem(new MenuItem("UseWCombo", "连招中使用").SetValue(true));
            Config.SubMenu("WMenu").AddItem(new MenuItem("UseWHarass", "骚扰中使用").SetValue(false));
            Config.SubMenu("WMenu").AddItem(new MenuItem("UseWFarm", "清线中使用").SetValue(true));
            Config.SubMenu("WMenu").AddItem(new MenuItem("UseWFarmMana", "清线最低蓝量").SetValue(new Slider(50)));
            Config.SubMenu("WMenu").AddItem(new MenuItem("UseWJFarm", "清野中使用").SetValue(true));
            Config.SubMenu("WMenu").AddItem(new MenuItem("UseWJFarmMana", "清线最低蓝量").SetValue(new Slider(50)));

            Config.AddSubMenu(new Menu("E 设置", "EMenu"));
            Config.SubMenu("EMenu").AddItem(new MenuItem("UseECombo", "连招中使用").SetValue(true));
            Config.SubMenu("EMenu").AddItem(new MenuItem("InterruptSpells", "打断中使用").SetValue(true));
            Config.SubMenu("EMenu").AddItem(new MenuItem("AutoEGC", "反突中使用").SetValue(true));

            Config.AddSubMenu(new Menu("R 设置", "RMenu"));
            Config.SubMenu("RMenu").AddItem(new MenuItem("autoR", "自动R", true).SetValue(true));
            Config.SubMenu("RMenu").AddItem(new MenuItem("autoRlast", "人性化R", true).SetValue(true));
            Config.SubMenu("RMenu").AddItem(new MenuItem("useR", "手动R按键", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press))); //32 == space
            Config.SubMenu("RMenu").AddItem(new MenuItem("trinkiet", "自动使用灯泡", true).SetValue(true));
            Config.SubMenu("RMenu").AddItem(new MenuItem("delayR", "释放延迟", true).SetValue(new Slider(0, 3000, 0)));
            Config.SubMenu("RMenu").AddItem(new MenuItem("MaxRangeR", "最大R半径", true).SetValue(new Slider(0, 5000, 0)));
            Config.SubMenu("RMenu").AddCircle("击杀文本提示", "文字提示R可击杀目标", true, Color.Orange);
            Config.SubMenu("RMenu").AddSlider("击杀文本X", "文字提示横向位置", 71);
            Config.SubMenu("RMenu").AddSlider("击杀文本Y", "文字提示纵向位置", 86);
            Config.SubMenu("RMenu").AddBool("击杀信号提示", "信号提示R可击杀目标(本地)", true);

            Config.AddSubMenu(new Menu("自动眼位", "自动眼位"));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWard", "启动", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoBuy", "lv9自动买灯泡", true).SetValue(false));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoPink", "自动真眼扫描", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWardCombo", "仅连招模式启动 ", true).SetValue(true));
            new AutoWard().Load();
            new Tracker().Load();

            Config.AddSubMenu(new Menu("显示设置", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q 范围").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W 范围").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E 范围").SetValue(new Circle(false, Color.FromArgb(150, Color.DodgerBlue))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R 范围").SetValue(new Circle(false, Color.FromArgb(150, Color.DodgerBlue))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RRangeM", "R 范围 (小地图)").SetValue(new Circle(false,Color.FromArgb(150, Color.DodgerBlue))));
            var dmgAfterComboItem = new MenuItem("DamageAfterR", "显示R伤害").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit += hero => (float)Player.GetSpellDamage(hero, SpellSlot.R) * new int[] { 0, 3, 4, 5 }[Player.GetSpell(SpellSlot.R).Level];
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate (object sender, OnValueChangeEventArgs eventArgs)
            {
                Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };
            Config.SubMenu("Drawings").AddItem(dmgAfterComboItem);
            Config.AddToMainMenu();
        }

        private static void LoadEvents()
        {
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnDraw += Drawing_OnDraw1;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Orbwalking.BeforeAttack += OrbwalkingOnBeforeAttack;
            Obj_AI_Base.OnIssueOrder += Obj_AI_Hero_OnIssueOrder;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
        }

        private static void Drawing_OnDraw1(EventArgs args)
        {
            var ShowT = Config.Item("击杀文本提示").GetValue<Circle>();
            if (ShowT.Active && KillableList?.Count > 0)
            {
                var killname = "R击杀名单\n";
                foreach (var k in KillableList)
                {
                    killname += (k.Name + "　").ToGBK() + $"({k.ChampionName.ToCN(IsChinese)})\n";
                }

                var KillTextColor = new ColorBGRA
                {
                    A = Config.Item("击杀文本提示").GetValue<Circle>().Color.A,
                    B = Config.Item("击杀文本提示").GetValue<Circle>().Color.B,
                    G = Config.Item("击杀文本提示").GetValue<Circle>().Color.G,
                    R = Config.Item("击杀文本提示").GetValue<Circle>().Color.R,
                };

 

                KillTextFont.DrawText(null, killname,
                    (int)(Drawing.Width * ((float)Config.Item("击杀文本X").GetValue<Slider>().Value / 100)),
                    (int)(Drawing.Height * ((float)Config.Item("击杀文本Y").GetValue<Slider>().Value / 100)),
                    KillTextColor);
            }
        }

        private static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.R)
            {
                if (Config.Item("trinkiet", true).GetValue<bool>() && !IsCastingR)
                {
                    if (Player.Level < 9)
                        ScryingOrb.Range = 2500;
                    else
                        ScryingOrb.Range = 3500;

                    if (ScryingOrb.IsReady())
                        ScryingOrb.Cast(Rtarget);
                    if (FarsightOrb.IsReady())
                        FarsightOrb.Cast(Rtarget);
                }
            }
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!Config.Item("InterruptSpells").GetValue<bool>()) return;
                  
            if (Player.Distance(sender) < E.Range)
            {
                E.Cast(sender);
            }
        }

        static void Obj_AI_Hero_OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (IsCastingR && Config.Item("BlockMovement").GetValue<bool>())
            {
                args.Process = false;
            }
        }

        private static void OrbwalkingOnBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            args.Process = AttacksEnabled;
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!Config.Item("AutoEGC").GetValue<bool>()) return;

            if (Player.Distance(gapcloser.Sender) < E.Range)
            {
                E.Cast(gapcloser.Sender);
            }
        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            if (args.SData.Name == "xerathlocuspulse")
            {
                lastR = Game.Time;
            }
        }

        private static void Combo()
        {
            UseSpells(Config.Item("UseQCombo").GetValue<bool>(), Config.Item("UseWCombo").GetValue<bool>(),
                Config.Item("UseECombo").GetValue<bool>());
        }

        private static void Harass()
        {
            UseSpells(Config.Item("UseQHarass").GetValue<bool>(), Config.Item("UseWHarass").GetValue<bool>(),
                false);
        }

        private static void UseSpells(bool useQ, bool useW, bool useE)
        {
            var qTarget = TargetSelector.GetTarget(Q.ChargedMaxRange, TargetSelector.DamageType.Magical);
            var wTarget = TargetSelector.GetTarget(W.Range + W.Width * 0.5f, TargetSelector.DamageType.Magical);
            var eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            Hacks.DisableCastIndicator = Q.IsCharging && useQ;

            if (eTarget != null && useE && E.IsReady())
            {
                if (Player.Distance(eTarget) < E.Range * 0.4f)
                    E.Cast(eTarget);
                else if ((!useW || !W.IsReady()))
                    E.Cast(eTarget);
            }

            if (useQ && Q.IsReady() && qTarget != null)
            {
                if (Q.IsCharging)
                {
                    Q.Cast(qTarget, false, false);
                }
                else if (!useW || !W.IsReady() || Player.Distance(qTarget) > W.Range)
                {
                    Q.StartCharging();
                }
            }

            if (wTarget != null && useW && W.IsReady())
                W.Cast(wTarget, false, true);
        }

        private static void Farm()
        {
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.ChargedMaxRange,
                MinionTypes.All);
            var rangedMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range + W.Width + 30,
                MinionTypes.Ranged);

            var useQ = Config.Item("UseQFarm").GetValue<bool>();
            var useW = Config.Item("UseWFarm").GetValue<bool>();

            if (useW && W.IsReady() && Player.ManaPercent >= Config.Item("UseWFarmMana").GetValue<Slider>().Value)
            {
                var locW = W.GetCircularFarmLocation(rangedMinionsW, W.Width * 0.75f);
                if (locW.MinionsHit >= 3 && W.IsInRange(locW.Position.To3D()))
                {
                    W.Cast(locW.Position);
                    return;
                }
                else
                {
                    var locW2 = W.GetCircularFarmLocation(allMinionsQ, W.Width * 0.75f);
                    if (locW2.MinionsHit >= 1 && W.IsInRange(locW.Position.To3D()))
                    {
                        W.Cast(locW.Position);
                        return;
                    }
                        
                }
            }

            if (useQ && Q.IsReady() && Player.ManaPercent >= Config.Item("UseQFarmMana").GetValue<Slider>().Value)
            {
                if (Q.IsCharging)
                {
                    var locQ = Q.GetLineFarmLocation(allMinionsQ);
                    if (allMinionsQ.Count == allMinionsQ.Count(m => Player.Distance(m) < Q.Range) && locQ.MinionsHit > 2 && locQ.Position.IsValid())
                        Q.Cast(locQ.Position);
                }
                else if (allMinionsQ.Count > 2)
                    Q.StartCharging();
            }
        }

        private static void JungleFarm()
        {
            var useQ = Config.Item("UseQJFarm").GetValue<bool>();
            var useW = Config.Item("UseWJFarm").GetValue<bool>();
            var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (mobs.Count > 0)
            {
                var mob = mobs[0];
                if (useW && W.IsReady() && Player.ManaPercent >= Config.Item("UseWJFarmMana").GetValue<Slider>().Value)
                {
                    W.Cast(mob);
                }
                else if (useQ && Q.IsReady() && Player.ManaPercent >= Config.Item("UseQJFarmMana").GetValue<Slider>().Value)
                {
                    if (!Q.IsCharging)
                        Q.StartCharging();
                    else
                        Q.Cast(mob);
                }
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget()))
            {
                if (!enemy.IsDead && enemy.IsValid && RKS(enemy))
                {
                    if (!KillableList.Contains(enemy))
                    {
                        KillableList.Add(enemy);
                    }
                }
                else
                {
                    if (KillableList.Contains(enemy))
                    {
                        KillableList.Remove(enemy);
                    }
                }
            }

            if (Player.IsDead)
                return;

            AutoWard.Enable = Config.Item("AutoWard", true).GetValue<bool>();
            AutoWard.AutoBuy = Config.Item("AutoBuy", true).GetValue<bool>();
            AutoWard.AutoPink = Config.Item("AutoPink", true).GetValue<bool>();
            AutoWard.OnlyCombo = Config.Item("AutoWardCombo", true).GetValue<bool>();
            AutoWard.InComboMode = Config.Item("ComboActive").GetValue<KeyBind>().Active;


            if (R.IsReady() && Config.Item("击杀信号提示").GetValue<bool>())
            {
                foreach (var enemy in HeroManager.Enemies.Where(h => h.IsValidTarget() && (float)Player.GetSpellDamage(h, SpellSlot.R) * new int[] { 0, 3, 4, 5 }[Player.GetSpell(SpellSlot.R).Level] > h.Health))
                {
                    Ping(enemy.Position.To2D());
                }
            }

            Q.MinHitChance = HitChance.VeryHigh;
            W.MinHitChance = HitChance.VeryHigh;
            E.MinHitChance = HitChance.VeryHigh;
            R.MinHitChance = HitChance.VeryHigh;

            Orbwalker.SetMovement(true);

            R.Range = 1200 * R.Level + 2000;

            if(R.IsReady())
            {
                Logic();
            }

            if (IsCastingR)
            {
                Orbwalker.SetMovement(false);
                return;
            }
            else
            {
                Orbwalker.SetMovement(true);
            }

            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (Config.Item("HarassActive").GetValue<KeyBind>().Active || Config.Item("HarassActiveT").GetValue<KeyBind>().Active)
                    Harass();

                if(Config.Item("LaneClearActive").GetValue<KeyBind>().Active)
                    Farm();

                if (Config.Item("JungleFarmActive").GetValue<KeyBind>().Active)
                    JungleFarm();
            }
        }

        private static void Logic()
        {
            R.Range = 2000 + R.Level * 1200;

            if (!IsCastingR)
                R.Range = R.Range - Config.Item("MaxRangeR", true).GetValue<Slider>().Value;

            var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            if (t.IsValidTarget())
            {
                if (Config.Item("useR", true).GetValue<KeyBind>().Active && !IsCastingR)
                {
                    R.Cast();
                }
                if (!t.IsValidTarget(W.Range) && Config.Item("autoR", true).GetValue<bool>() && !IsCastingR && t.CountAlliesInRange(500) == 0 && Player.CountEnemiesInRange(1100) == 0)
                {
                    if (RKS(t))
                    {
                        R.Cast();
                    }
                }
                if (Game.Time - lastR > 0.001 * (float)Config.Item("delayR", true).GetValue<Slider>().Value && IsCastingR)
                {
                    R.Cast(t, true);

                }
                Rtarget = R.GetPrediction(t).CastPosition;
            }
            else if (Config.Item("autoRlast", true).GetValue<bool>() && Game.Time - lastR > 0.001 * (float)Config.Item("delayR", true).GetValue<Slider>().Value && IsCastingR)
            {
                R.Cast(Rtarget);
            }
        }

        private static bool RKS(Obj_AI_Hero t)
        {
            if (Common.GetKsDamage(t, R) + (R.GetDamage(t) * R.Level) > t.Health)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (R.Level == 0)
                return;

            var menuItem = Config.Item(R.Slot + "RangeM").GetValue<Circle>();

            if (menuItem.Active)
                Utility.DrawCircle(Player.Position, R.Range, menuItem.Color, 1, 23, true);
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active && (spell.Slot != SpellSlot.R || R.Level > 0))
                    Render.Circle.DrawCircle(Player.Position, spell.Range, menuItem.Color);
            }
        }

        private static bool AttacksEnabled
        {
            get
            {
                if (IsCastingR)
                {
                    return false;
                }

                if (!ObjectManager.Player.CanAttack)
                {
                    return false;
                }


                if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
                {
                    return IsPassiveUp || (!Q.IsReady() && !W.IsReady() && !E.IsReady());
                }


                return true;
            }
        }

        private static void Ping(Vector2 position)
        {
            if (Utils.TickCount - LastPingT < 30 * 1000)
            {
                return;
            }

            LastPingT = Utils.TickCount;
            PingLocation = position;
            SimplePing();

            Utility.DelayAction.Add(150, SimplePing);
            Utility.DelayAction.Add(300, SimplePing);
            Utility.DelayAction.Add(400, SimplePing);
            Utility.DelayAction.Add(800, SimplePing);
        }

        private static void SimplePing()
        {
            Game.ShowPing(PingCategory.Fallback, PingLocation, true);
        }

        public static bool IsPassiveUp
        {
            get { return ObjectManager.Player.HasBuff("xerathascended2onhit"); }
        }

        public static bool IsCastingR
        {
            get
            {
                return ObjectManager.Player.HasBuff("XerathLocusOfPower2") ||
                       (ObjectManager.Player.LastCastedSpellName().Equals("XerathLocusOfPower2", StringComparison.InvariantCultureIgnoreCase) &&
                        Utils.TickCount - ObjectManager.Player.LastCastedSpellT() < 500);
            }
        }

    }
}
