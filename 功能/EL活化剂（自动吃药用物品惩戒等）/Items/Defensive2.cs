﻿namespace ElUtilitySuite.Items
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Security.Permissions;

    using LeagueSharp;
    using LeagueSharp.Common;

    internal class Defensive2 : IPlugin
    {
        #region Fields

        private readonly List<Item> defensiveItems;

        #endregion

        #region Constructors and Destructors

        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        public Defensive2()
        {
            this.defensiveItems =
                Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .Where(
                        x =>
                        x.Namespace != null && x.Namespace.Contains("DefensiveItems") && x.IsClass
                        && typeof(Item).IsAssignableFrom(x))
                    .Select(x => (Item)Activator.CreateInstance(x))
                    .ToList();
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the menu.
        /// </summary>
        /// <value>
        ///     The menu.
        /// </value>
        public Menu Menu { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Creates the menu.
        /// </summary>
        /// <param name="rootMenu">The root menu.</param>
        /// <returns></returns>
        public void CreateMenu(Menu rootMenu)
        {
            this.Menu = rootMenu.AddSubMenu(new Menu("Defensive", "omenu3"));

            foreach (var item in this.defensiveItems)
            {
                var submenu = new Menu(item.Name, (int)item.Id + item.Name);

                item.Menu = submenu;
                item.CreateMenu();

                this.Menu.AddSubMenu(submenu);
            }
        }

        /// <summary>
        ///     Loads this instance.
        /// </summary>
        public void Load()
        {
            Game.OnUpdate += this.Game_OnUpdate;
        }

        #endregion

        #region Methods

        private void Game_OnUpdate(EventArgs args)
        {
            foreach (var item in this.defensiveItems.Where(x => x.ShouldUseItem() && Items.CanUseItem((int)x.Id)))
            {
                item.UseItem();
            }
        }
        #endregion
    }
}