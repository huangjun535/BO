﻿using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using VayneHunter_Reborn.Modules.ModuleHelpers;
using VayneHunter_Reborn.Skills.Condemn;
using VayneHunter_Reborn.Utility;
using VayneHunter_Reborn.Utility.MenuUtility;

namespace VayneHunter_Reborn.Modules.ModuleList.Condemn
{
    class CondemnJungleMobs : IModule
    {
        private static readonly string[] MobNames =
            {
                "SRU_Red", "SRU_Blue", "SRU_Gromp", "SRU_Murkwolf",
                "SRU_Razorbeak", "SRU_Krug", "Sru_Crab",
                "TT_Spiderboss", "TTNGolem", "TTNWolf",
                "TTNWraith"
            };

        public void OnLoad()
        {

        }

        public bool ShouldGetExecuted()
        {
            return MenuExtensions.GetItemValue<bool>("dz191.vhr.farm.condemnjungle") &&
                   Variables.spells[SpellSlot.E].IsReady() && (Variables.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear);
        }

        public ModuleType GetModuleType()
        {
            return ModuleType.OnAfterAA;
        }

        public void OnExecute()
        {
            var owTarget = Variables.Orbwalker.GetTarget();
            if (owTarget is Obj_AI_Base && MobNames.Contains((owTarget as Obj_AI_Base).CharData.BaseSkinName))
            {
                var owTargetBase = (owTarget as Obj_AI_Base);
                if (owTargetBase.IsCondemnable(ObjectManager.Player.ServerPosition))
                {
                    Variables.spells[SpellSlot.E].CastOnUnit(owTargetBase);
                }
            }
        }
    }
}
