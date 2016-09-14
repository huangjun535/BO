using LeagueSharp;
using LeagueSharp.Common;

namespace iKalistaReborn
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += eventArgs =>
            {
                if (ObjectManager.Player.ChampionName != "Kalista")
                    return;
				Game.PrintChat("<font color='#990033'><b>DZ鏈€寮烘粦鏉块瀷</b></font><font color='#CCFF66'><b>-鍔犺浇鎴愬姛</b></font><font color='#FF9900'><b>-濞涙▊VIP鑴氭湰缇わ細&#50;&#49;&#53;&#50;&#50;6086</b></font>");

                new Kalista();
            };
        }
    }
}