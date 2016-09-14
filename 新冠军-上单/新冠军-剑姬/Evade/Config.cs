﻿
namespace YuLeFiora.Evade
{
    using System;
    using System.Linq;
    using System.Windows.Forms;
    using  LeagueSharp;
    using  LeagueSharp.Common;
    using  SharpDX;
    #region UI
    public static class AddUI
    {
        public static void Notif(string msg, int time)
        {
            var x = new Notification("ProjectFiora:  " + msg, time);
            Notifications.AddNotification(x);
        }

        public static LeagueSharp.Common.MenuItem Separator(this LeagueSharp.Common.Menu subMenu, string name)
        {
            return subMenu.AddItem(new LeagueSharp.Common.MenuItem(name, name));
        }

        public static LeagueSharp.Common.MenuItem Bool(this LeagueSharp.Common.Menu subMenu, string name, string display, bool state = true)
        {
            return subMenu.AddItem(new LeagueSharp.Common.MenuItem(name, display).SetValue(state));
        }

        public static LeagueSharp.Common.MenuItem KeyBind(this LeagueSharp.Common.Menu subMenu,
            string name,
            string display,
            System.Windows.Forms.Keys key,
            KeyBindType type = KeyBindType.Press, bool booler = false)
        {
            return subMenu.AddItem(new LeagueSharp.Common.MenuItem(name, display).SetValue<KeyBind>(new KeyBind((uint)key, type, booler)));
        }

        public static LeagueSharp.Common.MenuItem List(this LeagueSharp.Common.Menu subMenu, string name, string display, string[] array)
        {
            return subMenu.AddItem(new LeagueSharp.Common.MenuItem(name, display).SetValue<StringList>(new StringList(array)));
        }

        public static LeagueSharp.Common.MenuItem Slider(this LeagueSharp.Common.Menu subMenu, string name, string display, int cur, int min = 0, int max = 100)
        {
            return subMenu.AddItem(new LeagueSharp.Common.MenuItem(name, display).SetValue<Slider>(new Slider(cur, min, max)));
        }
    }
    #endregion UI
    internal static class Config
    {
        #region Constants

        public const int DiagonalEvadePointsCount = 7;

        public const int DiagonalEvadePointsStep = 20;

        public const int EvadingFirstTimeOffset = 250;

        public const int EvadingSecondTimeOffset = 80;

        public const int ExtraEvadeDistance = 15;

        public const int GridSize = 10;

        public const int SkillShotsExtraRadius = 9;

        public const int SkillShotsExtraRange = 20;

        #endregion

        #region Public Methods and Operators

        public static void CreateMenu()
        {
            var evadeMenu = Program.Menu.SubMenu("Evade");
            {
                var evadeSpells = new LeagueSharp.Common.Menu("格挡普攻", "Spells");
                {
                    foreach (
                        var spell in
                            EvadeSpellDatabase.Spells)
                    {
                        var subMenu = new LeagueSharp.Common.Menu(string.Format("{0} ({1})", spell.Name, spell.Slot), spell.Name);
                        {
                            subMenu.Bool(spell.Slot + "Tower", "Under Tower", false);
                            subMenu.Slider(spell.Slot + "Delay", "Extra Delay", 100, 0, 150);
                            subMenu.Slider("DangerLevel", "If Danger Level >=", 1, 1, 5);
                            subMenu.Bool("Enabled", "Enabled");
                        }
                        evadeSpells.AddSubMenu(subMenu);
                    }
                    evadeMenu.AddSubMenu(evadeSpells);
                }
                foreach (var spell in
                    SpellDatabase.Spells.Where(
                        i =>
                        HeroManager.Enemies.Any(
                            a =>
                            string.Equals(a.ChampionName, i.ChampionName, StringComparison.InvariantCultureIgnoreCase)))
                    )
                {
                    var subMenu = new LeagueSharp.Common.Menu(string.Format("{0} ({1})", spell.SpellName, spell.Slot), spell.SpellName);
                    {
                        subMenu.Slider("DangerLevel", "Danger Level", spell.DangerValue, 1, 5);
                        subMenu.Bool("IsDangerous", "Is Dangerous", spell.IsDangerous);
                        subMenu.Bool("DisableFoW", "Disable FoW Dodging", false);
                        subMenu.Bool("Draw", "Draw", false);
                        subMenu.Bool("Enabled", "Enabled", true);
                        evadeMenu.SubMenu(spell.ChampionName.ToLowerInvariant()).AddSubMenu(subMenu);
                    }
                }
                evadeMenu.Bool("DrawStatus", "Draw Evade Status");
                evadeMenu.KeyBind("Enabled", "Enabled",  System.Windows.Forms.Keys.K, KeyBindType.Toggle, true);
                evadeMenu.KeyBind("OnlyDangerous", "Dodge Only Dangerous", System.Windows.Forms.Keys.U);
            }
        }

        #endregion
    }
}