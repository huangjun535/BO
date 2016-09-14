using System;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Data.Enumerations;
using LeagueSharp.SDK;

namespace Ashe
{
    internal static class Extensions
    {
        internal static double GetRDamage(this Obj_AI_Base target)
        {
            return GameObjects.Player.GetSpellDamage(target, SpellSlot.R, DamageStage.Default);
        }

        internal static bool IsKillableWithR(this Obj_AI_Base target, bool rangeCheck = false, float extraDamage = 0f)
        {
            return target.IsValidTarget(rangeCheck ? SpellManager.R.Range : float.MaxValue) && target.Health + target.MagicalShield < target.GetRDamage() + extraDamage;
        }

        internal static double IsImmobileUntil(this Obj_AI_Hero unit)
        {
            var result =
                unit.Buffs.Where(
                    buff =>
                        buff.IsActive && Game.Time <= buff.EndTime &&
                        (buff.Type == BuffType.Charm || buff.Type == BuffType.Knockup || buff.Type == BuffType.Stun ||
                         buff.Type == BuffType.Suppression || buff.Type == BuffType.Snare || buff.Type == BuffType.Taunt))
                    .Aggregate(0d, (current, buff) => Math.Max(current, buff.EndTime));
            return result - Game.Time;
        }
    }
}
