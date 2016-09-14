namespace YuLeLeeSin_Rework
{
    using LeagueSharp;
    using LeagueSharp.SDK.UI;
    using System;
    using System.Collections.Generic;

    internal class SkinChance
    {
        private static Obj_AI_Hero Me => Program.Player;

        private static Menu Menu => Program.MainMenu;

        private static int SkinID;

        public static void Inject()
        {
            SkinID = Me.BaseSkinId;

            var SkinMenu = Menu.Add(new Menu("SkinChance", "换肤设置"));
            {
                SkinMenu.Add(new MenuBool("Enable", "启动", false));
                SkinMenu.Add(new MenuList<string>("SkinName", "选择皮肤", LeeSin));
            }

            Game.OnUpdate += OnUpdate;
        }

        private static void OnUpdate(EventArgs Args)
        {
            if (Me.IsDead)
            {
                return;
            }

            if (Menu["SkinChance"]["Enable"])
            {
                Me.SetSkin(Me.ChampionName, Menu["SkinChance"]["SkinName"].GetValue<MenuList>().Index);
            }
            else if (!Menu["SkinChance"]["Enable"])
            {
                Me.SetSkin(Me.ChampionName, SkinID);
            }
        }

        private static IEnumerable<string> LeeSin = new[]
        {
            "经典", "传统僧侣", "侍僧", "龙的传人", "至高之拳", "泳池派对", "SKT T1", "拳击手"
        };
    }
}
