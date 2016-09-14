﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using MAC_Reborn.Extras;

namespace MAC_Reborn.Handlers
{
    internal class PotionHandler : UtilityItem
    {
        private const string HpPotName = "RegenerationPotion";
        private const string MpPotName = "FlaskOfCrystalWater";
        private const int Hpid = 2003;
        private const int Mpid = 2004;
        private static Menu _menu;

        public override void Load(Menu config)
        {
            config.AddItem(new MenuItem("useHP", "红药").SetValue(true));
            config.AddItem(new MenuItem("useHPPercent", "Hp低于 %").SetValue(new Slider(35, 1)));
            config.AddItem(new MenuItem("sseperator", "       "));
            config.AddItem(new MenuItem("useMP", "蓝药").SetValue(true));
            config.AddItem(new MenuItem("useMPPercent", "Mp低于 %").SetValue(new Slider(35, 1)));

            _menu = config;

            Game.OnUpdate += GameOnOnGameUpdate;
        }

        private static void GameOnOnGameUpdate(EventArgs args)
        {
            var useHp = _menu.Item("useHP").GetValue<bool>();
            var useMp = _menu.Item("useMP").GetValue<bool>();
            if (ObjectManager.Player.IsRecalling() || ObjectManager.Player.InFountain() || ObjectManager.Player.InShop())
            {
                return;
            }

            if (useHp && ObjectManager.Player.HealthPercent <= _menu.Item("useHPPercent").GetValue<Slider>().Value &&
                !HasHealthPotBuff())
            {
                if (Items.CanUseItem(Hpid) && Items.HasItem(Hpid))
                {
                    Items.UseItem(Hpid);
                }
            }

            if (!useMp ||
                !(ObjectManager.Player.ManaPercent <= _menu.Item("useMPPercent").GetValue<Slider>().Value) ||
                HasMannaPutBuff()) return;

            if (Items.CanUseItem(Mpid) && Items.HasItem(Mpid))
            {
                Items.UseItem(Mpid);
            }
        }

        private static bool HasHealthPotBuff()
        {
            return ObjectManager.Player.HasBuff(HpPotName, true);
        }

        private static bool HasMannaPutBuff()
        {
            return ObjectManager.Player.HasBuff(MpPotName, true);
        }
    }
}
