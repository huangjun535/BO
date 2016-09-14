namespace YuLeKennen
{
    using LeagueSharp;
    using LeagueSharp.SDK;
    using LeagueSharp.SDK.UI;
    using YuLeKennen.Plugin;
    using YuLeLibrary;
    using System.Threading.Tasks;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Reflection;
    using System;

    internal class Program
    {
        internal const int IgniteRange = 600, FlashRange = 425;
        internal static Items.Item Bilgewater, BotRuinedKing, Youmuu, Tiamat, Hydra, Titanic;
        internal static SpellSlot Ignite = SpellSlot.Unknown, Flash = SpellSlot.Unknown;
        internal static Menu MainMenu;
        internal static Obj_AI_Hero Player;
        internal static Spell Q, W, E, R;

        private static void Main(string[] args)
        {
            Bootstrap.Init(args);
            Events.OnLoad += Events_OnLoad;
        }

        private static void Events_OnLoad()
        {
            if (GameObjects.Player.ChampionName != "Kennen")
                return;


            Player = GameObjects.Player;

            Init();
        }

        private static void Init()
        {
            MainMenu = new Menu("QQ群438230879", "QQ群438230879", true).Attach();

            new Kennen();

            Bilgewater = new Items.Item(ItemId.Bilgewater_Cutlass, 550);
            BotRuinedKing = new Items.Item(ItemId.Blade_of_the_Ruined_King, 550);
            Youmuu = new Items.Item(ItemId.Youmuus_Ghostblade, 0);
            Tiamat = new Items.Item(ItemId.Tiamat_Melee_Only, 400);
            Hydra = new Items.Item(ItemId.Ravenous_Hydra_Melee_Only, 400);
            Titanic = new Items.Item(3748, 0);

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