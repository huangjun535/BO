using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _SDK_Thresh___As_the_Chain_Warden {
	public static class TextHelper {
		#region 文字转换类方法

		/// <summary>
		/// 将文字转换为对话框用
		/// </summary>
		/// <param name="form"></param>
		/// <returns></returns>
		public static string ToUTF8(this string form) {
			var bytes = Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(form));
			return Encoding.Default.GetString(bytes);
		}

		/// <summary>
		/// 将文字转换为菜单用,或者将游戏中获取的文字转为可识别的
		/// </summary>
		/// <param name="form"></param>
		/// <returns></returns>
		public static string ToGBK(this string form) {
			var bytes = Encoding.Convert(Encoding.UTF8, Encoding.Default, Encoding.Default.GetBytes(form));
			return Encoding.Default.GetString(bytes);
		}

		public static string ToHtml(this string form, Color color, FontStlye fontStlye = FontStlye.Null) {
			string colorhx = "#" + color.ToArgb().ToString("X6");
			return form.ToHtml(colorhx, fontStlye);
		}

		/// <summary>
		/// 将文字转换为Html输出格式
		/// </summary>
		/// <param name="form">要输出的内容</param>
		/// <param name="FontName">字体名</param>
		/// <param name="FontSize">字体大小</param>
		/// <param name="FontColor">字体颜色</param>
		/// <returns>格式化后的内容</returns>
		public static string ToHtml(this string form, string FontName, int FontSize, Color FontColor) {
			string colorhx = "#" + FontColor.ToArgb().ToString("X6");
			form = form.ToUTF8();
			form = $"<font face=\"{FontName}\" size=\"{FontSize}\"  color=\"{colorhx}\">{form}</font>";
			return form;
		}

		/// <summary>
		/// 将文字转换为Html输出格式，颜色只能为白色
		/// </summary>
		/// <param name="form">要输出的内容</param>
		/// <param name="size">字体大小</param>
		/// <returns>格式化后的内容</returns>
		public static string ToHtml(this string form, int size) {
			form = form.ToUTF8();
			form = $"<font size=\"{size}\">{form}</font>";
			return form;
		}

		public static string ToHtml(this string form, string color, FontStlye fontStlye = FontStlye.Null) {
			form = form.ToUTF8();
			form = $"<font color=\"{color}\">{form}</font>";

			switch (fontStlye)
			{
				case FontStlye.Bold:
					form = $"<b>{form}</b>";
					break;
				case FontStlye.Cite:
					form = $"<i>{form}</i>";
					break;
			}
			return form;
		}

		#endregion

	}
}
