﻿using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace TheTwitch.Commons
{
    public static class Extensions
    {
        public static SpellState GetState(this SpellDataInst spellData)
        {
            switch ((int)spellData.State)
            {
                case 0:
                    return SpellState.Ready;
                case 2:
                    return SpellState.NotLearned;
                case 4:
                    return SpellState.Surpressed;
                case 5:
                    return SpellState.Cooldown;
                case 6:
                    return SpellState.NoMana;
                case 10:
                    return SpellState.Surpressed;
                default:
                    return SpellState.Unknown;
            }
        }

        public static SpellState GetState(this Spell spellData)
        {
            return spellData.Instance.GetState();
        }

        public static T ToEnum<T>(this string str)
        {
            return (T)Enum.Parse(typeof(T), str);
        }

        public static float GetHealthPercent(this Obj_AI_Hero entity, float health)
        {
            return health / entity.MaxHealth * 100f;
        }

        public static bool HasSpellShield(this Obj_AI_Hero entity)
        {
            return entity.HasBuff("bansheesveil") || entity.HasBuff("SivirE") || entity.HasBuff("NocturneW");
        }

        public static bool IsPoisoned(this Obj_AI_Base entity)
        {
            return entity.HasBuffOfType(BuffType.Poison);
        }

        public static float GetPoisonedTime(this Obj_AI_Base entity)
        {
            if (!entity.IsPoisoned()) return 0;
            return entity.Buffs.Where(buff => buff.Type == BuffType.Poison).OrderByDescending(poison => poison.EndTime).First().EndTime - Game.Time;
        }

        public static float GetIgniteDamage(this Obj_AI_Hero souce)
        {
            return 50 + souce.Level * 20;
        }

        public static float GetRemainingIgniteDamage(this Obj_AI_Base target)
        {
            var ignitebuff = target.GetBuff("summonerdot");
            if (ignitebuff == null) return 0;
            return (float)ObjectManager.Player.CalcDamage(target, Damage.DamageType.True, ((int)(ignitebuff.EndTime - Game.Time) + 1) * GetIgniteDamage(ignitebuff.Caster as Obj_AI_Hero) / 5);
        }

        public static bool IsFacingMe(this Obj_AI_Base source, float angle = 80)
        {
            if (source == null)
                return false;
            return source.Direction.To2D().Perpendicular().AngleBetween((ObjectManager.Player.Position - source.Position).To2D()) < angle;
        }

        public static bool NearFountain(this Obj_AI_Hero hero, float distance = 750)
        {
            var map = Utility.Map.GetMap();
            if (map != null && map.Type == Utility.Map.MapType.SummonersRift)
            {
                distance += 300;
            }
            return hero.IsVisible &&
                   ObjectManager.Get<Obj_SpawnPoint>()
                       .Any(sp => sp.Team == hero.Team && hero.Distance(sp.Position, true) < distance * distance);
        }

        public static bool IsBehindWindWall(this Obj_AI_Hero target, float delay = 0f, float radiusOrWidth = 0f, float speed = float.MaxValue, Prediction.SkillshotType testType = Prediction.SkillshotType.SkillshotLine)
        {
            return Prediction.Prediction.GetPrediction(new Prediction.PredictionInput
            {
                Collision = true,
                CollisionObjects = new[] { Prediction.CollisionableObjects.YasuoWall },
                Aoe = false,
                Delay = delay,
                From = ObjectManager.Player.ServerPosition,
                Radius = radiusOrWidth,
                Speed = speed,
                Type = testType,
                Unit = target
            }, true, true).Hitchance == Prediction.HitChance.Collision;
        }

        public static int GetBuffCountFixed(this Obj_AI_Base obj, string name)
        {
            return obj.GetBuffCount(name) < 2 ? obj.GetBuffCount(name) + 1 : obj.GetBuffCount(name);
        }

    }
}
