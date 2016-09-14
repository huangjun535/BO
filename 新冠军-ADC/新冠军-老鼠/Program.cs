using LeagueSharp;
using LeagueSharp.Common;
using System;
using YuLeLibrary;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using System.Reflection;
namespace YuLeTwitch
{
    internal class Program
    {
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;
        public static int tickNum = 4, tickIndex = 0;
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        private static void GameOnOnGameLoad()
        {
            if (ObjectManager.Player.ChampionName != "Twitch")
            {
                return;
            }

            Config = new Menu("新冠军老鼠Q群438230879", "YuLeTwitch", true).SetFontStyle(System.Drawing.FontStyle.Regular, SharpDX.Color.Pink);

            Config.AddSubMenu(new Menu("走砍设置", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
            new Twitch().Load();
            Config.AddToMainMenu();
            Game.OnUpdate += OnUpdate;
        }


        private static void OnUpdate(EventArgs args)
        {
            tickIndex++;

            if (tickIndex > 4)
                tickIndex = 0;
        }

        public static bool LagFree(int offset)
        {
            if (tickIndex == offset)
                return true;
            else
                return false;
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

        public static bool Farm { get { return Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed; } }

        public static bool Combo { get { return (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo);} }
    }
}