namespace YuLeVeigar.Utilities
{
    using LeagueSharp;
    using LeagueSharp.SDK;
    using LeagueSharp.SDK.UI;

    /// <summary>
    ///     The Mana manager class.
    /// </summary>
    internal class ManaManager
    {
        #region Public Methods and Operators

        /// <summary>
        ///     The minimum mana needed to cast the given Spell.
        /// </summary>
        public static int GetNeededHealth(SpellSlot slot, AMenuComponent value)
            =>
                value.GetValue<MenuSliderButton>().SValue
                + (int)(GameObjects.Player.Spellbook.GetSpell(slot).ManaCost / GameObjects.Player.MaxHealth * 100);

        /// <summary>
        ///     The minimum mana needed to cast the given Spell.
        /// </summary>
        public static int GetNeededMana(SpellSlot slot, AMenuComponent value)
            =>
                value.GetValue<MenuSliderButton>().SValue
                + (int)(GameObjects.Player.Spellbook.GetSpell(slot).ManaCost / GameObjects.Player.MaxMana * 100);

        #endregion
    }
}