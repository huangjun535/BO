using SharpDX;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mark_As_Dash {
	public static class Extension {
		private static readonly Dictionary<string, Rectangle> Measured = new Dictionary<string, Rectangle>();

		private static Rectangle GetMeasured(Font font, string text) {
			Rectangle rec;
			var key = font.Description.FaceName + font.Description.Width + font.Description.Height +
					  font.Description.Weight + text;
			if (!Measured.TryGetValue(key, out rec))
			{
				rec = font.MeasureText(null, text, FontDrawFlags.Center);
				Measured.Add(key, rec);
			}
			return rec;
		}

		public static void DrawTextCentered(this Font font,
			string text,
			Vector2 position,
			Color color,
			bool outline = false) {
			var measure = GetMeasured(font, text);
			if (outline)
			{
				font.DrawText(
					null, text, (int)(position.X + 1 - measure.Width * 0.5f),
					(int)(position.Y + 1 - measure.Height * 0.5f), Color.Black);
				font.DrawText(
					null, text, (int)(position.X - 1 - measure.Width * 0.5f),
					(int)(position.Y - 1 - measure.Height * 0.5f), Color.Black);
				font.DrawText(
					null, text, (int)(position.X + 1 - measure.Width * 0.5f),
					(int)(position.Y - measure.Height * 0.5f), Color.Black);
				font.DrawText(
					null, text, (int)(position.X - 1 - measure.Width * 0.5f),
					(int)(position.Y - measure.Height * 0.5f), Color.Black);
			}
			font.DrawText(
				null, text, (int)(position.X - measure.Width * 0.5f), (int)(position.Y - measure.Height * 0.5f), color);
		}

		public static void DrawTextCentered(this Font font, string text, int x, int y, Color color) {
			DrawTextCentered(font, text, new Vector2(x, y), color);
		}

		//转换为对话框用
		public static string ToUTF8(this string form) {
			var bytes = Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(form));
			return Encoding.Default.GetString(bytes);
		}
		//转换为菜单用
		public static string ToGBK(this string form) {
			var bytes = Encoding.Convert(Encoding.UTF8, Encoding.Default, Encoding.Default.GetBytes(form));
			return Encoding.Default.GetString(bytes);
		}


		public static string ToHtml(this string form, System.Drawing.Color color, FontStlye fontStlye = FontStlye.Null) {
			string colorhx = "#" + color.ToArgb().ToString("X6");
			return form.ToHtml(colorhx, fontStlye);
		}

		public static string ToHtml(this string form, string color, FontStlye fontStlye = FontStlye.Null) {
			form = form.ToUTF8();
			form = string.Format("<font color=\"{0}\">{1}</font>", color, form);

			if (fontStlye != FontStlye.Null)
			{
				switch (fontStlye)
				{
					case FontStlye.Bold:
						form = string.Format("<b>{0}</b>", form);
						break;
					case FontStlye.Cite:
						form = string.Format("<i>{0}</i>", form);
						break;
				}
			}
			return form;
		}
	}
}
