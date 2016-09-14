using LeagueSharp;
using LeagueSharp.Data.Enumerations;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Enumerations;
using LeagueSharp.SDK.UI;
using System;
using System.Linq;
using ComboSet = YuLeAshe.Config.Modes.Combo;
using FleeSet = YuLeAshe.Config.Modes.Flee;
using HarassSet = YuLeAshe.Config.Modes.Harass;

namespace YuLeAshe.Modes
{
    internal abstract class Modes
    {
        protected static Spell Q => SpellManager.Q;

        protected static Spell W => SpellManager.W;

        protected static Spell E => SpellManager.E;

        protected static Spell R => SpellManager.R;

        internal abstract bool ShouldBeExecuted();

        internal abstract void Execute();
    }

    internal sealed class Combo : Modes
    {
        internal override bool ShouldBeExecuted()
        {
            return Variables.Orbwalker.ActiveMode == OrbwalkingMode.Combo;
        }

        internal override void Execute()
        {
            if (!Variables.Orbwalker.CanMove)
            {
                return;
            }

            if (ComboSet.UseW && W.IsReady())
            {
                var target = Variables.TargetSelector.GetTargetNoCollision(W);
                if (target != null)
                {
                    W.Cast(target);
                }
            }

            if (ComboSet.UseR && R.IsReady())
            {
                foreach (var hero in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget()))
                {
                    var theDistance = hero.Distance(GameObjects.Player);

                    if (theDistance <= 500)
                    {
                        if (R.Cast(hero).HasFlag(CastStates.SuccessfullyCasted))
                        {
                            break;
                        }
                    }

                    if (theDistance <= 1000 && IsKillableWithR(hero, false, (float)GameObjects.Player.GetAutoAttackDamage(hero) * 2))
                    {
                        if (R.Cast(hero).HasFlag(CastStates.SuccessfullyCasted))
                        {
                            break;
                        }
                    }

                    if (theDistance <= 2500 && IsImmobileUntil(hero) > theDistance / R.Speed)
                    {
                        if (R.Cast(hero).HasFlag(CastStates.SuccessfullyCasted))
                        {
                            break;
                        }
                    }
                }
            }
        }

        internal static bool IsKillableWithR(Obj_AI_Base target, bool rangeCheck = false, float extraDamage = 0f)
        {
            return target.IsValidTarget(rangeCheck ? SpellManager.R.Range : float.MaxValue) && target.Health + target.MagicalShield < GetRDamage(target) + extraDamage;
        }

        internal static double GetRDamage(Obj_AI_Base target)
        {
            return GameObjects.Player.GetSpellDamage(target, SpellSlot.R, DamageStage.Default);
        }

        internal static double IsImmobileUntil(Obj_AI_Hero unit)
        {
            var result = unit.Buffs.Where(buff => buff.IsActive && Game.Time <= buff.EndTime && (buff.Type == BuffType.Charm || buff.Type == BuffType.Knockup || buff.Type == BuffType.Stun || buff.Type == BuffType.Suppression || buff.Type == BuffType.Snare || buff.Type == BuffType.Taunt)).Aggregate(0d, (current, buff) => Math.Max(current, buff.EndTime));
            return result - Game.Time;
        }
    }

    internal sealed class Harass : Modes
    {
        internal override bool ShouldBeExecuted()
        {
            return Variables.Orbwalker.ActiveMode == OrbwalkingMode.Hybrid;
        }

        internal override void Execute()
        {
            if (!Variables.Orbwalker.CanMove)
            {
                return;
            }

            if (HarassSet.UseW && W.IsReady() && GameObjects.Player.ManaPercent > HarassSet.MinMana)
            {
                var target = Variables.TargetSelector.GetTargetNoCollision(W);
                if (target != null)
                {
                    W.Cast(target);
                }
            }
        }
    }

    internal sealed class LaneClear : Modes
    {
        internal override bool ShouldBeExecuted()
        {
            return Variables.Orbwalker.ActiveMode == OrbwalkingMode.LaneClear;
        }

        internal override void Execute()
        {
            if (!Variables.Orbwalker.CanMove)
            {
                return;
            }

        }
    }

    internal sealed class JungleClear : Modes
    {
        internal override bool ShouldBeExecuted()
        {
            return Variables.Orbwalker.ActiveMode == OrbwalkingMode.LaneClear;
        }

        internal override void Execute()
        {
            if (!Variables.Orbwalker.CanMove)
            {
                return;
            }
        }
    }

    internal sealed class Flee : Modes
    {
        internal override bool ShouldBeExecuted()
        {
            return FleeSet._fleeKey.GetValue<MenuKeyBind>().Active;
        }

        internal override void Execute()
        {
            Variables.Orbwalker.Move(Game.CursorPos);

            if (FleeSet.UseW && W.IsReady())
            {
                W.Cast(GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(W.Range)).OrderBy(x => x.DistanceToPlayer()).FirstOrDefault());
            }
        }
    }

    internal sealed class PermaActive : Modes
    {
        internal override bool ShouldBeExecuted()
        {
            return true;
        }

        internal override void Execute() { }
    }
}
