﻿#region Copyright © 2015 Kurisu Solutions
// All rights are reserved. Transmission or reproduction in part or whole,
// any form or by any means, mechanical, electronical or otherwise, is prohibited
// without the prior written consent of the copyright owner.
// 
// Document:	Items/CoreItem.cs
// Date:		22/09/2015
// Author:		Robin Kurisu
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Activator.Base;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Activator.Items
{
    public class CoreItem
    {
        internal virtual int Id { get; set; }
        internal virtual int Priority { get; set; }
        internal virtual int Duration { get; set; }
        internal virtual bool Needed { get; set; }
        internal virtual string Name { get; set; }
        internal virtual string DisplayName { get; set; }
        internal virtual float Range { get; set; }
        internal virtual MenuType[] Category { get; set; }
        internal virtual MapType[] Maps { get; set; }

        internal virtual int DefaultMP { get; set; }
        internal virtual int DefaultHP { get; set; }

        public Menu Menu { get; private set; }
        public Menu Parent => Menu.Parent;
        public Obj_AI_Hero Player => ObjectManager.Player;

        public Champion Tar
        {
            get
            {
                return
                    Activator.Heroes.Where(
                        hero => hero.Player.IsEnemy && hero.Player.IsValidTarget(Range + 100) &&
                               !hero.Player.IsZombie).OrderBy(x => x.Player.Distance(Game.CursorPos)).FirstOrDefault();
            }
        }

        public static IEnumerable<CoreItem> PriorityList()
        {
            var hpi = from ii in Lists.Items where  LeagueSharp.Common.Items.CanUseItem(ii.Id) && ii.Needed
                      orderby ii.Menu.Item("prior" + ii.Name).GetValue<Slider>().Value descending 
                      select ii;

            return hpi;
        }

        public bool IsReady()
        {
            var ready = LeagueSharp.Common.Items.CanUseItem(Id);
            return ready;
        }

        public int[] Excluded => new[] { 3090, 3157, 3137, 3139, 3140, 3222 };

        public void UseItem(bool combo = false)
        {
            if (IsReady())
            {
                Utility.DelayAction.Add(80 - Priority * 10, () => Needed = true);
            }

            if (!combo || Activator.Origin.Item("usecombo").GetValue<KeyBind>().Active)
            {
                if (PriorityList().Any() && Name == PriorityList().First().Name)
                {
                    if (Utils.GameTimeTickCount - Activator.LastUsedTimeStamp > Duration || Excluded.Any(ex => Id.Equals(ex)))
                    {
                        if (!Activator.Player.HasBuffOfType(BuffType.Invisibility))
                        {
                            if (LeagueSharp.Common.Items.UseItem(Id))
                            {
                                Activator.LastUsedTimeStamp = Utils.GameTimeTickCount;
                                Activator.LastUsedDuration = Duration;
                                Utility.DelayAction.Add(100, () => Needed = false);
                            }
                        }
                    }
                }
            }
        }

        public void UseItem(Obj_AI_Base target, bool combo = false)
        {
            if (IsReady())
            {
                Utility.DelayAction.Add(80 - Priority * 10, () => Needed = true);
            }

            if (!combo || Activator.Origin.Item("usecombo").GetValue<KeyBind>().Active)
            {
                if (PriorityList().Any() && Name == PriorityList().First().Name)
                {
                    if (Utils.GameTimeTickCount - Activator.LastUsedTimeStamp > Duration || Excluded.Any(ex => Id.Equals(ex)))
                    {
                        if (!Activator.Player.HasBuffOfType(BuffType.Invisibility))
                        {
                            if (LeagueSharp.Common.Items.UseItem(Id, target))
                            {
                                Activator.LastUsedTimeStamp = Utils.GameTimeTickCount;
                                Activator.LastUsedDuration = Duration;
                                Utility.DelayAction.Add(100, () => Needed = false);
                            }
                        }
                    }
                }
            }
        }

        public void UseItem(Vector3 pos, bool combo = false)
        {
            if (IsReady())
            {
                Utility.DelayAction.Add(80 - Priority * 10, () => Needed = true);
            }

            if (!combo || Activator.Origin.Item("usecombo").GetValue<KeyBind>().Active)
            {
                if (PriorityList().Any() && Name == PriorityList().First().Name)
                {
                    if (Utils.GameTimeTickCount - Activator.LastUsedTimeStamp > Duration || Excluded.Any(ex => Id.Equals(ex)))
                    {
                        if (!Activator.Player.HasBuffOfType(BuffType.Invisibility))
                        {
                            if (LeagueSharp.Common.Items.UseItem(Id, pos))
                            {
                                Activator.LastUsedTimeStamp = Utils.GameTimeTickCount;
                                Activator.LastUsedDuration = Duration;
                                Utility.DelayAction.Add(100, () => Needed = false);
                            }
                        }
                    }
                }
            }
        }

        public CoreItem CreateMenu(Menu root)
        {
            try
            {
                Menu = new Menu(Name, "m" + Name);
                Menu.AddItem(new MenuItem("use" + Name, "Use " + DisplayName ?? Name)).SetValue(true);

                Menu.AddItem(new MenuItem("prior" + Name, DisplayName + " Priority"))
                    .SetValue(new Slider(Priority, 1, 7))
                    .SetTooltip("The Priority " + Name + " Will Activate Over Another Item (7 = Highest)");

                if (Category.Any(t => t == MenuType.SelfLowHP))
                {
                    if (Name.Contains("Pot") || Name.Contains("Flask") || Name.Contains("Biscuit"))
                    {
                        Menu.AddItem(new MenuItem("use" + Name + "cbat", "Use Only In Combat"))
                            .SetValue(false).SetTooltip("aka Taking damage from Minions and Heroes");
                    }
                }

                if (Category.Any(t => t == MenuType.EnemyLowHP))
                {
                    Menu.AddItem(new MenuItem("enemylowhp" + Name + "pct", "Use on Enemy HP % <="))
                        .SetValue(new Slider(Name == "Botrk" || Name == "Cutlass" ? 65: DefaultHP))
                        .SetTooltip("Will Use " + Name + " on Enemy if Their HP % < Value");
                }

                if (Category.Any(t => t == MenuType.SelfLowHP))
                    Menu.AddItem(new MenuItem("selflowhp" + Name + "pct", "Use on Hero HP % <="))
                        .SetValue(new Slider(DefaultHP <= 35 || DefaultHP >= 90 ? (Name == "Botrk" || Name == "Cutlass" ? 65 : 35) : 55))
                        .SetTooltip("Will Use " + Name + " When the Income Damage + Hero's HP % < Value");

                if (Category.Any(t => t == MenuType.SelfMuchHP))
                    Menu.AddItem(new MenuItem("selfmuchhp" + Name + "pct", "Use on Hero Dmg Dealt % >="))
                        .SetValue(new Slider(Duration == 101 ? 30 : 45))
                        .SetTooltip("Will Use " + Name + " When the Hero's Income Damage % > Value");

                if (Category.Any(t => t == MenuType.SelfLowMP))
                    Menu.AddItem(new MenuItem("selflowmp" + Name + "pct", "Use on Hero Mana % <="))
                        .SetValue(new Slider(DefaultMP));

                if (Category.Any(t => t == MenuType.SelfCount))
                    Menu.AddItem(new MenuItem("selfcount" + Name, "Use on Enemy Near Count >="))
                        .SetValue(new Slider(2, 1, 5));

                if (Category.Any(t => t == MenuType.SelfMinMP))
                    Menu.AddItem(new MenuItem("selfminmp" + Name + "pct", "Minimum Mana %")).SetValue(new Slider(55));

                if (Category.Any(t => t == MenuType.SelfMinHP))
                    Menu.AddItem(new MenuItem("selfminhp" + Name + "pct", "Minimum HP %")).SetValue(new Slider(55));

                if (Category.Any(t => t == MenuType.Gapcloser))
                {

                    var imenu = new Menu("Extra " + Name +" Settings", "miscmenu" + Name);

                    imenu.AddItem(new MenuItem("enemygap" + Name, Name + " on Gapcloser"))
                        .SetValue(true);

                    imenu.AddItem(new MenuItem("enemygapmelee" + Name, "-> Melee Only"))
                        .SetValue(false)
                        .Show(imenu.Item("enemygap" + Name).GetValue<bool>());

                    imenu.AddItem(new MenuItem("enemygapdanger" + Name, "-> Dangerous Only"))
                        .SetValue(true)
                        .Show(imenu.Item("enemygap" + Name).GetValue<bool>());

                    imenu.Item("enemygap" + Name).ValueChanged += (sender, args) =>
                    {
                        imenu.Item("enemygapmelee" + Name).Show(args.GetNewValue<bool>());
                        imenu.Item("enemygapdanger" + Name).Show(args.GetNewValue<bool>());
                    };

                    Menu.AddSubMenu(imenu);
                }

                if (Category.Any(t => t == MenuType.Zhonyas))
                {
                    Menu.AddItem(new MenuItem("use" + Name + "norm", "Use on Dangerous (Spells)")).SetValue(false);
                    Menu.AddItem(new MenuItem("use" + Name + "ulti", "Use on Dangerous (Ultimates Only)")).SetValue(true);
                }

                if (Category.Any(t => t == MenuType.Cleanse))
                {
                    var ccmenu = new Menu(Name + " Buff Types", Name.ToLower() + "cdeb");
                    var ssmenu = new Menu(Name + " Unique Buffs", Name.ToLower() + "xspe");

                    foreach (var b in Data.Auradata.BuffList
                        .Where(x => x.MenuName != null && (x.Cleanse && !x.DoT || x.Cleanse && x.DoT && Id == 3222))
                        .OrderByDescending(x => x.Cleanse)
                        .ThenBy(x => x.Evade)
                        .ThenBy(x => x.MenuName))
                    {
                        string xdot = b.DoT && b.Cleanse ? "[Danger]" : (b.DoT ? "[DoT]" : "[Danger]");

                        if (b.Champion != null)
                            foreach (var ene in Activator.Heroes.Where(x => x.Player.IsEnemy && b.Champion == x.Player.ChampionName))
                                ssmenu.AddItem(new MenuItem(Name + b.Name + "cc", b.MenuName + " " + xdot)).SetValue(true);
                        else
                            ssmenu.AddItem(new MenuItem(Name + b.Name + "cc", b.MenuName + " " + xdot)).SetValue(true);
                    }

                    ccmenu.AddItem(new MenuItem(Name + "cexh", "Exhaust")).SetValue(true);
                    ccmenu.AddItem(new MenuItem(Name + "csupp", "Supression")).SetValue(true);
                    ccmenu.AddItem(new MenuItem(Name + "cstun", "Stuns")).SetValue(true);
                    ccmenu.AddItem(new MenuItem(Name + "ccharm", "Charms")).SetValue(true);
                    ccmenu.AddItem(new MenuItem(Name + "ctaunt", "Taunts")).SetValue(true);
                    ccmenu.AddItem(new MenuItem(Name + "cflee", "Flee/Fear")).SetValue(true);
                    ccmenu.AddItem(new MenuItem(Name + "csnare", "Snares")).SetValue(true);
                    ccmenu.AddItem(new MenuItem(Name + "cpolymorph", "Polymorphs")).SetValue(true);
                    ccmenu.AddItem(new MenuItem(Name + "csilence", "Silences")).SetValue(false);
                    ccmenu.AddItem(new MenuItem(Name + "cblind", "Blinds")).SetValue(false);
                    ccmenu.AddItem(new MenuItem(Name + "cslow", "Slows")).SetValue(false);
                    ccmenu.AddItem(new MenuItem(Name + "cpoison", "Poisons")).SetValue(false);

                    Menu.AddSubMenu(ccmenu);
                        Menu.AddSubMenu(ssmenu);

                    Menu.AddItem(new MenuItem("use" + Name + "number", "Minimum Buffs to Use"))
                        .SetValue(new Slider(DefaultHP/5, 1, 5))
                        .SetTooltip("Will Only " + Name + " if Your Buff Count is >= " +
                                    Menu.Item("use" + Name + "number").GetValue<Slider>().Value);
                    Menu.AddItem(new MenuItem("use" + Name + "time", "Minimum Durration to Use"))
                        .SetValue(new Slider(250, 250, 2000))
                        .SetTooltip("Will not use unless the buff durration (stun, snare, etc) last at least this long (ms, 500 = 0.5 seconds)");

                    Menu.AddItem(new MenuItem("use" + Name + "od", "Use for Unique Only"))
                    .SetValue(false).SetTooltip("Use for Unique Only, Or Everything");

                    if (Id == 3222)
                        Menu.AddItem(new MenuItem("use" + Name + "dot", "Use for DoTs only if HP% <"))
                        .SetValue(new Slider(35)).SetTooltip("Will " + Name + " DoTs Spells if Below HP%");

                    Menu.AddItem(new MenuItem("use" + Name + "delay", "Activation Delay (in ms)")).SetValue(new Slider(100, 0, 1000));
                }

                if (Category.Any(t => t == MenuType.ActiveCheck))
                    Menu.AddItem(new MenuItem("mode" + Name, "Mode: "))
                        .SetValue(new StringList(new[] { "Always", "Combo" }));

                root.AddSubMenu(Menu);
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
                Game.PrintChat("<font color=\"#FFF280\">Exception thrown at CoreItem.CreateMenu: </font>: " + e.Message);
            }

            return this;
        }

        public virtual void OnTick(EventArgs args)
        {

        }

        public virtual void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {

        }
    }
}
