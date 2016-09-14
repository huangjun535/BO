﻿using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using VayneHunter_Reborn.Utility;
using VayneHunter_Reborn.Utility.MenuUtility;

namespace VayneHunter_Reborn.Skills.Tumble.VHRQ
{
    class VHRQLogic
    {
        public static List<Vector3> GetRotatedQPositions()
        {
            const int currentStep = 30;
            var direction = ObjectManager.Player.Direction.To2D().Perpendicular();

            var list = new List<Vector3>();
            for (var i = -105; i <= 105; i += currentStep)
            {
                var angleRad = Geometry.DegreeToRadian(i);
                var rotatedPosition = ObjectManager.Player.Position.To2D() + (300f * direction.Rotated(angleRad));
                list.Add(rotatedPosition.To3D());
            }
            return list;
        }

        public static Vector3 GetVHRQPosition()
        {
            var positions = GetRotatedQPositions();
            var enemyPositions = TumblePositioning.GetEnemyPoints();
            var safePositions = positions.Where(pos => !enemyPositions.Contains(pos.To2D())).ToList();
            var BestPosition = ObjectManager.Player.ServerPosition.Extend(Game.CursorPos, 300f);
            var AverageDistanceWeight = .65f;
            var ClosestDistanceWeight = .35f;

            var bestWeightedAvg = 0f;
            
            if (ObjectManager.Player.CountEnemiesInRange(1300f) <= 1)
            {
                var position = ObjectManager.Player.ServerPosition.Extend(Game.CursorPos, 300f);
                return position.IsSafe(true) ? position : Vector3.Zero;
            }

            foreach (var position in safePositions)
            {
                //Start le calculations    
                var enemy = GetClosestEnemy(position);
                if (enemy == null)
                {
                    continue;
                }
                
                if (ObjectManager.Player.Distance(enemy) < enemy.AttackRange - 85 && !enemy.IsMelee)
                {
                    return ObjectManager.Player.ServerPosition.Extend(Game.CursorPos, 300f);
                }

                var avgDist = GetAvgDistance(position);
                if (avgDist > -1)
                {
                    var closestDist = ObjectManager.Player.ServerPosition.Distance(enemy.ServerPosition);
                    var weightedAvg = closestDist * ClosestDistanceWeight + avgDist * AverageDistanceWeight;
                    if (weightedAvg > bestWeightedAvg && position.IsSafe())
                    {
                        bestWeightedAvg = weightedAvg;
                        BestPosition = position;
                    }
                }
            }

            return (BestPosition.IsSafe(true) && IsSafeEx(BestPosition)) ? BestPosition : Vector3.Zero;
        }

        public static Obj_AI_Hero GetClosestEnemy(Vector3 from)
        {
            if (Variables.Orbwalker.GetTarget() is Obj_AI_Hero)
            {
                var owAI = Variables.Orbwalker.GetTarget() as Obj_AI_Hero;
                if (owAI.IsValidTarget(Orbwalking.GetRealAutoAttackRange(null) + 120f, true, from))
                {
                    return owAI;
                }
            }

            return null;

            return
                HeroManager.Enemies
                    .FirstOrDefault(en => en.IsValidTarget(Orbwalking.GetRealAutoAttackRange(null), true, from));
        }

        public static bool IsSafeEx(Vector3 position)
        {
            var closeEnemies =
                    HeroManager.Enemies.FindAll(en => en.IsValidTarget(1500f) && !(en.Distance(ObjectManager.Player.ServerPosition) < en.AttackRange + 65f))
                    .OrderBy(en => en.Distance(position));

            return closeEnemies.All(
                                enemy =>
                                    position.CountEnemiesInRange(
                                        MenuExtensions.GetItemValue<bool>("dz191.vhr.misc.tumble.dynamicqsafety")
                                            ? enemy.AttackRange
                                            : 405f) <= 1);
        }

        public static float GetAvgDistance(Vector3 from)
        {
            var numberOfEnemies = from.CountEnemiesInRange(1000f);
            if (numberOfEnemies != 0)
            {
                var enemies = HeroManager.Enemies.Where(en => en.IsValidTarget(1000f, true, from)
                                                    &&
                                                    en.Health >
                                                    ObjectManager.Player.GetAutoAttackDamage(en)*3 +
                                                    Variables.spells[SpellSlot.W].GetDamage(en) +
                                                    Variables.spells[SpellSlot.Q].GetDamage(en)).ToList();
                var enemiesEx = HeroManager.Enemies.Where(en => en.IsValidTarget(1000f, true, from)).ToList();
                var LHEnemies = enemiesEx.Count() - enemies.Count();
                var totalDistance = 0f;

                totalDistance = (LHEnemies > 1 && enemiesEx.Count() > 2) ? 
                    enemiesEx.Sum(en => en.Distance(ObjectManager.Player.ServerPosition)) : 
                    enemies.Sum(en => en.Distance(ObjectManager.Player.ServerPosition));

                return totalDistance / numberOfEnemies;
            }
            return -1;
        }
    }
}
