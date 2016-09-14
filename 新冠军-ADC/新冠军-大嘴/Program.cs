﻿namespace YuLeKogMaw
{
    using System;
    using System.Linq;
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;
    using YuLeLibrary;
    using System.Threading.Tasks;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Reflection;

    internal class Program
    {
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q, W, E, R;
        public static int tickIndex = 0;
        private static Obj_AI_Hero Player = ObjectManager.Player;

        static void Main(string[] args) { CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;}

        private static void GameOnOnGameLoad()
        {
            if (ObjectManager.Player.ChampionName != "KogMaw")
            {
                return;
            }

            Config = new Menu("新冠军大嘴Q群438230879", "YuLeKogMaw", true).SetFontStyle(System.Drawing.FontStyle.Regular, SharpDX.Color.Chartreuse); ;

            Config.AddSubMenu(new Menu("走砍 设置", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            new KogMaw().Load();

            Config.AddToMainMenu();

            Game.OnUpdate += OnUpdate;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
        }

        private static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (!Player.IsMelee && Common.CollisionYasuo(Player.ServerPosition, args.Target.Position))
            {
                args.Process = false;
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            tickIndex++;

            if (tickIndex > 4)
                tickIndex = 0;
        }

        public static bool LagFree(int offset)
        {
            if (tickIndex == offset)
                return true;
            else
                return false;
        }

        public static bool Farm { get { return (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear) || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed; } }

        public static bool Combo { get { return (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo); } }

        public static bool None { get { return (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None); } }

        private static void GameOnOnGameLoad(EventArgs args)
        {
            Task.Factory.StartNew(
                () =>
                {GameOnOnGameLoad();
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

        public static void CastSpell(Spell QWER, Obj_AI_Base target)
        {
            YuLeLibarary.Prediction.SkillshotType CoreType2 = YuLeLibarary.Prediction.SkillshotType.SkillshotLine;
            bool aoe2 = false;

            if (QWER.Width > 80 && !QWER.Collision)
                aoe2 = true;

            var predInput2 = new YuLeLibarary.Prediction.PredictionInput
            {
                Aoe = aoe2,
                Collision = QWER.Collision,
                Speed = QWER.Speed,
                Delay = QWER.Delay,
                Range = QWER.Range,
                From = ObjectManager.Player.ServerPosition,
                Radius = QWER.Width,
                Unit = target,
                Type = CoreType2
            };
            var poutput2 = YuLeLibarary.Prediction.Prediction.GetPrediction(predInput2);

            if (QWER.Speed != float.MaxValue && Common.CollisionYasuo(ObjectManager.Player.ServerPosition, poutput2.CastPosition))
                return;

            if (poutput2.Hitchance >= YuLeLibarary.Prediction.HitChance.VeryHigh)
                QWER.Cast(poutput2.CastPosition);
            else if (predInput2.Aoe && poutput2.AoeTargetsHitCount > 1 && poutput2.Hitchance >= YuLeLibarary.Prediction.HitChance.High)
            {
                QWER.Cast(poutput2.CastPosition);
            }
        }
    }
}