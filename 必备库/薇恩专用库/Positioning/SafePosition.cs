﻿using System;
using System.Collections.Generic;
using System.Linq;
using DZLib.Hero;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace DZLib.Positioning
{
    public static class SafePosition
    {
        public static bool IsSafe(this Vector3 position, float range)
        {
            if (position.UnderTurret(true) && !ObjectManager.Player.UnderTurret(true))
            {
                return false;
            }

            var allies = position.CountAlliesInRange(ObjectManager.Player.AttackRange);
            var enemies = position.CountEnemiesInRange(ObjectManager.Player.AttackRange);
            var lhEnemies = position.GetLhEnemiesNear(ObjectManager.Player.AttackRange, 15).Count();

            if (enemies <= 1) ////It's a 1v1, safe to assume I can Q
            {
                return true;
            }

            if (position.UnderAllyTurret())
            {
                var nearestAllyTurret = ObjectManager.Get<Obj_AI_Turret>().Where(a => a.IsAlly).OrderBy(d => d.Distance(position, true)).FirstOrDefault();

                if (nearestAllyTurret != null)
                {
                    ////We're adding more allies, since the turret adds to the firepower of the team.
                    allies += 2;
                }
            }

            var normalCheck = (allies + 1 > enemies - lhEnemies);
            var PositionEnemiesCheck = true;

            var Vector2Position = position.To2D();
            var enemyPoints = PositioningHelper.GetEnemyZoneList(false);
            if (enemyPoints.Contains(Vector2Position))
            {
                PositionEnemiesCheck = false;
            }
            var closeEnemies = PositioningHelper.EnemiesClose;

            if (!closeEnemies.All(
                    enemy =>
                        position.CountEnemiesInRange(enemy.AttackRange) <= 1))
            {
                PositionEnemiesCheck = false;
            }

            return normalCheck && PositionEnemiesCheck;
        }
    }
}
