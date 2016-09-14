using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace Sharpshooter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            SharpShooter.Load();
        }
    }
}
