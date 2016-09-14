#region Copyright © 2015 Kurisu Solutions
// All rights are reserved. Transmission or reproduction in part or whole,
// any form or by any means, mechanical, electronical or otherwise, is prohibited
// without the prior written consent of the copyright owner.
// 
// Document:	Summoners/CoreSum.cs
// Date:		22/09/2015
// Author:		Robin Kurisu
#endregion

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace Activator.Summoners
{
    public class CoreSum
    {
        internal virtual string Name { get; set; }
        internal virtual string DisplayName { get; set; }
        internal virtual string[] ExtraNames { get; set; }
        internal virtual float Range { get; set; }
        internal virtual int Duration { get; set; }
        internal virtual int DefaultMP { get; set; }
        internal virtual int DefaultHP { get; set; }

        public Menu Menu { get; private set; }
        public Menu Parent => Menu.Parent;
        public SpellSlot Slot => Player.GetSpellSlot(Name);
        public Obj_AI_Hero Player => ObjectManager.Player;

        public CoreSum CreateMenu(Menu root)
        {
            try
            {
                Menu = new Menu(DisplayName, "m" + Name);

                if (!Name.Contains("smite") && !Name.Contains("teleport"))
                {
                    Menu.AddItem(new MenuItem("use" + Name, "Use " + DisplayName)).SetValue(true).Permashow();
                }

                AttachMenu(Menu);

                root.AddSubMenu(Menu);
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
                Game.PrintChat("<font color=\"#FFF280\">Exception thrown at CoreSum.CreateMenu: </font>: " + e.Message);
            }

            return this;
        }

        public bool IsReady()
        {
            return Player.GetSpellSlot(Name).IsReady() || 
                ExtraNames.Any(exname => Player.GetSpellSlot(exname).IsReady());
        }

        public string[] Excluded = { "summonerexhaust" };

        public void UseSpell(bool combo = false)
        {
            if (!combo || Activator.Origin.Item("usecombo").GetValue<KeyBind>().Active)
            {
                if (Excluded.Any(ex => Name.Equals(ex)) || // ignore limit
                    Utils.GameTimeTickCount - Activator.LastUsedTimeStamp > Activator.LastUsedDuration)
                {
                    if (Player.GetSpell(Slot).State == SpellState.Ready)
                    {
                        Player.Spellbook.CastSpell(Slot);
                        Activator.LastUsedTimeStamp = Utils.GameTimeTickCount;
                        Activator.LastUsedDuration = Name == "summonerexhaust" ? 0: Duration;
                    }
                }
            }
        }

        public void UseSpellOn(Obj_AI_Base target, bool combo = false)
        {
            if (!combo || Activator.Origin.Item("usecombo").GetValue<KeyBind>().Active)
            {
                if (Excluded.Any(ex => Name.Equals(ex)) || // ignore limit
                    Utils.GameTimeTickCount - Activator.LastUsedTimeStamp > Activator.LastUsedDuration) 
                {
                    if (Player.GetSpell(Slot).State == SpellState.Ready)
                    {
                        Player.Spellbook.CastSpell(Slot, target);
                        Activator.LastUsedTimeStamp = Utils.GameTimeTickCount;
                        Activator.LastUsedDuration = Name == "summonerexhaust" ? 0 : Duration;
                    }
                }
            }
        }

        public virtual void OnDraw(EventArgs args)
        {

        }

        public virtual void OnTick(EventArgs args)
        {

        }

        public virtual void AttachMenu(Menu menu)
        {
            
        }
    }
}
