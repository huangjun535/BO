using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using VayneHunter_Reborn.Modules.ModuleHelpers;
using VayneHunter_Reborn.Utility;
using VayneHunter_Reborn.Utility.MenuUtility;

namespace VayneHunter_Reborn.Modules.ModuleList.Tumble
{
    class NoAAStealth : IModule
    {
        public void OnLoad()
        {
            Orbwalking.BeforeAttack += OW;
        }

        private void OW(Orbwalking.BeforeAttackEventArgs args)
        {
            if (ShouldGetExecuted() && ObjectManager.Player.Buffs.Any(m => m.Name.ToLower() == "vaynetumblefade"))
            {
                if (ObjectManager.Player.CountEnemiesInRange(1100f) <= 1 
                    || ObjectManager.Player.CountEnemiesInRange(1100f) < Variables.Menu.Item("dz191.vhr.misc.tumble.noaa.enemies").GetValue<Slider>().Value)
                {
                    return;
                }


                args.Process = false;
            }
        }

        public bool ShouldGetExecuted()
        {
            return Variables.Menu.Item("dz191.vhr.misc.tumble.noaastealthex") != null 
                && Variables.Menu.Item("dz191.vhr.misc.tumble.noaastealthex").GetValue<KeyBind>().Active;
        }

        public ModuleType GetModuleType()
        {
            return ModuleType.Other;
        }

        public void OnExecute()
        {
        }
    }
}
