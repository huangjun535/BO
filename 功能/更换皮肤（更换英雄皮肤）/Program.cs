﻿using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace SkinsSharp
{
    class Program
    {
        private static Menu menu;
        private static Dictionary<String, int> ChampSkins = new Dictionary<String, int>();
		private static Dictionary<int, bool> Enabled = new Dictionary<int, bool>();
		private static Dictionary<Obj_AI_Hero, bool> WasDead = new Dictionary<Obj_AI_Hero, bool>();

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameLoad;
        }

        private static List<Obj_AI_Hero> HeroList = new List<Obj_AI_Hero>();

	    private static bool OnlyDefault
	    {
		    get { return menu.Item("onlydefault").GetValue<bool>(); }
	    }

	    static void GameLoad(EventArgs argss)
        {

            menu = new Menu("娛樂漢化：更换皮肤", "Skinswitcher", true);

            menu.AddItem(new MenuItem("forall", "启用所有（F5重载）").SetValue(false));
			menu.AddItem(new MenuItem("onlydefault", "保持自己原来的皮肤").SetValue(true));

			try
            {
                foreach (var hero in HeroManager.AllHeroes)
                {
                    if (!menu.Item("forall").GetValue<bool>() && hero.Name != ObjectManager.Player.Name)
                    {
                        continue;
                    }

                    HeroList.Add(hero);

                    WasDead.Add(hero, false);

					Enabled.Add(hero.NetworkId, false);

                    var currenthero = hero;

                    var herosubmenu = new Menu(hero.ChampionName + " (" + hero.Name + ") ", hero.ChampionName);

	                var skinselect = herosubmenu.AddItem(
                            new MenuItem("skin." + hero.ChampionName, "Change Skin")
                                .SetValue(
                                    new StringList(
                                        new[]
                                        {
                                            "Skin 0", "Skin 1", "Skin 2", "Skin 3", "Skin 4", "Skin 5", "Skin 6", "Skin 7",
                                            "Skin 8", "Skin 9", "Skin 10", "Skin 11", "Skin 12", "Skin 13", "Skin 14", "Skin 15"
                                        }, 0)));

                    ChampSkins.Add(hero.Name, skinselect.GetValue<StringList>().SelectedIndex);


	                if (OnlyDefault && hero.CharData.BaseSkinName == hero.SkinName)
	                {
		                Enabled[hero.NetworkId] = true;
		                hero.SetSkin(hero.ChampionName, ChampSkins[hero.Name]);
	                }
					else if (!OnlyDefault)
					{
						Enabled[hero.NetworkId] = true;
						hero.SetSkin(hero.ChampionName, ChampSkins[hero.Name]);
					}

	                menu.AddSubMenu(herosubmenu);

                    skinselect.ValueChanged += delegate (Object sender, OnValueChangeEventArgs args)
                    {
						Enabled[hero.NetworkId] = true;
                        var skinid = args.GetNewValue<StringList>().SelectedIndex;
                        if (hero.ChampionName == "Ezreal" && skinid == 5)
                        {
                            skinid += 1;
                        }
                        ChampSkins[currenthero.Name] = skinid;
                        currenthero.SetSkin(currenthero.ChampionName, ChampSkins[currenthero.Name]);
                    };
                }
            }
            catch (Exception e)
            {
                Console.Write(e + " " + e.StackTrace);
            }
            menu.AddToMainMenu();
            Game.OnUpdate += RenewSkins;
            //GameObject.OnFloatPropertyChange += FloatPropertyChange;
        }


        static void RenewSkins(EventArgs args)
        {
            foreach (var hero in HeroList)
            {
                if (!menu.Item("forall").GetValue<bool>() && !hero.IsMe)
                {
                    continue;
                }
                if (hero.IsDead && !WasDead[hero])
                {
                    WasDead[hero] = true;
                    continue;
                }
                 if (!hero.IsDead && WasDead[hero] && Enabled[hero.NetworkId])
                {
                    hero.SetSkin(hero.ChampionName, ChampSkins[hero.Name]);
                    WasDead[hero] = false;
                }
            }
        }


        static void FloatPropertyChange(GameObject sender, GameObjectFloatPropertyChangeEventArgs args)
        {
            try
            {
                if (!(sender is Obj_AI_Hero) || args.Property != "mHP" || sender.Name != ObjectManager.Player.Name && !menu.Item("forall").GetValue<bool>())
                {
                    return;
                }

                var hero = (Obj_AI_Hero) sender;

                if (args.OldValue.Equals(args.NewValue) && args.NewValue.Equals(hero.MaxHealth) && !hero.IsDead)
                {
                    hero.SetSkin(hero.ChampionName, ChampSkins[hero.Name]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}

