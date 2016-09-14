using LeagueSharp;
using LeagueSharp.Common;

namespace TheTwitch.Commons.Items
{
    public interface IActivateableItem
    {
        void Initialize(Menu menu, ItemManager itemManager);
        string GetDisplayName();
        void Update(Obj_AI_Hero target);
        void Use(Obj_AI_Base target);
        int GetRange();
        TargetSelector.DamageType GetDamageType();
    }
}
