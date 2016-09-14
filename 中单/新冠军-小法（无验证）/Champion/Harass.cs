
#pragma warning disable 1587

namespace YuLeVeigar.Champions.Veigar
{
    using System;
    using System.Linq;

    using YuLeVeigar.Utilities;

    using LeagueSharp;
    using LeagueSharp.SDK;
    using LeagueSharp.SDK.UI;
    using LeagueSharp.SDK.Utils;

    /// <summary>
    ///     The logics class.
    /// </summary>
    internal partial class Logics
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Called when the game updates itself.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        public static void Harass(EventArgs args)
        {
            if (!Targets.Target.IsValidTarget() || Invulnerable.Check(Targets.Target, DamageType.Magical))
            {
                return;
            }

            /// <summary>
            ///     The Q Harass Logic.
            /// </summary>
            if (Vars.Q.IsReady() && Targets.Target.IsValidTarget(Vars.Q.Range)
                && GameObjects.Player.ManaPercent
                > ManaManager.GetNeededMana(Vars.Q.Slot, Vars.Menu["q"]["harass"])
                && Vars.Menu["q"]["harass"].GetValue<MenuSliderButton>().BValue)
            {
                if (!Vars.Q.GetPrediction(Targets.Target).CollisionObjects.Any())
                {
                    Vars.Q.Cast(Vars.Q.GetPrediction(Targets.Target).UnitPosition);
                }
                else if (Vars.Q.GetPrediction(Targets.Target).CollisionObjects.Count == 1
                         && Vars.Q.GetPrediction(Targets.Target).CollisionObjects[0].Health
                         < (float)
                           GameObjects.Player.GetSpellDamage(
                               Vars.Q.GetPrediction(Targets.Target).CollisionObjects[0],
                               SpellSlot.Q))
                {
                    Vars.Q.Cast(Vars.Q.GetPrediction(Targets.Target).UnitPosition);
                }
            }

            /// <summary>
            ///     The W Harass Logic.
            /// </summary>
            if (Vars.W.IsReady() && Targets.Target.IsValidTarget(Vars.E.Range)
                && GameObjects.Player.ManaPercent
                > ManaManager.GetNeededMana(Vars.W.Slot, Vars.Menu["w"]["harass"])
                && Vars.Menu["w"]["harass"].GetValue<MenuSliderButton>().BValue)
            {
                Vars.W.Cast(Vars.W.GetPrediction(Targets.Target).CastPosition);
            }
        }

        #endregion
    }
}