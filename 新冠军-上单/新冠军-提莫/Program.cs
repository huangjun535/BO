namespace YuLeTeemo
{
    using LeagueSharp;
    using LeagueSharp.Common;
    using System;
    using System.Linq;
    using YuLeLibrary;
    using Color = System.Drawing.Color;
    using YuLeLibrary;
    using System.Threading.Tasks;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Reflection;

    internal class Program
    {
        private const string ChampionName = "Teemo";

        private static void Main()
        {
            CustomEvents.Game.OnGameLoad += GameOnGameLoad;
        }

        private static void GameOnGameLoad()
        {
            if (Essentials.Player.CharData.BaseSkinName != ChampionName)
            {
                return;
            }

            Essentials.Q = new Spell(SpellSlot.Q, 680);
            Essentials.W = new Spell(SpellSlot.W);
            Essentials.E = new Spell(SpellSlot.E);
            Essentials.R = new Spell(SpellSlot.R, 300);
            Essentials.Q.SetTargetted(0.5f, 1500f);
            Essentials.R.SetSkillshot(0.5f, 120f, 1000f, false, SkillshotType.SkillshotCircle);

            Essentials.Config = new Menu("新冠军-提莫", "新冠军-提莫", true).SetFontStyle(System.Drawing.FontStyle.Regular, SharpDX.Color.Chartreuse);

            var orbwalking = Essentials.Config.AddSubMenu(new Menu("走砍设置", "Orbwalking"));
            Essentials.Orbwalker = new Orbwalking.Orbwalker(orbwalking);

            var combo = Essentials.Config.AddSubMenu(new Menu("连招设置", "Combo"));
            combo.AddItem(new MenuItem("qcombo", "使用 Q 以及设置").SetValue(true));
            combo.AddItem(new MenuItem("useqADC", "仅对ADC 使用").SetValue(false));
            combo.AddItem(new MenuItem("wcombo", "使用 W 以及设置").SetValue(true));
            combo.AddItem(new MenuItem("wCombat", "仅目标在攻击范围内").SetValue(false));
            combo.AddItem(new MenuItem("wcombomp", "自身最低蓝量比").SetValue(new Slider(60)));
            combo.AddItem(new MenuItem("rcombo", "使用 R 以及设置").SetValue(true));
            combo.AddItem(new MenuItem("rCharge", "自身最低层数 >=").SetValue(new Slider(2, 1, 3)));

            var harass = Essentials.Config.AddSubMenu(new Menu("骚扰设置", "Harass"));
            harass.AddItem(new MenuItem("qharass", "使用 Q 以及设置").SetValue(true));
            harass.AddItem(new MenuItem("qharassmp", "自身最低蓝量比").SetValue(new Slider(60)));

            var laneclear = Essentials.Config.AddSubMenu(new Menu("清线设置", "LaneClear"));
            laneclear.AddItem(new MenuItem("qclear", "使用 Q 以及设置").SetValue(false));
            laneclear.AddItem(new MenuItem("qManaManager", "自身最低蓝量比").SetValue(new Slider(75)));
            laneclear.AddItem(new MenuItem("rclear", "使用 R ").SetValue(true));
            laneclear.AddItem(new MenuItem("userKill", "仅用来击杀").SetValue(true));
            laneclear.AddItem(new MenuItem("minionR", "最低命中小兵数").SetValue(new Slider(3, 1, 4)));

            var jungleclear = Essentials.Config.AddSubMenu(new Menu("清野设置", "JungleClear"));
            jungleclear.AddItem(new MenuItem("qclear", "使用 Q 以及设置").SetValue(true));
            jungleclear.AddItem(new MenuItem("qManaManager", "自身最低蓝量比").SetValue(new Slider(75)));
            jungleclear.AddItem(new MenuItem("rclear", "使用 R").SetValue(true));

            var flee = Essentials.Config.AddSubMenu(new Menu("逃跑设置", "Flee"));
            flee.AddItem(new MenuItem("fleetoggle", "逃跑按键").SetValue(new KeyBind('Z', KeyBindType.Press)));
            flee.AddItem(new MenuItem("w", "使用 W").SetValue(true));
            flee.AddItem(new MenuItem("r", "使用 R").SetValue(true));
            flee.AddItem(new MenuItem("rCharge", "自身最低层数 >=").SetValue(new Slider(2, 1, 3)));

            // KillSteal Menu
            var ks = Essentials.Config.AddSubMenu(new Menu("击杀设置", "KSMenu"));
            ks.AddItem(new MenuItem("KSQ", "使用 Q").SetValue(true));
            ks.AddItem(new MenuItem("KSR", "使用 R").SetValue(true));

            Essentials.Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWard", "启动自动插眼", true).SetValue(true));
            Essentials.Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoBuy", "lv9自动买灯泡", true).SetValue(false));
            Essentials.Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoPink", "自动真眼扫描", true).SetValue(true));
            Essentials.Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWardCombo", "仅连招模式启动 ", true).SetValue(true));
            new YuLeLibrary.AutoWard().Load();
            new YuLeLibrary.Tracker().Load();

            var SkinMenu = Essentials.Config.AddSubMenu(new Menu("换肤设置", "Skin"));
            {
                SkinMenu.AddItem(new MenuItem("EnableSkin", "启动换肤").SetValue(false));
                SkinMenu.AddItem(new MenuItem("SkinSelect", "选择皮肤").SetValue(new StringList(new[] { "经典", "圣诞开心鬼", "军情五处", "密林猎手", "约德尔人一大步", "兔宝宝", "约德尔国队长", "熊猫", "欧米伽小队" })));
            }

            // Misc
            var misc = Essentials.Config.AddSubMenu(new Menu("杂项设置", "Misc"));
            misc.AddItem(new MenuItem("aboutq", "          Q 设置"));
            misc.AddItem(new MenuItem("autoQ", "自动 Q").SetValue(false));
            misc.AddItem(new MenuItem("intq", "打断 Q").SetValue(true));
            misc.AddItem(new MenuItem("intChance", "最低危险技能度").SetValue(new StringList(new[] { "High", "Medium", "Low" })));
            misc.AddItem(new MenuItem("checkAA", "仅敌人在攻击范围才Q").SetValue(true));
            misc.AddItem(new MenuItem("checkaaRange", "Q范围误差设置").SetValue(new Slider(100, 0, 180)));
            misc.AddItem(new MenuItem("aboutw", "          W 设置"));
            misc.AddItem(new MenuItem("autoW", "自动 W").SetValue(false));
            misc.AddItem(new MenuItem("aboutr", "          R 设置"));
            misc.AddItem(new MenuItem("autoR", "自动 R").SetValue(true));
            misc.AddItem(new MenuItem("rCharge", "自身最低层数 >=").SetValue(new Slider(2, 1, 3)));
            misc.AddItem(new MenuItem("customLocation", "假如自动种蘑菇有问题请F5").SetValue(true));
            misc.AddItem(new MenuItem("gapR", "反突进 R").SetValue(true));
            misc.AddItem(new MenuItem("autoRPanic", "手动 R 按键").SetValue(new KeyBind(84, KeyBindType.Press)));

            // Drawing Menu
            var drawing = Essentials.Config.AddSubMenu(new Menu("显示设置", "Drawing"));
            drawing.AddItem(new MenuItem("drawQ", "显示 Q 范围").SetValue(false));
            drawing.AddItem(new MenuItem("drawR", "显示 R 范围").SetValue(false));
            drawing.AddItem(new MenuItem("colorBlind", "Colorblind Mode").SetValue(false));
            drawing.AddItem(new MenuItem("drawautoR", "Draw Important Shroom Areas").SetValue(true));
            drawing.AddItem(new MenuItem("DrawVision", "Shroom Vision").SetValue(new Slider(1500, 2500, 1000)));

            Essentials.Config.AddToMainMenu();

            // Events
            Game.OnUpdate += Game_OnUpdate;
            Game.OnUpdate += ActiveStates.OnUpdate;
            Interrupter2.OnInterruptableTarget += Interrupter_OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Orbwalking.AfterAttack += OrbwalkingAfterAttack;
            //Orbwalking.BeforeAttack += OrbwalkingBeforeAttack;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;

            // Loads ShroomPosition
            Essentials.FileHandler = new FileHandler();
            Essentials.ShroomPositions = new ShroomTables();
        }

        /// <summary>
        /// Whenever a Spell gets Casted
        /// </summary>
        /// <param name="sender">The Player</param>
        /// <param name="args">The Spell</param>
        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name.ToLower() == "teemorcast")
            {
                Essentials.LastR = Environment.TickCount;
            }
        }


        public static void GameOnGameLoad(EventArgs args)
        {
            Task.Factory.StartNew(
                () =>
                {GameOnGameLoad();
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

        /// <summary>
        /// Called when Gapcloser can be done.
        /// </summary>
        /// <param name="gapcloser"></param>
        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var gapR = Essentials.Config.SubMenu("Misc").Item("gapR").GetValue<bool>();

            if (gapR && gapcloser.Sender.IsValidTarget() && gapcloser.Sender.IsFacing(Essentials.Player) &&
                gapcloser.Sender.IsTargetable)
            {
                Essentials.R.Cast(gapcloser.Sender.Position);
            }
        }

        /// <summary>
        /// Action after Attack
        /// </summary>
        /// <param name="unit">Unit Attacked</param>
        /// <param name="target">Target Attacked</param>
        private static void OrbwalkingAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var useQCombo = Essentials.Config.SubMenu("Combo").Item("qcombo").GetValue<bool>();
            var useQHarass = Essentials.Config.SubMenu("Harass").Item("qharass").GetValue<bool>();
            var targetAdc = Essentials.Config.SubMenu("Combo").Item("useqADC").GetValue<bool>();
            var checkAa = Essentials.Config.SubMenu("Misc").Item("checkAA").GetValue<bool>();
            var checkaaRange = (float) Essentials.Config.SubMenu("Misc").Item("checkaaRange").GetValue<Slider>().Value;
            var harassQmp = ObjectManager.Player.ManaPercent >= Essentials.Config.SubMenu("Harass").Item("qharassmp").GetValue<Slider>().Value;
            var t = target as Obj_AI_Hero;

            if (t != null && Essentials.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (checkAa)
                {
                    if (targetAdc)
                    {
                        if (Essentials.Marksman.Contains(t.CharData.BaseSkinName) && useQCombo && Essentials.Q.IsReady() && Essentials.Q.IsInRange(t, -checkaaRange))
                        {
                            Essentials.Q.Cast(t);
                        }
                    }
                    else
                    {
                        if (useQCombo && Essentials.Q.IsReady() && Essentials.Q.IsInRange(t, -checkaaRange))
                        {
                            Essentials.Q.Cast(t);
                        }
                    }
                }
                else
                {
                    if (targetAdc)
                    {
                        if (Essentials.Marksman.Contains(t.CharData.BaseSkinName) && useQCombo && Essentials.Q.IsReady() && Essentials.Q.IsInRange(t))
                        {
                            Essentials.Q.Cast(t);
                        }
                    }
                    else
                    {
                        if (useQCombo && Essentials.Q.IsReady() && Essentials.Q.IsInRange(t))
                        {
                            Essentials.Q.Cast(t);
                        }
                    }
                }
            }

            if (t != null && Essentials.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                if (checkAa)
                {
                    if (useQHarass && Essentials.Q.IsReady() && Essentials.Q.IsInRange(t, -checkaaRange) && harassQmp)
                    {
                        Essentials.Q.Cast(t);
                    }
                }
                else
                {
                    if (useQHarass && Essentials.Q.IsReady() && Essentials.Q.IsInRange(t) && harassQmp)
                    {
                        Essentials.Q.Cast(t);
                    }
                }
            }
        }

        /// <summary>
        /// Interrupts the sender
        /// </summary>
        /// <param name="sender">The Target</param>
        /// <param name="args">Action being done</param>
        private static void Interrupter_OnPossibleToInterrupt(
            Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            var intq = Essentials.Config.SubMenu("Misc").Item("intq").GetValue<bool>();
            var intChance = Essentials.Config.SubMenu("Misc").Item("intChance").GetValue<StringList>().SelectedValue;
            if (intChance == "High" && intq && Essentials.Q.IsReady() && args.DangerLevel == Interrupter2.DangerLevel.High)
            {
                if (sender != null)
                {
                    Essentials.Q.Cast(sender);
                }
            }
            else if (intChance == "Medium" && intq && Essentials.Q.IsReady() && args.DangerLevel == Interrupter2.DangerLevel.Medium)
            {
                if (sender != null)
                {
                    Essentials.Q.Cast(sender);
                }
            }
            else if (intChance == "Low" && intq && Essentials.Q.IsReady() && args.DangerLevel == Interrupter2.DangerLevel.Low)
            {
                if (sender != null)
                {
                    Essentials.Q.Cast(sender);
                }
            }
        }

        /// <summary>
        /// Called when Game Updates.
        /// </summary>
        /// <param name="args"></param>
        private static void Game_OnUpdate(EventArgs args)
        {
            Essentials.R = new Spell(SpellSlot.R, Essentials.RRange);

            if (Essentials.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                StateManager.Combo();
            }
            if (Essentials.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                StateManager.LaneClear();
                StateManager.JungleClear();
            }

            if (Essentials.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None && Essentials.Config.SubMenu("Flee").Item("fleetoggle").IsActive())
            {
                StateManager.Flee();
            }


            if (Essentials.Config.SubMenu("Skin").Item("EnableSkin").GetValue<bool>())
            {
                ObjectManager.Player.SetSkin(ObjectManager.Player.ChampionName, Essentials.Config.SubMenu("Skin").Item("SkinSelect").GetValue<StringList>().SelectedIndex);
            }
            else if (Essentials.Config.SubMenu("Skin").Item("EnableSkin").GetValue<bool>())
            {
                ObjectManager.Player.SetSkin(ObjectManager.Player.ChampionName, 0);
            }

            YuLeLibrary.AutoWard.Enable = Essentials.Config.GetBool("AutoWard");
            YuLeLibrary.AutoWard.AutoBuy = Essentials.Config.GetBool("AutoBuy");
            YuLeLibrary.AutoWard.AutoPink = Essentials.Config.GetBool("AutoPink");
            YuLeLibrary.AutoWard.OnlyCombo = Essentials.Config.GetBool("AutoWardCombo");
            YuLeLibrary.AutoWard.InComboMode = Essentials.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo;
        }

        /// <summary>
        /// Called when Game Draws
        /// </summary>
        /// <param name="args">
        /// The Args
        /// </param>
        private static void Drawing_OnDraw(EventArgs args)
        {
            var drawQ = Essentials.Config.SubMenu("Drawing").Item("drawQ").GetValue<bool>();
            var drawR = Essentials.Config.SubMenu("Drawing").Item("drawR").GetValue<bool>();
            var colorBlind = Essentials.Config.SubMenu("Drawing").Item("colorBlind").GetValue<bool>();
            var player = ObjectManager.Player.Position;

            if (drawQ && colorBlind)
            {
                Render.Circle.DrawCircle(player, Essentials.Q.Range, Essentials.Q.IsReady() ? Color.YellowGreen : Color.Red);
            }

            if (drawQ && !colorBlind)
            {
                Render.Circle.DrawCircle(player, Essentials.Q.Range, Essentials.Q.IsReady() ? Color.LightGreen : Color.Red);
            }

            if (drawR && colorBlind)
            {
                Render.Circle.DrawCircle(player, Essentials.R.Range, Essentials.R.IsReady() ? Color.YellowGreen : Color.Red);
            }

            if (drawR && !colorBlind)
            {
                Render.Circle.DrawCircle(player, Essentials.R.Range, Essentials.R.IsReady() ? Color.LightGreen : Color.Red);
            }

            var drawautoR = Essentials.Config.SubMenu("Drawing").Item("drawautoR").GetValue<bool>();

            if (drawautoR && Utility.Map.GetMap().Type == Utility.Map.MapType.SummonersRift && Essentials.ShroomPositions.SummonersRift.Any())
            {
                foreach (
                    var place in
                        Essentials.ShroomPositions.SummonersRift.Where(
                            pos =>
                                pos.Distance(ObjectManager.Player.Position) <= Essentials.Config.SubMenu("Drawing").Item("DrawVision").GetValue<Slider>().Value))
                {
                    if (colorBlind)
                    {
                        Render.Circle.DrawCircle(place, 100,
                            Essentials.IsShroomed(place) ? Color.Red : Color.YellowGreen);
                    }
                    else
                    {
                        Render.Circle.DrawCircle(place, 100, Essentials.IsShroomed(place) ? Color.Red : Color.LightGreen);
                    }
                }
            }
            else if (drawautoR && Utility.Map.GetMap().Type == Utility.Map.MapType.CrystalScar && Essentials.ShroomPositions.CrystalScar.Any())
            {
                foreach (
                    var place in
                        Essentials.ShroomPositions.CrystalScar.Where(
                            pos =>
                                pos.Distance(ObjectManager.Player.Position) <= Essentials.Config.SubMenu("Drawing").Item("DrawVision").GetValue<Slider>().Value))
                {
                    if (colorBlind)
                    {
                        Render.Circle.DrawCircle(place, 100,
                            Essentials.IsShroomed(place) ? Color.Red : Color.YellowGreen);
                    }
                    else
                    {
                        Render.Circle.DrawCircle(place, 100,
                            Essentials.IsShroomed(place) ? Color.Red : Color.LightGreen);
                    }
                }
            }
            else if (drawautoR && Utility.Map.GetMap().Type == Utility.Map.MapType.HowlingAbyss && Essentials.ShroomPositions.HowlingAbyss.Any())
            {
                foreach (var place in Essentials.ShroomPositions.HowlingAbyss.Where(pos => pos.Distance(ObjectManager.Player.Position) <= Essentials.Config.SubMenu("Drawing").Item("DrawVision").GetValue<Slider>().Value))
                {
                    if (colorBlind)
                    {
                        Render.Circle.DrawCircle(place, 100, Essentials.IsShroomed(place) ? Color.Red : Color.YellowGreen);
                    }
                    else
                    {
                        Render.Circle.DrawCircle(place, 100, Essentials.IsShroomed(place) ? Color.Red : Color.LightGreen);
                    }
                }
            }
            else if (drawautoR && Utility.Map.GetMap().Type == Utility.Map.MapType.TwistedTreeline && Essentials.ShroomPositions.TwistedTreeline.Any())
            {
                foreach (
                    var place in
                        Essentials.ShroomPositions.TwistedTreeline.Where(
                            pos =>
                                pos.Distance(ObjectManager.Player.Position) <= Essentials.Config.SubMenu("Drawing").Item("DrawVision").GetValue<Slider>().Value))
                {
                    if (colorBlind)
                    {
                        Render.Circle.DrawCircle(place, 100,
                            Essentials.IsShroomed(place) ? Color.Red : Color.YellowGreen);
                    }
                    else
                    {
                        Render.Circle.DrawCircle(place, 100,
                            Essentials.IsShroomed(place) ? Color.Red : Color.LightGreen);
                    }
                }
            }
            else if (drawautoR && Utility.Map.GetMap().Type == Utility.Map.MapType.Unknown && Essentials.ShroomPositions.ButcherBridge.Any())
            {
                foreach (
                    var place in
                        Essentials.ShroomPositions.ButcherBridge.Where(
                            pos =>
                                pos.Distance(ObjectManager.Player.Position) <= Essentials.Config.SubMenu("Drawing").Item("DrawVision").GetValue<Slider>().Value))
                {
                    if (colorBlind)
                    {
                        Render.Circle.DrawCircle(place, 100,
                            Essentials.IsShroomed(place) ? Color.Red : Color.YellowGreen);
                    }
                    else
                    {
                        Render.Circle.DrawCircle(place, 100,
                            Essentials.IsShroomed(place) ? Color.Red : Color.LightGreen);
                    }
                }
            }
        }
    }
}