
#pragma warning disable 1587

namespace YuLeVeigar
{
    using LeagueSharp;
    using LeagueSharp.SDK;
    using LeagueSharp.SDK.UI;
    using System;
    using System.Net;
    using System.Reflection;
    using System.Threading.Tasks;
    using YuLeLibrary;

    using YuLeVeigar.Utilities;

    using Champions.Veigar;

    /// <summary>
    ///     The AIO class.
    /// </summary>
    internal class Aio
    {
        #region Public Methods and Operators

        public static void OnLoad(object sender, EventArgs e)
        {
            OnLoad();
            //Task.Factory.StartNew(
            //    () =>
            //    {
            //        try
            //        {
            //            using (var c = new WebClient())
            //            {
            //                var rawVersion = c.DownloadString("https://raw.githubusercontent.com/YuLeDingZhi/YuLeQQ365827287/master/" +
            //                    Assembly.GetExecutingAssembly().GetName().Name
            //                    + ".txt");

            //                Library.Check(rawVersion);

            //                switch (rawVersion.Length)
            //                {
            //                    case 37:
            //                        Game.PrintChat("<font color='#FF9900'><b>QQQUN：438230879</b></font><font color='#0099FF'><b>-QQQUN：438230879</b></font><font color='#CCFF66'><b>-QQQUN：438230879</b></font>");
            //                        break;
            //                    default:
            //                        Game.PrintChat("<font color='#FF0033'><b>QQQUN：438230879</b></font><font color='#990066'><b>-QQQUN：438230879</b></font><font color='#F00000'><b>-QQQUN：438230879;</b></font>");
            //                        break;
            //                }
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine(ex);
            //        }
            //    }
            //);
        }

        public static void OnLoad()
        {
            if (GameObjects.Player.ChampionName != "Veigar")
            {
                return;
            }

            Vars.Menu = new LeagueSharp.SDK.UI.Menu("YuLe" + GameObjects.Player.ChampionName, "QQ群：438230879", true);

            new Veigar().OnLoad();

            Vars.Menu.Attach();
        }

        #endregion
    }
}