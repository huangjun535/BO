
#pragma warning disable 1587

namespace YuLeVeigar.Champions.Veigar
{
    using YuLeVeigar.Utilities;

    using LeagueSharp.SDK.UI;

    /// <summary>
    ///     The menu class.
    /// </summary>
    internal class Menus
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Sets the menu.
        /// </summary>
        public static void Initialize()
        {
            Vars.QMenu = new Menu("q", "Q技能设置");
            {
                Vars.QMenu.Add(new MenuBool("combo", "连招", true));
                Vars.QMenu.Add(new MenuSliderButton("harass", "骚扰 / 自身蓝量 >= x%", 25, 0, 99, true));
                Vars.QMenu.Add(new MenuSliderButton("clear", "清线 / 自身蓝量 >= x%", 25, 0, 99, true));
                Vars.QMenu.Add(new MenuSliderButton("lasthit", "补刀 / 自身蓝量 >= x%", 0, 0, 99, true));
                Vars.QMenu.Add(new MenuBool("killsteal", "击杀", true));
            }
            Vars.Menu.Add(Vars.QMenu);

            Vars.WMenu = new Menu("w", "W技能设置");
            {
                Vars.WMenu.Add(new MenuBool("logical", "智能", true));
                Vars.WMenu.Add(new MenuSliderButton("harass", "骚扰 / 自身蓝量 >= x%", 50, 0, 99, true));
                Vars.WMenu.Add(new MenuSliderButton("laneclear", "清线 / 自身蓝量 >= x%", 50, 0, 99, true));
                Vars.WMenu.Add(new MenuSlider("minionshit", "清线 / 最低命中数 >= ", 3, 1, 5));
                Vars.WMenu.Add(new MenuSliderButton("jungleclear", "清野 / 自身蓝量 >= x%", 30, 0, 99, true));
                Vars.WMenu.Add(new MenuBool("killsteal", "击杀", true));
            }
            Vars.Menu.Add(Vars.WMenu);

            Vars.EMenu = new Menu("e", "E技能设置");
            {
                Vars.EMenu.Add(new MenuBool("combo", "连招", true));
                Vars.EMenu.Add(new MenuBool("gapcloser", "反突进", true));
                Vars.EMenu.Add(new MenuBool("interrupter", "打断技能", true));
                Vars.EMenu.Add(new MenuSliderButton("enemies", "自动 / 最小命中敌人数 >= ", 2, 2, 6, true));
            }
            Vars.Menu.Add(Vars.EMenu);

            Vars.RMenu = new Menu("r", "R技能设置");
            {
                Vars.RMenu.Add(new MenuBool("killsteal", "R击杀", true));
            }
            Vars.Menu.Add(Vars.RMenu);

            Vars.MiscMenu = new Menu("miscellaneous", "杂项设置");
            {
                Vars.MiscMenu.Add(new MenuBool("noaacombo", "连招时禁止普攻"));
                Vars.MiscMenu.Add(new MenuBool("qfarmmode", "在骚扰清线模式仅用Q补刀"));
                Vars.MiscMenu.Add(new MenuSliderButton("tear", "堆眼泪 / 自身蓝量 >= x%", 80, 0, 95, true));
                Vars.MiscMenu.Add(new MenuBool("support", "辅助模式"));
            }
            Vars.Menu.Add(Vars.MiscMenu);

            new AutoWard(Vars.Menu);

            var SkinMenu = Vars.Menu.Add(new Menu("SkinChance", "换肤设置"));
            {
                SkinMenu.Add(new MenuBool("Enable", "启动", false));
                SkinMenu.Add(new MenuList<string>("SkinName", "选择皮肤", new[] { "经典皮肤", "白魔法师", "冰壶选手", "灰胡子魔法师", "绿野仙踪", "魔导绅士", "穿着正装的恶魔", "邪恶圣诞老人", "最终BOSS" }));
            }

            Vars.DrawingsMenu = new Menu("drawings", "范围显示");
            {
                Vars.DrawingsMenu.Add(new MenuBool("q", "Q 范围", true));
                Vars.DrawingsMenu.Add(new MenuBool("w", "W 范围"));
                Vars.DrawingsMenu.Add(new MenuBool("e", "E 范围"));
                Vars.DrawingsMenu.Add(new MenuBool("r", "R 范围"));
            }
            Vars.Menu.Add(Vars.DrawingsMenu);
        }

        #endregion
    }
}