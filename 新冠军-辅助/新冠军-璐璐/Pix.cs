﻿using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using TreeLib.Extensions;
using Color = System.Drawing.Color;

namespace YuLeLuLu
{
    internal static class Pix
    {
        private const int EDuration = 6000;
        private static Obj_AI_Minion _instance;
        private static Render.Circle _circle;
        private static MenuItem _drawPix;
        private static int _lastECast;

        public static void Initialize(MenuItem drawPix)
        {
            _drawPix = drawPix;
            FindPix();
            _circle = new Render.Circle(Vector3.Zero, 50, Color.Purple);
            _circle.VisibleCondition += sender => _drawPix.IsActive();
            _circle.Add();

            Obj_AI_Base.OnBuffRemove += (sender, args) =>
            {
                if (args.Buff.SourceName == ObjectManager.Player.ChampionName &&
                    (args.Buff.Name == "luluevision" || args.Buff.Name == "lulufaerieshield"))
                {
                    _lastECast = 0;
                }
            };
            Game.OnUpdate += Game_OnUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.Slot == SpellSlot.E)
            {
                _lastECast = Utils.TickCount;
                SpellManager.PixQ.UpdateSourcePosition(args.Target.Position, args.Target.Position);
            }
        }

        public static List<Obj_AI_Base> GetMinions()
        {
            return MinionManager.GetMinions(
                _instance.ServerPosition, SpellManager.PixQ.Range, MinionTypes.All, MinionTeam.NotAlly);
        }

        public static MinionManager.FarmLocation GetFarmLocation()
        {
            var minions = GetMinions();
            return SpellManager.PixQ.GetLineFarmLocation(minions);
        }

        public static Obj_AI_Hero GetTarget(float range = 0)
        {
            var r = range == 0 ? SpellManager.Q.Range : range;
            return TargetSelector.GetTarget(r, TargetSelector.DamageType.Magical, true, null, _instance.ServerPosition);
        }

        public static Obj_AI_Base GetETarget(Obj_AI_Hero target)
        {
            return
                ObjectManager.Get<Obj_AI_Base>()
                    .Where(
                        o =>
                            o.IsValidTarget(SpellManager.E.Range, false, _instance.ServerPosition) &&
                            o.Distance(target) < 600 && (o.IsAlly || !SpellManager.Q.IsKillable(o)))
                    .OrderBy(o => o.Distance(target))
                    .ThenBy(o => o.Team != ObjectManager.Player.Team)
                    .FirstOrDefault();
        }

        public static bool IsValid()
        {
            return _instance != null && _instance.IsValid && _instance.DistanceToPlayer() < 1900;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }

            if (_instance == null)
            {
                FindPix();
                return;
            }

            _circle.Position = _instance.Position;
            SpellManager.PixQ.UpdateSourcePosition(_instance.ServerPosition, _instance.ServerPosition);
        }

        private static void FindPix()
        {
            _instance =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(m => m.IsValid && m.IsAlly && m.Name == "RobotBuddy")
                    .MinOrDefault(m => m.DistanceToPlayer());
        }
    }
}