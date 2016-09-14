namespace YuLeCassiopeia
{
    using LeagueSharp.Common;
    using System.Drawing;
    using Color = SharpDX.Color;

    /// <summary>
    ///     Assembly Configuration
    /// </summary>
    internal class Config
    {
        #region Static Fields

        /// <summary>
        ///     The instance
        /// </summary>
        private static Config instance;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the instance.
        /// </summary>
        /// <value>
        ///     The instance.
        /// </value>
        public static Config Instance => instance ?? (instance = new Config());

        /// <summary>
        ///     Gets or sets the menu.
        /// </summary>
        /// <value>
        ///     The menu.
        /// </value>
        public Menu Menu { get; set; }

        #endregion

        #region Public Indexers

        public bool this[string menuName] => this.Menu.Item(menuName).GetValue<bool>();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Loads this instance.
        /// </summary>
        /// <returns>A <see cref="Menu" /> for the <see cref="Orbwalking.Orbwalker" />.</returns>
        public Menu Load()
        {
            this.Menu = new Menu("QQ群：438230879", "YuLeCassiopeia", true).SetFontStyle(FontStyle.Bold, new Color(0, 255, 255));

            var orbwalkerSettings = new Menu("走砍设置", "OrbwalkerSettings");
            this.Menu.AddSubMenu(orbwalkerSettings);

            var comboMenu = new Menu("连招设置", "ComboSettings");
            var blackListMenu = new Menu("大招黑名单", "BlackPeopleLOL");
            HeroManager.Enemies.ForEach(x => blackListMenu.AddItem(new MenuItem($"Blacklist{x.ChampionName}", x.ChampionName)).SetValue(false));
            comboMenu.AddSubMenu(blackListMenu);
            comboMenu.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseEPoisonCombo", "使用E仅目标中毒").SetValue(false));
            comboMenu.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseREnemyFacing", "如果面对N个敌人使用R").SetValue(new Slider(2, 1, HeroManager.Enemies.Count)));
            comboMenu.AddItem(new MenuItem("UseRComboKillable", "如果连招能击杀使用R").SetValue(true));
            comboMenu.AddItem(new MenuItem("UseRAboveEnemyHp", "敌人血量低于百分比使用R").SetValue(new Slider(75)));
            this.Menu.AddSubMenu(comboMenu);

            var harassMenu = new Menu("骚扰设置", "HarassSettings");
            harassMenu.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("UseWHarass", "Use W").SetValue(true));
            harassMenu.AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
            harassMenu.AddItem(new MenuItem("UseEPoisonHarass", "使用E|仅目标中毒").SetValue(true));
            harassMenu.AddItem(new MenuItem("UseEFarmHarass", "Last hit with E").SetValue(true));
            harassMenu.AddItem(new MenuItem("HarassMana", "Harass Mana").SetValue(new Slider(50)));
            this.Menu.AddSubMenu(harassMenu);

            var waveClearMenu = new Menu("清线清野", "WaveClearSettings");
            waveClearMenu.AddItem(new MenuItem("UseQWaveClear", "Use Q").SetValue(true));
            waveClearMenu.AddItem(new MenuItem("UseEWaveClear", "Use E").SetValue(true));
            waveClearMenu.AddItem(new MenuItem("WaveClearChamps", "只在附近没有英雄的时候清兵").SetValue(false));
            waveClearMenu.AddItem(new MenuItem("WaveClearHarass", "清线时自动骚扰敌人").SetValue(true));
            waveClearMenu.AddItem(new MenuItem("WaveClearMana", "清兵最低蓝量").SetValue(new Slider(60)));
            this.Menu.AddSubMenu(waveClearMenu);

            var lastHitMenu = new Menu("补刀设置", "LastHitSettings");
            lastHitMenu.AddItem(new MenuItem("UseELastHit", "Use E").SetValue(true));
            lastHitMenu.AddItem(new MenuItem("LastHitMana", "补刀最低蓝量").SetValue(new Slider(60)));
            this.Menu.AddSubMenu(lastHitMenu);

            var ksMenu = new Menu("击杀设置", "KillStealSettings");
            ksMenu.AddItem(new MenuItem("UseQKS", "Use Q").SetValue(true));
            ksMenu.AddItem(new MenuItem("UseWKS", "Use W").SetValue(true));
            ksMenu.AddItem(new MenuItem("UseEKS", "Use E").SetValue(true));
            ksMenu.AddItem(new MenuItem("UseRKS", "Use R").SetValue(false));
            this.Menu.AddSubMenu(ksMenu);

            var miscMenu = new Menu("杂项设置", "MiscSettings");
            miscMenu.AddItem(new MenuItem("AutoAttackCombo", "连招中用普攻").SetValue(true));
            miscMenu.AddItem(new MenuItem("AutoAttackHarass", "骚扰中用普攻").SetValue(true));
            miscMenu.AddItem(new MenuItem("CustomTargeting", "智能选择目标").SetValue(true));
            miscMenu.AddItem(new MenuItem("AutoWCC", "自动W被控制的目标").SetValue(true));
            miscMenu.AddItem(new MenuItem("AntiGapcloser", "开启反突进模式").SetValue(true));
            miscMenu.AddItem(new MenuItem("Interrupter", "R技能打断技能").SetValue(true));
            miscMenu.AddItem(new MenuItem("DontQWIfTargetPoisoned", "目标中毒不使用QW").SetValue(true));
            miscMenu.AddItem(new MenuItem("StackTear", "自动叠眼泪").SetValue(true));
            this.Menu.AddSubMenu(miscMenu);

            this.Menu.SubMenu("自动眼位").AddItem(new MenuItem("AutoWard", "启动自动插眼", true).SetValue(true));
            this.Menu.SubMenu("自动眼位").AddItem(new MenuItem("AutoBuy", "lv9自动买灯泡", true).SetValue(true));
            this.Menu.SubMenu("自动眼位").AddItem(new MenuItem("AutoPink", "自动真眼扫描", true).SetValue(true));
            this.Menu.SubMenu("自动眼位").AddItem(new MenuItem("AutoWardCombo", "仅连招模式启动 ", true).SetValue(true));
            new AutoWard().Load();
            new Tracker().Load();

            var SkinMenu = this.Menu.AddSubMenu(new Menu("换肤设置", "Skin"));
            {
                SkinMenu.AddItem(new MenuItem("EnableSkin", "启动换肤").SetValue(false));
                SkinMenu.AddItem(new MenuItem("SkinSelect", "选择皮肤").SetValue(new StringList(new[] { "经典", "荒漠之咬", "深海妖姬", "蛇发女妖", "碧玉之牙" })));
            }

            var drawingSettings = new Menu("显示设置", "DrawingSettings");
            drawingSettings.AddItem(new MenuItem("DrawQ", "Draw Q").SetValue(false));
            drawingSettings.AddItem(new MenuItem("DrawW", "Draw W").SetValue(false));
            drawingSettings.AddItem(new MenuItem("DrawE", "Draw E").SetValue(false));
            drawingSettings.AddItem(new MenuItem("DrawR", "Draw R").SetValue(false));
            this.Menu.AddSubMenu(drawingSettings);

            this.Menu.AddToMainMenu();

            return orbwalkerSettings;
        }

        #endregion
    }
}