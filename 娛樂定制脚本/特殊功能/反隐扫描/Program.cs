using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace StealthDetector
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;  
        }

        static Menu config;
        static List<Spells> SpellList = new List<Spells>();
        static int Delay = 0;
        static float VayneBuffEndTime = 0;

        public struct Spells
        {
            public string ChampionName;
            public string SpellName;
            public SpellSlot slot;
        }

        static class SkillData
        {
            public static SpellSlot slot;
            public static float range;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            Obj_AI_Base.OnCreate += Obj_AI_Base_OnCreate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Game.OnUpdate += Game_OnUpdate;
            
            SpellList.Add(new Spells { ChampionName = "akali", SpellName = "akalismokebomb", slot = SpellSlot.W });   //Akali W
            SpellList.Add(new Spells { ChampionName = "shaco", SpellName = "deceive", slot = SpellSlot.Q }); //Shaco Q
            SpellList.Add(new Spells { ChampionName = "khazix", SpellName = "khazixr", slot = SpellSlot.R }); //Khazix R
            SpellList.Add(new Spells { ChampionName = "khazix", SpellName = "khazixrlong", slot = SpellSlot.R }); //Khazix R Evolved
            SpellList.Add(new Spells { ChampionName = "talon", SpellName = "talonshadowassault", slot = SpellSlot.R }); //Talon R
            SpellList.Add(new Spells { ChampionName = "monkeyking", SpellName = "monkeykingdecoy", slot = SpellSlot.W }); //Wukong W
            SpellList.Add(new Spells { ChampionName = "vayne", SpellName = "vaynetumble", slot = SpellSlot.Q }); //Vayne R-Q
            SpellList.Add(new Spells { ChampionName = "twitch", SpellName = "hideinshadows", slot = SpellSlot.Q }); //Twitch Q

            FuckingRengar();
            Menu();
        }

        static void Game_OnUpdate(EventArgs args)
        {
            var Vayne = HeroManager.Enemies.Find(x => x.ChampionName.ToLower() == "vayne");

            if (Vayne == null)
                return;

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy &&
                x.ChampionName.ToLower().Contains("vayne") &&
                x.Buffs.Any(y => y.Name == "VayneInquisition")))
            {
                VayneBuffEndTime = hero.Buffs.First(x => x.Name == "VayneInquisition").EndTime;
            }
        }

        static void Menu()
        {
            config = new Menu("Stealth Detector", "Stealth Detector", true);

            var Detector = new Menu("Stealth Detector", "Stealth Detector");
            {
                Detector.AddItem(new MenuItem("Always", "Always Use Detector").SetValue<bool>(false));
                Detector.AddItem(new MenuItem("Use", "Use Detector On Combo").SetValue(new KeyBind(32, KeyBindType.Press)));
                var Detector2 = new Menu("Track Him", "Track Him");
                {
                    foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy))
                    {
                        foreach (var spell in SpellList.Where(x => x.ChampionName.ToLower() == hero.ChampionName.ToLower()))
                        {
                            Detector2.AddItem(new MenuItem(hero.ChampionName.ToLower() + spell.slot.ToString(), hero.ChampionName + " - " + spell.slot.ToString()).SetValue(true));
                        }
                    }

                    if (HeroManager.Enemies.Any(x => x.ChampionName.ToLower() == "rengar"))
                    {
                        Detector2.AddItem(new MenuItem("RengarR", "Rengar R").SetValue(true));
                    }

                    if (HeroManager.Enemies.Any(x => x.ChampionName.ToLower() == "leblanc"))
                    {
                        Detector2.AddItem(new MenuItem("LeBlancR", "LeBlanc R").SetValue(true));
                    }

                    Detector.AddSubMenu(Detector2);
                }

                if (HeroManager.Enemies.Any(x => x.ChampionName.ToLower() == "rengar"))
                {
                    var FuckingRengar = new Menu("Fucking Rengar", "Fucking Rengar");
                    {
                        FuckingRengar.AddItem(new MenuItem("FRUse", "Use Anti Rengar").SetValue(true));

                        Detector.AddSubMenu(FuckingRengar);
                    }
                }

                config.AddSubMenu(Detector);
            }

            config.AddToMainMenu();
        }

        static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!config.Item("Use").GetValue<KeyBind>().Active && !config.Item("Always").GetValue<bool>())
                return;

            if (!sender.IsEnemy || sender.IsDead || !(sender is Obj_AI_Hero))
                return;

            if (SpellList.Exists(x => x.SpellName.Contains(args.SData.Name.ToLower())))
            {
                var _sender = sender as Obj_AI_Hero;

                if (!config.Item(_sender.ChampionName.ToLower() + _sender.GetSpellSlot(args.SData.Name).ToString()).GetValue<bool>())
                    return;

                if (CheckSlot() == SpellSlot.Unknown)
                    return;

                if (CheckWard())
                    return;

                if (ObjectManager.Player.Distance(sender.Position) > 800)
                    return;

                if (args.SData.Name.ToLower().Contains("vaynetumble") && Game.Time > VayneBuffEndTime)
                    return;
                
                if (Environment.TickCount - Delay > 1500 || Delay == 0)
                {
                    var pos = ObjectManager.Player.Distance(args.End) > 600 ? ObjectManager.Player.Position : args.End;
                    ObjectManager.Player.Spellbook.CastSpell(CheckSlot(), pos);
                    Delay = Environment.TickCount;
                }
            }
        }

        static void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            if (!config.Item("Use").GetValue<KeyBind>().Active && !config.Item("Always").GetValue<bool>())
                return;

            #region Rengar

            var Rengar = HeroManager.Enemies.Find(x => x.ChampionName.ToLower() == "rengar");

            if (Rengar != null)
            {
                if (config.Item("RengarR").GetValue<bool>())
                {
                    if (sender.IsEnemy && sender.Name.Contains("Rengar_Base_R_Alert"))
                    {
                        if (ObjectManager.Player.HasBuff("rengarralertsound") &&
                        !CheckWard() &&
                        !Rengar.IsVisible &&
                        !Rengar.IsDead &&
                            CheckSlot() != SpellSlot.Unknown)
                        {
                            ObjectManager.Player.Spellbook.CastSpell(CheckSlot(), ObjectManager.Player.Position);
                        }
                    }
                }

                var spell = new Spell(SkillData.slot, SkillData.range);

                if (config.Item("FRUse").GetValue<bool>() && SkillData.slot != SpellSlot.Unknown)
                {
                    if (sender.IsEnemy && sender.Name.Contains("Rengar_LeapSound.troy") &&
                        ObjectManager.Player.Distance(sender.Position) < 1700 &&
                        spell.IsReady())
                    {
                        if (spell.Range == 1 && ObjectManager.Player.Distance(Rengar.Position) < 750)
                        {
                            spell.Cast(ObjectManager.Player.Position);
                        }
                        else if (ObjectManager.Player.Distance(Rengar.Position) < spell.Range &&
                            Rengar.IsValidTarget())
                        {
                            spell.CastOnUnit(Rengar);
                        }
                    }
                }
            }

            #endregion

            #region Leblanc

            var Leblanc = HeroManager.Enemies.Find(x => x.ChampionName.ToLower() == "leblanc");

            if (Leblanc != null)
            {
                if (!config.Item("LeBlancR").GetValue<bool>())
                    return;

                if (ObjectManager.Player.Distance(sender.Position) > 600)
                    return;
                

                if (sender.IsEnemy && sender.Name == "LeBlanc_Base_P_poof.troy")
                {
                    if (!CheckWard() &&
                    !Leblanc.IsVisible &&
                    !Leblanc.IsDead &&
                        CheckSlot() != SpellSlot.Unknown)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(CheckSlot(), ObjectManager.Player.Position);
                    }
                }
            }

            #endregion
        }

        static void FuckingRengar()
        {
            switch (ObjectManager.Player.ChampionName)
            {
                case "Jinx":
                    SkillData.slot = SpellSlot.E;
                    SkillData.range = 1;
                    break;
                case "Vayne":
                    SkillData.slot = SpellSlot.E;
                    SkillData.range = 700;
                    break;
                case "Viktor":
                    SkillData.slot = SpellSlot.W;
                    SkillData.range = 1;
                    break;
                case "Tristana":
                    SkillData.slot = SpellSlot.R;
                    SkillData.range = 550;
                    break;
                default:
                    SkillData.slot = SpellSlot.Unknown;
                    break;
            }
        }

        static SpellSlot CheckSlot()
        {
            SpellSlot slot = SpellSlot.Unknown;

            if (Items.CanUseItem(3362) && Items.HasItem(3362, ObjectManager.Player))
            {
                slot = SpellSlot.Trinket;
            }
            else if (Items.CanUseItem(2043) && Items.HasItem(2043, ObjectManager.Player))
            {
                slot = ObjectManager.Player.GetSpellSlot("VisionWard");
            }
            else if (Items.CanUseItem(3364) && Items.HasItem(3364, ObjectManager.Player))
            {
                slot = SpellSlot.Trinket;
            }
            return slot;
        }

        static bool CheckWard()
        {
            var status = false;

            foreach (var a in ObjectManager.Get<Obj_AI_Minion>().Where(x => x.Name == "VisionWard"))
            {
                if (ObjectManager.Player.Distance(a.Position) < 450)
                {
                    status = true;
                }
            }

            return status;
        }
    }
}
