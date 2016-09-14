namespace YuLeBlitzcrank
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
        private static Menu Config;
        private static Orbwalking.Orbwalker Orbwalker;
        private static Spell Q, W, E, R;
        private static int tickIndex = 0, grab = 0, grabS = 0;
        private static Obj_AI_Hero Player;
        private static float grabW = 0;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        private static void GameOnOnGameLoad()
        {
            if (ObjectManager.Player.ChampionName != "Blitzcrank")
                return;

            Player = ObjectManager.Player;

            Q = new Spell(SpellSlot.Q, 920);
            W = new Spell(SpellSlot.W, 200);
            E = new Spell(SpellSlot.E, 475);
            R = new Spell(SpellSlot.R, 600);
            Q.SetSkillshot(0.25f, 80f, 1800f, true, SkillshotType.SkillshotLine);

            Config = new Menu("QQ群438230879", "YuLeBlitzcrank", true);

            Config.AddSubMenu(new Menu("走砍设置", "走砍设置"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("走砍设置"));

            Config.AddSubMenu(new Menu("拉人设置", "拉人设置"));
            Config.SubMenu("拉人设置").AddItem(new MenuItem("qCombo123", "假如你发现对面有人开躲避,请打开他的英雄名字", true));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                Config.SubMenu("拉人设置").AddItem(new MenuItem("grab" + enemy.ChampionName, enemy.ChampionName).SetValue(false));

            Config.AddSubMenu(new Menu("连招设置", "连招设置"));
            Config.SubMenu("连招设置").AddItem(new MenuItem("qCombo", "自动Q敌人进塔底下", true).SetValue(true));
            Config.SubMenu("连招设置").AddItem(new MenuItem("rCount", "Auto R if enemies in range", true).SetValue(new Slider(3, 0, 5)));
            Config.SubMenu("连招设置").AddItem(new MenuItem("rKs", "R ks", true).SetValue(false));

            Config.AddSubMenu(new Menu("自动使用", "自动使用"));
            Config.SubMenu("自动使用").AddItem(new MenuItem("qTur", "自动Q敌人进塔底下", true).SetValue(true));
            Config.SubMenu("自动使用").AddItem(new MenuItem("qCC", "自动Q无法移动的敌人", true).SetValue(true));
            Config.SubMenu("自动使用").AddItem(new MenuItem("autoW", "Q中后自动W", true).SetValue(true));
            Config.SubMenu("自动使用").AddItem(new MenuItem("afterGrab", "Q中后自动R", true).SetValue(true));
            Config.SubMenu("自动使用").AddItem(new MenuItem("autoE", "攻击前自动E", true).SetValue(true));
            Config.SubMenu("自动使用").AddItem(new MenuItem("afterAA", "攻击前自动R", true).SetValue(true));

            Config.AddSubMenu(new Menu("打断反突", "打断反突"));
            Config.SubMenu("打断反突").AddItem(new MenuItem("inter", "自动R打断技能", true)).SetValue(true);
            Config.SubMenu("打断反突").AddItem(new MenuItem("Gap", "自动R反突进", true)).SetValue(true);

            Config.AddSubMenu(new Menu("自动眼位", "自动眼位"));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWard", "启动", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoBuy", "lv9自动买灯泡", true).SetValue(false));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoPink", "自动真眼扫描", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWardCombo", "仅连招模式启动 ", true).SetValue(true));
            new YuLeLibrary.AutoWard().Load();
            new YuLeLibrary.Tracker().Load();

            Config.AddSubMenu(new Menu("显示设置", "显示设置"));
            Config.SubMenu("显示设置").AddItem(new MenuItem("qRange", "Q 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("rRange", "R 范围", true).SetValue(false));

            Config.AddToMainMenu();

            Game.OnUpdate += delegate
            {
                tickIndex++;

                if (tickIndex > 4)
                    tickIndex = 0;

                YuLeLibrary.AutoWard.Enable = Config.Item("AutoWard", true).GetValue<bool>();
                YuLeLibrary.AutoWard.AutoBuy = Config.Item("AutoBuy", true).GetValue<bool>();
                YuLeLibrary.AutoWard.AutoPink = Config.Item("AutoPink", true).GetValue<bool>();
                YuLeLibrary.AutoWard.OnlyCombo = Config.Item("AutoWardCombo", true).GetValue<bool>();
                YuLeLibrary.AutoWard.InComboMode = Combo;
            };
            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalking.BeforeAttack += BeforeAttack;
            Orbwalking.AfterAttack += afterAttack;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;

        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (LagFree(1) && Q.IsReady())
                LogicQ();
            if (LagFree(2) && R.IsReady())
                LogicR();
            if (LagFree(3) && W.IsReady() && Config.Item("autoW", true).GetValue<bool>())
                LogicW();

            if (!Q.IsReady() && Game.Time - grabW > 2)
            {
                foreach (var t in HeroManager.Enemies.Where(t => t.HasBuff("rocketgrab2")))
                {
                    grabS++;
                    grabW = Game.Time;
                }
            }
        }

        private static void LogicW()
        {
            foreach (var target in HeroManager.Enemies.Where(target => target.IsValidTarget(R.Range) && target.HasBuff("rocketgrab2")))
                W.Cast();
        }

        private static void LogicR()
        {
            bool rKs = Config.Item("rKs", true).GetValue<bool>();
            bool afterGrab = Config.Item("afterGrab", true).GetValue<bool>();
            foreach (var target in HeroManager.Enemies.Where(target => target.IsValidTarget(R.Range)))
            {
                if (rKs && R.GetDamage(target) > target.Health)
                    R.Cast();
                if (afterGrab && target.IsValidTarget(400) && target.HasBuff("rocketgrab2"))
                    R.Cast();
            }
            if (Player.CountEnemiesInRange(R.Range) >= Config.Item("rCount", true).GetValue<Slider>().Value && Config.Item("rCount", true).GetValue<Slider>().Value > 0)
                R.Cast();
        }

        private static void LogicQ()
        {
            var qTur = Player.UnderAllyTurret() && Config.Item("qTur", true).GetValue<bool>();
            var qCC = Config.Item("qCC", true).GetValue<bool>();

            foreach (var t in HeroManager.Enemies.Where(t => t.IsValidTarget(Q.Range)))
            {
                if (!t.HasBuffOfType(BuffType.SpellImmunity) && !t.HasBuffOfType(BuffType.SpellShield) && Player.Distance(t.ServerPosition) > 225 && !Config.Item("grab" + t.ChampionName).GetValue<bool>())
                {
                    if (Combo && Config.Item("qCombo", true).GetValue<bool>())
                        CastSpell(Q, t);
                    else if (qTur)
                        CastSpell(Q, t);

                    if (qCC)
                    {
                        if (!Common.CanMove(t))
                            Q.Cast(t, true);
                        Q.CastIfHitchanceEquals(t, HitChance.Dashing);
                        Q.CastIfHitchanceEquals(t, HitChance.Immobile);
                    }
                }
            }
        }

        private static void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (E.IsReady() && args.Target.IsValid<Obj_AI_Hero>() && Config.Item("autoE", true).GetValue<bool>())
                E.Cast();
        }

        private static void afterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (Config.Item("afterAA", true).GetValue<bool>() && R.IsReady() && target is Obj_AI_Hero)
            {
                R.Cast();
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (R.IsReady() && Config.Item("Gap", true).GetValue<bool>() && gapcloser.Sender.IsValidTarget(R.Range))
                R.Cast();
        }

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (R.IsReady() && Config.Item("inter", true).GetValue<bool>() && sender.IsValidTarget(R.Range))
                R.Cast();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Config.Item("qRange", true).GetValue<bool>())
            {
                if (Q.IsReady())
                    Render.Circle.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Cyan, 1);
            }
            if (Config.Item("rRange", true).GetValue<bool>())
            {
                if (R.IsReady())
                    Render.Circle.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Gray, 1);
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == "RocketGrabMissile")
            {
                Utility.DelayAction.Add(500, Orbwalking.ResetAutoAttackTimer);
                grab++;
            }
        }

        public static bool LagFree(int offset)
        {
            if (tickIndex == offset)
                return true;
            else
                return false;
        }

        public static bool Combo { get { return (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo); } }

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
