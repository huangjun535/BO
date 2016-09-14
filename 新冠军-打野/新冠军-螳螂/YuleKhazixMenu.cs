using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace SephKhazix
{
    class KhazixMenu
    {
        public static Menu menu;
        internal Orbwalking.Orbwalker Orbwalker;

        public KhazixMenu()
        {
            menu = new Menu("Khazix", "Khazix", true);

            var ow = menu.AddSubMenu("走砍");
            Orbwalker = new Orbwalking.Orbwalker(ow);

            var Qset = menu.AddSubMenu("Q设置");
            Qset.AddItems("    连招Q设置");
            Qset.AddBool("UseQCombo", "使用 Q");
            Qset.AddBool("UseEGapclose", "连招E前使用Q");
            Qset.AddItems("    骚扰Q设置");
            Qset.AddBool("UseQHarass", "使用 Q");
            Qset.AddItems("    清线清野Q设置");
            Qset.AddBool("UseQFarm", "使用 Q");
            Qset.AddItems("    击杀Q设置");
            Qset.AddBool("UseQKs", "使用 Q");

            var Wset = menu.AddSubMenu("W设置");
            Wset.AddItems("    连招W设置");
            Wset.AddBool("UseWCombo", "使用 W");
            Wset.AddBool("UseEGapcloseW", "连招E前使用W");
            Wset.AddItems("    骚扰W设置");
            Wset.AddBool("UseWHarass", "使用 W");
            Wset.AddItems("    清线清野W设置");
            Wset.AddBool("UseWFarm", "使用 W");
            Wset.AddSlider("Farm.WHealth", "使用W|自己最低生命百分比", 80, 0, 100);
            Wset.AddItems("    击杀W设置");
            Wset.AddBool("UseWKs", "使用 W");
            Wset.AddItems("    自动W设置");
            Wset.AddBool("Harass.AutoWI", "自动W 无法移动目标");
            Wset.AddBool("Harass.AutoWD", "自动W 突进");
            Wset.AddKeyBind("Harass.Key", "自动W 按键", "H".ToCharArray()[0], KeyBindType.Toggle).Permashow();

            var Eset = menu.AddSubMenu("E设置");
            Eset.AddItems("    连招E设置");
            Eset.AddBool("UseECombo", "使用 E");
            Eset.AddItems("    清线清野E设置");
            Eset.AddBool("UseEFarm", "使用 E");
            Eset.AddItems("    击杀E设置");
            Eset.AddBool("UseEKs", "使用 E");
            Eset.AddSlider("Edelay", "E 延迟 (ms)", 0, 0, 300);
            Eset.AddItems("    二段E设置");
            Eset.AddBool("djumpenabled", "启动");
            Eset.AddSlider("JEDelay", "二段跳中间延迟", 250, 250, 500);
            Eset.AddSList("jumpmode", "跳跃模式", new[] { "默认模式", "下面设置" }, 0);
            Eset.AddBool("noauto", "等待Q的CD");
            Eset.AddBool("jcursor", "第一段跳跃为鼠标位置");
            Eset.AddBool("jcursor2", "第而段跳跃为鼠标位置");
            Eset.AddItems("上面两个关闭的话则为默认模式");

            //Combo
            var Rset = menu.AddSubMenu("R设置");
            Rset.AddItems("    连招R设置");
            Rset.AddBool("UseRCombo", "使用 R");
            Rset.AddBool("UseRGapcloseW", "使用R(目标比较远)");
            Rset.AddBool("Safety.noaainult", "大招开启时禁止普攻", false);

            var Misc = menu.AddSubMenu("杂项");
            Misc.AddItems("    清线清野");
            Misc.AddBool("ClearEnable", "清线技能开关(鼠标滑轮控制)").SetValue(true).Permashow();
            Misc.AddItems("    物品使用");
            Misc.AddBool("UseItems", "连招中使用");
            Misc.AddBool("UseItemsFarm", "清线清野中使用").SetValue(true);
            Misc.AddBool("UseTiamatKs", "击杀中使用");
            Misc.AddItems("    击杀设置");
            Misc.AddBool("Kson", "启动击杀");
            Misc.AddBool("Ksbypass", "使用E击杀前进行安全检测", false);
            Misc.AddBool("UseEQKs", "自动EQ击杀");
            Misc.AddBool("UseEWKs", "自动EW击杀");
            Misc.AddItems("    召唤师技能");
            Misc.AddBool("UseIgnite", "自动点燃");
            Misc.AddItems("    安全检测");
            Misc.AddBool("Safety.Enabled", "启动");
            Misc.AddBool("Safety.CountCheck", "检查周围附近友军敌人情况");
            Misc.AddItem(new MenuItem("Safety.Ratio", "附近最少友军数").SetValue(new Slider(1, 0, 5)));
            Misc.AddBool("Safety.TowerJump", "禁止塔底跳跃");
            Misc.AddSlider("Safety.MinHealth", "跳跃最低血量比例", 15, 0, 100);

            var draw = menu.AddSubMenu("显示");
            draw.AddBool("Drawings.Disable", "关闭所有显示", true);
            draw.AddCircle("DrawQ", "显示 Q", 0, System.Drawing.Color.White);
            draw.AddCircle("DrawW", "显示 W", 0, System.Drawing.Color.Red);
            draw.AddCircle("DrawE", "显示 E", 0, System.Drawing.Color.Green);
            draw.AddBool("jumpdrawings", "显示二段跳范围");
            draw.AddBool("Debugon", "显示孤立目标");

            menu.AddToMainMenu();
        }

        internal bool GetBool(string name)
        {
            return menu.Item(name).GetValue<bool>();
        }

        internal bool GetKeyBind(string name)
        {
            return menu.Item(name).GetValue<KeyBind>().Active;
        }

        internal float GetSliderFloat(string name)
        {
            return menu.Item(name).GetValue<Slider>().Value;
        }

        internal int GetSlider(string name)
        {
            return menu.Item(name).GetValue<Slider>().Value;
        }

        internal int GetSL(string name)
        {
            return menu.Item(name).GetValue<StringList>().SelectedIndex;
        }

        internal string GetSLVal(string name)
        {
            return menu.Item(name).GetValue<StringList>().SelectedValue;
        }

        internal Circle GetCircle(string name)
        {
            return menu.Item(name).GetValue<Circle>();
        }
    }
}
