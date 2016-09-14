namespace YuLeVladimir_Rework.Core
{
    using LeagueSharp;
    using LeagueSharp.Data.Enumerations;
    using LeagueSharp.SDK;
    using LeagueSharp.SDK.Enumerations;
    using LeagueSharp.SDK.UI;
    using LeagueSharp.SDK.Utils;
    using SharpDX;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;
    using Menu = LeagueSharp.SDK.UI.Menu;

    internal static class Common
    {
        internal static int cBlank = -1;

        internal static bool CanFlash => Program.Flash != SpellSlot.Unknown && Program.Flash.IsReady();

        internal static bool CanIgnite => Program.Ignite != SpellSlot.Unknown && Program.Ignite.IsReady();

        internal static bool CanSmite => Program.Smite != SpellSlot.Unknown && Program.Smite.IsReady();

        private static int GetSmiteDmg => new[] { 390, 410, 430, 450, 480, 510, 540, 570, 600, 640, 680, 720, 760, 800, 850, 900, 950, 1000 }[Program.Player.Level - 1];

        public static MenuBool Bool(this Menu subMenu, string name, string display, bool state = true)
        {
            return subMenu.Add(new MenuBool(name, display, state));
        }

        public static MenuKeyBind KeyBind(this Menu subMenu, string name, string display, Keys key, KeyBindType type = KeyBindType.Press)
        {
            return subMenu.Add(new MenuKeyBind(name, display, key, type));
        }

        public static MenuList List(this Menu subMenu, string name, string display, string[] array, int value = 0)
        {
            return subMenu.Add(new MenuList<string>(name, display, array) { Index = value });
        }

        public static MenuSeparator Separator(this Menu subMenu, string display)
        {
            cBlank += 1;
            return subMenu.Add(new MenuSeparator("blank" + cBlank, display));
        }

        public static MenuSlider Slider(this Menu subMenu, string name, string display, int cur, int min = 0, int max = 100)
        {
            return subMenu.Add(new MenuSlider(name, display, cur, min, max));
        }

        internal static bool CanHitCircle(this Spell spell, Obj_AI_Base unit)
        {
            return spell.IsInRange(spell.GetPredPosition(unit));
        }

        internal static bool CanLastHit(this Spell spell, Obj_AI_Base unit, double dmg, double subDmg = 0)
        {
            var hpPred = spell.GetHealthPrediction(unit);
            return hpPred > 0 && hpPred - subDmg < dmg;
        }

        internal static CastStates Casting(this Spell spell, Obj_AI_Base unit, bool aoe = false, CollisionableObjects collisionable = CollisionableObjects.Minions | CollisionableObjects.YasuoWall)
        {
            if (!unit.IsValidTarget())
            {
                return CastStates.InvalidTarget;
            }

            if (!spell.IsReady())
            {
                return CastStates.NotReady;
            }

            if (spell.CastCondition != null && !spell.CastCondition())
            {
                return CastStates.FailedCondition;
            }

            var pred = spell.GetPrediction(unit, aoe, -1, collisionable);

            if (pred.CollisionObjects.Count > 0)
            {
                return CastStates.Collision;
            }

            if (spell.RangeCheckFrom.DistanceSquared(pred.CastPosition) > spell.RangeSqr)
            {
                return CastStates.OutOfRange;
            }

            if (pred.Hitchance < spell.MinHitChance && (!pred.Input.AoE || pred.Hitchance < HitChance.High || pred.AoeTargetsHitCount < 2))
            {
                return CastStates.LowHitChance;
            }

            if (!Program.Player.Spellbook.CastSpell(spell.Slot, pred.CastPosition))
            {
                return CastStates.NotCasted;
            }

            spell.LastCastAttemptT = Variables.TickCount;

            return CastStates.SuccessfullyCasted;
        }

        internal static CastStates CastingBestTarget(this Spell spell, bool aoe = false, CollisionableObjects collisionable = CollisionableObjects.Minions | CollisionableObjects.YasuoWall)
        {
            return spell.Casting(spell.GetTarget(spell.Width / 2), aoe, collisionable);
        }

        internal static void CastSpellSmite(this Spell spell, Obj_AI_Hero target, bool smiteCol)
        {
            var pred1 = spell.GetPrediction(target, false, -1, CollisionableObjects.YasuoWall);

            if (pred1.Hitchance < spell.MinHitChance)
            {
                return;
            }

            var pred2 = spell.GetPrediction(target, false, -1, CollisionableObjects.Minions);

            if (pred2.Hitchance == HitChance.Collision)
            {
                if (smiteCol && CastSmiteKillCollision(pred2.CollisionObjects))
                {
                    spell.Cast(pred1.CastPosition);
                }
            }
            else if (pred2.Hitchance >= spell.MinHitChance)
            {
                spell.Cast(pred2.CastPosition);
            }
        }

        internal static Vector3 GetPredPosition(this Spell spell, Obj_AI_Base unit, bool useRange = false)
        {
            var pos = Movement.GetPrediction(unit, spell.Delay, 1, spell.Speed).UnitPosition;

            return useRange && !spell.IsInRange(pos) ? unit.ServerPosition : pos;
        }

        internal static bool IsCasted(this CastStates state)
        {
            return state == CastStates.SuccessfullyCasted;
        }

        internal static bool IsWard(this Obj_AI_Minion minion)
        {
            return minion.GetMinionType().HasFlag(MinionTypes.Ward) && minion.CharData.BaseSkinName != "BlueTrinket";
        }

        internal static List<Obj_AI_Base> ListEnemies(bool includeClones = false)
        {
            var list = new List<Obj_AI_Base>();

            list.AddRange(GameObjects.EnemyHeroes);
            list.AddRange(ListMinions(includeClones));

            return list;
        }

        internal static List<Obj_AI_Minion> ListMinions(bool includeClones = false)
        {
            var list = new List<Obj_AI_Minion>();

            list.AddRange(GameObjects.Jungle);
            list.AddRange(GameObjects.EnemyMinions.Where(i => i.IsMinion() || i.IsPet(includeClones)));

            return list;
        }

        private static bool CastSmiteKillCollision(List<Obj_AI_Base> col)
        {
            if (col.Count > 1 || !CanSmite)
            {
                return false;
            }

            var obj = col.First();

            return obj.Health <= GetSmiteDmg && obj.DistanceToPlayer() < Program.SmiteRange && Program.Player.Spellbook.CastSpell(Program.Smite, obj);
        }
    }
}