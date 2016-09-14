namespace YuleKayle
{
    using LeagueSharp;
    using LeagueSharp.Common;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using YuLeLibrary;
    using System.Threading.Tasks;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Reflection;

    internal class Program
    {
        public static Menu Menu;
        public static Orbwalking.Orbwalker Orbwalker;
        public static int tickIndex = 0;
        public static Obj_SpawnPoint enemySpawn;
        public static List<Obj_AI_Hero> Enemies = new List<Obj_AI_Hero>() , Allies = new List<Obj_AI_Hero>();
        private static Obj_AI_Hero Player;
        private static string HeroName = "Kayle";
        private static YuLeLibrary.Library Lib;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        private static void GameOnOnGameLoad()
        {
            enemySpawn = ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(x => x.IsEnemy);

            if (ObjectManager.Player.ChampionName != HeroName)
                return;

            Player = ObjectManager.Player;

            Menu = new Menu(HeroName, HeroName, true);
            Menu.AddSubMenu(new Menu("走砍设置", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalking"));
            AddList();
            new Kayle().Load();
            Menu.AddToMainMenu();

            Game.OnUpdate += OnUpdate;
        }

        private static void AddList()
        {
            foreach (var hero in HeroManager.Enemies)
            {
                if (hero.IsEnemy && hero.Team != Player.Team)
                {
                    Enemies.Add(hero);
                }
            }

            foreach (var hero in HeroManager.Allies)
            {
                if (hero.IsAlly && hero.Team == Player.Team)
                    Allies.Add(hero);
            }
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

        public static bool LaneClear { get { return (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear); } }

        public static bool Combo { get { return (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo); } }
    }
}