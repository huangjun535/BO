﻿using System;
using Activator.Base;
using LeagueSharp;
using LeagueSharp.Common;

namespace Activator.Items.Offensives
{
    class _3077 : CoreItem
    {
        internal override int Id => 3077;
        internal override int Priority => 5;
        internal override string Name => "Tiamat";
        internal override string DisplayName => "Tiamat";
        internal override int Duration => 100;
        internal override float Range => 350f;    
        internal override MenuType[] Category => new[] { MenuType.SelfLowHP, MenuType.EnemyLowHP };
        internal override MapType[] Maps => new[] { MapType.Common };
        internal override int DefaultHP => 95;
        internal override int DefaultMP => 0;

        public _3077()
        {
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
        }

        private void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (Player.ChampionName == "Riven")
                return;

            if (!Menu.Item("use" + Name).GetValue<bool>() || !IsReady())
                return;

            var hero = target as Obj_AI_Hero;
            if (hero.IsValidTarget(Range))
            {
                if (!Parent.Item(Parent.Name + "useon" + hero.NetworkId).GetValue<bool>())
                    return;

                if (hero.Health / hero.MaxHealth * 100 <= Menu.Item("enemylowhp" + Name + "pct").GetValue<Slider>().Value)
                {
                    UseItem(Tar.Player, true);
                }

                if (Player.Health / Player.MaxHealth * 100 <= Menu.Item("selflowhp" + Name + "pct").GetValue<Slider>().Value)
                {
                    UseItem(Tar.Player, true);
                }
            }
        }

        public override void OnTick(EventArgs args)
        {

        }
    }
}
