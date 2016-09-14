using System;
using Activator.Base;
using LeagueSharp;
using LeagueSharp.Common;

namespace Activator.Spells.Evaders
{
    class alphastrike : CoreSpell
    {
        internal override string Name => "alphastrike";
        internal override string DisplayName => "Alpha Strike | Q";
        internal override float Range => 600f;
        internal override MenuType[] Category => new[] { MenuType.SpellShield, MenuType.Zhonyas };

        public override void OnTick(EventArgs args)
        {
            if (!Menu.Item("use" + Name).GetValue<bool>() || !IsReady())
                return;

            foreach (var hero in Activator.Allies())
            {
                if (hero.Player.NetworkId == Player.NetworkId)
                {
                    if (hero.Attacker == null)
                        continue;

                    if (!Parent.Item(Parent.Name + "useon" + hero.Player.NetworkId).GetValue<bool>())
                        continue;

                    if (hero.Attacker.Distance(hero.Player.ServerPosition) <= Range)
                    {
                        if (Menu.Item("ss" + Name + "all").GetValue<bool>())
                            if (hero.IncomeDamage > 0 && hero.HitTypes.Contains(HitType.Spell))
                                CastOnBestTarget((Obj_AI_Hero) hero.Attacker);

                        if (Menu.Item("ss" + Name + "cc").GetValue<bool>())
                            if (hero.IncomeDamage > 0 && hero.HitTypes.Contains(HitType.CrowdControl))
                                CastOnBestTarget((Obj_AI_Hero) hero.Attacker);

                        if (Menu.Item("use" + Name + "norm").GetValue<bool>())
                            if (hero.IncomeDamage > 0 && hero.HitTypes.Contains(HitType.Danger))
                                CastOnBestTarget((Obj_AI_Hero) hero.Attacker);

                        if (Menu.Item("use" + Name + "ulti").GetValue<bool>())
                            if (hero.IncomeDamage > 0 && hero.HitTypes.Contains(HitType.Ultimate))
                                CastOnBestTarget((Obj_AI_Hero) hero.Attacker);
                    }
                }
            }
        }
    }
}
