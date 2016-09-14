using System;
using LeagueSharp;
using LeagueSharp.Common;
using VayneHunter_Reborn.Utility;

namespace VayneHunter_Reborn
{
    class Program
    {
        private static string ChampionName = "Vayne";
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Variables.GameOnOnGameLoad;
        }

        public static void GameOnOnGameLoad()
        {
            if (ObjectManager.Player.ChampionName != ChampionName)
            {
                return;
            }
            VHRBootstrap.OnLoad();

        }
    }
}
