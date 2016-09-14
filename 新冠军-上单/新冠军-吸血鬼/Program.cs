namespace YuLeVladimir_Rework
{
    using LeagueSharp;
    using LeagueSharp.SDK;
    using LeagueSharp.SDK.UI;
    using System.Linq;
    using YuLeLibrary;
    using System.Threading.Tasks;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Reflection;
    using System;

    internal class Program
    {
        internal const int FlashRange = 425, IgniteRange = 600, SmiteRange = 570;
        internal static Items.Item Bilgewater, BotRuinedKing, Youmuu, Tiamat, Hydra, Titanic;
        internal static SpellSlot Flash = SpellSlot.Unknown, Ignite = SpellSlot.Unknown, Smite = SpellSlot.Unknown;
        internal static Menu MainMenu;
        internal static Obj_AI_Hero Player;
        internal static Spell Q, Q2, Q3, W, W2, E, E2, R, R2, R3;

        static void Main(string[] args)
        {
            Bootstrap.Init(args);
            Events.OnLoad += Events_OnLoad;
        }

        private static void Events_OnLoad()
        {
            if (GameObjects.Player.ChampionName != "Vladimir")
                return;

            Player = GameObjects.Player;
            InitMenu();
            InitItem();
            InitSummonerSpell();
        }

        private static void InitItem()
        {
            Bilgewater = new Items.Item(ItemId.Bilgewater_Cutlass, 550);
            BotRuinedKing = new Items.Item(ItemId.Blade_of_the_Ruined_King, 550);
            Youmuu = new Items.Item(ItemId.Youmuus_Ghostblade, 0);
            Tiamat = new Items.Item(ItemId.Tiamat_Melee_Only, 400);
            Hydra = new Items.Item(ItemId.Ravenous_Hydra_Melee_Only, 400);
            Titanic = new Items.Item(3748, 0);
        }

        private static void InitMenu()
        {
            MainMenu = new Menu("QQ群438230879", "QQ群438230879", true).Attach();

            new Plugin.Vladimir();
        }

        private static void InitSummonerSpell()
        {
            foreach (var smite in Player.Spellbook.Spells.Where(i => (i.Slot == SpellSlot.Summoner1 || i.Slot == SpellSlot.Summoner2) && i.Name.ToLower().Contains("smite")))
            {
                Smite = smite.Slot;
                break;
            }
            Ignite = Player.GetSpellSlot("SummonerDot");
            Flash = Player.GetSpellSlot("SummonerFlash");
        }

        public static void Events_OnLoad(object sender, System.EventArgs e)
        {
            Task.Factory.StartNew(
                () =>
                {Events_OnLoad();
                    try
                    {

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            );
        }
    }
}