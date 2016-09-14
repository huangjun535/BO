﻿using System;
using System.Linq;
using ClipperLib;
using iSeriesReborn.Utility.Positioning;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using VayneHunter_Reborn.Skills.Tumble;
using VayneHunter_Reborn.Skills.Tumble.VHRQ;
using VayneHunter_Reborn.Utility.Helpers;
using VayneHunter_Reborn.Utility.MenuUtility;
using Color = System.Drawing.Color;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

namespace VayneHunter_Reborn.Utility
{
    class DrawManager
    {
        public static void OnLoad()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            var drakeWallQPos = new Vector2(11514, 4462);
            var midWallQPos = new Vector2(6962, 8952);
            if (MenuExtensions.GetItemValue<bool>("dz191.vhr.draw.spots"))
            {
                if (ObjectManager.Player.Distance(drakeWallQPos) <= 1500f && PlayerHelper.IsSummonersRift())
                {
                //    Render.Circle.DrawCircle(new Vector2(12050, 4827).To3D(), 65f, Color.AliceBlue);
                }
                if (ObjectManager.Player.Distance(midWallQPos) <= 1500f && PlayerHelper.IsSummonersRift())
                {
                //    Render.Circle.DrawCircle(new Vector2(6962, 8952).To3D(), 65f, Color.AliceBlue);
                }
            }

            if (MenuExtensions.GetItemValue<bool>("dz191.vhr.draw.range"))
            {
                DrawEnemyZone();
            }

            if (MenuExtensions.GetItemValue<bool>("dz191.vhr.draw.qpos"))
            {
                var QPosition = VHRQLogic.GetVHRQPosition();
                Render.Circle.DrawCircle(QPosition, 35, Color.Yellow);
            }

            DrawCondemnRectangles();
        }

        private static void DrawCondemnRectangles()
        {
            if (!MenuExtensions.GetItemValue<bool>("dz191.vhr.draw.condemn") || !Variables.spells[SpellSlot.E].IsReady())
            {
                return;
            }

            var HeroList = HeroManager.Enemies.Where(
                                     h =>
                                         h.IsValidTarget(Variables.spells[SpellSlot.E].Range + 130f) &&
                                         !h.HasBuffOfType(BuffType.SpellShield) &&
                                         !h.HasBuffOfType(BuffType.SpellImmunity));
            //dz191.vhr.misc.condemn.rev.accuracy
            //dz191.vhr.misc.condemn.rev.nextprediction
            var MinChecksPercent = MenuExtensions.GetItemValue<Slider>("dz191.vhr.misc.condemn.accuracy").Value;
            var PushDistance = MenuExtensions.GetItemValue<Slider>("dz191.vhr.misc.condemn.pushdistance").Value;

            foreach (var Hero in HeroList)
            {
                if (MenuExtensions.GetItemValue<bool>("dz191.vhr.misc.condemn.onlystuncurrent") &&
                    Hero.NetworkId != Variables.Orbwalker.GetTarget().NetworkId)
                {
                    continue;
                }

                if (Hero.Health + 10 <=
                    ObjectManager.Player.GetAutoAttackDamage(Hero) *
                    MenuExtensions.GetItemValue<Slider>("dz191.vhr.misc.condemn.noeaa").Value)
                {
                    continue;
                }


                var targetPosition = Hero.Position;
                var finalPosition = targetPosition.Extend(ObjectManager.Player.ServerPosition, -PushDistance);
                var finalPosition_ex = Hero.ServerPosition.Extend(ObjectManager.Player.ServerPosition, -PushDistance);

                var condemnRectangle = new VHRPolygon(VHRPolygon.Rectangle(targetPosition.To2D(), finalPosition.To2D(), Hero.BoundingRadius));
                var condemnRectangle_ex = new VHRPolygon(VHRPolygon.Rectangle(Hero.ServerPosition.To2D(), finalPosition_ex.To2D(), Hero.BoundingRadius));

                var points = condemnRectangle.Points.Select(v2 => new IntPoint(v2.X, v2.Y)).ToList();
                var poly_ex = Helpers.Geometry.ToPolygon(points);

                if (
                    condemnRectangle.Points.Count(
                        point => NavMesh.GetCollisionFlags(point.X, point.Y).HasFlag(CollisionFlags.Wall)) >=
                    condemnRectangle.Points.Count() * (MinChecksPercent / 100f) &&
                    condemnRectangle_ex.Points.Count(
                        point => NavMesh.GetCollisionFlags(point.X, point.Y).HasFlag(CollisionFlags.Wall)) >=
                    condemnRectangle_ex.Points.Count() * (MinChecksPercent / 100f))
                {
                    poly_ex.Draw(Color.Chartreuse, 3);
                }
                else
                {
                    poly_ex.Draw(Color.Red, 3);
                }
            }
        }

        public static void DrawEnemyZone()
        {
            var currentPath = TumblePositioning.GetEnemyPoints().Select(v2 => new IntPoint(v2.X, v2.Y)).ToList();
            var currentPoly = Helpers.Geometry.ToPolygon(currentPath);
            currentPoly.Draw(Color.White);
        }
    }
}
