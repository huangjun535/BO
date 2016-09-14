using LeagueSharp.Common;

namespace iTwitch
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var twitch = new Twitch();
            CustomEvents.Game.OnGameLoad += twitch.OnGameLoad;
        }
    }
}