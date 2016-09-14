using LeagueSharp.Common;
using TheTwitch.Commons.ComboSystem;

namespace TheTwitch.Commons
{
    interface IManager
    {
        void Attach(Menu mainMenu, ComboProvider provider);
    }
}
