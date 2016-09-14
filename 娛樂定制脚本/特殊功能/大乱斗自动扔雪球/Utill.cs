using LeagueSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;

namespace Mark_As_Dash {
	public static class Utill {

		public static List<Obj_AI_Base> GetObj() {
			List<Obj_AI_Base> list = new List<Obj_AI_Base>();
			list.AddRange(ObjectManager.Get<Obj_AI_Base>().Where(o => o.Position.Distance(Game.CursorPos) < 70));
            return list;
		}

		public static string GetInfo() {
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("=========对象信息=============");

			var objs = GetObj();
			foreach (var obj in objs)
			{
				sb.AppendLine("对象名:" + obj.Name.ToGBK() + "\t");
				if (obj != null)
				{
					var hero = obj as Obj_AI_Base;
					sb.AppendLine("名字：" + hero.CharData.BaseSkinName);
					string Buffstr = " ";
					foreach (var buffer in hero.Buffs)
					{
						Buffstr += buffer.Name + "\t";
					}
					sb.AppendLine("Buff:" + Buffstr);
				}
			}
			return sb.ToString();
		}

		#region 一组调试工具
		public static void Print(string format, params object[] param) {
			string s = string.Format(format, param);
			Game.PrintChat(s.ToHtml("#AAAAFF", FontStlye.Cite));
		}
		public static void Debug(string content, DebugLevel level = DebugLevel.Info, Output way = Output.Console) {
			if (way == Output.Console)
			{
				ConsoleColor color = ConsoleColor.White;
				if (level == DebugLevel.Info)
				{
					color = ConsoleColor.Green;
				}
				else if (level == DebugLevel.Warning)
				{
					color = ConsoleColor.Yellow;
				}
				else if (level == DebugLevel.Wrang)
				{
					color = ConsoleColor.Red;
				}
				Console.ForegroundColor = color;
				Console.WriteLine("扔雪球：" + content);
				Console.ForegroundColor = ConsoleColor.White;
			}
			else
			{
				System.Drawing.Color color = System.Drawing.Color.White;
				if (level == DebugLevel.Info)
				{
					color = System.Drawing.ColorTranslator.FromHtml("#AAAAFF");
				}
				else if (level == DebugLevel.Warning)
				{
					color = System.Drawing.Color.Orange;
				}
				else if (level == DebugLevel.Wrang)
				{
					color = System.Drawing.Color.Red;
				}
			}
		}
		public static void DebugChat(string format, params object[] param) {
			string s = string.Format(format, param);
			Game.PrintChat(s.ToHtml("#AAAAFF", FontStlye.Cite));
		}
		public static void DebugConsole(string format, params object[] param) {
			Console.ForegroundColor = ConsoleColor.DarkBlue;
			Console.WriteLine("扔雪球：" + format, param);
			Console.ForegroundColor = ConsoleColor.White;
		}
		#endregion
	}
}
