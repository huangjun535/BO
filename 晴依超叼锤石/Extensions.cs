using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Enumerations;
using LeagueSharp.SDK.UI;
using LeagueSharp.SDK.Utils;
using SharpDX;
using Menu = LeagueSharp.SDK.UI.Menu;
using Color=  System.Drawing.Color;

namespace _SDK_Thresh___As_the_Chain_Warden {
	public static class Extensions {
		public static bool HasProcessDamage(this Obj_AI_Hero target) {
			if (target.InventoryItems.Any(item => item.Id == ItemId.Sunfire_Cape))
			{
				return true;
			}
			if (target.HasBuff("盖伦E") && target.GetBuffLaveTime("盖伦E") > 0.5)
			{
				return true;
			}
			if (target.HasBuff("蒙多W") && target.HasBuff("龙女E"))
			{
				return true;
			}
			return false;
		}

		public static bool IsInRange(this Obj_AI_Base from,Obj_AI_Base to,float range)
		{
			return from.Distance(to) < range;
		}

		public static bool InBase(this Obj_AI_Base hero)
		{
			return GameObjects.EnemySpawnPoints
				.Any(item => hero.Distance(item) < item.GetRealAutoAttackRange());
		}


		public static bool CastReverse(this Spell spell, Obj_AI_Base target) {
			var eCastPosition = spell.GetPrediction(target).CastPosition;
			var position = ObjectManager.Player.ServerPosition + ObjectManager.Player.ServerPosition - eCastPosition;
			return spell.Cast(position);
		}

		public static bool CastReverse(this Spell spell, Vector2 pos) {
			var position = ObjectManager.Player.ServerPosition + ObjectManager.Player.ServerPosition - pos.ToVector3();
			return spell.Cast(position);
		}

		/// <summary>
		/// 反向施放技能
		/// </summary>
		/// <param name="spell"></param>
		/// <param name="pos"></param>
		/// <returns></returns>
		public static bool CastReverse(this Spell spell, Vector3 pos) {
			var position = ObjectManager.Player.ServerPosition + ObjectManager.Player.ServerPosition - pos;
			return spell.Cast(position);
		}

		/// <summary>
		/// 是否拥有技能免疫或者无敌或者无视控制状态
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public static bool HasSpellShield(this Obj_AI_Hero target) {
			return target.HasBuffOfType(BuffType.SpellShield) 
				|| target.HasBuffOfType(BuffType.SpellImmunity)
				|| target.HasBuff("OlafRagnarok");
		}

		/// <summary>
		/// 两个单位之间是否有墙
		/// </summary>
		/// <param name="from"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static bool HasWall(this Obj_AI_Base from, Obj_AI_Base target) {
			if (GetFirstWallPoint(from.Position, target.Position) != null)
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// 单位到目标坐标之间是否有墙
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		public static bool HasWall(this Obj_AI_Base from, Vector3 to) {
			if (GetFirstWallPoint(from.Position, to) != null)
			{
				return true;
			}
			return false;
		}

		public static Vector2? GetFirstWallPoint(Vector3 from, Vector3 to, float step = 25) {
			return GetFirstWallPoint(from.ToVector2(), to.ToVector2(), step);
		}

		public static Vector2? GetFirstWallPoint(Vector2 from, Vector2 to, float step = 25) {
			var direction = (to - from).Normalized();

			for (float d = 0; d < from.Distance(to); d = d + step)
			{
				var testPoint = from + d * direction;
				var flags = NavMesh.GetCollisionFlags(testPoint.X, testPoint.Y);
				if (flags.HasFlag(CollisionFlags.Wall) || flags.HasFlag(CollisionFlags.Building))
				{
					return from + (d - step) * direction;
				}
			}
			return null;
		}

		public static float GetBuffLaveTime(this Obj_AI_Base target,string buffName) {
			return
				target.Buffs.Where(buff => buff.Name == buffName)
					.OrderByDescending(buff => buff.EndTime - Game.Time)
					.Select(buff => buff.EndTime)
					.FirstOrDefault() - Game.Time;
		}

		public static List<Obj_AI_Hero> ListAllyHeroesInRange(this Obj_AI_Base target, float range) {
			return GameObjects.AllyHeroes.Where(e => !e.IsDead && e.IsValid && e.Distance(ObjectManager.Player) < range).ToList();
		}

		public static bool IsFleeing(this Obj_AI_Hero hero, Obj_AI_Base target) {
			if (hero == null || target == null)
			{
				return false;
			}

			if (hero.Path.Count() > 0 && target.Distance(hero.Position) < target.Distance(hero.Path.Last()))
			{
				return true;
			}
			return false;
		}

		public static bool IsHunting(this Obj_AI_Hero hero, Obj_AI_Base target) {

			if (target == null)
			{
				return false;
			}
			if (target.Path.Count() > 0 && hero.Distance(target.Position) > hero.Distance(target.Path.Last()))
			{
				return true;
			}
			return false;
		}

		public static Menu AddMainMenu(string name, string display) {
			var menu = new Menu(name, display, true);
			menu.Attach();
			return menu;
		}

		public static Menu AddSubMenu(this Menu menu, string name, string display) {
			return menu.Add(new Menu(name, display));
		}

		public static MenuBool AddBool(this Menu menu, string name, string display, bool value = true) {
			return menu.Add(new MenuBool(name, display, value));
		}

		public static bool GetBool(this Menu menu, string name) {
			return menu.GetValue<MenuBool>(name).Value;
		}

		public static MenuButton AddButton(this Menu menu, string name, string display, string buttonText) {
			return menu.Add(new MenuButton(name, display, buttonText));
		}

		public static MenuColor AddColor(this Menu menu, string name, string display, ColorBGRA color) {
			return menu.Add(new MenuColor(name, display, color));
		}

		public static MenuColor GetDraw(this Menu menu, string name) {
			return menu.GetValue<MenuColor>(name);
		}

		public static System.Drawing.Color GetDrawColor(this Menu menu, string name) {
			return menu.GetValue<MenuColor>(name).Color.ToSystemColor();
		}

		public static System.Drawing.Color ToSystemColor(this Color color) {
			return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
		}

		public static bool GetDrawToggle(this Menu menu, string name) {
			return menu.GetValue<MenuColor>(name).Active;
		}

		public static MenuKeyBind AddKeyBind(this Menu menu, string name, string display, Keys key, KeyBindType bindType = KeyBindType.Press) {
			return menu.Add(new MenuKeyBind(name, display, key, bindType));
		}

		public static bool GetKeyActive(this Menu menu, string name) {
			return menu.GetValue<MenuKeyBind>(name).Active;
		}

		public static MenuList<T> AddList<T>(this Menu menu, string name, string display, IEnumerable<T> list) {
			return menu.Add(new MenuList<T>(name, display, list));
		}

		public static MenuSlider AddSlider(this Menu menu, string name, string display, int defult = 0, int min = 0,
			int max = 100) {
			return menu.Add(new MenuSlider(name, display, defult, min, max));
		}

		public static int GetSlider(this Menu menu, string name) {
			return menu.GetValue<MenuSlider>(name).Value;
		}

		public static MenuSeparator AddSeparator(this Menu menu) {
			return menu.Add(new MenuSeparator("", ""));
		}

		public static MenuSeparator AddSeparator(this Menu menu, string name, string display = "") {
			return menu.Add(new MenuSeparator(name, display));
		}

		
		public static IEnumerable<Obj_AI_Hero> ListEnemyHeroesInRange(this Obj_AI_Base form, float range) {
			return GameObjects.EnemyHeroes
				.Where(eh => eh.Distance(form) < range && eh.IsValid && !eh.IsDead && !eh.IsZombie);
		}

		public static void DrawRange(this Spell spell, MenuColor draw, bool onlyWhenReady = true) {
			if (spell.Range > 0)
			{
				if (onlyWhenReady && spell.IsReady() || !onlyWhenReady)
				{
					Render.Circle.DrawCircle(GameObjects.Player.Position, spell.Range, draw.Color.ToSystemColor());
				}
			}
		}
	}
}
