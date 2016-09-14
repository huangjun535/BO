using System;
using System.Collections.Generic;

using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Enumerations;
using LeagueSharp.SDK.Utils;
using NLog;
using YuLeLibrary;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using System.Reflection;
using SharpDX;
using System.Linq;

namespace YuLeAshe
{
    internal static class Program
    {
        private const string ChampName = "Ashe";

        internal static void Main(string[] args)
        {
            Bootstrap.Init();
            Events.OnLoad += Load_OnLoad;
        }

        private static void Load_OnLoad()
        {
            if (!ObjectManager.Player.ChampionName.Equals(ChampName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            Config.Initialize();
            SpellManager.Initialize();
            ModeManager.Initialize();

            Drawing.OnDraw += Drawing_OnDraw;
            Variables.Orbwalker.OnAction += Orbwalker_OnAction;
            Events.OnGapCloser += Events_OnGapCloser;
            Events.OnInterruptableTarget += Events_OnInterruptableTarget;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Config.Auto.AutoE.UseEKey && SpellManager.E.IsReady())
            {
                SpellManager.E.Cast(ObjectManager.Player.Position.Extend(Game.CursorPos, 10000));
            }

            if (Config.Misc.RKey.UseRKey && SpellManager.R.IsReady())
            {
                var target = Variables.TargetSelector.GetTarget(2000, DamageType.Magical);

                if (target != null && target.IsHPBarRendered)
                    SpellManager.R.Cast(target);
            }

            setbool();
        }

        private static void Load_OnLoad(object sender, EventArgs e)
        {
            Task.Factory.StartNew(
                () =>
                {Load_OnLoad();
                    try
                    {
                        
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            );
        }

        private static void setbool()
        {
            YuLeLibrary.AutoWard.Enable = Config.AutoWards.AutoWard;
            YuLeLibrary.AutoWard.AutoBuy = Config.AutoWards.AutoBuy;
            YuLeLibrary.AutoWard.AutoPink = Config.AutoWards.AutoPink;
            YuLeLibrary.AutoWard.OnlyCombo = Config.AutoWards.AutoWardCombo;
            YuLeLibrary.AutoWard.InComboMode = Variables.Orbwalker.ActiveMode == OrbwalkingMode.Combo;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Drawings.DrawWRange)
            {
                Render.Circle.DrawCircle(GameObjects.Player.Position, SpellManager.W.Range, System.Drawing.Color.DeepSkyBlue);
            }

            if (Config.Drawings.DrawRRange)
            {
                Render.Circle.DrawCircle(GameObjects.Player.Position, SpellManager.R.Range, System.Drawing.Color.DeepSkyBlue);
            }
        }

        private static void Orbwalker_OnAction(object sender, OrbwalkingActionArgs e)
        {
            switch (e.Type)
            {
                case OrbwalkingType.None:
                    break;
                case OrbwalkingType.Movement:
                    break;
                case OrbwalkingType.StopMovement:
                    break;
                case OrbwalkingType.BeforeAttack:
                    break;
                case OrbwalkingType.AfterAttack:
                    Orbwalker_AfterAttack(e);
                    break;
                case OrbwalkingType.OnAttack:
                    break;
                case OrbwalkingType.NonKillableMinion:
                    break;
            }
        }

        private static void Orbwalker_AfterAttack(OrbwalkingActionArgs e)
        {
            var targetAsHero = e.Target as Obj_AI_Hero;

            switch (Variables.Orbwalker.ActiveMode)
            {
                case OrbwalkingMode.Combo:
                    if (targetAsHero == null)
                    {
                        break;
                    }

                    if (Config.Modes.Combo.UseQ && SpellManager.Q.IsReady() && targetAsHero.InAutoAttackRange())
                    {
                        SpellManager.Q.Cast();
                    }
                    break;
                case OrbwalkingMode.Hybrid:
                    if (targetAsHero == null)
                    {
                        break;
                    }

                    if (Config.Modes.Harass.UseQ && SpellManager.Q.IsReady() && targetAsHero.InAutoAttackRange())
                    {
                        SpellManager.Q.Cast();
                    }
                    break;
                case OrbwalkingMode.LaneClear:
                    LaneC();
                    JungleC();
                    break;
            }
        }

        private static void LaneC()
        {
            var Mins = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(SpellManager.W.Range));

            if (Mins.Count() > 0)
            {
                if (Config.Modes.LaneClear.UseQ && SpellManager.Q.IsReady() && GameObjects.Player.ManaPercent > Config.Modes.LaneClear.MinMana)
                {
                    SpellManager.Q.Cast();
                }

                if (Config.Modes.LaneClear.UseW && SpellManager.W.IsReady() && GameObjects.Player.ManaPercent > Config.Modes.LaneClear.MinMana)
                {
                    SpellManager.W.Cast(Mins.FirstOrDefault().Position);
                }
            }
        }

        private static void JungleC()
        {
            var Jung = GameObjects.Jungle.Where(x => x.IsValidTarget(SpellManager.W.Range));

            if (Jung.Count() > 0)
            {
                if (Config.Modes.JungleClear.UseQ && SpellManager.Q.IsReady() && GameObjects.Player.ManaPercent > Config.Modes.JungleClear.MinMana)
                {
                    SpellManager.Q.Cast();
                }

                if (Config.Modes.JungleClear.UseW && SpellManager.W.IsReady() && GameObjects.Player.ManaPercent > Config.Modes.JungleClear.MinMana)
                {
                    SpellManager.W.Cast(Jung.FirstOrDefault().Position);
                }
            }
        }

        private static void Events_OnGapCloser(object sender, Events.GapCloserEventArgs e)
        {
            if (Config.Misc.AntiGapcloser.UseRGapcloser && SpellManager.R.IsReady() && e.Sender.IsValidTarget(SpellManager.R.Range))
            {
                SpellManager.R.Cast(e.Sender);
            }
        }

        private static void Events_OnInterruptableTarget(object sender, Events.InterruptableTargetEventArgs e)
        {
            if (Config.Misc.AutoInterrupt.UseRInterrupt && SpellManager.R.IsReady() && e.Sender.IsValidTarget(SpellManager.R.Range))
            {
                SpellManager.R.Cast(e.Sender);
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.Slot == SpellSlot.Q)
            {
                Variables.Orbwalker.ResetSwingTimer();
            }

            if (Config.Auto.AutoE.UseEFlash && sender.IsEnemy && args.SData.Name.Equals("summonerflash", StringComparison.OrdinalIgnoreCase) && sender.Distance(GameObjects.Player) <= 2250)
            {
                SpellManager.E.Cast(args.End);
            }
        }
    }
}
