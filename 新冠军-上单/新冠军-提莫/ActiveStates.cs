﻿using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace YuLeTeemo
{
    internal class ActiveStates
    {
        public static void OnUpdate(EventArgs args)
        {
            var autoQ = Essentials.Config.SubMenu("Misc").Item("autoQ").GetValue<bool>();
            var autoW = Essentials.Config.SubMenu("Misc").Item("autoW").GetValue<bool>();

            // Reworked Auto Q and W
            if (autoQ && autoW)
            {
                AutoQw();
            }
            else if (autoQ)
            {
                AutoQ();
            }
            else if (autoW)
            {
                AutoW();
            }

            if (Essentials.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None)
            {
                // Auto Shroom
                if (Essentials.Config.SubMenu("Misc").Item("autoR").GetValue<bool>())
                {
                    AutoShroom();
                }

                // KillSteal
                if (Essentials.Config.SubMenu("KSMenu").Item("KSQ").GetValue<bool>()
                    || Essentials.Config.SubMenu("KSMenu").Item("KSR").GetValue<bool>())
                {
                    StateManager.KillSteal();
                }
            }
        }

        /// <summary>
        /// Does the AutoShroom
        /// </summary>
        public static void AutoShroom()
        {
            var autoRPanic = Essentials.Config.SubMenu("Misc").Item("autoRPanic").IsActive();

            // Panic Key now makes you move
            if (autoRPanic)
            {
                ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            }

            if (!Essentials.R.IsReady() || ObjectManager.Player.IsRecalling() || autoRPanic)
            {
                return;
            }

            var target = HeroManager.Enemies.FirstOrDefault(t => Essentials.R.IsInRange(t) && t.IsValidTarget());

            if (target != null)
            {
                if (target.HasBuff("zhonyasringshield") && Essentials.R.IsReady() && Essentials.R.IsInRange(target))
                {
                    Essentials.R.Cast(target.Position);
                }
            }
            else
            {
                var rCharge = Essentials.Config.SubMenu("Misc").Item("rCharge").GetValue<Slider>().Value;
                var rCount = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Ammo;

                switch (Utility.Map.GetMap().Type)
                {
                    case Utility.Map.MapType.SummonersRift:
                        if (Essentials.ShroomPositions.SummonersRift.Any())
                        {
                            foreach (
                                var place in
                                    Essentials.ShroomPositions.SummonersRift.Where(
                                        pos =>
                                            pos.Distance(ObjectManager.Player.Position) <= Essentials.R.Range &&
                                            !Essentials.IsShroomed(pos))
                                        .Where(
                                            place =>
                                                rCharge <= rCount && Environment.TickCount - Essentials.LastR > 5000))
                            {
                                Essentials.R.Cast(place);
                            }
                        }
                        break;
                    case Utility.Map.MapType.HowlingAbyss:
                        if (Essentials.ShroomPositions.HowlingAbyss.Any())
                        {
                            foreach (
                                var place in
                                    Essentials.ShroomPositions.HowlingAbyss.Where(
                                        pos =>
                                            pos.Distance(ObjectManager.Player.Position) <= Essentials.R.Range &&
                                            !Essentials.IsShroomed(pos))
                                        .Where(
                                            place =>
                                                rCharge <= rCount && Environment.TickCount - Essentials.LastR > 5000))
                            {
                                Essentials.R.Cast(place);
                            }
                        }
                        break;
                    case Utility.Map.MapType.CrystalScar:
                        if (Essentials.ShroomPositions.CrystalScar.Any())
                        {
                            foreach (
                                var place in
                                    Essentials.ShroomPositions.CrystalScar.Where(
                                        pos =>
                                            pos.Distance(ObjectManager.Player.Position) <= Essentials.R.Range &&
                                            !Essentials.IsShroomed(pos))
                                        .Where(
                                            place =>
                                                rCharge <= rCount && Environment.TickCount - Essentials.LastR > 5000))
                            {
                                Essentials.R.Cast(place);
                            }
                        }
                        break;
                    case Utility.Map.MapType.TwistedTreeline:
                        if (Essentials.ShroomPositions.TwistedTreeline.Any())
                        {
                            foreach (
                                var place in
                                    Essentials.ShroomPositions.TwistedTreeline.Where(
                                        pos =>
                                            pos.Distance(ObjectManager.Player.Position) <= Essentials.R.Range &&
                                            !Essentials.IsShroomed(pos))
                                        .Where(
                                            place =>
                                                rCharge <= rCount && Environment.TickCount - Essentials.LastR > 5000))
                            {
                                Essentials.R.Cast(place);
                            }
                        }
                        break;
                    default:
                        if (Utility.Map.GetMap().Type.ToString() == "Unknown")
                        {
                            if (Essentials.ShroomPositions.ButcherBridge.Any())
                            {
                                foreach (
                                    var place in
                                        Essentials.ShroomPositions.ButcherBridge.Where(
                                            pos =>
                                                pos.Distance(ObjectManager.Player.Position) <= Essentials.R.Range &&
                                                !Essentials.IsShroomed(pos))
                                            .Where(
                                                place =>
                                                    rCharge <= rCount &&
                                                    Environment.TickCount - Essentials.LastR > 5000))
                                {
                                    Essentials.R.Cast(place);
                                }
                            }
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Auto Q
        /// </summary>
        public static void AutoQ()
        {
            var target = TargetSelector.GetTarget(Essentials.Q.Range, TargetSelector.DamageType.Magical);
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.Position, Essentials.Q.Range);

            if (Essentials.Q.IsReady() && 1 <= allMinionsQ.Count)
            {
                foreach (
                    var minion in
                        allMinionsQ.Where(
                            minion => minion.Health <= Essentials.Q.GetDamage(minion) && Essentials.Q.IsInRange(minion)))
                {
                    Essentials.Q.CastOnUnit(minion);
                }
            }

            if (target != null)
            {
                if (Essentials.Q.IsReady() && Essentials.Q.IsInRange(target) && target.IsValidTarget() &&
                    ObjectManager.Player.ManaPercent >= 25)
                {
                    Essentials.Q.Cast(target);
                }
            }
        }

        /// <summary>
        /// Auto W
        /// </summary>
        public static void AutoW()
        {
            if (Essentials.W.IsReady())
            {
                Essentials.W.Cast(ObjectManager.Player);
            }
        }

        /// <summary>
        /// Auto Q and W
        /// </summary>
        public static void AutoQw()
        {
            var target = TargetSelector.GetTarget(Essentials.Q.Range, TargetSelector.DamageType.Magical);
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.Position, Essentials.Q.Range);

            if (Essentials.W.IsReady())
            {
                Essentials.W.Cast();
            }

            if (Essentials.Q.IsReady() && 1 <= allMinionsQ.Count)
            {
                foreach (
                    var minion in
                        allMinionsQ.Where(
                            minion => minion.Health <= Essentials.Q.GetDamage(minion) && Essentials.Q.IsInRange(minion)))
                {
                    Essentials.Q.CastOnUnit(minion);
                }
            }

            if (target != null)
            {
                if (Essentials.Q.IsReady() && Essentials.Q.IsInRange(target) && target.IsValidTarget() &&
                    ObjectManager.Player.ManaPercent >= 25)
                {
                    Essentials.Q.Cast(target);
                }
            }
        }
    }
}