using LeagueSharp;
using LeagueSharp.Common;
using System;
using YuLeViktor.Library.Logger;

namespace YuLeViktor.Abstracts
{
    internal class SkinManager
    {
        private static Menu _menu;

        public static void AddToMenu(Menu menu)
        {
            try
            {
                _menu = menu;
                menu.AddItem(new MenuItem("EnableSkin", "启动换肤").SetValue(false));
                menu.AddItem(new MenuItem("SkinSelect", "选择皮肤").SetValue(new StringList(new[] { "经典", "初号机", "全金属狂潮", "创世者" })));


                Game.OnUpdate += Game_OnUpdate;
            }
            catch (Exception ex)
            {
                Global.Logger.AddItem(new LogItem(ex));
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (_menu.Item("EnableSkin").GetValue<bool>())
            {
                ObjectManager.Player.SetSkin(ObjectManager.Player.CharData.BaseSkinName, _menu.Item("SkinSelect").GetValue<StringList>().SelectedIndex);
            }
            else if (!_menu.Item("EnableSkin").GetValue<bool>())
            {
                ObjectManager.Player.SetSkin(ObjectManager.Player.CharData.BaseSkinName, 0);
            }
        }
    }
}