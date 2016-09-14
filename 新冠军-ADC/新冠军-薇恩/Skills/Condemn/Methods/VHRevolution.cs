﻿using System.Collections.Generic;
using System.Linq;
using iSeriesReborn.Utility.Positioning;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SPrediction;
using VayneHunter_Reborn.Utility;
using VayneHunter_Reborn.Utility.MenuUtility;

namespace VayneHunter_Reborn.Skills.Condemn.Methods
{
    class VHRevolution
    {
        public static Obj_AI_Base GetTarget(Vector3 fromPosition)
        {
            return VHReborn.GetTarget(fromPosition);

            var HeroList = HeroManager.Enemies.Where(
                                    h =>
                                        h.IsValidTarget(Variables.spells[SpellSlot.E].Range) &&
                                        !h.HasBuffOfType(BuffType.SpellShield) &&
                                        !h.HasBuffOfType(BuffType.SpellImmunity));
                    //dz191.vhr.misc.condemn.rev.accuracy
                    //dz191.vhr.misc.condemn.rev.nextprediction
           var MinChecksPercent = MenuExtensions.GetItemValue<Slider>("dz191.vhr.misc.condemn.accuracy").Value;
           var PushDistance = MenuExtensions.GetItemValue<Slider>("dz191.vhr.misc.condemn.pushdistance").Value;

            if (PushDistance >= 410)
            {
                var PushEx = PushDistance;
                PushDistance -= (10 + (PushEx - 410)/2);
            }

            if (ObjectManager.Player.ServerPosition.UnderTurret(true))
            {
                    return null;
            }

            foreach (var Hero in HeroList)
            {
                if (MenuExtensions.GetItemValue<bool>("dz191.vhr.misc.condemn.onlystuncurrent") &&
                    Hero.NetworkId != Variables.Orbwalker.GetTarget().NetworkId)
                {
                    continue;
                }

                if (Hero.Health + 10 <=
                    ObjectManager.Player.GetAutoAttackDamage(Hero)*
                    MenuExtensions.GetItemValue<Slider>("dz191.vhr.misc.condemn.noeaa").Value)
                {
                    continue;
                }


                var targetPosition = Vector3.Zero;

                var pred = Variables.spells[SpellSlot.E].GetSPrediction(Hero);
                if (pred.HitChance > HitChance.Impossible)
                {
                    targetPosition = pred.UnitPosition.To3D();
                }

                if (targetPosition == Vector3.Zero)
                {
                    return null;
                }

                var finalPosition = targetPosition.Extend(ObjectManager.Player.ServerPosition, -PushDistance);
                var finalPosition_ex = Hero.ServerPosition.Extend(ObjectManager.Player.ServerPosition, -PushDistance);

                var condemnRectangle = new VHRPolygon(VHRPolygon.Rectangle(targetPosition.To2D(), finalPosition.To2D(), Hero.BoundingRadius));
                var condemnRectangle_ex = new VHRPolygon(VHRPolygon.Rectangle(Hero.ServerPosition.To2D(), finalPosition_ex.To2D(), Hero.BoundingRadius));

                if (IsBothNearWall(Hero))
                {
                    return null;
                }

                if (condemnRectangle.Points.Count(point => NavMesh.GetCollisionFlags(point.X, point.Y).HasFlag(CollisionFlags.Wall)) >= condemnRectangle.Points.Count() * (MinChecksPercent / 100f)
                    && condemnRectangle_ex.Points.Count(point => NavMesh.GetCollisionFlags(point.X, point.Y).HasFlag(CollisionFlags.Wall)) >= condemnRectangle_ex.Points.Count() * (MinChecksPercent / 100f))
                {
                    return Hero;
                }
            }
            return null;
        }

        private static bool IsBothNearWall(Obj_AI_Base target)
        {
            var positions =
                GetWallQPositions(target, 110).ToList().OrderBy(pos => pos.Distance(target.ServerPosition, true));
            var positions_ex =
            GetWallQPositions(ObjectManager.Player, 110).ToList().OrderBy(pos => pos.Distance(ObjectManager.Player.ServerPosition, true));

            if (positions.Any(p => p.IsWall()) && positions_ex.Any(p => p.IsWall()))
            {
                return true;
            }
            return false;
        }

        private static Vector3[] GetWallQPositions(Obj_AI_Base player, float Range)
        {
            Vector3[] vList =
            {
                (player.ServerPosition.To2D() + Range * player.Direction.To2D()).To3D(),
                (player.ServerPosition.To2D() - Range * player.Direction.To2D()).To3D()

            };

            return vList;
        }
    }
   
}
