﻿namespace YuLeJhin
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
        public static SebbyLib.Orbwalking.Orbwalker Orbwalker;
        public static int tickNum = 4, tickIndex = 0;
        private static Obj_AI_Hero Player = ObjectManager.Player;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        private static void GameOnOnGameLoad()
        {
            Config = new Menu("新冠军-戏命师", "新冠军-戏命师", true).SetFontStyle(System.Drawing.FontStyle.Regular, SharpDX.Color.Chartreuse);

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new SebbyLib.Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            new Jhin().Load();

            Config.AddToMainMenu();
            Game.OnUpdate += OnUpdate;
        }

        private static void OnUpdate(EventArgs args)
        {
            tickIndex++;

            if (tickIndex > 4)
                tickIndex = 0;

            AutoWard.Enable = Config.GetBool("AutoWard");
            AutoWard.AutoBuy = Config.GetBool("AutoBuy");
            AutoWard.AutoPink = Config.GetBool("AutoPink");
            AutoWard.OnlyCombo = Config.GetBool("AutoWardCombo");
            AutoWard.InComboMode = Orbwalker.ActiveMode == SebbyLib.Orbwalking.OrbwalkingMode.Combo;

            if (Config.Item("EnableSkin").GetValue<bool>())
            {
                ObjectManager.Player.SetSkin(ObjectManager.Player.CharData.BaseSkinName, Config.Item("SkinSelect").GetValue<StringList>().SelectedIndex);
            }
            else if (!Config.Item("EnableSkin").GetValue<bool>())
            {
                ObjectManager.Player.SetSkin(ObjectManager.Player.CharData.BaseSkinName, 0);
            }
        }

        public static bool LagFree(int offset)
        {
            if (tickIndex == offset)
                return true;
            else
                return false;
        }

        public static bool Farm { get { return (Orbwalker.ActiveMode == SebbyLib.Orbwalking.OrbwalkingMode.LaneClear) || Orbwalker.ActiveMode == SebbyLib.Orbwalking.OrbwalkingMode.Mixed || Orbwalker.ActiveMode == SebbyLib.Orbwalking.OrbwalkingMode.Freeze; } }

        public static bool None { get { return (Orbwalker.ActiveMode == SebbyLib.Orbwalking.OrbwalkingMode.None); } }

        public static bool Combo { get { return (Orbwalker.ActiveMode == SebbyLib.Orbwalking.OrbwalkingMode.Combo); } }

        public static bool LaneClear { get { return (Orbwalker.ActiveMode == SebbyLib.Orbwalking.OrbwalkingMode.LaneClear); } }

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
            SebbyLib.Movement.SkillshotType CoreType2 = SebbyLib.Movement.SkillshotType.SkillshotLine;
            bool aoe2 = false;

            if (QWER.Type == SkillshotType.SkillshotCircle)
            {
                //CoreType2 = SebbyLib.Movement.SkillshotType.SkillshotCircle;
                //aoe2 = true;
            }

            if (QWER.Width > 80 && !QWER.Collision)
                aoe2 = true;

            var predInput2 = new SebbyLib.Movement.PredictionInput
            {
                Aoe = aoe2,
                Collision = QWER.Collision,
                Speed = QWER.Speed,
                Delay = QWER.Delay,
                Range = QWER.Range,
                From = Player.ServerPosition,
                Radius = QWER.Width,
                Unit = target,
                Type = CoreType2
            };
            var poutput2 = SebbyLib.Movement.Prediction.GetPrediction(predInput2);

            //var poutput2 = QWER.GetPrediction(target);

            if (QWER.Speed != float.MaxValue && SebbyLib.OktwCommon.CollisionYasuo(Player.ServerPosition, poutput2.CastPosition))
                return;

            if (poutput2.Hitchance >= SebbyLib.Movement.HitChance.VeryHigh)
                QWER.Cast(poutput2.CastPosition);
            else if (predInput2.Aoe && poutput2.AoeTargetsHitCount > 1 && poutput2.Hitchance >= SebbyLib.Movement.HitChance.High)
            {
                QWER.Cast(poutput2.CastPosition);
            }
        }
    }
}