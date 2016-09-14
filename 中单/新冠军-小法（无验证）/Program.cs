
#pragma warning disable 1587

namespace YuLeVeigar
{
    using LeagueSharp.SDK;

    internal class Program
    {
        #region Methods

        /// <summary>
        ///     The entry point of the application.
        /// </summary>
        private static void Main()
        {
            Bootstrap.Init();
            Events.OnLoad += Aio.OnLoad;
        }

        #endregion
    }
}