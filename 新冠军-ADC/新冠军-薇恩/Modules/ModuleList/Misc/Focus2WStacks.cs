﻿using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using VayneHunter_Reborn.Modules.ModuleHelpers;
using VayneHunter_Reborn.Utility;
using VayneHunter_Reborn.Utility.Helpers;
using VayneHunter_Reborn.Utility.MenuUtility;

namespace VayneHunter_Reborn.Modules.ModuleList.Misc
{
    class Focus2WStacks : IModule
    {
        public void OnLoad()
        {

        }

        public bool ShouldGetExecuted()
        {
            return MenuExtensions.GetItemValue<bool>("dz191.vhr.misc.general.specialfocus") && Variables.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo;
        }

        public ModuleType GetModuleType()
        {
            return ModuleType.OnUpdate;
        }

        public void OnExecute()
        {
            var target = HeroManager.Enemies.Find(en => en.IsValidTarget(ObjectManager.Player.AttackRange + 65f + 65f) && en.Has2WStacks());
            if (target != null)
            {
                TargetSelector.SetTarget(target);
                Variables.Orbwalker.ForceTarget(target);
            }

            if (Game.Time < 25 * 60 * 1000)
            {
                var ADC =
                    HeroManager.Enemies.Where(m => TargetSelector.GetPriority(m) > 4 && m.IsValidTarget()).OrderBy(m => m.TotalAttackDamage).FirstOrDefault();

                if (ADC != null && Orbwalking.InAutoAttackRange(ADC))
                {
                    TargetSelector.SetTarget(target);
                    Variables.Orbwalker.ForceTarget(target);
                }
                else
                {
                    TargetSelector.SetTarget(null);
                    Variables.Orbwalker.ForceTarget(Variables.Orbwalker.GetTarget() as Obj_AI_Base);
                }
            }
        }
    }
}
