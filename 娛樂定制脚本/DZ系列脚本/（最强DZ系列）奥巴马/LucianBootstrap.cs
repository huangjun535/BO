using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;

namespace iLucian
{
    class LucianBootstrap
    {

        internal static void OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "Lucian")
            {
                return;
            }
            var lucian = new Lucian();
            lucian.OnLoad();
        }

    }
}
