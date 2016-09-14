using LeagueSharp;
using LeagueSharp.Common;

namespace VayneHunter_Reborn.External
{
    class DamagePrediction
    {
        public delegate void OnKillableDelegate(Obj_AI_Hero sender, Obj_AI_Hero target, SpellData sData);
        public static event OnKillableDelegate OnSpellWillKill;

        static DamagePrediction()
        {
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            return;
        }

        static float GetDamage(Obj_AI_Hero hero, Obj_AI_Hero target, SpellSlot slot)
        {
            return (float)hero.GetSpellDamage(target, slot);
        }
    }
}
