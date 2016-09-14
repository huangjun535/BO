﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using SharpDX;
using Color = System.Drawing.Color;
using ItemData = LeagueSharp.Common.Data.ItemData;
namespace Rengar
{
    public static class Auto
    {
        public static Obj_AI_Hero Player { get{ return ObjectManager.Player; } }
        public static void BadaoActivate()
        {
            Game.OnUpdate += Game_OnUpdate;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
        }

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (Variables.AutoEInterrupt.GetValue<bool>() && Player.Mana == 5 && Variables.E.IsReady())
            {
                if (sender.IsValidTarget(Variables.E.Range))
                {
                    Helper.CastE(sender);
                }
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Player.Health * 100 / Player.MaxHealth <= Variables.AutoWHeal.GetValue<Slider>().Value && Player.Mana == 5 && Variables.W.IsReady()
                          && (Player.Health * 100 / Player.MaxHealth <= 10 || Player.CountEnemiesInRange(1000) > 0))
            {
                Variables.W.Cast();
            }

            if (Variables.AutoWKS.GetValue<bool>() && Variables.W.IsReady() && Player.Mana != 5)
            {
                foreach (var target in HeroManager.Enemies.Where(x => x.IsValidTarget(500) && !x.IsZombie))
                {
                    if (target.Health <= Variables.W.GetDamage(target))
                        Variables.W.Cast(target);
                }
            }

            if (Variables.AutoESK.GetValue<bool>() && Variables.E.IsReady())
            {
                foreach (var target in HeroManager.Enemies.Where(x => x.IsValidTarget(Variables.E.Range) && !x.IsZombie))
                {
                    if (target.Health <= Variables.E.GetDamage(target))
                        Helper.CastE(target);
                }
            }

            if (Helper.HasSmite && Variables.AutoSmiteSteal.GetValue<bool>())
            {
                var creep = MinionManager.GetMinions(800, MinionTypes.All, MinionTeam.Neutral)
                    .Where(x => x.CharData.BaseSkinName.Contains("SRU_Dragon") || x.CharData.BaseSkinName.Contains("SRU_Baron"));
                foreach (var x in creep.Where(y => Player.Distance(y.Position) <= Player.BoundingRadius + 500 + y.BoundingRadius))
                {
                    if (x != null && x.Health <= Helper.SmiteDamage)
                        Player.Spellbook.CastSpell(Variables.Smite, x);
                }
            }

            if (Helper.HasSmite && Variables.AutoSmiteKS.GetValue<bool>())
            {

                if (Helper.hasSmiteBlue)
                {
                    var hero = HeroManager.Enemies.Where(x => x.IsValidTarget(800) && !x.IsZombie);
                    foreach (var x in hero.Where(y => Player.Distance(y.Position) <= Player.BoundingRadius + 500 + y.BoundingRadius))
                    {
                        if ( x.Health <= Helper.SmiteBlueDamage)
                            Player.Spellbook.CastSpell(Variables.Smite, x);
                    }
                }

            }
        }
    }
}
