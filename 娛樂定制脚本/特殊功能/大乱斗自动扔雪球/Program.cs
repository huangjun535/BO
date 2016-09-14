using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;

namespace Mark_As_Dash {
	class Program {

		public static Obj_AI_Hero Player = ObjectManager.Player;
		public static Obj_AI_Base MarkTarget;
        public static Menu Config;
		public static Spell SnowBall;
		public static Font font;
		public static List<Obj_AI_Hero> ignoreList = new List<Obj_AI_Hero>();
		public static List<TargetSelector.DamageType> DamageTypeList = new List<TargetSelector.DamageType>{ TargetSelector.DamageType .Magical, TargetSelector.DamageType .Physical, TargetSelector.DamageType.True};
		public static ColorBGRA FontColor = new ColorBGRA(Color.YellowGreen.B, Color.YellowGreen.G, Color.YellowGreen.R, Color.YellowGreen.A);

		static void Main(string[] args) {
			CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
		}

		private static void Game_OnGameLoad(EventArgs args) {
			if (!LoadSpell())
			{
				return;
			}
			font = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "微软雅黑", Height = 28 });

			LoadMenu();

			Game.OnUpdate += Game_OnUpdate;
			Drawing.OnDraw += Drawing_OnDraw;
		}

		private static void Drawing_OnDraw(EventArgs args) {
			//font.DrawTextCentered("标记中目标：" + Player.ChampionName + "(" + Player.Name.ToGBK() + ")", Drawing.WorldToScreen(Player.Position), FontColor);

			if (Config.Item(Player.ChampionName + "DamageType").GetValue<StringList>().SelectedIndex == 0)
			{
				font.DrawTextCentered("请先设置你的伤害类型", Drawing.WorldToScreen(Player.Position), FontColor);
				return;
			}

			if (MarkTarget!=null)
			{
				if (MarkTarget.Type == GameObjectType.obj_AI_Hero)
				{
					var target = MarkTarget as Obj_AI_Hero;
                    font.DrawTextCentered("标记中目标：" + target.ChampionName+"("+target.Name.ToUTF8()+")", Drawing.WorldToScreen(Player.Position), FontColor);
				}
				else if(MarkTarget.Type == GameObjectType.obj_AI_Minion)
				{
					var enemies = MarkTarget.GetEnemiesInRange(500);
                    if (enemies.Count<=0)
					{
						font.DrawTextCentered("标记中小兵附近没有敌人", Drawing.WorldToScreen(Player.Position), FontColor);
					}
					else
					{
						if (enemies.Count <=2 && enemies.Any(e => e.HealthPercent<20))
						{
							font.DrawTextCentered("标记中小兵500码内有一个残血敌人", Drawing.WorldToScreen(Player.Position), FontColor);
						}
						else
						{
							font.DrawTextCentered("标记中小兵500码内有"+ enemies.Count+"个敌人", Drawing.WorldToScreen(Player.Position), FontColor);
						}
					}
					
				}

			}
			var DrawRangeShow = Config.Item("DrawRange").GetValue<Circle>();
            if (DrawRangeShow.Active && GetSnowBallState() == SnowBallState.Mark)
			{
				//Render.Circle.DrawCircle(Player.Position,SnowBall.Range,DrawRangeShow.Color,2);
				Utility.DrawCircle(Player.Position, SnowBall.Range, DrawRangeShow.Color, 2, 23, true);
			}
		}

		private static void Game_OnUpdate(EventArgs args) {
			//if (Config.Item("SmitCast").GetValue<KeyBind>().Active)
			//{
			//	Console.WriteLine(Utill.GetInfo());
			//}
			

			if (Config.Item(Player.ChampionName + "DamageType").GetValue<StringList>().SelectedIndex == 0) return;

			#region 取雪球标记目标
			if (GetSnowBallState() == SnowBallState.Dash)
			{
				foreach (var enemy in ObjectManager.Get<Obj_AI_Base>().Where(o => o.IsEnemy && o.IsValid && !o.IsDead && o.Distance(Player) <= SnowBall.Range + 400 && (o.Type == GameObjectType.obj_AI_Minion || o.Type == GameObjectType.obj_AI_Hero)))
				{
					if (enemy.HasBuff("snowballfollowupself")
						|| enemy.HasBuff("porothrowfollowup"))
					{
						MarkTarget = enemy;
						break;
					}
					else
					{
						MarkTarget = null;
					}
				}
			}
			else
			{
				MarkTarget = null;
			}
			#endregion

			#region 连招时 标记目标 或 打伤害
		
			if (Config.Item("Combo").GetValue<KeyBind>().Active)
			{
				if (GetSnowBallState() == SnowBallState.Mark)
				{
					var Target = TargetSelector.GetTarget(SnowBall.Range, DamageTypeList[Config.Item(Player.ChampionName + "DamageType").GetValue<StringList>().SelectedIndex], false, ignoreList);
					//Game.PrintChat("Combo SnowBall");
					CastMark(Target);
                }
				
			}
			#endregion

			#region 抢人头
			if (Config.Item("KS").GetValue<bool>())
			{
				if (GetSnowBallState() == SnowBallState.Mark)
				{
					foreach (var enemy in HeroManager.Enemies.Where(o => o.IsValid && !o.IsDead && o.Distance(Player) <= SnowBall.Range - 10))
					{
						if (enemy.Health < GetSnowBallDmg())
						{
							//Game.PrintChat("enemy.Health:" + enemy.Health + "  SnowBallDmg:" + GetSnowBallDmg());
							//Game.PrintChat("KS1 SnowBall");
							CastMark(enemy);
						}
					}
				}
				if (GetSnowBallState() == SnowBallState.Dash
					&& MarkTarget!=null && MarkTarget.Type == GameObjectType.obj_AI_Hero
					&& MarkTarget.CountEnemiesInRange(700) < Player.CountEnemiesInRange(700))
				{
					var enemy = MarkTarget as Obj_AI_Hero;
					if (enemy.Health <= GetSnowBallDmg())
					{
						//Game.PrintChat("KS2 SnowBall");
						SnowBall.Cast();
					}
				}
			}
			

			#endregion

			#region 半手动
			if ((Config.Item("SmitCast").GetValue<KeyBind>().Active 
				|| Config.Item("Toggle").GetValue<KeyBind>().Active) 
				&& GetSnowBallState() == SnowBallState.Mark)
			{
                var Target = TargetSelector.GetTarget(SnowBall.Range, DamageTypeList[Config.Item(Player.ChampionName + "DamageType").GetValue<StringList>().SelectedIndex], false, ignoreList);
				//Game.PrintChat("SmitCast SnowBall");
				//CastMark(Target,HitChance.High);
				CastMark(Target);
			}

			#endregion
		}

		private static bool CastMark(Obj_AI_Hero target, HitChance hitChance = HitChance.VeryHigh) {
			var MarkPrediction = SnowBall.GetPrediction(target);
			if (MarkPrediction.Hitchance >= hitChance)
			{
                return SnowBall.Cast(MarkPrediction.CastPosition);
			}
			return false;
		}

		private static double GetSnowBallDmg() {
			if (!SnowBall.IsReady())
			{
				return 0;
			}
			else 
			{
				if ((int)Game.Type == 6)
				{
					return 20 + (10 * Player.Level);
				}
				if (Game.Type == GameType.ARAM)
				{
					return 10 + (5 * Player.Level);
				}
				return 0;					
			}
			
        }

		private enum SnowBallState {
			Cooldown,
			Mark,
			Dash
		}

		private static SnowBallState GetSnowBallState() {
			if (!SnowBall.IsReady())
			{
				return SnowBallState.Cooldown;
			}
			else
			{
				if ("summonersnowball" == SnowBall.Instance.Name || "summonerporothrow" == SnowBall.Instance.Name)
				{
					return SnowBallState.Mark;
				}
				else
				{
					return SnowBallState.Dash;
				}
				
			}
		}

		private static bool LoadSpell() {

			var slotARAM = ObjectManager.Player.GetSpellSlot("summonersnowball");
			var slotPORO = ObjectManager.Player.GetSpellSlot("summonerporothrow");
			if (slotARAM != SpellSlot.Unknown)
			{
				SnowBall = new Spell(slotARAM, 1450);
			}
			else if (slotPORO != SpellSlot.Unknown)
			{
				SnowBall = new Spell(slotPORO, 2450);
			}
			else
			{
				return false;
			}
			SnowBall.SetSkillshot(0.33f, 50f, 1600, true, SkillshotType.SkillshotLine);
			return true;

			//var slot1 = Player.Spellbook.GetSpell(SpellSlot.Summoner1);
			//var slot2 = Player.Spellbook.GetSpell(SpellSlot.Summoner2);

			//Console.WriteLine(slot1.Name + " - " + slot2.Name);
			//Console.WriteLine(Game.Type.ToString());

			//if (Game.Type == GameType.ARAM)
			//{
			//	if (slot1.Name.Contains("summonersnowball"))
			//	{
			//		SnowBall = new Spell(slot1.Slot, 1500);

			//		return true;
			//	}
			//	else if (slot2.Name.Contains("summonersnowball"))
			//	{
			//		SnowBall = new Spell(slot2.Slot, 1500);
			//		return true;
			//	}
			//}
			//else if ((int)Game.Type == 6)
			//{
			//	if (slot1.Name.Contains("summonerporothrow"))
			//	{
			//		SnowBall = new Spell(slot1.Slot, 2450);
			//		return true;
			//	}

			//	else if (slot2.Name.Contains("summonerporothrow"))
			//	{
			//		SnowBall = new Spell(slot2.Slot, 2450);
			//		return true;
			//	}
			//}
			//return false;
		}

		private static void LoadMenu() {
			Config = new Menu("大乱斗扔雪球", "AsMarkDash", true);
			Config.AddToMainMenu();
			var ListMenu = Config.AddSubMenu(new Menu("砸雪球名单", "List"));
			foreach (var enemy in HeroManager.Enemies)
			{
				ListMenu.AddItem(new MenuItem("List" + enemy.NetworkId, enemy.ChampionName + "(" + enemy.Name.ToGBK() + ")").SetValue(true)).ValueChanged += Program_ValueChanged; ;
			}

			var PredictConfig = Config.AddSubMenu(new Menu("Predict Settings", "预判设置"));
			PredictConfig.AddItem(new MenuItem("预判模式", "Prediction Mode").SetValue(new StringList(new[] { "Common", "OKTW" }, 1)));
			PredictConfig.AddItem(new MenuItem("命中率", "HitChance").SetValue(new StringList(new[] { "Very High", "High", "Medium" })));

			Config.AddItem(new MenuItem("Combo", "连招时打伤害").SetValue(new KeyBind(32, KeyBindType.Press)));
			Config.AddItem(new MenuItem(Player.ChampionName + "DamageType", "自己主要的伤害类型").SetValue(
				new StringList(new[] { "未设置", "物理", "法术", "真实伤害" })));
			
			Config.AddItem(new MenuItem("KS", "抢人头").SetValue(true));
			Config.AddItem(new MenuItem("SmitCast", "半手动施放").SetValue(new KeyBind('G', KeyBindType.Press)));
			Config.AddItem(new MenuItem("Toggle", "一直使用").SetValue(new KeyBind('O', KeyBindType.Toggle)));
			Config.AddItem(new MenuItem("DrawRange","显示范围").SetValue(new Circle()));

			var MultiLanguageConfig = Config.AddSubMenu(new Menu("MultiLanguage Settings", "语言选择"));
			MultiLanguageConfig.AddItem(new MenuItem("选择语言", "Selecte Language").SetValue(new StringList(new[] { "English", "中文" }))).ValueChanged += MultiLanguage_ValueChanged;

			//ChangeLanguage(MultiLanguageConfig.Item("选择语言").GetValue<StringList>().SelectedIndex);
			ChangeLanguage(1);
        }
		private static void ChangeLanguage(int SelectedIndex) {
			List<Dictionary<string, string>> Languages = new List<Dictionary<string, string>> {
				MultiLanguage.English,
				MultiLanguage.Chinese
			};
			var Language = Languages[SelectedIndex];

			List<object> menus = GetSubMenus(Config);

			foreach (var item in menus)
			{
				if (item is Menu)
				{
					var m = item as Menu;
					var DisplayName = Language.Find(l => l.Key == m.Name).Value;
					if (!string.IsNullOrEmpty(DisplayName))
					{
						m.DisplayName = DisplayName;
					}
				}
				else
				{
					var m = item as MenuItem;
					var DisplayName = Language.Find(l => l.Key == m.Name).Value;
					if (!string.IsNullOrEmpty(DisplayName))
					{
						m.DisplayName = DisplayName;
					}
				}
			}
		}

		private static List<object> GetSubMenus(Menu menu) {
			List<object> AllMenus = new List<object>();
			AllMenus.Add(menu);
			foreach (var item in menu.Items)
			{
				AllMenus.Add(item);
			}
			foreach (var item in menu.Children)
			{
				AllMenus.AddRange(GetSubMenus(item));
			}
			return AllMenus;
		}

		private static void MultiLanguage_ValueChanged(object sender, OnValueChangeEventArgs e) {
			ChangeLanguage(e.GetNewValue<StringList>().SelectedIndex);
		}

		public static bool CastMark(Obj_AI_Hero target) {
			var hitChangceIndex = Config.Item("命中率").GetValue<StringList>().SelectedIndex;

			if (Config.Item("预判模式").GetValue<StringList>().SelectedIndex == 0)
			{
				var hitChangceList = new[] { HitChance.VeryHigh, HitChance.High, HitChance.Medium };
				return SnowBall.CastIfHitchanceEquals(target, hitChangceList[hitChangceIndex]);
			}
			else if (Config.Item("预判模式").GetValue<StringList>().SelectedIndex == 1)
			{
				var hitChangceList = new[] { OKTWPrediction.HitChance.VeryHigh, OKTWPrediction.HitChance.High, OKTWPrediction.HitChance.Medium };
				return CastOKTW(target, hitChangceList[hitChangceIndex]);
			}
			return false;
		}

		public static bool CastOKTW(Obj_AI_Hero target, OKTWPrediction.HitChance hitChance) {
			var spell = SnowBall;

			OKTWPrediction.SkillshotType CoreType2 = OKTWPrediction.SkillshotType.SkillshotLine;
			bool aoe2 = false;

			var predInput2 = new OKTWPrediction.PredictionInput
			{
				Aoe = aoe2,
				Collision = spell.Collision,
				Speed = spell.Speed,
				Delay = spell.Delay,
				Range = spell.Range,
				From = Player.ServerPosition,
				Radius = spell.Width,
				Unit = target,
				Type = CoreType2
			};
			var poutput2 = OKTWPrediction.Prediction.GetPrediction(predInput2);
			if (poutput2.Hitchance >= hitChance)
			{
				return spell.Cast(poutput2.CastPosition);
			}
			return false;
		}

		private static void Program_ValueChanged(object sender, OnValueChangeEventArgs e) {
		
			var menuItem = sender as MenuItem;
			foreach (var enemy in HeroManager.Enemies)
			{
				if (menuItem.Name == "List" + enemy.NetworkId)
				{
					if (e.GetNewValue<bool>())
					{
						ignoreList.Add(enemy);
					}
					else
					{
						ignoreList.Remove(enemy);
					}
					
					break;
				}
			}
			Console.WriteLine("ignoreList.Count:"+ ignoreList.Count);
		}
	}
}
