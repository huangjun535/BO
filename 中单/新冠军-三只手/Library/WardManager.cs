using LeagueSharp;
using LeagueSharp.Common;
using System;
using YuLeLibrary;

namespace YuLeViktor.Abstracts
{
    internal class WardManager
    {
        private static Menu _menu;

        internal static void AddToMenu(Menu menu)
        {
            _menu = menu;

            menu.AddItem(new MenuItem("AutoWard", "启动", true).SetValue(true));
            menu.AddItem(new MenuItem("AutoBuy", "lv9自动买灯泡", true).SetValue(false));
            menu.AddItem(new MenuItem("AutoPink", "自动真眼扫描", true).SetValue(true));
            menu.AddItem(new MenuItem("AutoWardCombo", "仅连招模式启动 ", true).SetValue(true));
            menu.AddItem(new MenuItem("AutoWardKey", "连招按键", true).SetValue(new KeyBind(32, KeyBindType.Press)));
            new AutoWard().Load();
            new Tracker().Load();


            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            AutoWard.Enable = _menu.GetBool("AutoWard");
            AutoWard.AutoBuy = _menu.GetBool("AutoBuy");
            AutoWard.AutoPink = _menu.GetBool("AutoPink");
            AutoWard.OnlyCombo = _menu.GetBool("AutoWardCombo");
            AutoWard.InComboMode = _menu.GetKey("AutoWardKey");;
        }
    }
}