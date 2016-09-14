
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
        public static void Combo(EventArgs args)
        {
            if (Bools.HasSheenBuff() || !Targets.Target.IsValidTarget()
                || Invulnerable.Check(Targets.Target, DamageType.Magical))
            {
                return;
            }

            /// <summary>
            ///     The E Combo Logic.
            /// </summary>
            if (Vars.E.IsReady() && GameObjects.Player.ManaPercent > 25 && Targets.Target.IsValidTarget(Vars.E.Range)
                && Vars.Menu["e"]["combo"].GetValue<MenuBool>().Value)
            {
                Vars.E.Cast(
                    Vars.E.GetPrediction(Targets.Target)
                        .CastPosition.Extend(GameObjects.Player.ServerPosition, -Vars.E.Width / 2));
            }

            /// <summary>
            ///     The Q Combo Logic.
            /// </summary>
            if (Vars.Q.IsReady() && Targets.Target.IsValidTarget(Vars.Q.Range)
                && Vars.Menu["q"]["combo"].GetValue<MenuBool>().Value)
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
        }

        #endregion
    }
}