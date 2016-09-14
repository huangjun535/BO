namespace Eat_My_Cass
{
    using YuLeCassiopeia;

    using LeagueSharp.Common;

    /// <summary>
    /// The program.
    /// </summary>
    public class Program
    {
        #region Methods

        /// <summary>
        /// Mains the specified arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += new YuLeCassiopeia().OnLoader;
        }

        #endregion
    }
}