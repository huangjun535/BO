using LeagueSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mark_As_Dash {
	public class MultiLanguage {
		public static Obj_AI_Hero Player = ObjectManager.Player;

		public static Dictionary<string, string> Chinese { get; private set; } = new Dictionary<string, string> {

			{ "AsMarkDash","晴依扔雪球"},
			{"List","砸雪球名单" },
			{ "Combo", "连招时打伤害"},
			{ Player.ChampionName + "DamageType","自己主要的伤害类型"},
			{ "预判设置", "预判设置"},
			{ "预判模式", "预判模式"},
			{ "命中率", "命中率"},
			{ "KS", "抢人头"},
			{ "SmitCast", "半手动施放"},
			{ "Toggle", "一直使用"},
			{ "DrawRange","显示范围"},
			{ "语言选择", "MultiLanguage Settings"},
			{ "选择语言", "Selecte Language"},
			{ "标识目标", "标识目标"},
			{ "调试","调试"}

		};

		public static Dictionary<string, string> English { get; private set; } = new Dictionary<string, string> {

			{ "AsMarkDash","晴依扔雪球"},
			{"List","砸雪球名单" },
			{ "Combo", "连招时打伤害"},
			{ Player.ChampionName + "DamageType","Selecte your damage type"},
			{ "预判设置", "Predict Settings"},
			{ "预判模式", "Prediction Mode"},
			{ "命中率", "HitChance"},
			{ "KS", "ks"},
			{ "SmitCast", "Cast"},
			{ "Toggle", "Always Cast"},
			{ "DrawRange","Draw Range"},

			{ "语言选择", "多语言设置"},
			{ "选择语言", "选择语言"},
			{ "标识目标", "Draw Target"},
			{ "调试","Debug Mod"}
		};

	}
}
