namespace Irelia
{
    using System;
    using LeagueSharp;
    using LeagueSharp.Common;
    using YuLeLibrary;

    class IreliaMenu
    {
        public static Menu Menu;

        public static void Initialize()
        {
            Menu = new Menu("QQ群438230879", "QQ群438230879", true);

            var targetSelectorMenu = Menu.AddSubMenu(new Menu("目标选择", "Target Selector"));
            {
                Menu.AddItem(new MenuItem("force.target", "集中攻击选择的目标").SetValue(true));
                Menu.AddItem(new MenuItem("force.target.range", "搜索范围:").SetValue(new Slider(1500, 0, 2500)));
            }

            var orbwalkerMenu = Menu.AddSubMenu(new Menu("走砍设置", "Orbwalker"));
            {
                Program.Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
                orbwalkerMenu.AddItem(new MenuItem("flee", "逃跑").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
            }

            var QMenu = Menu.AddSubMenu(new Menu("Q设置", "QSETTING"));
            {
                QMenu.AddItem(new MenuItem("combo.qqqqqqq", "         连招选项"));
                QMenu.AddItem(new MenuItem("combo.q", "连招中使用").SetValue(true));
                QMenu.AddItem(new MenuItem("combo.q.minrange", "最近Q突进敌人范围").SetValue(new Slider(450, 0, 650)));
                QMenu.AddItem(new MenuItem("combo.q.undertower", "假如敌人血量低于<= % 允许塔底Q他").SetValue(new Slider(40)));
                QMenu.AddItem(new MenuItem("combo.q.lastsecond", "当W的buff准备消失时自动Q").SetValue(true));
                QMenu.AddItem(new MenuItem("combo.q.gc", "自动Q残血兵突进敌人").SetValue(true));
                QMenu.AddItem(new MenuItem("harass.qqqqqqq", "         骚扰选项"));
                QMenu.AddItem(new MenuItem("harass.q", "骚扰中使用").SetValue(true));
                QMenu.AddItem(new MenuItem("harass.q.minrange", "最近Q突进敌人范围").SetValue(new Slider(450, 0, 650)));
                QMenu.AddItem(new MenuItem("harass.q.undertower", "假如敌人血量低于<= % 允许塔底Q他").SetValue(new Slider(40)));
                QMenu.AddItem(new MenuItem("harass.q.lastsecond", "当W的buff准备消失时自动Q").SetValue(true));
                QMenu.AddItem(new MenuItem("harass.q.gc", "自动Q残血兵突进敌人").SetValue(true));
                QMenu.AddItem(new MenuItem("lane.qqqqqqq", "         清线选项"));
                QMenu.AddItem(new MenuItem("laneclear.q", "清线中使用").SetValue(true));
                QMenu.AddItem(new MenuItem("laneclear.qut", "禁止塔下Q").SetValue(true));
                QMenu.AddItem(new MenuItem("ks.qqqqqqq", "         击杀选项"));
                QMenu.AddItem(new MenuItem("misc.ks.q", "击杀中使用").SetValue(true));
                QMenu.AddItem(new MenuItem("flee.qqqqqqq", "         逃跑选项"));
                QMenu.AddItem(new MenuItem("flee.q", "逃跑中使用").SetValue(true));
            }

            var WMenu = Menu.AddSubMenu(new Menu("W设置", "WSETTING"));
            {
                WMenu.AddItem(new MenuItem("combo.wwwwwww", "         连招选项"));
                WMenu.AddItem(new MenuItem("combo.w", "连招中使用").SetValue(true));
                WMenu.AddItem(new MenuItem("harass.wwwwwww", "         骚扰选项"));
                WMenu.AddItem(new MenuItem("harass.w", "骚扰中使用").SetValue(true));
            }



            var EMenu = Menu.AddSubMenu(new Menu("E设置", "ESETTING"));
            {
                EMenu.AddItem(new MenuItem("combo.eeeeeee", "         连招选项"));
                EMenu.AddItem(new MenuItem("combo.e", "连招中使用").SetValue(true));
                EMenu.AddItem(new MenuItem("combo.e.logic", "使用娱乐智能逻辑").SetValue(true));
                EMenu.AddItem(new MenuItem("harass.eeeeeee", "         骚扰选项"));
                EMenu.AddItem(new MenuItem("harass.e", "骚扰中使用").SetValue(true));
                EMenu.AddItem(new MenuItem("harass.e.logic", "使用娱乐智能逻辑").SetValue(true));
                EMenu.AddItem(new MenuItem("ks.eeeeeee", "         击杀选项"));
                EMenu.AddItem(new MenuItem("misc.ks.e", "击杀中使用").SetValue(true));
                EMenu.AddItem(new MenuItem("flee.eeeeeee", "         逃跑选项"));
                EMenu.AddItem(new MenuItem("flee.e", "逃跑中使用").SetValue(true));
                EMenu.AddItem(new MenuItem("misc.eeeeeee", "         杂项选项"));
                EMenu.AddItem(new MenuItem("misc.age", "突进中使用").SetValue(true));
                EMenu.AddItem(new MenuItem("misc.interrupt", "打断危险技能").SetValue(true));
                EMenu.AddItem(new MenuItem("misc.stunundertower", "塔下眩晕敌人转移仇恨").SetValue(true));
            }


            var RMenu = Menu.AddSubMenu(new Menu("R设置", "RSETTING"));
            {
                RMenu.AddItem(new MenuItem("combo.rrrrrrr", "         连招选项"));
                EMenu.AddItem(new MenuItem("combo.r", "连招中使用").SetValue(true));
                EMenu.AddItem(new MenuItem("combo.r.weave", "自动RBuff在身的目标").SetValue(true));
                EMenu.AddItem(new MenuItem("combo.r.selfactivated", "仅在自身血量过低时候").SetValue(false));
                EMenu.AddItem(new MenuItem("harass.rrrrrrr", "         骚扰选项"));
                RMenu.AddItem(new MenuItem("harass.r", "骚扰中使用").SetValue(true));
                RMenu.AddItem(new MenuItem("harass.r.weave", "自动RBuff在身的目标").SetValue(true));
                RMenu.AddItem(new MenuItem("lane.rrrrrrr", "         清线选项"));
                RMenu.AddItem(new MenuItem("laneclear.r", "清线选项").SetValue(true));
                RMenu.AddItem(new MenuItem("laneclear.r.minimum", "最少命中小兵数").SetValue(new Slider(2, 1, 6)));
                RMenu.AddItem(new MenuItem("ks.rrrrrrr", "         击杀选项"));
                RMenu.AddItem(new MenuItem("misc.ks.r", "击杀中使用").SetValue(true));
                RMenu.AddItem(new MenuItem("flee.rrrrrrr", "         逃跑选项"));
                RMenu.AddItem(new MenuItem("flee.r", "逃跑中使用").SetValue(true));
            }

            var ManaMenu = Menu.AddSubMenu(new Menu("蓝量管理", "MANASETTING"));
            {
                ManaMenu.AddItem(new MenuItem("harass.ManaMenu", "         骚扰选项"));
                ManaMenu.AddItem(new MenuItem("harass.mana", "最低蓝量比例").SetValue(new Slider(40, 1)));
                ManaMenu.AddItem(new MenuItem("lane.ManaMenu", "         清线选项"));
                ManaMenu.AddItem(new MenuItem("laneclear.mana", "最低蓝量比例").SetValue(new Slider(40, 1)));
            }

            var WARDMenu = Menu.AddSubMenu(new Menu("自动插眼", "AUTOWARD"));
            {
                WARDMenu.AddItem(new MenuItem("AutoWard", "启动", true).SetValue(true));
                WARDMenu.AddItem(new MenuItem("AutoBuy", "lv9自动买灯泡", true).SetValue(false));
                WARDMenu.AddItem(new MenuItem("AutoPink", "自动真眼扫描", true).SetValue(true));
                WARDMenu.AddItem(new MenuItem("AutoWardCombo", "仅连招模式启动 ", true).SetValue(true));
                new AutoWard().Load();
                new Tracker().Load();
            }

            var MISCMENU = Menu.AddSubMenu(new Menu("杂项设置", "MISCMENGW"));
            {
                MISCMENU.AddItem(new MenuItem("combo.items", "智能使用物品").SetValue(true));
                MISCMENU.AddItem(new MenuItem("combo.ignite", "智能点燃击杀").SetValue(true));
                MISCMENU.AddItem(new MenuItem("ClearEnable", "清线技能开关(鼠标滑轮控制)", true).SetValue(true)).Permashow();
            }


            var drawingsMenu = Menu.AddSubMenu(new Menu("显示设置", "Drawings settings"));
            {
                drawingsMenu.AddItem(new MenuItem("drawings.q", "显示 Q").SetValue(true));
                drawingsMenu.AddItem(new MenuItem("drawings.e", "显示 E").SetValue(true));
                drawingsMenu.AddItem(new MenuItem("drawings.r", "显示 R").SetValue(true));
                var dmgAfterCombo = new MenuItem("dmgAfterCombo", "显示连招伤害").SetValue(true);
                Utility.HpBarDamageIndicator.DamageToUnit = Program.ComboDamage;
                Utility.HpBarDamageIndicator.Enabled = dmgAfterCombo.GetValue<bool>();
                drawingsMenu.AddItem(dmgAfterCombo);
            }

            Menu.AddToMainMenu();
        }
    }
}
