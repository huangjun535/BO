namespace YuLeZed_Rework
{
    using LeagueSharp;
    using LeagueSharp.SDK;
    using LeagueSharp.SDK.UI;
    using System;
    using System.Linq;
    using YuLeLibrary;
    using System.Threading.Tasks;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Reflection;

    internal class Program
    {
        internal const int FlashRange = 425, IgniteRange = 600, SmiteRange = 570;
        internal static Items.Item Bilgewater, BotRuinedKing, Youmuu, Tiamat, Hydra, Titanic;
        internal static SpellSlot Flash = SpellSlot.Unknown, Ignite = SpellSlot.Unknown, Smite = SpellSlot.Unknown;
        internal static Menu MainMenu;
        internal static Obj_AI_Hero Player;
        internal static Spell Q, Q2, Q3, W, E, R;
        internal static string[] AutoEnableList =
        {
             "Annie", "Ahri", "Akali", "Anivia", "Annie", "Brand", "Cassiopeia", "Diana", "Evelynn", "FiddleSticks", "Fizz", "Gragas", "Heimerdinger", "Karthus",
             "Kassadin", "Katarina", "Kayle", "Kennen", "Leblanc", "Lissandra", "Lux", "Malzahar", "Mordekaiser", "Morgana", "Nidalee", "Orianna",
             "Ryze", "Sion", "Swain", "Syndra", "Teemo", "TwistedFate", "Veigar", "Viktor", "Vladimir", "Xerath", "Ziggs", "Zyra", "Velkoz", "Azir", "Ekko",
             "Ashe", "Caitlyn", "Corki", "Draven", "Ezreal", "Graves", "Jayce", "Jinx", "KogMaw", "Lucian", "MasterYi", "MissFortune", "Quinn", "Shaco", "Sivir",
             "Talon", "Tristana", "Twitch", "Urgot", "Varus", "Vayne", "Yasuo", "Zed", "Kindred", "AurelionSol"
        };

        private static void Main(string[] args)
        {
            Bootstrap.Init(args);
            Events.OnLoad += Events_OnLoad;
        }

        private static void Events_OnLoad()
        {
            Player = GameObjects.Player;

            if (GameObjects.Player.ChampionName.ToLower() != "zed")
                return;

            MainMenu = new Menu("QQ群：438230879", "QQ群：438230879", true).Attach();

            new Plugin.Zed();

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

        public static void Events_OnLoad(object sender, EventArgs e)
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