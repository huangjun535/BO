using LeagueSharp;
using VayneHunter_Reborn.External.Activator;
using VayneHunter_Reborn.Utility.MenuUtility;

namespace VayneHunter_Reborn.Utility
{
    class VHRBootstrap
    {
        public static void OnLoad()
        {
            Variables.Menu = new LeagueSharp.Common.Menu("娛樂制作-新冠军薇恩","dz191.vhr", true);

            SPrediction.Prediction.Initialize(Variables.Menu);
            MenuGenerator.OnLoad();
            Activator.OnLoad();
            VHR.OnLoad();
            DrawManager.OnLoad();
        }
    }
}
