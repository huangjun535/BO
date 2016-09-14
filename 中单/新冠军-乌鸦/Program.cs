namespace YuLeSwain
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
        public static int tickNum = 4, tickIndex = 0;
        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell E, Q, R, W;
        public static float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;
        public static bool Ractive = false;

        private static string[] Spells =
        {
            "katarinar","drain","consume","absolutezero", "staticfield","reapthewhirlwind","jinxw","jinxr","shenstandunited","threshe","threshrpenta","threshq","meditate","caitlynpiltoverpeacemaker", "volibearqattack",
            "cassiopeiapetrifyinggaze","ezrealtrueshotbarrage","galioidolofdurand","luxmalicecannon", "missfortunebullettime","infiniteduress","alzaharnethergrasp","lucianq","velkozr","rocketgrabmissile"
        };

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        private static void GameOnOnGameLoad()
        {
            if (ObjectManager.Player.ChampionName != "Swain")
                return;

            Q = new Spell(SpellSlot.Q, 700);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, 625);
            R = new Spell(SpellSlot.R, 675);

            Q.SetSkillshot(0.5f, 200f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(1.5f, 240f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            Config = new Menu("QQ群：438230879", "QQ群：438230879", true).SetFontStyle(System.Drawing.FontStyle.Regular, SharpDX.Color.Pink);

            Config.AddSubMenu(new Menu("走砍设置", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.SubMenu("Q 设置").AddItem(new MenuItem("autoQ", "自动 Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("harrasQ", "骚扰 Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("jungleQ", "清野 Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("Qlist", " 使用Q对象", true));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
                Config.SubMenu("Q 设置").AddItem(new MenuItem("Quse" + enemy.ChampionName, enemy.ChampionName, true).SetValue(true));

            Config.SubMenu("W 设置").AddItem(new MenuItem("WmodeCombo", "连招 W 模式", true).SetValue(new StringList(new[] { "always", "落点检测" }, 1)));
            Config.SubMenu("W 设置").AddItem(new MenuItem("farmW", "清线 W", true).SetValue(true));
            Config.SubMenu("W 设置").AddItem(new MenuItem("WMana", "清线 W 最低蓝量", true).SetValue(new Slider(80, 100, 0)));
            Config.SubMenu("W 设置").AddItem(new MenuItem("LCWminions", "清线 W 最低命中小兵数", true).SetValue(new Slider(3, 10, 0)));
            Config.SubMenu("W 设置").AddItem(new MenuItem("jungleW", "清野 W", true).SetValue(true));
            Config.SubMenu("W 设置").AddItem(new MenuItem("autoW", "敌人被控制自动W", true).SetValue(true));
            Config.SubMenu("W 设置").AddItem(new MenuItem("Wspell", "自动W危险技能", true).SetValue(true));
            Config.SubMenu("W 设置").AddItem(new MenuItem("Int", "自动W打断技能", true).SetValue(true));
            Config.SubMenu("W 设置").AddItem(new MenuItem("Waoe", "自动W 多个目标", true).SetValue(new Slider(3, 5, 0)));
            Config.SubMenu("W 设置").AddItem(new MenuItem("WmodeGC", "反突进模式", true).SetValue(new StringList(new[] { "敌人落点位置", "我的预判位置" }, 0)));
            Config.SubMenu("W 设置").AddItem(new MenuItem("Qlist", " 使用W对象", true));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
                Config.SubMenu("W 设置").AddItem(new MenuItem("WGCchampion" + enemy.ChampionName, enemy.ChampionName, true).SetValue(true));

            Config.SubMenu("E 设置").AddItem(new MenuItem("autoE", "自动 E", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("harrasE", "骚扰 E", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("jungleE", "清野 E", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("elist", " 使用E对象", true));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
                Config.SubMenu("E 设置").AddItem(new MenuItem("Euse" + enemy.ChampionName, enemy.ChampionName, true).SetValue(true));

            Config.SubMenu("R 设置").AddItem(new MenuItem("autoR", "自动 R", true).SetValue(true));
            Config.SubMenu("R 设置").AddItem(new MenuItem("Raoe", "自动R|附近敌人数", true).SetValue(new Slider(2, 5, 1)));
            Config.SubMenu("R 设置").AddItem(new MenuItem("harrasR", "骚扰 R", true).SetValue(true));
            Config.SubMenu("R 设置").AddItem(new MenuItem("farmR", "清线 R", true).SetValue(true));
            Config.SubMenu("R 设置").AddItem(new MenuItem("RMana", "清线 R 最低蓝量", true).SetValue(new Slider(80, 100, 0)));
            Config.SubMenu("R 设置").AddItem(new MenuItem("LCminions", "清线 R 最低命中小兵数", true).SetValue(new Slider(3, 10, 0)));
            Config.SubMenu("R 设置").AddItem(new MenuItem("jungleR", "清野 R", true).SetValue(true));

            Config.AddSubMenu(new Menu("自动眼位", "自动眼位"));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWard", "启动", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoBuy", "lv9自动买灯泡", true).SetValue(false));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoPink", "自动真眼扫描", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWardCombo", "仅连招模式启动 ", true).SetValue(true));
            new YuLeLibrary.AutoWard().Load();
            new YuLeLibrary.Tracker().Load();

            Config.SubMenu("显示设置").AddItem(new MenuItem("qRange", "Q 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("wRange", "W 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("eRange", "E 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("rRange", "R 范围", true).SetValue(false));

            Config.AddToMainMenu();

            Game.OnUpdate += SwitchTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
        }

        private static void LogicR()
        {
            if (Ractive)
            {
                if (LaneClear && Config.Item("farmR", true).GetValue<bool>())
                {
                    var allMinions = YuLeLibrary.Cache.GetMinions(Player.Position, R.Range);
                    var mobs = YuLeLibrary.Cache.GetMinions(Player.Position, R.Range, MinionTeam.Neutral);
                    if (mobs.Count > 0)
                    {
                        if (!Config.Item("jungleR", true).GetValue<bool>())
                        {
                            R.Cast();
                        }
                    }
                    else if (allMinions.Count > 0)
                    {
                        if (allMinions.Count < 2 || Player.ManaPercent < Config.Item("RMana", true).GetValue<Slider>().Value)
                            R.Cast();
                        else if (Player.ManaPercent < Config.Item("Mana", true).GetValue<Slider>().Value)
                            R.Cast();
                    }
                    else
                        R.Cast();

                }
                else if ((Player.Position.CountEnemiesInRange(R.Range + 400) == 0 || Player.Mana < EMANA) && ((Farm && Config.Item("farmR", true).GetValue<bool>()) || None))
                {
                    R.Cast();
                }
            }
            else
            {
                var countAOE = Player.CountEnemiesInRange(R.Range);
                if (countAOE > 0)
                {
                    if (Combo && Config.Item("autoR", true).GetValue<bool>())
                        R.Cast();
                    else if (Farm && Config.Item("harrasR", true).GetValue<bool>())
                        R.Cast();
                    else if (countAOE >= Config.Item("Raoe", true).GetValue<Slider>().Value)
                        R.Cast();
                }
                if (LaneClear && Player.ManaPercent > Config.Item("RMana", true).GetValue<Slider>().Value && Config.Item("farmR", true).GetValue<bool>())
                {
                    var allMinions = YuLeLibrary.Cache.GetMinions(Player.ServerPosition, R.Range);

                    if (allMinions.Count >= Config.Item("LCminions", true).GetValue<Slider>().Value)
                        R.Cast();
                }
            }
        }

        private static void LogicW()
        {
            var t = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            if (t.IsValidTarget())
            {
                if (Combo)
                {
                    if (Config.Item("WmodeCombo", true).GetValue<StringList>().SelectedIndex == 1)
                    {
                        if (W.GetPrediction(t).CastPosition.Distance(t.Position) > 100)
                        {
                            if (Player.Position.Distance(t.ServerPosition) > Player.Position.Distance(t.Position))
                            {
                                if (t.Position.Distance(Player.ServerPosition) < t.Position.Distance(Player.Position))
                                    CastSpell(W, t);
                            }
                            else
                            {
                                if (t.Position.Distance(Player.ServerPosition) > t.Position.Distance(Player.Position))
                                    CastSpell(W, t);
                            }
                        }
                    }
                    else
                    {
                        CastSpell(W, t);
                    }
                }

                W.CastIfWillHit(t, Config.Item("Waoe", true).GetValue<Slider>().Value);
            }
            else if (LaneClear && Player.ManaPercent > Config.Item("WMana", true).GetValue<Slider>().Value && Config.Item("farmW", true).GetValue<bool>())
            {
                var minionList = YuLeLibrary.Cache.GetMinions(Player.ServerPosition, W.Range);
                var farmPosition = W.GetCircularFarmLocation(minionList, W.Width);

                if (farmPosition.MinionsHit > Config.Item("LCWminions", true).GetValue<Slider>().Value)
                    W.Cast(farmPosition.Position);
            }

            if (Config.Item("autoW", true).GetValue<bool>())
                foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(W.Range) && !YuLeLibrary.Common.CanMove(enemy)))
                    W.Cast(enemy, true);

        }

        private static void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (t.IsValidTarget())
            {
                if (t.Health < YuLeLibrary.Common.GetKsDamage(t, Q) + E.GetDamage(t))
                    Q.Cast(t);
                if (!Config.Item("Quse" + t.ChampionName, true).GetValue<bool>())
                    return;
                if (Combo && Player.Mana > RMANA + EMANA)
                    Q.Cast(t);
                else if (Farm && Config.Item("harrasQ", true).GetValue<bool>() && Player.Mana > RMANA + EMANA + WMANA + EMANA)
                    Q.Cast(t);
                else if ((Combo || Farm))
                {
                    foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range) && !YuLeLibrary.Common.CanMove(enemy)))
                        Q.Cast(enemy);
                }
            }
        }

        private static void LogicE()
        {
            var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (t.IsValidTarget())
            {

                if (t.Health < E.GetDamage(t) + YuLeLibrary.Common.GetKsDamage(t, Q))
                    E.CastOnUnit(t);
                if (!Config.Item("Euse" + t.ChampionName, true).GetValue<bool>())
                    return;
                if (Combo && Player.Mana > RMANA + EMANA)
                    E.CastOnUnit(t);
                else if (Farm && Config.Item("harrasE", true).GetValue<bool>() && Player.Mana > RMANA + EMANA + WMANA + EMANA)
                    E.CastOnUnit(t);
            }
        }

        private static void Jungle()
        {
            if (LaneClear)
            {
                var mobs = YuLeLibrary.Cache.GetMinions(Player.ServerPosition, Q.Range, MinionTeam.Neutral);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];

                    if (W.IsReady() && Config.Item("jungleW", true).GetValue<bool>())
                    {
                        W.Cast(mob.ServerPosition);
                        return;
                    }
                    if (E.IsReady() && Config.Item("jungleE", true).GetValue<bool>())
                    {
                        E.CastOnUnit(mob);
                        return;
                    }
                    if (Q.IsReady() && Config.Item("jungleQ", true).GetValue<bool>())
                    {
                        Q.CastOnUnit(mob);
                        return;
                    }
                    if (R.IsReady() && Config.Item("jungleR", true).GetValue<bool>() && !Ractive)
                    {
                        R.Cast();
                        return;
                    }
                }
            }
        }

        private static void SetMana()
        {

            QMANA = Q.Instance.ManaCost;
            WMANA = W.Instance.ManaCost;
            EMANA = E.Instance.ManaCost;

            if (!R.IsReady())
                RMANA = WMANA - Player.PARRegenRate * W.Instance.Cooldown;
            else
                RMANA = R.Instance.ManaCost;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("qRange", true).GetValue<bool>())
            {
                if (Q.IsReady())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan, 1);
            }
            if (Config.Item("wRange", true).GetValue<bool>())
            {
                if (W.IsReady())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.Orange, 1);
            }
            if (Config.Item("eRange", true).GetValue<bool>())
            {
                if (E.IsReady())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Yellow, 1);
            }
            if (Config.Item("rRange", true).GetValue<bool>())
            {
                if (R.IsReady())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.Gray, 1);
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (LagFree(0))
            {
                SetMana();
                Ractive = Player.HasBuff("SwainMetamorphism");
                Jungle();
            }

            if (LagFree(1) && E.IsReady() && Config.Item("autoE", true).GetValue<bool>())
                LogicE();

            if (LagFree(2) && Q.IsReady() && Config.Item("autoQ", true).GetValue<bool>())
                LogicQ();

            if (LagFree(3) && W.IsReady())
                LogicW();

            if (LagFree(4) && R.IsReady() && Config.Item("autoR", true).GetValue<bool>())
                LogicR();
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (W.IsReady() && Player.Mana > RMANA + WMANA)
            {
                var t = gapcloser.Sender;
                if (t.IsValidTarget(W.Range) && Config.Item("WGCchampion" + t.ChampionName, true).GetValue<bool>())
                {
                    if (Config.Item("WmodeGC", true).GetValue<StringList>().SelectedIndex == 0)
                        W.Cast(gapcloser.End);
                    else
                        W.Cast(Player.ServerPosition);
                }
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!W.IsReady() || sender.IsMinion || !sender.IsEnemy || !Config.Item("Wspell", true).GetValue<bool>() || !sender.IsValid<Obj_AI_Hero>() || !sender.IsValidTarget(W.Range))
                return;

            var foundSpell = Spells.Find(x => args.SData.Name.ToLower() == x);
            if (foundSpell != null)
            {
                W.Cast(sender.Position);
            }
        }

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (W.IsReady() && Config.Item("Int", true).GetValue<bool>() && sender.IsValidTarget(W.Range))
                W.Cast(sender.Position);
        }

        private static void SwitchTick(EventArgs args)
        {
            tickIndex++;

            if (tickIndex > 4)
                tickIndex = 0;

            YuLeLibrary.AutoWard.Enable = Config.Item("AutoWard", true).GetValue<bool>();
            YuLeLibrary.AutoWard.AutoBuy = Config.Item("AutoBuy", true).GetValue<bool>();
            YuLeLibrary.AutoWard.AutoPink = Config.Item("AutoPink", true).GetValue<bool>();
            YuLeLibrary.AutoWard.OnlyCombo = Config.Item("AutoWardCombo", true).GetValue<bool>();
            YuLeLibrary.AutoWard.InComboMode = Combo;
        }

        public static bool LagFree(int offset)
        {
            if (tickIndex == offset)
                return true;
            else
                return false;
        }

        public static bool Farm { get { return (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && Config.Item("harassLaneclear").GetValue<bool>()) || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed; } }

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