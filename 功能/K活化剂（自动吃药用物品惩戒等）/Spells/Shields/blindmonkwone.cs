﻿using System;
using Activator.Base;
using LeagueSharp;
using LeagueSharp.Common;

namespace Activator.Spells.Shields
{
    class blindmonkwone : CoreSpell
    {
        internal override string Name => "blindmonkwone";
        internal override string DisplayName => "Safeguard | W";
        internal override float Range => 700f;
        internal override MenuType[] Category => new[] { MenuType.SelfLowHP, MenuType.SelfMuchHP, MenuType.SelfMinMP };
        internal override int DefaultHP => 95;
        internal override int DefaultMP => 45;

        public override void OnTick(EventArgs args)
        {
            if (!Menu.Item("use" + Name).GetValue<bool>() || !IsReady())
                return;

            if (Player.Mana / Player.MaxMana * 100 <
                Menu.Item("selfminmp" + Name + "pct").GetValue<Slider>().Value)
                return;

            foreach (var hero in Activator.Allies())
            {                    
                if (!Parent.Item(Parent.Name + "useon" + hero.Player.NetworkId).GetValue<bool>())
                    continue;

                if (Player.GetSpell(SpellSlot.W).Name.ToLower() != "blindmonkwone")
                    return;

                if (hero.Player.Distance(Player.ServerPosition) <= Range)
                {
                    if (hero.IncomeDamage / hero.Player.MaxHealth * 100 >=
                        Menu.Item("selfmuchhp" + Name + "pct").GetValue<Slider>().Value)
                            UseSpellOn(hero.Player);

                    if (hero.Player.Health / hero.Player.MaxHealth * 100 <=
                        Menu.Item("selflowhp" + Name + "pct").GetValue<Slider>().Value)
                    {
                        if (hero.IncomeDamage > 0 ||  hero.MinionDamage > hero.Player.Health)
                            UseSpellOn(hero.Player);
                    }
                }
            }
        }
    }
}
