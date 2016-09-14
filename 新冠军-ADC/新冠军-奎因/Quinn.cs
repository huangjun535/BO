namespace YuLeQuinn
{
    using System;
    using System.Linq;
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;
    using YuLeLibrary;

    class Quinn
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Spell Q, W, E, R;
        private float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;

        public Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }
        public void Load()
        {
            Q = new Spell(SpellSlot.Q, 1000);
            E = new Spell(SpellSlot.E, 700);
            W = new Spell(SpellSlot.W, 2100);
            R = new Spell(SpellSlot.R, 550);

            Q.SetSkillshot(0.25f, 90f, 1550, true, SkillshotType.SkillshotLine);
            E.SetTargetted(0.25f, 2000f);

            Config.SubMenu("Q技能设置").AddItem(new MenuItem("autoQ", "自动Q", true).SetValue(true));
            Config.SubMenu("Q技能设置").AddItem(new MenuItem("harrasQ", "骚扰Q", true).SetValue(true));

            Config.SubMenu("E技能设置").AddItem(new MenuItem("autoE", "自动E", true).SetValue(true));
            Config.SubMenu("E技能设置").AddItem(new MenuItem("harrasE", "骚扰E", true).SetValue(true));
            Config.SubMenu("E技能设置").AddItem(new MenuItem("Int", "打断E", true).SetValue(true));
            Config.SubMenu("E技能设置").AddItem(new MenuItem("AGC", "反突进E", true).SetValue(true));
            Config.SubMenu("E技能设置").AddItem(new MenuItem("gaptarget", "   反突进目标", true));
            foreach (var enemy in HeroManager.Enemies)
                Config.SubMenu("E技能设置").AddItem(new MenuItem("gap" + enemy.ChampionName, enemy.ChampionName, true).SetValue(true));

            Config.SubMenu("清线清野").AddItem(new MenuItem("farmQ", "清线Q", true).SetValue(true));
            Config.SubMenu("清线清野").AddItem(new MenuItem("Mana", "清线Q最低蓝量", true).SetValue(new Slider(70, 100, 0)));
            Config.SubMenu("清线清野").AddItem(new MenuItem("LCminions", "清线Q最低命中", true).SetValue(new Slider(3, 10, 0)));
            Config.SubMenu("清线清野").AddItem(new MenuItem("jungleQ", "清野Q", true).SetValue(true));
            Config.SubMenu("清线清野").AddItem(new MenuItem("jungleE", "清野E", true).SetValue(true));

            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWard", "启动", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoBuy", "lv9自动买灯泡", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoPink", "自动真眼扫描", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWardCombo", "仅连招模式启动 ", true).SetValue(true));
            new AutoWard().Load();
            new Tracker().Load();

            var SkinMenu = Config.AddSubMenu(new Menu("特效换肤", "特效换肤"));
            {
                SkinMenu.AddItem(new MenuItem("EnableSkin", "启动换肤").SetValue(false));
                SkinMenu.AddItem(new MenuItem("SkinSelect", "选择皮肤").SetValue(new StringList(new[] { "经典", "血羽凤凰", "勇敢的心", "死亡之鹰" })));
            }

            Config.SubMenu("杂项设置").AddItem(new MenuItem("autoW", "自动W", true).SetValue(true));
            Config.SubMenu("杂项设置").AddItem(new MenuItem("autoR", "自动R", true).SetValue(true));
            Config.SubMenu("杂项设置").AddItem(new MenuItem("focusP", "连招时集中攻击有标记的敌人", true).SetValue(true));
            Config.SubMenu("杂项设置").AddItem(new MenuItem("farmP", "清线时首先攻击有标记的敌人", true).SetValue(true));

            Config.SubMenu("显示设置").AddItem(new MenuItem("qRange", "Q 范围", true).SetValue(true));
            Config.SubMenu("显示设置").AddItem(new MenuItem("wRange", "W 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("eRange", "E 范围", true).SetValue(false));

            Game.OnUpdate += Game_OnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += afterAttack;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
        }

        private void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if(args.Target.Type == GameObjectType.obj_AI_Hero && Config.Item("focusP", true).GetValue<bool>() && args.Target.HealthPercent > 40)
            {
                var orbTarget = args.Target as Obj_AI_Hero;
                if (!orbTarget.HasBuff("quinnw"))
                {
                    var best = HeroManager.Enemies.FirstOrDefault(enemy => enemy.IsValidTarget() && Orbwalking.InAutoAttackRange(enemy) && enemy.HasBuff("quinnw"));
                    if(best != null)
                        Orbwalker.ForceTarget(best);
                }
            }
            else if(Program.LaneClear && args.Target.Type == GameObjectType.obj_AI_Minion && Config.Item("farmP", true).GetValue<bool>())
            {
                var bestMinion = Cache.GetMinions(Player.Position, Player.AttackRange).FirstOrDefault(minion => minion.IsValidTarget() && Orbwalking.InAutoAttackRange(minion) && minion.HasBuff("quinnw"));

                if (bestMinion != null)
                    Orbwalker.ForceTarget(bestMinion);
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Program.LagFree(1))
                SetMana();
            if (Program.LagFree(2) && Q.IsReady() && Config.Item("autoQ", true).GetValue<bool>())
                LogicQ();

            if (Program.LagFree(4) && R.IsReady() && Config.Item("autoR", true).GetValue<bool>())
                LogicR();
        }
        private void Jungle()
        {
            if (Program.LaneClear && Player.Mana > RMANA + WMANA + RMANA + WMANA)
            {
                var mobs = Cache.GetMinions(Player.ServerPosition, 700, MinionTeam.Neutral);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    if (mob.HasBuff("QuinnW"))
                        return;

                    if (Q.IsReady() && Config.Item("jungleQ", true).GetValue<bool>())
                    {
                        Q.Cast(mob.ServerPosition);
                        return;
                    }

                    if (E.IsReady() && Config.Item("jungleE", true).GetValue<bool>())
                    {
                        E.CastOnUnit(mob);
                        return;
                    }
                }
            }
        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (E.IsReady() && Config.Item("Int", true).GetValue<bool>() && sender.IsValidTarget(E.Range))
                E.CastOnUnit(sender);
        }

        private void afterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if ( target.Type == GameObjectType.obj_AI_Hero)
            {
                var t = target as Obj_AI_Hero;
                if (E.IsReady() && Config.Item("autoE", true).GetValue<bool>() && t.IsValidTarget(E.Range) && t.CountEnemiesInRange(800) < 3)
                {
                    if (Program.Combo && Player.Mana > RMANA + EMANA)
                        E.Cast(t);
                    else if (Program.Farm && Player.Mana > RMANA + EMANA + QMANA + WMANA && Config.Item("harrasE", true).GetValue<bool>())
                    {
                        E.Cast(t);
                    }
                    else if (Common.GetKsDamage(t, E) > t.Health)
                        E.Cast(t);
                }
                if (Q.IsReady() && t.IsValidTarget(Q.Range))
                {
                    if (Program.Combo && Player.Mana > RMANA + QMANA)
                        Program.CastSpell(Q, t);
                    else if (Program.Farm && Player.Mana > RMANA + EMANA + QMANA + WMANA && Config.Item("harrasQ", true).GetValue<bool>())
                    {
                        Program.CastSpell(Q, t);
                    }
                    else if (Common.GetKsDamage(t, Q) > t.Health)
                        Program.CastSpell(Q, t);

                    if (!Program.None && Player.Mana > RMANA + QMANA + EMANA)
                    {
                        foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range) && !Common.CanMove(enemy)))
                            Q.Cast(enemy);
                    }
                }
            }
            Jungle();
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (E.IsReady() && Config.Item("AGC", true).GetValue<bool>() && Config.Item("gap" + gapcloser.Sender.ChampionName,true).GetValue<bool>())
            {
                var t = gapcloser.Sender;
                if (t.IsValidTarget(E.Range))
                {
                    E.Cast(t);
                }
            }
        }

        private void LogicR()
        {
            if (Player.InFountain() && R.Instance.Name == "QuinnR")
            {
                R.Cast();
            }
        }

        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget())
            {
                if (Orbwalking.InAutoAttackRange(t) && t.HasBuff("quinnw"))
                    return;
                if (Program.Combo && Player.Mana > RMANA + QMANA)
                    Program.CastSpell(Q, t);
                else if (Program.Farm && Player.Mana > RMANA + EMANA + QMANA + WMANA && Config.Item("harrasQ", true).GetValue<bool>())
                {
                    Program.CastSpell(Q, t);
                }
                else if (Common.GetKsDamage(t, Q) > t.Health)
                    Program.CastSpell(Q, t);

                if (!Program.None && Player.Mana > RMANA + QMANA + EMANA)
                {
                    foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range) && !Common.CanMove(enemy)))
                        Q.Cast(enemy);
                }
            }
            else if (Program.LaneClear && Player.ManaPercent > Config.Item("Mana", true).GetValue<Slider>().Value && Config.Item("farmQ", true).GetValue<bool>() && Player.Mana > RMANA + QMANA)
            {
                var minionList = Cache.GetMinions(Player.ServerPosition, Q.Range - 150);
                var farmPosition = Q.GetCircularFarmLocation(minionList, 150);
                if (farmPosition.MinionsHit >= Config.Item("LCminions", true).GetValue<Slider>().Value)
                    Q.Cast(farmPosition.Position);
            }
        }

        private void SetMana()
        {
            QMANA = Q.Instance.ManaCost;
            WMANA = W.Instance.ManaCost;
            EMANA = E.Instance.ManaCost;

            if (!R.IsReady())
                RMANA = WMANA - Player.PARRegenRate * W.Instance.Cooldown;
            else
                RMANA = R.Instance.ManaCost;
        }

        private void Drawing_OnDraw(EventArgs args)
        {

            if (Config.Item("qRange", true).GetValue<bool>())
            {
                if (Q.IsReady())
                    Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan, 1, 1);
            }
            if (Config.Item("wRange", true).GetValue<bool>())
            {
                if (W.IsReady())
                    Utility.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.Orange, 1, 1);
            }
            if (Config.Item("eRange", true).GetValue<bool>())
            {
                if (E.IsReady())
                    Utility.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Yellow, 1, 1);
            }
        }
    }
}
