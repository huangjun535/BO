﻿using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using VayneHunter_Reborn.Modules.ModuleHelpers;
using VayneHunter_Reborn.Skills.Condemn;
using VayneHunter_Reborn.Utility;
using VayneHunter_Reborn.Utility.MenuUtility;

namespace VayneHunter_Reborn.Modules.ModuleList.Condemn
{
    class AutoKindred : IModule
    {
        public void OnLoad()
        {

        }

        public bool ShouldGetExecuted()
        {
            return false && MenuExtensions.GetItemValue<bool>("dz191.vhr.misc.condemn.repelkindred") &&
                   Variables.spells[SpellSlot.E].IsReady() && ObjectManager.Player.CountEnemiesInRange(1500f) == 1 
                   && ObjectManager.Player.GetAlliesInRange(1500f).Count(m => !m.IsMe) == 0;
        }

        public ModuleType GetModuleType()
        {
            return ModuleType.OnUpdate;
        }

        public void OnExecute()
        {
            var CondemnTarget =
                HeroManager.Enemies.FirstOrDefault(h => h.IsValidTarget(Variables.spells[SpellSlot.E].Range) && h.HasBuff("KindredRNoDeathBuff") &&
                        h.HealthPercent <= 10);
            if (CondemnTarget.IsValidTarget())
            {
                Variables.spells[SpellSlot.E].CastOnUnit(CondemnTarget);
            }
        }
    }
}
