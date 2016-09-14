namespace YuLeEzreal
{
    using System;
    using System.Linq;
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;
    using System.Threading.Tasks;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Reflection;

    internal class Program
    {
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;
        public static int tickIndex = 0;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        private static void GameOnOnGameLoad()
        {
            Config = new Menu("新冠军-伊泽", "新冠军-伊泽", true).SetFontStyle(System.Drawing.FontStyle.Regular, SharpDX.Color.Chartreuse);

            Config.AddSubMenu(new Menu("走砍设置", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            new Ezreal().Load();

            Config.AddToMainMenu();
            Game.OnUpdate += OnUpdate;
        }

        private static void OnUpdate(EventArgs args)
        {
            tickIndex++;

            if (tickIndex > 4)
                tickIndex = 0;

            AutoWard.Enable = Config.Item("AutoWard", true).GetValue<bool>();
            AutoWard.AutoBuy = Config.Item("AutoBuy", true).GetValue<bool>();
            AutoWard.AutoPink = Config.Item("AutoPink", true).GetValue<bool>();
            AutoWard.OnlyCombo = Config.Item("AutoWardCombo", true).GetValue<bool>();
            AutoWard.InComboMode = Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo;

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

        public static bool Farm { get { return Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Freeze; } }

        public static bool None { get { return (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None); } }

        public static bool Combo { get { return (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo); } }

        public static bool LaneClear { get { return (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear); } }

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
            SebbyLib.Prediction.SkillshotType CoreType2 = SebbyLib.Prediction.SkillshotType.SkillshotLine;
            bool aoe2 = false;

            if (QWER.Width > 80 && !QWER.Collision)
                aoe2 = true;

            var predInput2 = new SebbyLib.Prediction.PredictionInput
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
            var poutput2 = SebbyLib.Prediction.Prediction.GetPrediction(predInput2);

            if (QWER.Speed != float.MaxValue && SebbyLib.OktwCommon.CollisionYasuo(ObjectManager.Player.ServerPosition, poutput2.CastPosition))
                return;

            if (poutput2.Hitchance >= SebbyLib.Prediction.HitChance.VeryHigh)
                QWER.Cast(poutput2.CastPosition);
            else if (predInput2.Aoe && poutput2.AoeTargetsHitCount > 1 && poutput2.Hitchance >= SebbyLib.Prediction.HitChance.High)
            {
                QWER.Cast(poutput2.CastPosition);
            }
        }
    }
}