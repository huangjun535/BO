using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Enumerations;
using LeagueSharp.SDK.Utils;
using SharpDX;

namespace _SDK_Thresh___As_the_Chain_Warden {
	public static class PositionHelper {
		private static List<LastPosition> lastPositions = new List<LastPosition>();

		public static void Init()
		{
			Events.OnLoad += Events_OnLoad;
			
		}

		private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
		{
			if(sender.IsAlly || sender.Type != GameObjectType.obj_AI_Hero)return;
			var target = sender as Obj_AI_Hero;

			var lastPosition = lastPositions.FirstOrDefault(lp => lp.Hero.Equals(target));
			if (lastPosition != null && target!=null)
			{
				if (target.ChampionName.Equals("Pantheon") && args.Slot == SpellSlot.R)
				{
					lastPosition.IsTeleporting = true;
					lastPosition.TeleportEnd = Variables.TickCount + 2 * 1000;
					DelayAction.Add(2 * 1000, () =>
					{
						lastPosition.IsTeleporting = false;
					});
				}
				else if (target.ChampionName.Equals("Shen") && args.Slot == SpellSlot.R)
				{
					lastPosition.IsTeleporting = true;
					lastPosition.TeleportEnd = Variables.TickCount + 3 * 1000;
					DelayAction.Add(3 * 1000, () =>
					{
						lastPosition.IsTeleporting = false;
					});
				}
				else if (target.ChampionName.Equals("TahmKench") && args.Slot == SpellSlot.R)
				{
					lastPosition.IsTeleporting = true;
					lastPosition.TeleportEnd = Variables.TickCount + 3 * 1000;
					DelayAction.Add(3 * 1000, () =>
					{
						lastPosition.IsTeleporting = false;
					});
				}
			}
		}

		private static void Events_OnTeleport(object sender, TeleportEventArgs e) {
			if (!e.Object.IsEnemy) return;
			
			if (e.Type == TeleportType.TwistedFate || e.Type == TeleportType.Teleport)
			{
				var lastPosition = lastPositions.FirstOrDefault(lp => lp.Hero.Equals(e.Object));
				if (lastPosition != null)
				{
					if (e.Status == TeleportStatus.Start)
					{
						lastPosition.IsTeleporting = true;
						lastPosition.TeleportEnd = e.Start + e.Duration;
					}
					if (e.Status == TeleportStatus.Finish)
					{
						lastPosition.IsTeleporting = false;
						lastPosition.TeleportEnd = 0;
					}
					if (e.Status == TeleportStatus.Abort)
					{
						lastPosition.IsTeleporting = false;
						lastPosition.Position = lastPosition.Hero.ServerPosition;
						lastPosition.TeleportEnd = 0;
					}
				}
				
			}
			
		}

		private static void Events_OnDash(object sender, Events.DashArgs e) {
			if (e.Unit.IsEnemy)
			{
				var lastPosition = lastPositions.FirstOrDefault(lp => lp.Hero.Equals(e.Unit));
				if (lastPosition != null)
				{
					lastPosition.LastSeen = e.EndTick;
					lastPosition.Position = e.EndPos.ToVector3();
				}
			}
		}

		private static void Game_OnUpdate(EventArgs args) {
			foreach (var lp in lastPositions)
			{
				if (lp.Hero.IsDead || lp.Hero.IsZombie)
				{
					var objSpawnPoint = GameObjects.EnemySpawnPoints.FirstOrDefault();
					if (objSpawnPoint != null)
						lp.Position = objSpawnPoint.Position;
					lp.LastSeen = Variables.TickCount;
				}
				else if (!lp.IsTeleporting && !lp.Hero.IsDashing())
				{
					lp.Position = lp.Hero.ServerPosition;
					lp.LastSeen = Variables.TickCount;
					if (lp.Hero.HasBuff("Shen"))
					{
						var shenLastPosition = lastPositions.FirstOrDefault(slp => slp.Hero.ChampionName.Equals("Shen"));
						if (shenLastPosition != null)
							shenLastPosition.Position = lp.Hero.Position;
					}
				}
			}
		}

		private static void Events_OnLoad(object sender, EventArgs e) {
			foreach (var hero in GameObjects.EnemyHeroes)
			{
				lastPositions.Add(new LastPosition()
				{
					Hero = hero,
					LastSeen = Variables.TickCount,
					Position = hero.ServerPosition
				});
			}

			Game.OnUpdate += Game_OnUpdate;
			Events.OnDash += Events_OnDash;
			Events.OnTeleport += Events_OnTeleport;
			Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
		}



		public static int CountEnemyInRangeEx(this Obj_AI_Base unit,bool withTurret = true, float range = TurretHelper.TurretAttackRange, int timeTick = 3000)
		{
			var enemies = CountEnemyHeroesInRangeEx(unit,range,timeTick);
			if (withTurret && unit.GetEnemyTurret(range)!=null)
			{
				enemies += 1;
			}
			return enemies;
		}

		public static int CountEnemyHeroesInRangeEx(this Obj_AI_Base unit, float range = 700,int timeTick = 3000)
		{
			return unit.ServerPosition.CountEnemyHeroesInRangeEx(range,timeTick);
		}

		public static int CountEnemyHeroesInRangeEx(this Vector3 from, float range = 700, int timeTick = 3000)
		{
			return lastPositions.Count(
					lp => 
						!lp.Hero.IsDead && !lp.Hero.IsZombie 
						&& lp.Position.Distance(from) < range
						&& Variables.TickCount - lp.LastSeen < timeTick);
		}

		public static int CountEnemiesInRangeDeley(this Obj_AI_Hero hero, float range, float delay)
		{
			return GameObjects.EnemyHeroes
				.Where(t => t.IsValidTarget())
				.Select(t => Movement.GetPrediction(t, delay).CastPosition)
				.Count(prepos => hero.Distance(prepos) < range);
		}
	}

	public class LastPosition
	{
		public Obj_AI_Hero Hero { get; set; }
		public Vector3 Position { get; set; }
		public int LastSeen { get; set; }
		public bool IsTeleporting { get; set; }
		public int TeleportEnd { get; set; }
	}
}
