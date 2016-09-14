﻿using LeagueSharp;
using LeagueSharp.Common;

namespace TreeLib.Core.Extensions
{
    public static class UnitExtensions
    {
        public static bool CanAAKill(this Obj_AI_Base unit)
        {
            return unit.IsValidTarget(Orbwalking.GetRealAutoAttackRange(ObjectManager.Player)) &&
                   ObjectManager.Player.GetAutoAttackDamage(unit) > unit.Health;
        }
    }
}