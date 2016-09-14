namespace YuLeKogMaw
{
    using LeagueSharp;
    using LeagueSharp.Common;

    public class Humanizer
    {
        private int ExpireTime;

        public Humanizer(int lifespan)
        {
            ExpireTime = Utils.TickCount + lifespan;
        }


        public bool ShouldDestroy
        {
            get
            {
                return !ObjectManager.Player.HasBuff("KogMawBioArcaneBarrage") || Utils.TickCount > ExpireTime;
            }
        }
    }
}