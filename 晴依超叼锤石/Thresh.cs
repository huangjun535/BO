using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Data;
using LeagueSharp.Data.DataTypes;
using LeagueSharp.SDK;
using LeagueSharp.SDK.UI;
using LeagueSharp.SDK.Enumerations;
using LeagueSharp.SDK.UI.Skins;
using LeagueSharp.SDK.Utils;
using LeagueSharp.SDK.Signals;
using SharpDX;
using Keys = System.Windows.Forms.Keys;
using LeagueSharp.Data.Enumerations;
using LeagueSharp.SDK.MoreLinq;
using LeagueSharp.SDK.TSModes;

namespace _SDK_Thresh___As_the_Chain_Warden {
	public class Thresh {

		public string RootMenuName => "_SDK_Thresh___As_the_Chain_Warden";

		public Obj_AI_Hero Player => ObjectManager.Player;
		public Spell Q { get; set; }
		public Spell W { get; set; }
		public Spell E { get; set; }
		public Spell R { get; set; }
		public Spell Flash { get; set; }
		public Keys FlashKey { get; set; }

		public Menu Config { get; set; }
		public HitChance MHitChance => GetHitChance();
		public HitChance[] HitChances =  { HitChance.VeryHigh, HitChance.High, HitChance.Medium };

		/// <summary>
		/// Q中的目标
		/// </summary>
		public Obj_AI_Base Qedtarget => GameObjects.Enemy.Find(e => e.HasBuff(QBuffName) && e.GetBuff("ThreshQ").Caster.IsMe);

		/// <summary>
		/// Q BUFF名字，敌人被Q中，以及Q锁中人时自己身上都有这个buff
		/// </summary>
		private const string QBuffName = "ThreshQ";

		public Thresh() {
			Events.OnLoad += Events_OnLoad;
		}

		private void Events_OnLoad(object sender, EventArgs e) {
			if (Player.ChampionName != "Thresh") return;
			InitSpell();
			InitUi();

			PositionHelper.Init();

			Game.OnUpdate += Game_OnUpdate;
			Drawing.OnDraw += Drawing_OnDraw;
			Variables.Orbwalker.OnAction += Orbwalker_OnAction;
			Events.OnInterruptableTarget += Events_OnInterruptableTarget;
			Events.OnGapCloser += Events_OnGapCloser;
			Events.OnDash += Events_OnDash;
			//Events.OnTeleport += Events_OnTeleport;
			Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
			Spellbook.OnCastSpell += Spellbook_OnCastSpell;
			//Obj_AI_Base.OnBuffAdd += Obj_AI_Base_OnBuffAdd;
			//Obj_AI_Base.OnBuffRemove += Obj_AI_Base_OnBuffRemove;
		}

		private void Obj_AI_Base_OnBuffRemove(Obj_AI_Base sender, Obj_AI_BaseBuffRemoveEventArgs args) {
			if (sender.IsEnemy && args.Buff.Name == "ThreshQ" && args.Buff.Caster.IsMe)
			{
				//Qedtarget = sender;
			}
		}

		private void Obj_AI_Base_OnBuffAdd(Obj_AI_Base sender, Obj_AI_BaseBuffAddEventArgs args) {
			if (sender.IsEnemy && args.Buff.Name == "ThreshQ" && args.Buff.Caster.IsMe)
			{
				//Qedtarget = null;
			}
		}

		private void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args) {

			if ((args.Slot == SpellSlot.E || args.Slot == SpellSlot.R) && Player.IsDashing())
			{
				args.Process = false;
			}
		}

		private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args) {
			//点灯笼.克隆模式ID取要查找，在克隆模式下这个办法不能知道是否点的是自己的灯笼
			if ((int)Game.Type != 7
				&& sender.IsAlly
				&& args.Slot == (SpellSlot)62
				&& Qedtarget != null
				&& (sender.Distance(Player) > W.Range * 2 / 3 || Player.GetBuffLaveTime(QBuffName)> Q.Delay))
			{
				//Game.PrintChat("DEBUG:ally click lantern");
				CastQ2();
			}

			#region 自动W
			if (!W.IsReady() || !sender.IsEnemy || !sender.IsValidTarget(1500) || !(sender is Obj_AI_Hero)) return;

			double value = 20 + (Player.Level * 20) + (0.4 * Player.FlatMagicDamageMod);

			foreach (var ally in GameObjects.AllyHeroes.Where(ally => ally.IsValid && !ally.IsDead && Player.Distance(ally.ServerPosition) < W.Range + 200))
			{
				double dmg = 0;
				if (args.Target != null && args.Target.NetworkId == ally.NetworkId)
				{
					dmg = dmg + ((Obj_AI_Hero)sender).GetSpellDamage(ally, args.Slot);
				}
				else
				{
					var castArea = ally.Distance(args.End) * (args.End - ally.ServerPosition).Normalized() + ally.ServerPosition;
					if (castArea.Distance(ally.ServerPosition) < ally.BoundingRadius / 2)
						dmg = dmg + ((Obj_AI_Hero)sender).GetSpellDamage(ally, args.Slot);
					else
						continue;
				}

				if (dmg > 0)
				{
					if (dmg > value)
						W.Cast(ally.Position);
					else if (Player.Health - dmg < Player.CountEnemyHeroesInRange(700) * Player.Level * 20)
						W.Cast(ally.Position);
					else if (ally.Health - dmg < ally.Level * 10)
						W.Cast(ally.Position);
				}
			}
			#endregion
		}

		private void Events_OnDash(object sender, Events.DashArgs e) {
			
			if (e.Unit.Type != GameObjectType.obj_AI_Hero || e.Unit.IsAlly || e.Unit.HasBuffOfType(BuffType.SpellShield)) return;
			if (!E.IsReady() || Player.Distance(e.EndPos) < Player.Distance(e.StartPos)) return;

			var ePos = e.StartPos.Extend(e.EndPos,E.Delay * e.Speed).ToVector3();

			if (E.IsInRange(ePos))
			{
				E.CastReverse(ePos);
			}


			//else if (Q.IsReady(e.Duration) && Q.IsInRange(e.EndPos)
			//	&& Q.Delay + e.EndPos.Distance(e.StartPos) / Q.Speed <= e.Duration + 0.1
			//	&& Q.Cast(e.EndPos))
			//{
			//	return;
			//}

		}

		private void Events_OnGapCloser(object sender, Events.GapCloserEventArgs e) {
			
			if (!e.Sender.IsEnemy || e.Sender.HasBuffOfType(BuffType.SpellShield)) return;
			if (e.SkillType == GapcloserType.Targeted)
			{
				if (E.CanCast((Obj_AI_Base)e.Target) || E.IsReady(e.TickCount) && E.IsInRange((Obj_AI_Base)e.Target))
				{
					if (e.Sender.ChampionName == "MasterYi" || e.Sender.ChampionName == "Fizz" && e.Slot == SpellSlot.E)
					{
						DelayAction.Add(e.TickCount, () => { E.Cast((Obj_AI_Base)e.Target); });
						return;
					}
					else if (E.Cast(e.Sender) == CastStates.SuccessfullyCasted)
					{
						return;
					}
				}
				else if (Q.CanCast((Obj_AI_Base)e.Target) || !E.IsReady(e.TickCount) && Q.IsReady(e.TickCount) && Q.IsInRange((Obj_AI_Base)e.Target))
				{
					if (e.Sender.ChampionName == "MasterYi")
					{
						DelayAction.Add(e.TickCount, () => { Q.Cast((Obj_AI_Base)e.Target); });
						return;
					}
					else if (Q.Cast(e.Sender) == CastStates.SuccessfullyCasted)
					{
						return;
					}
				}

			}
			else
			{
				if (E.IsReady(e.TickCount) && E.IsInRange(e.End))
				{
					if (e.Sender.ChampionName == "Fizz" && e.Slot == SpellSlot.E)
					{
						DelayAction.Add(e.TickCount, () => { E.Cast((Obj_AI_Base)e.Target); });
						return;
					}
					else if (E.Cast(e.Sender) == CastStates.SuccessfullyCasted)
					{
						return;
					}
				}
				else if (!E.IsReady(e.TickCount) && Q.IsReady(e.TickCount) && Q.IsInRange(e.End))
				{
					if (e.Sender.ChampionName == "Fizz" && e.Slot == SpellSlot.E)
					{
						DelayAction.Add(e.TickCount, () => { Q.Cast((Obj_AI_Base)e.Target); });
						return;
					}
					else if (Q.Cast(e.Sender) == CastStates.SuccessfullyCasted)
					{
						return;
					}
				}
			}
		}

		private void Events_OnInterruptableTarget(object sender, Events.InterruptableTargetEventArgs e) {
			if (e.Sender.HasBuffOfType(BuffType.SpellShield)) return;
			if (e.Sender.IsValidTarget(E.Range) && E.IsReady() && E.Cast(e.Sender) == CastStates.SuccessfullyCasted)
			{
				return;
			}
			if (e.Sender.IsValidTarget(Q.Range) && Q.IsReady() && Q.Cast(e.Sender) == CastStates.SuccessfullyCasted)
			{
				return;
			}
		}

		private void Orbwalker_OnAction(object sender, OrbwalkingActionArgs e) {

			if (Config["辅助模式设置"]["辅助模式开关"].GetValue<MenuBool>().Value)
			{
				if (e.Type == OrbwalkingType.BeforeAttack)
				{
					if (e.Target.Type == GameObjectType.obj_AI_Minion)
					{
						var minion = e.Target as Obj_AI_Minion;

						if (minion != null  && minion.CountAllyHeroesInRange(Config["辅助模式设置"]["辅助模式距离"].GetValue<MenuSlider>().Value) >= 2)
						{
							if (GameObjects.Player.GetAutoAttackDamage(minion) >= e.Target.Health)
							{
								e.Process = false;
							}
						}
					}
				}
			}

			//if (Config["辅助模式设置"]["辅助模式开关"].GetValue<MenuBool>().Value
			//	&& e.Target.Type == GameObjectType.obj_AI_Minion
			//	&& e.Target.Position.CountAllyHeroesInRange(Config["辅助模式设置"]["辅助模式距离"].GetValue<MenuSlider>().Value) >= 2
			//	&& e.Type == OrbwalkingType.BeforeAttack
			//	&& Player.GetAutoAttackDamage((Obj_AI_Base)e.Target) > e.Target.Health)
			//{
			//	e.Process = false;
			//}
		}

		private void Drawing_OnDraw(EventArgs args) {
			
			var qColor = Config["显示设置"]["Q颜色"].GetValue<MenuColor>();
			var qToggle = Config["显示设置"]["显示Q"].GetValue<MenuBool>();
			if (Q.IsReady() || !Config["显示设置"]["技能可用才显示"].GetValue<MenuBool>())
			{

				if (Flash != null && Config["智能施法"]["智能F"].GetValue<MenuKeyBind>().Active
					&& Flash.IsReady())
				{
					Render.Circle.DrawCircle(Player.Position, Q.Range + Flash.Range, qColor, 2);
				}
				if (qToggle)
				{
					Render.Circle.DrawCircle(Player.Position, Q.Range, qColor, 2);
				}
					
			}

			var wColor = Config["显示设置"]["W颜色"].GetValue<MenuColor>();
			var wToggle = Config["显示设置"]["显示W"].GetValue<MenuBool>();
			if (W.IsReady() || !Config["显示设置"]["技能可用才显示"].GetValue<MenuBool>())
			{
				if(wToggle)
				Render.Circle.DrawCircle(Player.Position, W.Range, wColor, 2);
			}
;
			var eColor = Config["显示设置"]["E颜色"].GetValue<MenuColor>();
			var eToggle = Config["显示设置"]["显示E"].GetValue<MenuBool>();
			if (E.IsReady() || !Config["显示设置"]["技能可用才显示"].GetValue<MenuBool>())
			{
				if(eToggle)
				Render.Circle.DrawCircle(Player.Position, E.Range, eColor, 2);
			}

			var rColor = Config["显示设置"]["R颜色"].GetValue<MenuColor>();
			var rToggle = Config["显示设置"]["显示R"].GetValue<MenuBool>();
			if (R.IsReady() || !Config["显示设置"]["技能可用才显示"].GetValue<MenuBool>())
			{
				if(rToggle)
				Render.Circle.DrawCircle(Player.Position, R.Range, rColor, 2);
			}
		}

		private bool CastR()
		{
			//0预判，1实时
			var heroesInRange = Config["技能设置"]["大招模式"].GetValue<MenuList>().Index == 0
					? Player.CountEnemiesInRangeDeley(R.Range,R.Delay)
					: Player.CountEnemyHeroesInRange(R.Range);

			return heroesInRange >= Config["技能设置"]["大招人数"].GetValue<MenuSlider>().Value && R.Cast();
		}

		private void Game_OnUpdate(EventArgs args) {
			//if (Player.IsDashing())
			//{
			//	Console.WriteLine("====取buff了===========");
			//	foreach (var buff in Player.Buffs)
			//	{
			//		Console.WriteLine(buff.Name + "\t" + buff.SourceName.ToGBK() + "\t" + buff.DisplayName);
			//	}
			//}
			if (Config["信息介绍"]["DebugE"].GetValue<MenuKeyBind>().Active)
			{
				Variables.Orbwalker.Move(Game.CursorPos);
				var target = Variables.TargetSelector.GetTarget(E);
				if (target!=null && target.IsValid)
				{
					var p1 = target.Position;
					if (E.CastReverse(target))
					{
						DelayAction.Add(1000, () =>
						{
							var p2 = target.Position;
							Console.WriteLine("=================");
							Console.WriteLine("p1:" + p1);
							Console.WriteLine("p2:" + p2);
							Console.WriteLine("距离：" + p1.Distance(p2));
							Game.PrintChat("Distance: " + p1.Distance(p2));
						});
					}
				}
			}
			if (Config["信息介绍"]["DebugQ"].GetValue<MenuKeyBind>().Active)
			{
				Variables.Orbwalker.Move(Game.CursorPos);
				var target = Variables.TargetSelector.GetTarget(Q);
				if (target != null && target.IsValid)
				{
					var p1 = target.Position;
					if (Q.Cast(target) == CastStates.SuccessfullyCasted)
					{
						DelayAction.Add(1500, () =>
						{
							var p2 = target.Position;
							Console.WriteLine("=================");
							Console.WriteLine("p1:" + p1);
							Console.WriteLine("p2:" + p2);
							Console.WriteLine("距离：" + p1.Distance(p2));
							Game.PrintChat("Distance: " + p1.Distance(p2));
						});
					}
				}
			}

			//return;

			foreach (var enemyHero in GameObjects.EnemyHeroes.Where(e=>!e.IsDead && !e.IsZombie && e.IsValidTarget() && !e.HasSpellShield()))
			{
				if (enemyHero!=null)
				{
					if (E.IsReady() && E.CastIfHitchanceEquals(enemyHero, HitChance.Dashing) == CastStates.SuccessfullyCasted)
					{
						break;
					}
					if (GetQState() == CastState.First && Q.CastIfHitchanceEquals(enemyHero, HitChance.Dashing) == CastStates.SuccessfullyCasted)
					{
						break;
					}
				}
				
			}

			CastR();
			CastW();

			if (Config["智能施法"]["智能Q"].GetValue<MenuKeyBind>().Active)
			{
				if (Variables.TargetSelector.Selected.Target!=null 
					&& Variables.TargetSelector.Selected.Target.IsValidTarget(Q.Range)
					&& !Variables.TargetSelector.Selected.Target.IsDead 
					&& !Variables.TargetSelector.Selected.Target.IsZombie
					&& !Variables.TargetSelector.Selected.Target.HasSpellShield())
				{
					Q.Cast(Variables.TargetSelector.Selected.Target);
				}
				else
				{
					Q.CastOnBestTarget();
				}
			}

			if (Config["智能施法"]["智能W"].GetValue<MenuKeyBind>().Active)
			{
				CastW();
			}

			if (Config["智能施法"]["智能E"].GetValue<MenuKeyBind>().Active)
			{
				var target = Variables.TargetSelector.GetTarget(E);
				CastE(target);
			}

			if (Flash!=null && Config["智能施法"]["智能F"].GetValue<MenuKeyBind>().Active)
			{
				FlashQ();
			}

			//自动控到塔下
			CatchToTower();

			if (Variables.Orbwalker.ActiveMode == OrbwalkingMode.Combo)
			{
				var ignorQList = GameObjects.EnemyHeroes.Where(e => !Config["技能设置"]["Q名单"][e.Name].GetValue<MenuBool>().Value);
				var target = Variables.TargetSelector.GetTarget(Q, true, ignorQList);
				CastE(target);
				CastQ1(target);
				if (Qedtarget!=null)
				{
					var jungle = GetJungle(W.Range + 200);
					if (jungle != null && jungle.IsValid 
						&& jungle.Distance(Player) <= W.Range + 200
						&& (jungle.Distance(Player) > 500 || jungle.HasWall(Player))
						&& (jungle.Distance(Qedtarget)>500 || jungle.HasWall(Qedtarget)))
					{
						W.Cast(jungle);
					}
					CastQ2();
				}

			}
			else if(Variables.Orbwalker.ActiveMode == OrbwalkingMode.Hybrid)
			{
				var ignorQList = GameObjects.EnemyHeroes.Where(e => !Config["技能设置"]["Q名单"][e.Name].GetValue<MenuBool>().Value);
				var target = Variables.TargetSelector.GetTarget(Q, true, ignorQList);
				CastE(target);
			}
			else if (Variables.Orbwalker.ActiveMode == OrbwalkingMode.LaneClear)
			{
				int eNum = Config["技能设置"]["清兵E"].GetValue<MenuSlider>();
				if (eNum == 0) return;

				var eFarm = E.GetLineFarmLocation(GameObjects.EnemyMinions.Where(m=> E.IsInRange(m)).ToList());
				if (eFarm.MinionsHit>=eNum)
				{
					E.Cast(eFarm.Position);
				}

				var eJungleList = GameObjects.Jungle.Where(j => E.CanCast(j)).ToList();
				var eJungle = E.GetLineFarmLocation(eJungleList);
				if (eJungle.MinionsHit >= eNum || eJungleList.Any(j=> j.GetJungleType() == JungleType.Large || j.GetJungleType() == JungleType.Legendary))
				{
					E.Cast(eJungle.Position);
				}
			}
		}

		private void CastW() {
			foreach (var allyHero in GameObjects.AllyHeroes.Where(h =>!h.IsMe && !h.IsDead && !h.IsZombie && h.IsValid))
			{
				if (Config["技能设置"]["W名单"][allyHero.Name].GetValue<MenuBool>().Value
					&& allyHero.Distance(Player) <= W.Range + 200
					&& (Qedtarget == null || allyHero.Distance(Qedtarget)>500)
					&& (allyHero.Distance(Player) > 500 || allyHero.HasWall(Player)))
				{
					
					if ((allyHero.IsRooted || allyHero.IsStunned || allyHero.HasBuffOfType(BuffType.Slow)
							|| allyHero.HasBuffOfType(BuffType.Fear) || allyHero.HasBuffOfType(BuffType.Taunt)
							|| allyHero.HealthPercent<20)
						&& allyHero.CountEnemyHeroesInRangeEx(500) > allyHero.CountAllyHeroesInRange(500))
					{
						W.Cast(allyHero);
					}
				}
			}
		}

		private void QFlash()
		{
			FlashQ();
		}

		private void FlashQ()
		{
			Variables.Orbwalker.Move(Game.CursorPos);

			Obj_AI_Hero target = null;
			if (Variables.TargetSelector.Selected.Target != null
					&& Variables.TargetSelector.Selected.Target.IsValidTarget(Q.Range + Flash.Range)
					&& !Variables.TargetSelector.Selected.Target.IsDead
					&& !Variables.TargetSelector.Selected.Target.IsZombie
					&& !Variables.TargetSelector.Selected.Target.HasSpellShield())
			{
				target = Variables.TargetSelector.Selected.Target;
			}
			else
			{
				target = Variables.TargetSelector.GetTarget(Q.Range + Flash.Range,DamageType.Physical);
			}

			if (target!=null)
			{
				if (Q.IsInRange(target))
				{
					if (Variables.TargetSelector.Selected.Target != null
						&& Variables.TargetSelector.Selected.Target.IsValidTarget(Q.Range)
						&& !Variables.TargetSelector.Selected.Target.IsDead
						&& !Variables.TargetSelector.Selected.Target.IsZombie
						&& !Variables.TargetSelector.Selected.Target.HasSpellShield())
					{
						Q.Cast(Variables.TargetSelector.Selected.Target);
					}
					else
					{
						Q.CastOnBestTarget();
					}
				}
				else
				{
					var extendPosition = Player.Position.Extend(target.Position, Flash.Range);
					if (extendPosition.IsWall())
					{
						return;
					}
					Q.UpdateSourcePosition(extendPosition, extendPosition);
					if (Config["智能施法"]["智能F方式"].GetValue<MenuList>().Index == 0)
					{
						var prediction = Q.GetPrediction(target);
						if (prediction.Hitchance >= HitChance.VeryHigh)
						{
							if (Q.Cast(prediction.CastPosition))
							{
								var t = Player.Distance(prediction.CastPosition)/Q.Speed - Game.Ping/2f;
								Game.PrintChat("QF:"+t);
								DelayAction.Add(300, () =>
								{
									Flash.Cast(target);
								});
							}
						}
					}
					else
					{
						var prediction = Q.GetPrediction(target);
						if (prediction.Hitchance >= HitChance.VeryHigh)
						{
							Flash.Cast(extendPosition);
							Q.Cast(prediction.CastPosition);
						}
					}
					Q.UpdateSourcePosition(Player.Position, Player.Position);
				}
			}
		}

		private bool CastQ1(Obj_AI_Hero target) {
			if (target != null && target.IsValidTarget(Q.Range) && !target.HasSpellShield() 
				&& GetQState() == CastState.First &&
				 (target.Distance(Player)> Player.GetRealAutoAttackRange()
				 || target.MoveSpeed > Player.MoveSpeed && target.Distance(Player) > Player.GetRealAutoAttackRange() - 60
				 || Player.HasWall(target))
				 || Player.HealthPercent < 20f)
			{
				return Q.Cast(target) == CastStates.SuccessfullyCasted;
			}
			return false;
		}

		private bool IsAllyDashWithW()
		{
			//threesisters
			return GameObjects.AllyHeroes.Any(a => a.HasBuff("InitializeShieldMarker") && a.GetBuff("InitializeShieldMarker").Caster.IsMe);
		}

		private bool CastQ2() {

			//Game.PrintChat("Qedtarget.CountEnemyHeroesInRangeEx()"+ Qedtarget.CountEnemyHeroesInRangeEx());
			//Game.PrintChat("Player.Position.CountAllyHeroesInRange(1000)"+ Player.Position.CountAllyHeroesInRange(1000));
			if (Qedtarget != null && Qedtarget.IsValid
				&& (Qedtarget.IsUnderEnemyTurret() == null && !Qedtarget.InBase() || !Config["防御塔设置"]["Q不进敌塔"].GetValue<MenuBool>())
				&& Qedtarget.CountEnemyHeroesInRangeEx() <= Player.Position.CountAllyHeroesInRange(1000))
			{
				if (Qedtarget is Obj_AI_Hero && IsAllyDashWithW())
				{
					return Q.Cast();
				}

				if (Qedtarget is Obj_AI_Hero && Qedtarget.GetBuffLaveTime(QBuffName) < 0.4)
				{
					return Q.Cast();
				}
				if (Qedtarget is Obj_AI_Minion
					&& Qedtarget.CountEnemyHeroesInRangeEx(E.Range) > 0
					&& Qedtarget.CountAllyHeroesInRange(1000) >= Qedtarget.CountEnemyHeroesInRangeEx(1000) + 2
					&& Player.CountAllyHeroesInRange(W.Range) >= 0
					)
				{
					return Q.Cast();
				}
			}
			return false;
		}

		private void CastE(Obj_AI_Hero target,bool limtRange = false) {
			if (target ==null || !E.CanCast(target) || target.HasSpellShield() || Qedtarget!=null || GetQState() == CastState.Second) return;

			var tower = target.GetAllyTurret();
			if (tower != null && tower.IsAlly && target.Distance(tower) < tower.CastRange + E.Range)
			{
				if (target.Position.Extend(Player.ServerPosition,  E.Range / 4).IsUnderAllyTurret() && Qedtarget == null && E.CastReverse(target))
				{
					return;
				}
				if (target.Position.Extend(Player.ServerPosition, -E.Range / 4).IsUnderAllyTurret() && Qedtarget == null && E.Cast(target) == CastStates.SuccessfullyCasted)
				{
					return;
				}
			}

			//Game.PrintChat("结果"+ (Player.Position.CountAllyHeroesInRange(1000) >= Player.CountEnemyHeroesInRangeEx()));
			if (Player.Position.CountAllyHeroesInRange(1000)>= Player.CountEnemyHeroesInRangeEx())
			{
				if (E.CastReverse(target))
				{
					return;
				}
			}
			else if (GetCarry()!=null)
			{
				var caryy = GetCarry();
				//拉以后位置
				var pull = target.Position.Extend(Player.Position, E.Range / 4);
				//推以后位置
				var push = target.Position.Extend(Player.Position, - E.Range / 4);

				if (target.HealthPercent < caryy.HealthPercent - 15)
				{
					if (target.Distance(caryy) < caryy.Distance(pull))
					{
						E.CastReverse(target);
					}
					else
					{
						E.Cast(target);
					}
				}
				else
				{
					if (target.Distance(caryy) < caryy.Distance(push))
					{
						E.CastReverse(target);
					}
					else
					{
						E.Cast(target);
					}
				}

				//if (target.HealthPercent < caryy.HealthPercent
					
				//	&& E.CastReverse(target))
				//{
				//	return;
				//}
				//if ((target.HealthPercent > caryy.HealthPercent || target.IsZombie) && E.Cast(target)== CastStates.SuccessfullyCasted)
				//{
				//	return;
				//}
			}
			else
			{
				if ((target.HealthPercent > Player.HealthPercent || target.IsZombie) 
					&& (!limtRange || target.Distance(Player)>E.Range/2) 
					&& E.Cast(target) == CastStates.SuccessfullyCasted)
				{
					return;
				}
				else if(target.HealthPercent < Player.HealthPercent
					&& (!limtRange || target.Distance(Player) > E.Range / 2)
					&& E.CastReverse(target))
				{
					return;
				}
			}
			

		}

		private void CatchToTower()
		{
			var tower = Player.GetAllyTurret();
			if (tower!=null && !tower.IsDead && (tower.Target == null || tower.Target.IsDead || tower.Target.Type == GameObjectType.obj_AI_Minion))
			{
				var targets = Variables.TargetSelector.GetTargets(Q.Range);
				var eTarget = targets.FirstOrDefault(t => E.CanCast(t) && t.Distance(tower) < tower.CastRange + E.Range);
				var qTarget = targets.FirstOrDefault(t => Q.CanCast(t) && t.Distance(tower) < tower.CastRange + Q.Range / 2);

				if (eTarget!=null && (tower.Target?.Type != GameObjectType.obj_AI_Minion || ((Obj_AI_Hero)tower.Target).HasProcessDamage()))
				{
					if (eTarget.Position.Extend(Player.ServerPosition,  E.Range / 4).IsUnderAllyTurret() && Qedtarget==null && E.CastReverse(eTarget))
					{
						return;
					}
					if (eTarget.Position.Extend(Player.ServerPosition, - E.Range / 4).IsUnderAllyTurret() && Qedtarget == null && E.Cast(eTarget) == CastStates.SuccessfullyCasted)
					{
						return;
					}
				}
				else if (qTarget != null && (tower.Target?.Type != GameObjectType.obj_AI_Minion || ((Obj_AI_Hero)tower.Target).HasProcessDamage())
					&& Player.IsUnderAllyTurret()!=null && Q.CanCast(qTarget) && CastQ1(qTarget))
				{
					return;
				}
				
			}
		}
		public int GetDefaultPriority(Obj_AI_Hero hero) {
			var priorityCategories = Data.Get<ChampionPriorityData>().PriorityCategories;
			return priorityCategories.FirstOrDefault(i => i.Champions.Contains(hero.ChampionName))?.Value ?? 1;
		}

		private Obj_AI_Hero GetCarry(float range = 1075) {
			var max = GameObjects.AllyHeroes
				.Where(h => h.IsValid && !h.IsMe && !h.IsZombie && !h.IsDead && h.Distance(Player)<range)
				.MaxOrDefault(GetDefaultPriority);
			return max;

			var list = GameObjects.AllyHeroes.Where(a => !a.IsMe && a.IsValid && !a.IsDead && !a.IsZombie && a.Distance(Player) < range);
			var ad = list.OrderByDescending(d => d.TotalAttackDamage).FirstOrDefault();
			var ap = list.OrderByDescending(d => d.TotalMagicalDamage).FirstOrDefault();
			if (ap?.TotalMagicalDamage < Player.TotalMagicalDamage
				&& ad?.TotalAttackDamage > Player.TotalAttackDamage)
			{
				return ad;
			}
			if (ap?.TotalMagicalDamage > Player.TotalMagicalDamage
				&& ad?.TotalAttackDamage < Player.TotalAttackDamage)
			{
				return ap;
			}
			return ad ?? ap;
		}

		private Obj_AI_Hero GetJungle(float range = 1300)
		{
			return GameObjects.AllyHeroes
				.Where(a => !a.IsMe && !a.IsDead && !a.IsZombie && a.IsValid
				            && a.Distance(Player) < range
				            && a.Distance(Player) > W.Range/2)
				.MaxOrDefault(a => a.Distance(Player));
		}

		private HitChance GetHitChance() {
			int index = Config["预判设置"]["命中率"].GetValue<MenuList>().Index;
			return HitChances[index];
		}

		private void InitUi() {
			Game.PrintChat(
				"魂锁典狱 - 锤石".ToHtml(30)
				+ "  "
				+ "游走的孤魂野鬼啊，我其实是绿灯侠 ".ToHtml(System.Drawing.Color.Goldenrod, FontStlye.Cite));

			Config = new Menu(RootMenuName,"晴依锤石",true);

			var infoConfig = Config.Add(new Menu("信息介绍", "信息介绍"));
			infoConfig.Add(new MenuSeparator("info1", "Thresh - As the Chain Warden"));
			infoConfig.Add(new MenuSeparator("info2", "作者：晴依"));
			infoConfig.Add(new MenuSeparator("info3", "呜谢：@花边/@王桀/@寂寞"));
			infoConfig.Add(new MenuKeyBind("DebugE", "测试E距离", Keys.G,KeyBindType.Press));
			infoConfig.Add(new MenuKeyBind("DebugQ", "测试Q距离", Keys.H, KeyBindType.Press));

			var spellConfig = Config.Add(new Menu("技能设置", "技能设置"));
			spellConfig.Add(new MenuSeparator("Q技能设置", "Q技能设置"));
			var qList = spellConfig.Add(new Menu("Q名单", "Q名单"));
			foreach (var enemy in GameObjects.EnemyHeroes)
			{
				qList.Add(new MenuBool(enemy.Name, $"{enemy.ChampionName}（{enemy.Name.ToGBK()}）", true));
			}
			spellConfig.Add(new MenuBool("不自动Q2", "不自动Q2"));
			spellConfig.Add(new MenuSlider("不Q2当敌人数多", "当敌人个数>队友?个时不自动Q2",1,0,5));

			spellConfig.Add(new MenuSeparator("W技能设置", "W技能设置"));
			var wList = spellConfig.Add(new Menu("W名单", "W名单"));
			foreach (var ally in GameObjects.AllyHeroes)
			{
				wList.Add(new MenuBool(ally.Name, $"{ally.ChampionName}（{ally.Name.ToGBK()}）", true));
			}

			spellConfig.Add(new MenuSeparator("E技能设置", "E技能设置"));
			spellConfig.Add(new MenuSlider("清兵E", "E清兵个数(为0不清)",3,0,10));

			spellConfig.Add(new MenuSeparator("R技能设置", "R技能设置"));
			spellConfig.Add(new MenuSlider("大招人数", "大招人数",2,1,5));
			spellConfig.Add(new MenuList<string>("大招模式", "大招人数计算", new[] { "预判", "实时"}));

			var smartCastConfig = Config.Add(new Menu("智能施法", "智能施法"));
			smartCastConfig.Add(new MenuKeyBind("智能Q", "智能 Q", Keys.Q | Keys.Control, KeyBindType.Press));
			smartCastConfig.Add(new MenuKeyBind("智能W", "智能 W", Keys.W | Keys.Control, KeyBindType.Press));
			smartCastConfig.Add(new MenuKeyBind("智能E", "智能 E", Keys.E | Keys.Control, KeyBindType.Press));
			if (Flash!=null)
			{
				smartCastConfig.Add(new MenuKeyBind("智能F", "智能 闪现", FlashKey | Keys.Control, KeyBindType.Press));
				smartCastConfig.Add(new MenuList<string>("智能F方式", "智能闪现方式",new []{"Q闪","闪Q"} ));
			}
			
			var towerConfig = Config.Add(new Menu("防御塔设置", "防御塔设置"));
			towerConfig.Add(new MenuBool("控制塔攻击的敌人", "Q/E 塔攻击的敌人",true));
			towerConfig.Add(new MenuBool("拉敌人进塔", "Q/E 敌人进塔", true));
			towerConfig.Add(new MenuBool("Q不进敌塔", "不Q2进敌塔", true));

			var supportConfig = Config.Add(new Menu("辅助模式设置", "辅助模式设置"));
			supportConfig.Add(new MenuBool("辅助模式开关", "辅助模式", true));
			supportConfig.Add(new MenuSlider("辅助模式距离", "辅助模式距离", (int)Player.AttackRange + 200, (int)Player.AttackRange, 2000));

			var miscConfig = Config.Add(new Menu("其它设置", "其它设置"));
			miscConfig.Add(new MenuBool("出门黄眼", "出门买黄眼", true));
			miscConfig.Add(new MenuBool("眼石后买扫描", "购买眼石后买扫描",true));
			miscConfig.Add(new MenuBool("助攻嘲讽", "击杀或助攻后亮图标大笑"));

			var drawConfig = Config.Add(new Menu("显示设置", "显示设置"));
			drawConfig.Add(new MenuBool("技能可用才显示", "技能可用才显示",true));
			drawConfig.Add(new MenuBool("显示Q", "显示 Q 范围", true));
			drawConfig.Add(new MenuColor("Q颜色", "显示 Q 颜色", Color.YellowGreen));

			drawConfig.Add(new MenuBool("显示W", "显示 W 范围", true));
			drawConfig.Add(new MenuColor("W颜色", "显示 W 颜色", Color.YellowGreen));

			drawConfig.Add(new MenuBool("显示E", "显示 E 范围", true));
			drawConfig.Add(new MenuColor("E颜色", "显示 E 颜色", Color.YellowGreen));

			drawConfig.Add(new MenuBool("显示R", "显示 R 范围", true));
			drawConfig.Add(new MenuColor("R颜色", "显示 R 颜色", Color.YellowGreen));

			AutoLevelHelper.Initialize(Config);

			Config.Attach();
		}

		private void InitSpell() {
			Q = new Spell(SpellSlot.Q, 1055);
			W = new Spell(SpellSlot.W, 950);
			E = new Spell(SpellSlot.E, 450);
			R = new Spell(SpellSlot.R, 430);

			Q.SetSkillshot(0.5f, 80, 1900f, true, SkillshotType.SkillshotLine);
			W.SetSkillshot(0.2f, 10, float.MaxValue, false, SkillshotType.SkillshotCircle);
			E.SetSkillshot(0.25f, 100, float.MaxValue, false, SkillshotType.SkillshotLine);

			var flashSlot = Player.GetSpellSlot("SummonerFlash");
			if (flashSlot == SpellSlot.Summoner1)
			{
				Flash = new Spell(flashSlot, 425);
				FlashKey = Keys.D;
			}
			else if (flashSlot == SpellSlot.Summoner2)
			{
				Flash = new Spell(flashSlot, 425);
				FlashKey = Keys.F;
			}
			
		}

		enum CastState {
			NotReady,
			First,
			Second
		}

		private CastState GetQState()
		{
			if (!Q.IsReady())
			{
				return CastState.NotReady;
			}
			if (Q.Instance.Name == "ThreshQ")
			{
				return CastState.First;
			}
			if (Q.Instance.Name == "threshqleap")
			{
				return CastState.Second;
			}
			return CastState.NotReady;
		}
	}
}