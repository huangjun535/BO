using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LeagueSharp;

namespace Activator
{
    public static class Updater
    {
        public static void UpdateCheck()
        {
            Task.Factory.StartNew(
                () =>
                {
                    try
                    {
                        // updater by h3h3
                        using (var c = new WebClient())
                        {
                            var rawVersion =
                                c.DownloadString(
                                    "https://raw.githubusercontent.com/xKurisu/Activator/master/Activator/Properties/AssemblyInfo.cs");

                            var match =
                                new Regex(
                                    @"\[assembly\: AssemblyVersion\(""(\d{1,})\.(\d{1,})\.(\d{1,})\.(\d{1,})""\)\]")
                                    .Match(rawVersion);

                            if (match.Success)
                            {
                                var gitVersion =
                                    new Version(
                                        string.Format(
                                            "{0}.{1}.{2}.{3}",
                                            match.Groups[1],
                                            match.Groups[2],
                                            match.Groups[3],
                                            match.Groups[4]));

                                if (gitVersion != Activator.Version)
                                {
                                    Game.PrintChat("<font color='#FF0033'><b>鍙嬫儏鎻愮ず</b></font><font color='#FF0033'><b>-鎮ㄧ殑娲诲寲鍓備笉鏄渶鏂扮増</b></font><font color='#FF0033'><b>-璇峰埌濞涙▊VIP鑴氭湰缇わ細\u0032&#49;&#53;&#50;&#50;\u0036\u0030&#56;&#54;鏇存柊</b></font> (" + gitVersion + ")");
                                }
                            }
                        }
                    }

                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
        }
    }
}
