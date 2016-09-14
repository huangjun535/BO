﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace OneKeyToWin_AIO_Sebby.Core
{
    class OktwTs
    {
        private Menu Config = Program.Config;
        private static SebbyLib.Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        private Obj_AI_Hero FocusTarget, DrawInfo = null;
        private float LatFocusTime = Game.Time;

        public void LoadOKTW()
        {
            Config.SubMenu("Target Selector OKTW©").AddItem(new MenuItem("TsAa", "Auto-attack MODE:").SetValue(new StringList(new[] { "Fast kill", "Priority", "Common TS" }, 2)));

            // FAST KILL //////////////////////////
            Config.SubMenu("Target Selector OKTW©").AddItem(new MenuItem("extraFocus", "One Focus To Win").SetValue(Player.IsMelee));
            Config.SubMenu("Target Selector OKTW©").AddItem(new MenuItem("extraRang", "Extra Focus Range").SetValue(new Slider(300, 0, 600)));
            Config.SubMenu("Target Selector OKTW©").AddItem(new MenuItem("extraTime", "Time out focus time (ms)").SetValue(new Slider(2000, 0, 4000)));
            Config.SubMenu("Target Selector OKTW©").AddItem(new MenuItem("drawFocus", "Draw notification").SetValue(true));

            int i = 5;

            foreach (var enemy in HeroManager.Enemies.OrderBy(enemy => enemy.MaxHealth / Player.GetAutoAttackDamage(enemy)))
            {
                Config.SubMenu("Target Selector OKTW©").AddItem(new MenuItem("TsAaPriority" + enemy.ChampionName, enemy.ChampionName).SetValue(new Slider(i, 0, 5))).DontSave();
                i--;
            }
            SebbyLib.Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            SebbyLib.Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Game.OnUpdate += OnUpdate;
            Game.OnWndProc += Game_OnWndProc;
            Drawing.OnDraw += OnDraw;
        }

        private void OnDraw(EventArgs args)
        {
            if (Config.Item("TsAa").GetValue<StringList>().SelectedIndex == 2)
            {
                return;
            }
            if (DrawInfo.IsValidTarget() && (int)(Game.Time * 10) % 2 == 0 && Config.Item("drawFocus").GetValue<bool>())
            {
                Utility.DrawCircle(Player.Position, Player.AttackRange + Player.BoundingRadius + Config.Item("extraRang").GetValue<Slider>().Value, System.Drawing.Color.Gray, 1, 1);

                drawText("FORCE FOCUS", DrawInfo.Position, System.Drawing.Color.Orange);
            }
        }

        public static void drawText(string msg, Vector3 Hero, System.Drawing.Color color, int weight = 0)
        {
            var wts = Drawing.WorldToScreen(Hero);
            Drawing.DrawText(wts[0] - (msg.Length) * 5, wts[1] + weight, color, msg);
        }

        private void Game_OnWndProc(WndEventArgs args)
        {
            //Program.debug("WND: " + args.WParam);
            if(args.WParam == 16)
            {
                var priority = Config.Item("TsAa").GetValue<StringList>().SelectedIndex == 1;
                foreach (var enemy in HeroManager.Enemies)
                {
                    Config.Item("TsAaPriority" + enemy.ChampionName).Show(priority);

                }

                Config.Item("extraFocus").Show(!priority);
                Config.Item("extraRang").Show(!priority && Config.Item("extraFocus").GetValue<bool>());
                Config.Item("extraTime").Show(!priority && Config.Item("extraFocus").GetValue<bool>());
                Config.Item("drawFocus").Show(!priority && Config.Item("extraFocus").GetValue<bool>());

                if(Config.Item("TsAa").GetValue<StringList>().SelectedIndex == 2)
                {
                    foreach (var enemy in HeroManager.Enemies)
                    {
                        Config.Item("TsAaPriority" + enemy.ChampionName).Show(false);

                    }
                    Config.Item("extraFocus").Show(false);
                    Config.Item("extraRang").Show(false);
                    Config.Item("extraTime").Show(false);
                    Config.Item("drawFocus").Show(false);
                }
            }
        }

        private void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if(target is Obj_AI_Hero)
            {
                FocusTarget = (Obj_AI_Hero)target;
                LatFocusTime = Game.Time;
            }
        }

        private void Orbwalking_BeforeAttack(SebbyLib.Orbwalking.BeforeAttackEventArgs args)
        {
            if (Config.Item("TsAa").GetValue<StringList>().SelectedIndex != 0 || !Config.Item("extraFocus").GetValue<bool>() || !Program.Combo)
            {
                DrawInfo = null;
                return;
            }

            if (args.Target is Obj_AI_Hero )
            {
                var newTarget = (Obj_AI_Hero)args.Target;
                var forceFocusEnemy = newTarget;
               // if (newTarget.Health / Player.GetAutoAttackDamage(newTarget) > FocusTarget.Health / Player.GetAutoAttackDamage(FocusTarget))
                {

                }
               // else
                {
                    var aaRange = Player.AttackRange + Player.BoundingRadius + Config.Item("extraRang").GetValue<Slider>().Value;

                    foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(aaRange)))
                    {
                        if (enemy.Health / Player.GetAutoAttackDamage(enemy) + 1 < forceFocusEnemy.Health / Player.GetAutoAttackDamage(forceFocusEnemy))
                        {
                            forceFocusEnemy = enemy;
                        }
                    }
                }
                if(forceFocusEnemy.NetworkId != newTarget.NetworkId && Game.Time - LatFocusTime < Config.Item("extraTime").GetValue<Slider>().Value /1000)
                {
                    args.Process = false;
                    Program.debug("Focus: " + forceFocusEnemy.ChampionName);
                    DrawInfo = forceFocusEnemy;
                    return;
                }
            }
            DrawInfo = null;
        }

        private void OnUpdate(EventArgs args)
        {
            if (Config.Item("TsAa").GetValue<StringList>().SelectedIndex == 2 || !SebbyLib.Orbwalking.CanAttack() || !Program.Combo)
            {
                return;
            }

            var orbT = Orbwalker.GetTarget();
           
            if (orbT != null)
            {
                var bestTarget = (Obj_AI_Hero)orbT;
                var hitToBestTarget = bestTarget.Health / Player.GetAutoAttackDamage(bestTarget);

                if (Config.Item("TsAa").GetValue<StringList>().SelectedIndex == 0)
                {
                    foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget() && Orbwalker.InAutoAttackRange(enemy)))
                    {
                        if (enemy.Health / Player.GetAutoAttackDamage(enemy) < hitToBestTarget)
                        {
                            bestTarget = enemy;
                        }
                    }
                }
                else
                {
                    foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget() && Orbwalker.InAutoAttackRange(enemy)))
                    {

                        if(enemy.Health / Player.GetAutoAttackDamage(enemy) < 3)
                        {
                            bestTarget = enemy;
                            break;
                        }
                        if (Config.Item("TsAaPriority" + enemy.ChampionName).GetValue<Slider>().Value > Config.Item("TsAaPriority" + bestTarget.ChampionName).GetValue<Slider>().Value)
                        {
                            bestTarget = enemy;
                        }
                        
                    }
                }
                if (bestTarget.NetworkId != orbT.NetworkId)
                {
                    Program.debug("force " + bestTarget.ChampionName);
                    Orbwalker.ForceTarget(bestTarget);
                }
            }
        }
    }
}
