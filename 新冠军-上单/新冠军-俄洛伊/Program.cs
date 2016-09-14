using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using YuLeLibrary;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using System.Reflection;


namespace YuLeIllaoi
{
    class Program
    {
        public static Spell Q,W,E,R;
        private static readonly Obj_AI_Hero Illaoi = ObjectManager.Player;
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;
        public static string[] HighChamps =
            {
                "Ahri", "Anivia", "Annie", "Ashe", "Azir", "Brand", "Caitlyn", "Cassiopeia", "Corki", "Draven",
                "Ezreal", "Graves", "Jinx", "Kalista", "Karma", "Karthus", "Katarina", "Kennen", "KogMaw", "Leblanc",
                "Lucian", "Lux", "Malzahar", "MasterYi", "MissFortune", "Orianna", "Quinn", "Sivir", "Syndra", "Talon",
                "Teemo", "Tristana", "TwistedFate", "Twitch", "Varus", "Vayne", "Veigar", "VelKoz", "Viktor", "Xerath",
                "Zed", "Ziggs","Kindred"
            };

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad()
        {
            if (Illaoi.ChampionName != "Illaoi")
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 850);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 900);
            R = new Spell(SpellSlot.R, 450);

            Q.SetSkillshot(.484f, 0, 500, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(.066f, 50, 1900, true, SkillshotType.SkillshotLine);

            Config = new Menu("QQ群438230879", "QQ群438230879", true).SetFontStyle(System.Drawing.FontStyle.Regular, SharpDX.Color.Pink);
            {
                Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("走砍设置"));

                var harassMenu = new Menu("Harass Settings", "Harass Settings");
                {

                    harassMenu.AddItem(new MenuItem("harass.mana", "Mana Manager").SetValue(new Slider(20, 1, 99)));
                    Config.AddSubMenu(harassMenu);
                }

                var qMenu = Config.AddSubMenu(new Menu("Q 设置", "Q Settings"));
                {
                    qMenu.AddItem(new MenuItem("q.combo", "连招中使用").SetValue(true));
                    qMenu.AddItem(new MenuItem("q.ghost.combo", "连招中使用 (幽魂模式)").SetValue(true));
                    qMenu.AddItem(new MenuItem("q.harass", "骚扰中使用").SetValue(true));
                    qMenu.AddItem(new MenuItem("q.ghost.harass", "骚扰中使用 (幽魂模式)").SetValue(true));
                    qMenu.AddItem(new MenuItem("q.clear", "清线中使用").SetValue(true));
                    qMenu.AddItem(new MenuItem("q.minion.hit", "清线中使用 -> 最低命中数").SetValue(new Slider(3, 1, 6)));
                    qMenu.AddItem(new MenuItem("q.ks", "击杀中使用").SetValue(true));
                }

                var wMenu = Config.AddSubMenu(new Menu("W 设置", "W Settings"));
                {
                    wMenu.AddItem(new MenuItem("w.combo", "连招中使用").SetValue(true));
                    wMenu.AddItem(new MenuItem("w.combos", "连招中使用 -> 智能").SetValue(true));
                    wMenu.AddItem(new MenuItem("w.harass", "骚扰中使用").SetValue(true));
                    wMenu.AddItem(new MenuItem("w.jg", "清野中使用").SetValue(true));
                    wMenu.AddItem(new MenuItem("w.ks", "击杀中使用").SetValue(true));
                }

                var eMenu = Config.AddSubMenu(new Menu("E 设置", "E Settings"));
                {
                    eMenu.AddItem(new MenuItem("e.combo", "连招中使用").SetValue(true));
                    eMenu.AddItem(new MenuItem("e.harass", "骚扰中使用").SetValue(true));
                    eMenu.AddItem(new MenuItem("e.whte", "E使用对象"));
                    foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(o => o.IsEnemy))
                        eMenu.AddItem(new MenuItem("enemy." + enemy.CharData.BaseSkinName, enemy.ChampionName).SetValue(HighChamps.Contains(enemy.ChampionName)));
                }

                var rMenu = Config.AddSubMenu(new Menu("R 设置", "R Settings"));
                {
                    rMenu.AddItem(new MenuItem("r.combo", "连招中使用").SetValue(true));
                    rMenu.AddItem(new MenuItem("r.min.hit", "连招中使用 -> 最低命中数").SetValue(new Slider(3, 1, 5)));
                    rMenu.AddItem(new MenuItem("r.min.hp", "连招中使用 -> 最低生命百分比").SetValue(new Slider(30, 10, 40)));
                    rMenu.AddItem(new MenuItem("r.ks", "击杀中使用").SetValue(true));
                }

                Config.AddSubMenu(new Menu("自动眼位", "自动眼位"));
                Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWard", "启动", true).SetValue(true));
                Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoBuy", "lv9自动买灯泡", true).SetValue(true));
                Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoPink", "自动真眼扫描", true).SetValue(true));
                Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWardCombo", "仅连招模式启动 ", true).SetValue(true));
                new AutoWard().Load();
                new Tracker().Load();

                var drawMenu = Config.AddSubMenu(new Menu("显示 设置", "Draw Settings"));
                {
                    drawMenu.AddItem(new MenuItem("q.draw", "Q 范围").SetValue(new Circle(false, Color.White)));
                    drawMenu.AddItem(new MenuItem("w.draw", "W 范围").SetValue(new Circle(false, Color.Gold)));
                    drawMenu.AddItem(new MenuItem("e.draw", "E 范围").SetValue(new Circle(false, Color.DodgerBlue)));
                    drawMenu.AddItem(new MenuItem("r.draw", "R 范围").SetValue(new Circle(false, Color.GreenYellow)));
                    drawMenu.AddItem(new MenuItem("aa.indicator", "平A伤害").SetValue(new Circle(false, Color.Gold)));
                    drawMenu.AddItem(new MenuItem("passive.draw", "被动显示").SetValue(new Circle(false, Color.Gold)));
                }

                Config.AddToMainMenu();
            }
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender != null && sender.IsMe && args.Slot == SpellSlot.W)
            {
                Orbwalking.ResetAutoAttackTimer();
            }
        }

        private static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe)
            {
                switch (Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        {
                            if (target.Type == GameObjectType.obj_AI_Hero)
                            {
                                if (Config.Item("w.combo").GetValue<bool>() && W.IsReady())
                                {
                                    if (target.IsValidTarget(W.Range))
                                    {
                                        W.Cast();
                                    }
                                }
                            }
                        }
                        break;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        {
                            if (MinionManager.GetMinions(float.MaxValue, MinionTypes.All, MinionTeam.Neutral).Any(x => x.NetworkId == target.NetworkId))
                            {
                                if (Config.Item("w.jg").GetValue<bool>() && W.IsReady())
                                {
                                    if (target.IsValidTarget(W.Range))
                                    {
                                        W.Cast();
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Task.Factory.StartNew(
                () =>
                {Game_OnGameLoad();
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

        private static void Drawing_OnDraw(EventArgs args)
        {
            var menuItem1 = Config.Item("q.draw").GetValue<Circle>();
            var menuItem2 = Config.Item("w.draw").GetValue<Circle>();
            var menuItem3 = Config.Item("e.draw").GetValue<Circle>();
            var menuItem4 = Config.Item("r.draw").GetValue<Circle>();
            var menuItem5 = Config.Item("aa.indicator").GetValue<Circle>();
            var menuItem6 = Config.Item("passive.draw").GetValue<Circle>();
            if (menuItem1.Active && Q.IsReady())
            {
                Render.Circle.DrawCircle(new Vector3(Illaoi.Position.X, Illaoi.Position.Y, Illaoi.Position.Z), Q.Range, menuItem1.Color, 5);
            }
            if (menuItem2.Active && W.IsReady())
            {
                Render.Circle.DrawCircle(new Vector3(Illaoi.Position.X, Illaoi.Position.Y, Illaoi.Position.Z), W.Range, menuItem2.Color, 5);
            }
            if (menuItem3.Active && E.IsReady())
            {
                Render.Circle.DrawCircle(new Vector3(Illaoi.Position.X, Illaoi.Position.Y, Illaoi.Position.Z), E.Range, menuItem3.Color, 5);
            }
            if (menuItem4.Active && R.IsReady())
            {
                Render.Circle.DrawCircle(new Vector3(Illaoi.Position.X, Illaoi.Position.Y, Illaoi.Position.Z), R.Range, menuItem4.Color, 5);
            }
            if (menuItem4.Active)
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(1500) && x.IsValid && x.IsVisible && !x.IsDead && !x.IsZombie))
                {
                    Drawing.DrawText(enemy.HPBarPosition.X, enemy.HPBarPosition.Y, menuItem5.Color, string.Format("{0} Basic Attack = Kill", AaIndicator(enemy)));
                }
            }
            if (menuItem5.Active)
            {
                var enemy = HeroManager.Enemies.FirstOrDefault(x => x.IsValidTarget(2000));
                foreach (var passive in ObjectManager.Get<Obj_AI_Minion>().Where(x=> x.Name == "God"))
                {
                    Render.Circle.DrawCircle(new Vector3(passive.Position.X, passive.Position.Y, passive.Position.Z), 850, menuItem5.Color, 2);
                    if (enemy != null)
                    {
                        var xx = Drawing.WorldToScreen(passive.Position.Extend(enemy.Position, 850));
                        var xy = Drawing.WorldToScreen(passive.Position);
                        Drawing.DrawLine(xy.X, xy.Y, xx.X, xx.Y, 5, Color.Gold);
                    }
                    
                }
            }
        }
        private static int AaIndicator(Obj_AI_Hero enemy)
        {
            double aCalculator = ObjectManager.Player.CalcDamage(enemy, Damage.DamageType.Physical, Illaoi.TotalAttackDamage());
            double killableAaCount = enemy.Health / aCalculator;
            int totalAa = (int)Math.Ceiling(killableAaCount);
            return totalAa;
        }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;

                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;

                case Orbwalking.OrbwalkingMode.LaneClear:
                    Clear();
                    break;
            }

            KillSteal(args);

            AutoWard(args);
        }

        private static void AutoWard(EventArgs args)
        {
            YuLeLibrary.AutoWard.Enable = Config.Item("AutoWard", true).GetValue<bool>();
            YuLeLibrary.AutoWard.AutoBuy = Config.Item("AutoBuy", true).GetValue<bool>();
            YuLeLibrary.AutoWard.AutoPink = Config.Item("AutoPink", true).GetValue<bool>();
            YuLeLibrary.AutoWard.OnlyCombo = Config.Item("AutoWardCombo", true).GetValue<bool>();
            YuLeLibrary.AutoWard.InComboMode = Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo;
        }

        private static void KillSteal(EventArgs args)
        {
            if (Q.IsReady() && Config.Item("q.ks").GetValue<bool>())
            {
                var enemy = HeroManager.Enemies.FirstOrDefault(x => x.IsValidTarget(Q.Range) && x.Health < Q.GetDamage(x));
                var enemyGhost = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(x => x.Name == enemy.Name);
                if (enemy != null && enemyGhost == null)
                {
                    if (Q.CanCast(enemy) && Q.GetPrediction(enemy).Hitchance >= HitChance.High && Q.GetPrediction(enemy).CollisionObjects.Count == 0)
                    {
                        Q.Cast(enemy);
                    }
                }
            }

            if (W.IsReady() && Config.Item("w.ks").GetValue<bool>())
            {
                var tentacle = HeroManager.Enemies.FirstOrDefault(x => x.IsValidTarget(W.Range) && x.Health < W.GetDamage(x));

                if (tentacle != null)
                {
                    W.Cast(tentacle);
                }
            }

            if (R.IsReady() && Config.Item("r.ks").GetValue<bool>())
            {
                foreach (var enemy in HeroManager.Enemies.Where(o => o.IsValidTarget(R.Range) && !o.IsDead && !o.IsZombie && o.Health < R.GetDamage(o)))
                {
                    if (enemy != null)
                    {
                        R.Cast(enemy);
                    }
                }
            }
        }

        private static void Combo()
        {
            if (Q.IsReady() && Config.Item("q.combo").GetValue<bool>())
            {
                var enemy = HeroManager.Enemies.FirstOrDefault(x => x.IsValidTarget(Q.Range));
                var enemyGhost = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(x=> x.Name == enemy.Name);
                if (enemy != null && enemyGhost == null )
                {
                    if (Q.CanCast(enemy) && Q.GetPrediction(enemy).Hitchance >= HitChance.High && Q.GetPrediction(enemy).CollisionObjects.Count == 0)
                    {
                        Q.Cast(enemy);
                    }
                }
                if (enemy == null && enemyGhost != null && Config.Item("q.ghost.combo").GetValue<bool>())
                {
                    if (Q.CanCast(enemyGhost) && Q.GetPrediction(enemyGhost).Hitchance >= HitChance.High && Q.GetPrediction(enemyGhost).CollisionObjects.Count == 0)
                    {
                        Q.Cast(enemyGhost);
                    }
                }
                   
                
            }
            if (W.IsReady() && Config.Item("w.combo").GetValue<bool>())
            {
                if (ObjectManager.Player.CountEnemiesInRange(Orbwalking.GetAttackRange(ObjectManager.Player))>=1 && Config.Item("w.combos").GetValue<bool>())
                {
                    W.Cast();
                }

                var tentacle = ObjectManager.Get<Obj_AI_Minion>().First(x=> x.Name == "God");

                if (tentacle != null)
                {
                    foreach (var enemy in HeroManager.Enemies.Where(x=> x.IsValidTarget(500)))
                    {
                        W.Cast();
                    }
                }
            }
            if (E.IsReady() && Config.Item("e.combo").GetValue<bool>())
            {
                foreach (var enemy in HeroManager.Enemies.Where(o => o.IsValidTarget(E.Range) && !o.IsDead && !o.IsZombie))
                {
                    if (Config.Item("enemy." + enemy.CharData.BaseSkinName).GetValue<bool>() && E.GetPrediction(enemy).Hitchance >= HitChance.High
                        && E.GetPrediction(enemy).CollisionObjects.Count == 0)
                    {
                        E.Cast(enemy);
                    }
                } 
            }
            if (R.IsReady() && Config.Item("r.combo").GetValue<bool>())
            {
                foreach (var enemy in HeroManager.Enemies.Where(o => o.IsValidTarget(R.Range) && !o.IsDead && !o.IsZombie))
                {
                    if (Illaoi.CountEnemiesInRange(R.Range) >= Config.Item("r.min.hit").GetValue<Slider>().Value)
                    {
                        R.Cast();
                    }

                    if (enemy != null)
                    {
                        if (Config.Item("r.min.hp").GetValue<Slider>().Value >= ObjectManager.Player.HealthPercent)
                        {
                            R.Cast();
                        }
                    }
                } 
            }
        }
        private static void Harass()
        {
            if (Illaoi.ManaPercent < Config.Item("harass.mana").GetValue<Slider>().Value)
            {
                return;
            }
            if (Q.IsReady() && Config.Item("q.harass").GetValue<bool>())
            {
                var enemy = HeroManager.Enemies.FirstOrDefault(x => x.IsValidTarget(Q.Range));
                var enemyGhost = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(x => x.Name == enemy.Name);
                if (enemy != null && enemyGhost == null)
                {
                    if (Q.CanCast(enemy) && Q.GetPrediction(enemy).Hitchance >= HitChance.High
                        && Q.GetPrediction(enemy).CollisionObjects.Count == 0)
                    {
                        Q.Cast(enemy);
                    }
                }
                if (enemy == null && enemyGhost != null && Config.Item("q.ghost.harass").GetValue<bool>())
                {
                    if (Q.CanCast(enemyGhost) && Q.GetPrediction(enemyGhost).Hitchance >= HitChance.High
                        && Q.GetPrediction(enemyGhost).CollisionObjects.Count == 0)
                    {
                        Q.Cast(enemyGhost);
                    }
                }
            }
            if (W.IsReady() && Config.Item("w.harass").GetValue<bool>())
            {
                var tentacle = ObjectManager.Get<Obj_AI_Minion>().First(x => x.Name == "God");
                if (tentacle != null)
                {
                    foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(850)))
                    {
                        W.Cast();
                    }
                }

            }
            if (E.IsReady() && Config.Item("e.harass").GetValue<bool>())
            {
                foreach (var enemy in HeroManager.Enemies.Where(o => o.IsValidTarget(E.Range) && !o.IsDead && !o.IsZombie))
                {
                    if (Config.Item("enemy." + enemy.CharData.BaseSkinName).GetValue<bool>() && E.GetPrediction(enemy).Hitchance >= HitChance.High
                        && E.GetPrediction(enemy).CollisionObjects.Count == 0)
                    {
                        E.Cast(enemy);
                    }
                }
            }
        }
        private static void Clear()
        {
            if (Illaoi.ManaPercent < Config.Item("clear.mana").GetValue<Slider>().Value)
            {
                return;
            }

            var minionCount = MinionManager.GetMinions(Illaoi.Position, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
            if (Q.IsReady() && Config.Item("q.clear").GetValue<bool>())
            {
                var mfarm = Q.GetLineFarmLocation(minionCount);
                if (minionCount.Count >= Config.Item("q.minion.hit").GetValue<Slider>().Value)
                {
                    Q.Cast(mfarm.Position);
                }
            }
        }
    }
}
