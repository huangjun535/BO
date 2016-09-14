﻿/*
 Copyright 2015 - 2015 SPrediction
 Drawings.cs is part of SPrediction
 
 SPrediction is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.
 
 SPrediction is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 GNU General Public License for more details.
 
 You should have received a copy of the GNU General Public License
 along with SPrediction. If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SPrediction
{
    /// <summary>
    /// SPrediction Drawings class
    /// </summary>
    public static class Drawings
    {
        #region Internal Properties

        internal static string s_DrawHitChance;
        internal static Vector2 s_DrawPos;
        internal static Vector2 s_DrawDirection;
        internal static int s_DrawTick;
        internal static int s_DrawWidth;

        #endregion

        #region Private Properties

        private static int s_HitCount;
        private static int s_CastCount;
        private static List<Tuple<string, int>> LastSpells;

        #endregion

        #region Initializer Method

        public static void Initialize()
        {
            LastSpells = new List<Tuple<string, int>>();
            s_DrawTick = 0;

            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            AttackableUnit.OnDamage += Obj_AI_Hero_OnDamage;
            Game.OnEnd += Game_OnGameEnd;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// OnDraw event for prediction drawings
        /// </summary>
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (ConfigMenu.SelectedPrediction.SelectedIndex == 0 && ConfigMenu.EnableDrawings)
            {
                foreach (Obj_AI_Hero enemy in HeroManager.Enemies)
                {
                    var waypoints = enemy.GetWaypoints();
                    if (waypoints != null && waypoints.Count > 1)
                    {
                        for (int i = 0; i < waypoints.Count - 1; i++)
                        {
                            Vector2 posFrom = Drawing.WorldToScreen(waypoints[i].To3D());
                            Vector2 posTo = Drawing.WorldToScreen(waypoints[i + 1].To3D());
                            Drawing.DrawLine(posFrom, posTo, 2, System.Drawing.Color.Aqua);
                        }

                        Vector2 pos = Drawing.WorldToScreen(waypoints[waypoints.Count - 1].To3D());
                        Drawing.DrawText(pos.X, pos.Y, System.Drawing.Color.Black, (waypoints.PathLength() / enemy.MoveSpeed).ToString("0.00")); //arrival time
                    }
                }

                if (Utils.TickCount - s_DrawTick <= 2000)
                {
                    Vector2 centerPos = Drawing.WorldToScreen((s_DrawPos - s_DrawDirection * 5).To3D());
                    Vector2 startPos = Drawing.WorldToScreen((s_DrawPos - s_DrawDirection * s_DrawWidth).To3D());
                    Vector2 endPos = Drawing.WorldToScreen((s_DrawPos + s_DrawDirection * s_DrawWidth).To3D());
                    Drawing.DrawLine(startPos, endPos, 3, System.Drawing.Color.Gold);
                    Drawing.DrawText(centerPos.X, centerPos.Y, System.Drawing.Color.Red, s_DrawHitChance);
                }

                Drawing.DrawText(ConfigMenu.HitChanceDrawingX, ConfigMenu.HitChanceDrawingY, System.Drawing.Color.Red, String.Format("Casted Spell Count: {0}", s_CastCount));
                Drawing.DrawText(ConfigMenu.HitChanceDrawingX, ConfigMenu.HitChanceDrawingY + 20, System.Drawing.Color.Red, String.Format("Hit Spell Count: {0}", s_HitCount));
                Drawing.DrawText(ConfigMenu.HitChanceDrawingX, ConfigMenu.HitChanceDrawingY + 40, System.Drawing.Color.Red, String.Format("Hitchance (%): {0}%", s_CastCount > 0 ? (((float)s_HitCount / s_CastCount) * 100).ToString("00.00") : "n/a"));
            }
        }

        /// <summary>
        /// OnDamage event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void Obj_AI_Hero_OnDamage(AttackableUnit sender, AttackableUnitDamageEventArgs args)
        {
            LastSpells.RemoveAll(p => Environment.TickCount - p.Item2 > 2000);
            if (args.SourceNetworkId == ObjectManager.Player.NetworkId && HeroManager.Enemies.Exists(p => p.NetworkId == args.TargetNetworkId))
            {
                if (LastSpells.Count != 0)
                {
                    LastSpells.RemoveAt(0);
                    s_HitCount++;
                }
            }
        }

        /// <summary>
        /// OnProcessSpellCast event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            LastSpells.RemoveAll(p => Environment.TickCount - p.Item2 > 2000);
            if (sender.IsMe && !args.SData.IsAutoAttack() && ConfigMenu.CountHitChance)
            {
                if (args.Slot == SpellSlot.Q && !LastSpells.Exists(p => p.Item1 == args.SData.Name))
                {
                    LastSpells.Add(new Tuple<string, int>(args.SData.Name, Environment.TickCount));
                    s_CastCount++;
                }
            }
        }

        /// <summary>
        /// OnGameEnd event
        /// </summary>
        /// <param name="args"></param>
        private static void Game_OnGameEnd(EventArgs args)
        {
            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache", String.Format("sprediction_{0}_{1}_{2}.txt", ObjectManager.Player.ChampionName, DateTime.Now.ToString("dd-MM"), Environment.TickCount.ToString("x8")));
            File.WriteAllText(file,
                String.Format("Champion : {1}{0}Casted Spell Count: {2}{0}Hit Spell Count: {3}{0}Hitchance(%) : {4}{0}",
                Environment.NewLine,
                ObjectManager.Player.ChampionName,
                s_CastCount,
                s_HitCount,
                s_CastCount > 0 ? (((float)s_HitCount / s_CastCount) * 100).ToString("00.00") : "n/a"));
        }
        #endregion
    }
}
