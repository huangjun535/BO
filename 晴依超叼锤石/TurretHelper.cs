using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.SDK;

namespace _SDK_Thresh___As_the_Chain_Warden {
	public static class TurretHelper {

		public const float TurretAttackRange = 775f;
		public const float TurretSightRange = 1095f;

		/// <summary>
		/// 目标是否在我军塔下
		/// </summary>
		/// <param name="target"></param>
		/// <returns>如果目标在塔下，返回该塔，否则返回null</returns>
		public static Obj_AI_Turret IsUnderAllyTurret(this Obj_AI_Base target)
		{
			var turret = GetAllyTurret(target);
			return (turret != null && target.Distance(turret) < TurretAttackRange) 
					? turret 
					: null;
		}

		public static Obj_AI_Turret GetAllyTurret(this Obj_AI_Base target,float range = 1095)
		{
			return GameObjects.AllyTurrets.Find(t => t.IsValid && !t.IsDead && t.Health >= 1f && t.IsVisible && t.Distance(target) < range);
		}

		public static Obj_AI_Turret IsUnderEnemyTurret(this Obj_AI_Base target) {
			var turret = GetEnemyTurret(target);
			return (turret != null && target.Distance(turret) < TurretAttackRange)
					? turret
					: null;
		}

		public static Obj_AI_Turret GetEnemyTurret(this Obj_AI_Base target, float range = 1095) {
			return GameObjects.EnemyTurrets.Find(t => t.IsValid && !t.IsDead && t.Health >= 1f && t.IsVisible && t.Distance(target) < range);
		}

		public static Obj_AI_Turret GetMostCloseTower(this Obj_AI_Base target) {
			if (target.IsDead) return null;

			return GameObjects.Turrets
				.Where(t => t.IsValid && !t.IsDead && t.Health >= 1f && t.IsVisible && t.Distance(target) < 1000)
				.MaxOrDefault(t => t.DistanceToPlayer());
		}

	}
}
