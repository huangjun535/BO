using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace BadaoKingdom
{
    static class Program
    {
        public static readonly List<string> SupportedChampion = new List<string>()
        {
            "MissFortune"
        };
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (!SupportedChampion.Contains(ObjectManager.Player.ChampionName))
            {
                return;
            }
            Game.PrintChat("<font color='#990033'><b>璧忛噾鐚庝汉</b></font><font color='#CCFF66'><b>-鍔犺浇鎴愬姛</b></font><font color='#FF9900'><b>-</b></font>");
            BadaoChampion.BadaoMissFortune.BadaoMissFortune.BadaoActivate();
        }
    }
}
