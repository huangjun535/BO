using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using LeagueSharp.Common.Data;
using ItemData = LeagueSharp.Common.Data.ItemData;
using YuLeFiora.Evade;

namespace YuLeFiora
{
    class EvadeSkillShots
    {
        #region Evade
        public static void Evading()
        {
            var parry = Evade.EvadeSpellDatabase.Spells.FirstOrDefault(i => i.Enable && i.IsReady && i.Slot == SpellSlot.W);
            if (parry == null)
            {
                return;
            }
            var skillshot =
                Evade.Evade.SkillshotAboutToHit(Program.Player, 0 + Game.Ping + Program.Menu.SubMenu("Evade").SubMenu("Spells").SubMenu(parry.Name).Item("WDelay").GetValue<Slider>().Value)
                    .Where(
                        i =>
                        parry.DangerLevel <= i.DangerLevel)
                    .MaxOrDefault(i => i.DangerLevel);
            if (skillshot != null)
            {
                var target = Program.GetTarget(Program.W.Range);
                if (target.IsValidTarget(Program.W.Range))
                    Program.Player.Spellbook.CastSpell(parry.Slot, target.Position);
                else
                {
                    var hero = HeroManager.Enemies.FirstOrDefault(x => x.IsValidTarget(Program.W.Range));
                    if (hero != null)
                        Program.Player.Spellbook.CastSpell(parry.Slot, hero.Position);
                    else
                        Program.Player.Spellbook.CastSpell(parry.Slot, Program.Player.ServerPosition.Extend(skillshot.Start.To3D(), 100));
                }
            }
        }
        #endregion Evade

    }
}
