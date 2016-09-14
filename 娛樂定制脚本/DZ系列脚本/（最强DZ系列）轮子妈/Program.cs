using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSivir
{
    class Program
    {
        static void Main(string[] args)
        {
            var sivir = new Sivir();
            CustomEvents.Game.OnGameLoad += sivir.OnLoad;
        }
    }
}
