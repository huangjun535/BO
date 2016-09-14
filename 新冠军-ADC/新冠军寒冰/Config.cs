using LeagueSharp.SDK.Enumerations;
using LeagueSharp.SDK.UI;

namespace YuLeAshe
{
    internal static class Config
    {
        private const string MenuName = "QQ群438230879";

        internal static readonly Menu Menu;

        static Config()
        {
            Menu = new Menu(MenuName, MenuName, true).Attach();

            Modes.Initialize();
            Auto.Initialize();
            Misc.Initialize();
            Hitchance.Initialize();
            AutoWards.Initilalize();
            SkinManager.Initialize(Menu);
            Drawings.Initialize();
        }

        internal static void Initialize() { }

        internal static class Modes
        {
            internal static readonly Menu Menu;

            static Modes()
            {
                Menu = Config.Menu;

                Combo.Initialize();
                Harass.Initialize();
                LaneClear.Initialize();
                JungleClear.Initialize();
                Flee.Initialize();
            }

            internal static void Initialize() { }

            internal static class Combo
            {
                internal static readonly Menu Menu;

                private static readonly MenuBool _useQ;
                private static readonly MenuBool _useW;
                private static readonly MenuBool _useR;

                static Combo()
                {
                    Menu = Modes.Menu.Add(new Menu("Combo", "Combo"));

                    _useQ = Menu.Add(new MenuBool("UseQ", "Use Q", true));
                    _useW = Menu.Add(new MenuBool("UseW", "Use W", true));
                    _useR = Menu.Add(new MenuBool("UseR", "Use R", true));
                }

                internal static bool UseQ => _useQ.Value;

                internal static bool UseW => _useW.Value;

                internal static bool UseR => _useR.Value;

                internal static void Initialize() { }
            }

            internal static class Harass
            {
                internal static readonly Menu Menu;

                private static readonly MenuBool _useQ;
                private static readonly MenuBool _useW;

                private static readonly MenuSliderButton _minMana;

                static Harass()
                {
                    Menu = Modes.Menu.Add(new Menu("Harass", "Harass"));

                    _useQ = Menu.Add(new MenuBool("UseQ", "Use Q", true));
                    _useW = Menu.Add(new MenuBool("UseW", "Use W", true));
                    _minMana = Menu.Add(new MenuSliderButton("Mana", "Min Mana %", 70, 0, 100, true));
                }

                internal static bool UseQ => _useQ.Value;

                internal static bool UseW => _useW.Value;

                internal static int MinMana => _minMana.Value;

                internal static void Initialize() { }
            }

            internal static class LaneClear
            {
                internal static readonly Menu Menu;

                private static readonly MenuSliderButton _minMana;

                private static readonly MenuBool _useQ;
                private static readonly MenuBool _useW;

                static LaneClear()
                {
                    Menu = Modes.Menu.Add(new Menu("LaneClear", "LaneClear"));

                    _useQ = Menu.Add(new MenuBool("UseQ", "Use Q", false));
                    _useW = Menu.Add(new MenuBool("UseW", "Use W", false));
                    _minMana = Menu.Add(new MenuSliderButton("MinMana", "Min Mana %", 70, 0, 100, true));
                }

                internal static bool UseQ => _useQ.Value;

                internal static bool UseW => _useW.Value;

                internal static int MinMana => _minMana.Value;

                internal static void Initialize() { }
            }

            internal static class JungleClear
            {
                internal static readonly Menu Menu;

                private static readonly MenuBool _useQ;
                private static readonly MenuBool _useW;
                private static readonly MenuSliderButton _minMana;
                
                static JungleClear()
                {
                    Menu = Modes.Menu.Add(new Menu("JungleClear", "JungleClear"));

                    _useQ = Menu.Add(new MenuBool("UseQ", "Use Q", true));
                    _useW = Menu.Add(new MenuBool("UseW", "Use W", true));
                    _minMana = Menu.Add(new MenuSliderButton("MinMana", "Min Mana %", 0, 0, 100, true));
                }

                internal static bool UseQ => _useQ.Value;

                internal static bool UseW => _useW.Value;

                internal static int MinMana => _minMana.Value;

                internal static void Initialize() { }
            }

            internal static class Flee
            {
                internal static readonly Menu Menu;
                internal static readonly MenuKeyBind _fleeKey;
                private static readonly MenuBool _useW;

                static Flee()
                {
                    Menu = Modes.Menu.Add(new Menu("Flee", "Flee"));
                    _fleeKey = Menu.Add(new MenuKeyBind("FleeKey", "Flee", System.Windows.Forms.Keys.T, KeyBindType.Press));
                    _useW = Menu.Add(new MenuBool("UseW", "Use W", true));
                }

                internal static bool UseW => _useW.Value;

                internal static void Initialize() { }
            }
        }

        internal static class AutoWards
        {
            internal static readonly Menu Menu;

            private static readonly MenuBool _Enable;
            private static readonly MenuBool _AutoBuy;
            private static readonly MenuBool _AutoPink;
            private static readonly MenuBool _AutoWardCombo;

            static AutoWards()
            {
                Menu = Config.Menu.Add(new Menu("眼位", "自动眼位"));

                _Enable = Menu.Add(new MenuBool("AutoWard", "启动", true));
                _AutoBuy = Menu.Add(new MenuBool("AutoBuy", "lv9自动买灯泡", true));
                _AutoPink = Menu.Add(new MenuBool("AutoPink", "自动真眼扫描", true));
                _AutoWardCombo = Menu.Add(new MenuBool("AutoWardCombo", "仅连招模式启动", true));
                new YuLeLibrary.AutoWard().Load();
                new YuLeLibrary.Tracker().Load();
            }

            internal static bool AutoWardCombo => _AutoWardCombo.Value;

            internal static bool AutoPink => _AutoPink.Value;

            internal static bool AutoBuy => _AutoBuy.Value;

            internal static bool AutoWard => _Enable.Value;

            internal static void Initilalize() { }
        }

        internal static class Auto
        {
            internal static readonly Menu Menu;

            static Auto()
            {
                Menu = Config.Menu.Add(new Menu("Auto", "Auto"));

                AutoE.Initialize();
            }

            internal static void Initialize() { }

            internal static class AutoE
            {
                internal static readonly Menu Menu;

                private static readonly MenuBool _UseEFlash;
                private static readonly MenuKeyBind _UseEKey;
                private static readonly MenuBool _UseEFlash1;

                static AutoE()
                {
                    Menu = Auto.Menu;

                    _UseEFlash = Menu.Add(new MenuBool("UseEtoFlash", "自动E敌人闪现位置", true));
                    _UseEKey = Menu.Add(new MenuKeyBind("UseEKey", "E到鼠标尽头位置", System.Windows.Forms.Keys.G, KeyBindType.Press));
                    _UseEFlash1 = Menu.Add(new MenuBool("UseEtoFlas123123h", "敌人进草自动E", true));
                }

                internal static bool UseEFlash => _UseEFlash.Value;

                internal static bool UseEKey => _UseEKey.Active;

                internal static void Initialize() { }
            }
        }

        internal static class Hitchance
        {
            internal static readonly Menu Menu;

            private static readonly MenuList<HitChance> _WHitchance;
            private static readonly MenuList<HitChance> _RHitchance;

            internal static HitChance WHitChance => _WHitchance.SelectedValue;

            internal static HitChance RHitChance => _RHitchance.SelectedValue;

            static Hitchance()
            {
                Menu = Misc.Menu.Add(new Menu("Hitchance", "技能命中率"));

                _WHitchance = Menu.Add(new MenuList<HitChance>("WHitchance", "W 命中率", new[] { HitChance.Medium, HitChance.High, HitChance.VeryHigh }) { SelectedValue = HitChance.High });
                _WHitchance.ValueChanged += (sender, args) =>
                {
                    SpellManager.W.MinHitChance = _WHitchance.SelectedValue;
                };

                _RHitchance = Menu.Add(new MenuList<HitChance>("RHitchance", "R 命中率", new[] { HitChance.Medium, HitChance.High, HitChance.VeryHigh }) { SelectedValue = HitChance.High });
                _RHitchance.ValueChanged += (sender, args) =>
                {
                    SpellManager.R.MinHitChance = _RHitchance.SelectedValue;
                };
            }

            internal static void Initialize() { }
        }

        internal static class Misc
        {
            internal static readonly Menu Menu;

            static Misc()
            {
                Menu = Config.Menu.Add(new Menu("Misc", "Misc"));

                AntiGapcloser.Initialize();
                AutoInterrupt.Initialize();
                RKey.Initialize();
            }

            internal static void Initialize() { }

            internal static class RKey
            {
                internal static readonly Menu Menu;
                private static readonly MenuKeyBind _useRKey;

                static RKey()
                {
                    Menu = Misc.Menu;

                    _useRKey = Menu.Add(new MenuKeyBind("UseRKey", "手动R按键", System.Windows.Forms.Keys.T, KeyBindType.Press));
                }

                internal static bool UseRKey => _useRKey.Active;

                internal static void Initialize() { }
            }

            internal static class AntiGapcloser
            {
                internal static readonly Menu Menu;
                private static readonly MenuBool _useRGapcloser;

                static AntiGapcloser()
                {
                    Menu = Misc.Menu;

                    _useRGapcloser = Menu.Add(new MenuBool("UseRGapcloser", "自动 R 反突进", true));
                }

                internal static bool UseRGapcloser => _useRGapcloser.Value;

                internal static void Initialize() { }
            }

            internal static class AutoInterrupt
            {
                internal static readonly Menu Menu;
                private static readonly MenuBool _useRInterrupt;

                static AutoInterrupt()
                {
                    Menu = Misc.Menu;

                    _useRInterrupt = Menu.Add(new MenuBool("UseRInterrupt", "自动 R 打断技能", true));
                }

                internal static bool UseRInterrupt => _useRInterrupt.Value;

                internal static void Initialize() { }
            }
        }

        internal static class Drawings
        {
            internal static readonly Menu Menu;
            private static readonly MenuBool _drawWRange;
            private static readonly MenuBool _drawRRange;

            static Drawings()
            {
                Menu = Config.Menu.Add(new Menu("Drawings", "显示"));

                _drawWRange = Menu.Add(new MenuBool("drawWRange", "显示 W 范围"));
                _drawRRange = Menu.Add(new MenuBool("drawRRange", "显示 R 范围"));
            }

            internal static bool DrawWRange => _drawWRange.Value;

            internal static bool DrawRRange => _drawRRange.Value;

            internal static void Initialize() { }
        }
    }
}
