﻿namespace YuLeAzir
{
    using LeagueSharp;
    using LeagueSharp.Common;
    using System;
    using System.Linq;

    public static class Combo
    {
        public static Obj_AI_Hero Player { get{ return ObjectManager.Player; } }

        public static void Initialize()
        {
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Program._orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
                return;
            if (Soldiers.enemies.Any() && OrbwalkCommands.CanDoAttack())
            {
                var target = Soldiers.enemies.OrderByDescending(x => x.Health).LastOrDefault();
                OrbwalkCommands.AttackTarget(target);
            }
            if (Soldiers.splashautoattackchampions.Any() && OrbwalkCommands.CanDoAttack())
            {
                var splashAutoAttackChampion = Soldiers.splashautoattackchampions
                    .OrderByDescending(x => x.SplashAutoAttackChampions.MinOrDefault(y => y.Health).Health).LastOrDefault();
                if (splashAutoAttackChampion != null)
                {
                    var target = splashAutoAttackChampion.MainMinion;
                    if (target.IsValidTarget())
                        OrbwalkCommands.AttackTarget(target);
                }
            }
            if (!Soldiers.enemies.Any() && OrbwalkCommands.CanDoAttack())
            {
                var target = Program._orbwalker.GetTarget();
                if (target.IsValidTarget() && !target.IsZombie)
                {
                    OrbwalkCommands.AttackTarget(target);
                }
            }
            if (Program._q.IsReady() && OrbwalkCommands.CanMove() && Program.qcombo && (!Program.donotqcombo || (!Soldiers.enemies.Any() && !Soldiers.splashautoattackchampions.Any())))
            {
                var target = TargetSelector.GetTarget(Program._q.Range, TargetSelector.DamageType.Magical);
                foreach (var obj in Soldiers.soldier)
                {
                    Program._q.SetSkillshot(0.0f, 65f, 1500f, false, SkillshotType.SkillshotLine, obj.Position, Player.Position);
                    Program._q.Cast(target);
                }
            }
            if (Program._w.IsReady() && OrbwalkCommands.CanMove() && Program.wcombo)
            {
                var target = TargetSelector.GetTarget(Program._w.Range + 300, TargetSelector.DamageType.Magical);
                if (target.IsValidTarget() && !target.IsZombie && (!Soldiers.enemies.Contains(target) || Player.CountEnemiesInRange(1000) >= 2))
                {
                    var x = Player.Distance(target.Position) > Program._w.Range ? Player.Position.Extend(target.Position, Program._w.Range)
                        : target.Position;
                    Program._w.Cast(x);
                }
            }
            if (Program._w.IsReady() && OrbwalkCommands.CanMove() && !Soldiers.soldier.Any() && Program.wcombo && Program.Qisready())
            {
                var target = TargetSelector.GetTarget(Program._w.Range + 300, TargetSelector.DamageType.Magical);
                if (target == null || !target.IsValidTarget() || target.IsZombie)
                {
                    var tar = HeroManager.Enemies.Where(x => x.IsValidTarget(Program._q.Range) && !x.IsZombie).OrderByDescending(x => Player.Distance(x.Position)).LastOrDefault();
                    if (tar.IsValidTarget() && !tar.IsZombie)
                    {
                        var x = Player.Distance(tar.Position) > Program._w.Range ? Player.Position.Extend(tar.Position, Program._w.Range)
                            : tar.Position;
                        Program._w.Cast(x);
                    }
                }
            }
        }
    }
}
