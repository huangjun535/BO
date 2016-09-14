using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace DZLib.Hero
{
    static class HeroExtensions
    {
        public static List<Obj_AI_Hero> GetLhEnemiesNear(this Vector3 position, float range, float healthpercent)
        {
            return HeroManager.Enemies.Where(hero => hero.IsValidTarget(range, true, position) && hero.HealthPercent <= healthpercent).ToList();
        }
    }
}
