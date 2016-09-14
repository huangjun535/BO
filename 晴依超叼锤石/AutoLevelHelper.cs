using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.UI;
using LeagueSharp.SDK.Utils;

namespace _SDK_Thresh___As_the_Chain_Warden {
	public static class AutoLevelHelper
	{

		private static Obj_AI_Hero Player => GameObjects.Player;
		private static Menu Config { get; set; }
		private static int Lvl1 { get; set; }
		private static int Lvl2 { get; set; }
		private static int Lvl3 { get; set; }
		private static int Lvl4 { get; set; }
		private static int LvlStart { get; set; }

		public static void Initialize(Menu menu)
		{
			var autoLevelConfig = menu.Add(new Menu("自动加点设置", "自动加点设置"));
			autoLevelConfig.Add(new MenuBool("自动加点", "自动加点", true));
			autoLevelConfig.Add(new MenuBool("只自动升级大", "只自动升级大"));
			autoLevelConfig.Add(new MenuSlider("加点延迟", "加点延迟", 700, 200, 2000));
			autoLevelConfig.Add(new MenuList<string>(Player.ChampionName + "最主", "最重要:", new[] { "R", "Q", "W", "E" })).ValueChanged +=
				(s, e) =>
				{
					Lvl1 = autoLevelConfig[Player.ChampionName + "最主"].GetValue<MenuSlider>().Value;
				} ;
			autoLevelConfig.Add(new MenuList<string>(Player.ChampionName + "主升", "主升:", new[] { "R", "Q", "W", "E" })).ValueChanged +=
				(s, e) =>
				{
					Lvl2 = autoLevelConfig[Player.ChampionName + "主升"].GetValue<MenuSlider>().Value;
				};
			autoLevelConfig.Add(new MenuList<string>(Player.ChampionName + "副升", "副升:", new[] { "R", "Q", "W", "E" })).ValueChanged +=
				(s, e) =>
				{
					Lvl3 = autoLevelConfig[Player.ChampionName + "副升"].GetValue<MenuSlider>().Value;
				};
			autoLevelConfig.Add(new MenuList<string>(Player.ChampionName + "最后", "最后升:", new[] { "R", "Q", "W", "E" })).ValueChanged +=
				(s, e) =>
				{
					Lvl4 = autoLevelConfig[Player.ChampionName + "最后"].GetValue<MenuSlider>().Value;
				};
			autoLevelConfig.Add(new MenuSlider(Player.ChampionName + "启用等级", "启用等级", 3, 1, 5)).ValueChanged +=
				(s, e) =>
				{
					LvlStart = autoLevelConfig[Player.ChampionName + "启用等级"].GetValue<MenuSlider>().Value;
				};

			Config = autoLevelConfig;
			Obj_AI_Base.OnLevelUp += Obj_AI_Base_OnLevelUp;
		}

		private static bool levelNotLearned()
		{
			if (Player.Level >= LvlStart)
			{
				if (Player.Spellbook.GetSpell(SpellSlot.Q).State == SpellState.NotLearned)
				{
					Player.Spellbook.LevelSpell(SpellSlot.Q);
					return true;
				}
				else if (Player.Spellbook.GetSpell(SpellSlot.W).State == SpellState.NotLearned)
				{
					Player.Spellbook.LevelSpell(SpellSlot.W);
					return true;
				}
				else if (Player.Spellbook.GetSpell(SpellSlot.E).State == SpellState.NotLearned)
				{
					Player.Spellbook.LevelSpell(SpellSlot.E);
					return true;
				}
				else if (Player.Spellbook.GetSpell(SpellSlot.R).State == SpellState.NotLearned)
				{
					Player.Spellbook.LevelSpell(SpellSlot.R);
					return true;
				}
			}
			return false;
		}

		private static void LevelUp(int index)
		{
			switch (index)
			{
				case 0:
					Player.Spellbook.LevelSpell(SpellSlot.R);
					break;
				case 1:
					Player.Spellbook.LevelSpell(SpellSlot.Q);
					break;
				case 2:
					Player.Spellbook.LevelSpell(SpellSlot.W);
					break;
				case 3:
					Player.Spellbook.LevelSpell(SpellSlot.E);
					break;
			}
		}

		private static void Obj_AI_Base_OnLevelUp(Obj_AI_Base sender, EventArgs args) {
			if (!sender.IsMe 
				|| !Config["自动加点"].GetValue<MenuBool>().Value
				|| GameObjects.Player.Level < LvlStart
				|| Lvl1 == Lvl2 || Lvl1 == Lvl3 || Lvl1 == Lvl4
				|| Lvl2 == Lvl3 || Lvl2 == Lvl4
				|| Lvl3 == Lvl4)
			{
				return;
			}

			if (levelNotLearned())
			{
				return;
			}

			var delay = Config["加点延迟"].GetValue<MenuSlider>().Value;
			DelayAction.Add(delay, () =>
			{
				LevelUp(Lvl1);
			});
			DelayAction.Add(delay + 20, () =>
			{
				LevelUp(Lvl2);
			});
			DelayAction.Add(delay + 40, () =>
			{
				LevelUp(Lvl3);
			});
			DelayAction.Add(delay + 60, () =>
			{
				LevelUp(Lvl4);
			});
		}
	}
}
