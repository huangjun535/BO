namespace LeagueSharp.Common
{
    using Properties;
    using SharpDX;
    using SharpDX.Text;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;
    using System.Web.Script.Serialization;
    using Color = System.Drawing.Color;

    /// <summary>
    ///     The menu settings.
    /// </summary>
    internal static class MenuSettings
    {
        #region Static Fields

        public static readonly Color ActiveBackgroundColor = Color.FromArgb(0, 37, 53);

        /// <summary>
        ///     The menu starting position.
        /// </summary>
        public static Vector2 BasePosition = new Vector2(10, 10);

        /// <summary>
        ///     Indicates whether to draw the menu.
        /// </summary>
        private static bool drawMenu;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a static instance of the <see cref="MenuSettings" /> class.
        /// </summary>
        static MenuSettings()
        {
            drawMenu = MenuGlobals.DrawMenu;
            Game.OnWndProc += args => OnWndProc(new WndEventComposition(args));
        }

        #endregion

        #region Public Properties

        public static Color BackgroundColor
        {
            get
            {
                return Color.FromArgb(Menu.Root.Item("BackgroundAlpha").GetValue<Slider>().Value, Color.Black);
            }
        }

        /// <summary>
        ///     Gets the menu configuration path.
        /// </summary>
        public static string MenuConfigPath
        {
            get
            {
                return Path.Combine(Config.AppDataDirectory, "MenuConfigCommon");
            }
        }

        /// <summary>
        ///     Gets or sets the size of the menu font.
        /// </summary>
        public static int MenuFontSize { get; set; }

        /// <summary>
        ///     Gets the menu item height.
        /// </summary>
        public static int MenuItemHeight
        {
            get
            {
                return 32;
            }
        }

        /// <summary>
        ///     Gets the menu item width.
        /// </summary>
        public static int MenuItemWidth
        {
            get
            {
                return 250;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the value indicating whether to draw the menu.
        /// </summary>
        internal static bool DrawMenu
        {
            get
            {
                return drawMenu;
            }

            set
            {
                MenuGlobals.DrawMenu = drawMenu = value;
            }
        }

        #endregion

        #region Methods

        private static void OnWndProc(WndEventComposition args)
        {
            if ((args.Msg == WindowsMessages.WM_KEYUP || args.Msg == WindowsMessages.WM_KEYDOWN)
                && args.WParam == Config.ShowMenuPressKey)
            {
                DrawMenu = args.Msg == WindowsMessages.WM_KEYDOWN;
            }

            if (args.Msg == WindowsMessages.WM_KEYUP && args.WParam == Config.ShowMenuToggleKey)
            {
                DrawMenu = !DrawMenu;
            }
        }

        #endregion
    }

    public static class MultiLanguages
    {
        /// <summary>
        /// The translations
        /// </summary>
        private static Dictionary<string, string> Translations = new Dictionary<string, string>();

        /// <summary>
        /// Initializes static members of the <see cref="MultiLanguage"/> class.
        /// </summary>
        static MultiLanguages()
        {
            LoadLanguage(Config.SelectedLanguage);
        }

        /// <summary>
        /// Translates the text into the loaded language.
        /// </summary>
        /// <param name="textToTranslate">The text to translate.</param>
        /// <returns>System.String.</returns>
        public static string _(string textToTranslate)
        {
            var textToTranslateToLower = textToTranslate.ToLower();
            return Translations.ContainsKey(textToTranslateToLower) ? Translations[textToTranslateToLower] : textToTranslate;
        }

        /// <summary>
        /// Loads the language.
        /// </summary>
        /// <param name="languageName">Name of the language.</param>
        /// <returns><c>true</c> if the operation succeeded, <c>false</c> otherwise false.</returns>
        public static bool LoadLanguage(string languageName)
        {
            try
            {
                var languageStrings = new System.Resources.ResourceManager("LeagueSharp.Common.Properties.Resources", typeof(Resources).Assembly).GetString("NightMoon");

                if (string.IsNullOrEmpty(languageStrings))
                {
                    return false;
                }

                languageStrings = DesDecrypt(languageStrings);

                Translations = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(languageStrings); return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        private static string DesDecrypt(string decryptString)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes("1076751236".Substring(0, 8));
            byte[] keyIV = keyBytes;
            byte[] inputByteArray = Convert.FromBase64String(decryptString);
            DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
            MemoryStream mStream = new MemoryStream();
            CryptoStream cStream = new CryptoStream(mStream, provider.CreateDecryptor(keyBytes, keyIV), CryptoStreamMode.Write);
            cStream.Write(inputByteArray, 0, inputByteArray.Length);
            cStream.FlushFinalBlock();
            return Encoding.UTF8.GetString(mStream.ToArray());
        }
    }
}