namespace YuLeZilean
{
    using LeagueSharp.Common;

    internal class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Zilean.GameOnOnGameLoad;
        }
    }
}