using LeagueSharp;
using LeagueSharp.Common;
using TreeLib.Core;

namespace YuLeLuLu
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Lulu.GameOnOnGameLoad;
        }

        public static void GameOnOnGameLoad()
        {
            if (ObjectManager.Player.ChampionName == "Lulu")
            {
                Bootstrap.Initialize();
                var s = new Lulu();
            }
        }
    }
}