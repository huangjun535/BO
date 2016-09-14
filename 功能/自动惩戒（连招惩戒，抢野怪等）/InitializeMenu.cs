namespace ElSmite
{
    using System.Drawing;

    using LeagueSharp.Common;

    // ReSharper disable once ClassNeverInstantiated.Global
    public class InitializeMenu
    {
        #region Static Fields

        public static Menu Menu;

        #endregion

        #region Public Methods and Operators

        public static void Load()
        {
            Menu = new Menu("ElSmite", "ElSmite", true);
            Menu.AddItem(new MenuItem("Smite.Spell", "Use Spell + Smite combo").SetValue(true));

            var mainMenu = Menu.AddSubMenu(new Menu("Default", "Default"));
            {
                mainMenu.AddItem(
                       new MenuItem("ElSmite.Activated", "Activated").SetValue(
                           new KeyBind("M".ToCharArray()[0], KeyBindType.Toggle, true)));
                mainMenu.AddItem(new MenuItem("Smite.Ammo", "Save 1 smite charge").SetValue(true)).SetFontStyle(FontStyle.Regular, SharpDX.Color.Green);


                if (Entry.IsSummonersRift)
                {
                    mainMenu.SubMenu("Big Mobs").SubMenu("Dragons").AddItem(new MenuItem("SRU_Dragon_Air", "Air Dragon").SetValue(true));
                    mainMenu.SubMenu("Big Mobs").SubMenu("Dragons").AddItem(new MenuItem("SRU_Dragon_Earth", "Earth Dragon").SetValue(true));
                    mainMenu.SubMenu("Big Mobs").SubMenu("Dragons").AddItem(new MenuItem("SRU_Dragon_Fire", "Fire Dragon").SetValue(true));
                    mainMenu.SubMenu("Big Mobs").SubMenu("Dragons").AddItem(new MenuItem("SRU_Dragon_Water", "Water Dragon").SetValue(true));
                    mainMenu.SubMenu("Big Mobs").SubMenu("Dragons").AddItem(new MenuItem("SRU_Dragon_Elder", "Elder Dragon").SetValue(true));

                    mainMenu.SubMenu("Big Mobs").AddItem(new MenuItem("SRU_Baron", "Baron").SetValue(true));
                    mainMenu.SubMenu("Big Mobs").AddItem(new MenuItem("SRU_Red", "Red buff").SetValue(true));
                    mainMenu.SubMenu("Big Mobs").AddItem(new MenuItem("SRU_Blue", "Blue buff").SetValue(true));
                    mainMenu.SubMenu("Big Mobs").AddItem(new MenuItem("SRU_RiftHerald", "Rift Herald").SetValue(true));

                    mainMenu.SubMenu("Small Mobs").AddItem(new MenuItem("SRU_Gromp", "Gromp").SetValue(false));
                    mainMenu.SubMenu("Small Mobs").AddItem(new MenuItem("SRU_Murkwolf", "Wolves").SetValue(false));
                    mainMenu.SubMenu("Small Mobs").AddItem(new MenuItem("SRU_Krug", "Krug").SetValue(false));
                    mainMenu.SubMenu("Small Mobs").AddItem(new MenuItem("SRU_Razorbeak", "Chicken camp").SetValue(false));
                    mainMenu.SubMenu("Small Mobs").AddItem(new MenuItem("Sru_Crab", "Crab").SetValue(false));
                }

                if (Entry.IsTwistedTreeline)
                {
                    mainMenu.AddItem(new MenuItem("TT_Spiderboss", "Vilemaw Enabled").SetValue(true));
                    mainMenu.AddItem(new MenuItem("TT_NGolem", "Golem Enabled").SetValue(true));
                    mainMenu.AddItem(new MenuItem("TT_NWolf", "Wolf Enabled").SetValue(true));
                    mainMenu.AddItem(new MenuItem("TT_NWraith", "Wraith Enabled").SetValue(true));
                }
            }

            var combatMenu = Menu.AddSubMenu(new Menu("Champion smite", "Championsmite"));
            {
                combatMenu.AddItem(new MenuItem("ElSmite.KS.Activated", "Use smite to killsteal").SetValue(true));
            }

            var drawMenu = Menu.AddSubMenu(new Menu("Drawings", "Drawings"));
            {
                drawMenu.AddItem(new MenuItem("ElSmite.Draw.Range", "Draw smite Range").SetValue(new Circle()));
                drawMenu.AddItem(new MenuItem("ElSmite.Draw.Text", "Draw smite text").SetValue(true));
                drawMenu.AddItem(new MenuItem("ElSmite.Draw.Damage", "Draw smite Damage").SetValue(true));
            }

            Menu.AddItem(new MenuItem("seperator", ""));
            Menu.AddItem(new MenuItem("422442fsaafsf", string.Format("Version: {0}", Entry.ScriptVersion)));
            Menu.AddItem(new MenuItem("by.jQuery", "Made By jQuery"));

            Menu.AddToMainMenu();
        }

        #endregion
    }
}