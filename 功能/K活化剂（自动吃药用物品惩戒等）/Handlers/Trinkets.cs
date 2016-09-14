using System;
using System.Linq;
using Activator.Base;
using LeagueSharp;
using LeagueSharp.Common;

namespace Activator.Handlers
{
    internal class Trinkets
    {
        internal static int Limiter;
        internal static int TrinketId;
        internal static bool Upgrade;
        internal static Obj_AI_Hero Player;
        private static readonly int[] _stones = { 2049, 2045, 2301, 2302, 2303 };
        private static bool _hazstone => _stones.Any(x => Player != null && LeagueSharp.Common.Items.HasItem(x));

        public static void Init()
        {
            Player = ObjectManager.Player;

            Game.OnUpdate += Game_OnUpdate;
            Obj_AI_Base.OnLevelUp += Obj_AI_Base_OnLevelUp;
            Obj_AI_Base.OnPlaceItemInSlot += Obj_AI_Base_OnPlaceItemInSlot;
            Utility.DelayAction.Add(1000, FindTrinket);
        }

        private static void FindTrinket()
        {
            var item = Player.InventoryItems.FirstOrDefault(i => i.SpellSlot == SpellSlot.Trinket);
            if (item != null)
            {
                TrinketId = (int) item.Id;

                if ((TrinketId == 3340 || TrinketId == 3341) && Player.Level >= 9)
                {
                    Upgrade = true;
                }
            }
        }

        private static void Obj_AI_Base_OnPlaceItemInSlot(Obj_AI_Base sender, Obj_AI_BasePlaceItemInSlotEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            var itemid = (int) args.Id;

            switch (itemid)
            {
                case 3340:
                    TrinketId = 3340; // ward
                    break;
                case 3341:
                    TrinketId = 3341; // sweeper
                    break;
            }
        }

        private static void Obj_AI_Base_OnLevelUp(Obj_AI_Base sender, EventArgs args)
        {
            var hero = sender as Obj_AI_Hero;
            if (hero != null && hero.IsMe)
            {
                if (Player.Level == 9)
                {
                    Upgrade = true;
                }
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!Activator.Origin.Item("autotrinket").GetValue<bool>())
            {
                return;
            }

            if (Utils.GameTimeTickCount - Limiter < 1000 || !Upgrade)
            {
                return;         
            }

            if (MenuGUI.IsShopOpen)
            {
                if (Player.InShop() || Player.IsDead)
                {
                    if (TrinketId == 3340 && !LeagueSharp.Common.Items.HasItem(3363))
                    {
                        if (Helpers.GetRole(Player) == PrimaryRole.Marksman)
                            if (Player.BuyItem((ItemId) 3363))
                                Upgrade = false;

                        if (Helpers.GetRole(Player) == PrimaryRole.Mage)
                            if (Player.BuyItem((ItemId) 3363))
                                Upgrade = false;
                    }

                    if (TrinketId == 3340 && !LeagueSharp.Common.Items.HasItem(3364))
                    {
                        if (Helpers.GetRole(Player) == PrimaryRole.Assassin)
                            if (Player.BuyItem((ItemId) 3364))
                                Upgrade = false;

                        if (_hazstone)
                        {
                            if (Player.BuyItem((ItemId) 3364))
                                Upgrade = false;
                        }
                    }

                    if (TrinketId == 3341 && !LeagueSharp.Common.Items.HasItem(3364))
                    {
                        if (Player.BuyItem((ItemId) 3364))
                            Upgrade = false;
                    }
                }
            }

            Limiter = Utils.GameTimeTickCount;
        }
    }
}
