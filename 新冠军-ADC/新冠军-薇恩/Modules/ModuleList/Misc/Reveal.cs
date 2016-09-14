﻿using LeagueSharp;
using LeagueSharp.Common;
using VayneHunter_Reborn.Modules.ModuleHelpers;
using VayneHunter_Reborn.Utility.Helpers;
using VayneHunter_Reborn.Utility.MenuUtility;

namespace VayneHunter_Reborn.Modules.ModuleList.Misc
{
    class Reveal : IModule
    {
        private static readonly Items.Item Trinket = new Items.Item(3364, 600f);
        private static readonly Items.Item Ward = new Items.Item(2043, 600f);

        public void OnLoad()
        {
            StealthHelper.OnStealth += StealthHelper_OnStealth;
        }

        void StealthHelper_OnStealth(StealthHelper.OnStealthEventArgs obj)
        {
            //Using First the Trinket then the vision ward.

            if (MenuExtensions.GetItemValue<bool>("dz191.vhr.misc.general.reveal"))
            {
                if (obj.IsStealthed
                    && obj.Sender.IsValidTarget(600f))
                {
                    var objectPosition = obj.Sender.ServerPosition;
                    if (Trinket.IsOwned() && Trinket.IsReady())
                    {
                        var extend = ObjectManager.Player.ServerPosition.Extend(objectPosition, 400f);
                        Trinket.Cast(extend);
                        return;
                    }

                    if (Ward.IsOwned() && Ward.IsReady())
                    {
                        var extend = ObjectManager.Player.ServerPosition.Extend(objectPosition, 400f);
                        Ward.Cast(extend);
                    }
                }
            }
        }

        public bool ShouldGetExecuted()
        {
            return false;
        }

        public ModuleType GetModuleType()
        {
            return ModuleType.OnUpdate;
        }

        public void OnExecute(){ }
    }
}
