namespace YuLeVarus
{
    using System;
    using System.Linq;
    using LeagueSharp;
    using LeagueSharp.Common;
    using YuLeLibrary;

    class YuLeVarus
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Spell Q, W, E, R;
        private float CastTime, QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;
        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public float AArange = ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius * 2;
        public bool CanCast;
        public void Load()
        {
            Q = new Spell(SpellSlot.Q, 925);
            W = new Spell(SpellSlot.Q, 0);
            E = new Spell(SpellSlot.E, 975);
            R = new Spell(SpellSlot.R, 1050);

            Q.SetSkillshot(0.25f, 70, 1650, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.35f, 120, 1500, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.25f, 120, 1950, false, SkillshotType.SkillshotLine);
            Q.SetCharged("VarusQ", "VarusQ", 925, 1600, 1.5f);

            Config.SubMenu("Q 设置").AddItem(new MenuItem("autoQ", "自动 Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("farmQ", "清线 Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("jungleQ", "清野 Q", true).SetValue(true));

            Config.SubMenu("E 设置").AddItem(new MenuItem("autoE", "自动 E", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("farmE", "清线 E", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("jungleE", "清野 E", true).SetValue(true));

            Config.SubMenu("R 设置").AddItem(new MenuItem("autoR", "自动 R", true).SetValue(true));
            Config.SubMenu("R 设置").AddItem(new MenuItem("rCount", "自动R敌人数(仅连招)", true).SetValue(new Slider(3, 0, 5)));
            Config.SubMenu("R 设置").AddItem(new MenuItem("useR", "手动R按键", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press))); //32 == space
            Config.SubMenu("R 设置").AddItem(new MenuItem("RSFDAW", "   R反突列表", true));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
                Config.SubMenu("R 设置").AddItem(new MenuItem("GapCloser" + enemy.ChampionName, enemy.ChampionName).SetValue(false));

            Config.SubMenu("蓝量管理").AddItem(new MenuItem("Mana", "清线最低蓝量", true).SetValue(new Slider(60, 0, 100)));
            Config.SubMenu("蓝量管理").AddItem(new MenuItem("JungleMana", "清野最低蓝量", true).SetValue(new Slider(40, 0, 100)));

            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWard", "启动", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoBuy", "lv9自动买灯泡", true).SetValue(false));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoPink", "自动真眼扫描", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWardCombo", "仅连招模式启动 ", true).SetValue(true));
            new AutoWard().Load();
            new Tracker().Load();

            Config.SubMenu("显示设置").AddItem(new MenuItem("qRange", "Q 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("eRange", "E 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("rRange", "R 范围", true).SetValue(false));

            Config.AddItem(new MenuItem("ClearEnable", "清线技能开关(鼠标滑轮控制)", true).SetValue(true)).Permashow();

            Game.OnWndProc += Game_OnWndProc;
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Game.OnUpdate += Orbwalking_AfterAttack;
        }

        private void Orbwalking_AfterAttack(EventArgs args)
        {
            if (ClearEnable && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                var Mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

                if (Mobs.Count() > 0)
                {
                    foreach (var mob in Mobs)
                    {
                        if (Config.Item("jungleQ", true).GetValue<bool>() && Player.ManaPercent > Config.Item("JungleMana", true).GetValue<Slider>().Value)
                            CastQ(mob);

                        if (Config.Item("jungleE", true).GetValue<bool>() && Player.ManaPercent > Config.Item("JungleMana", true).GetValue<Slider>().Value)
                        {
                            var Efarm = E.GetLineFarmLocation(Mobs);
                            if (Efarm.MinionsHit > 2)
                            {
                                E.Cast(Efarm.Position);
                            }
                            else if (Common.GetBuffCount(mob, "varuswdebuff") > 2)
                            {
                                E.Cast(mob, true);
                            }
                        }
                    }
                }

            }
        }

        private void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg == 0x20a)
            {
                Config.Item("ClearEnable", true).SetValue(!Config.Item("ClearEnable", true).GetValue<bool>());
            }
        }

        private bool ClearEnable
        {
            get
            {
                return Config.GetBool("ClearEnable");
            }
        }


        private void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("qRange", true).GetValue<bool>() && Q.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan, 1);
            }

            if (Config.Item("eRange", true).GetValue<bool>() && E.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Yellow, 1);
            }

            if (Config.Item("rRange", true).GetValue<bool>() && R.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.Gray, 1);
            }
        }

        private void setbool()
        {
            AutoWard.Enable = Config.GetBool("AutoWard");
            AutoWard.AutoBuy = Config.GetBool("AutoBuy");
            AutoWard.AutoPink = Config.GetBool("AutoPink");
            AutoWard.OnlyCombo = Config.GetBool("AutoWardCombo");
            AutoWard.InComboMode = Program.Combo;
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (R.IsReady() && Config.Item("GapCloser" + gapcloser.Sender.ChampionName).GetValue<bool>())
            {
                var Target = gapcloser.Sender;
                if (Target.IsValidTarget(R.Range))
                {
                    R.Cast(Target.ServerPosition, true);
                }
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            setbool();

            if (R.IsReady())
            {
                if (Config.Item("useR", true).GetValue<KeyBind>().Active)
                {
                    var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                    if (t.IsValidTarget())
                        R.Cast(t);
                }
            }
            if (Program.LagFree(0))
            {
                SetMana();
                if (!CanCast)
                {
                    if (Game.Time - CastTime > 1)
                    {
                        CanCast = true;
                        return;
                    }
                    var t = Orbwalker.GetTarget() as Obj_AI_Base;
                    if (t.IsValidTarget())
                    {
                        if (Common.GetBuffCount(t, "varuswdebuff") < 3)
                            CanCast = true;
                    }
                    else
                    {
                        CanCast = true;
                    }
                }
            }

            if (E.IsReady() && Config.Item("autoQ", true).GetValue<bool>())
                LogicE();
            if (Q.IsReady() && Config.Item("autoE", true).GetValue<bool>() && !Player.IsWindingUp)
                LogicQ();
            if (R.IsReady() && Config.Item("autoR", true).GetValue<bool>())
                LogicR();

                Farm();
        }

        private void Farm()
        {
            if (Program.LaneClear && E.IsReady() && Config.Item("farmE", true).GetValue<bool>() && ClearEnable)
            {
                var mobs = Cache.GetMinions(Player.ServerPosition, E.Range, MinionTeam.Neutral);
                if (mobs.Count > 0 && Player.Mana > RMANA + EMANA + QMANA && Common.GetBuffCount(mobs[0], "varuswdebuff") == 3)
                {
                    E.Cast(mobs[0]);
                }

                if (Player.ManaPercent > Config.Item("Mana", true).GetValue<Slider>().Value)
                {
                    var allMinionsE = Cache.GetMinions(Player.ServerPosition, E.Range);
                    var Efarm = Q.GetCircularFarmLocation(allMinionsE, E.Width);
                    if (Efarm.MinionsHit > 3)
                    {
                        E.Cast(Efarm.Position);
                    }
                }
            }
        }

        private void LogicR()
        {
            foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(R.Range)))
            {

                if (enemy.CountEnemiesInRange(400) >= Config.Item("rCount", true).GetValue<Slider>().Value && Config.Item("rCount", true).GetValue<Slider>().Value > 0)
                {
                    R.Cast(enemy, true, true);
                }
                if ((enemy.CountAlliesInRange(600) == 0 || Player.Health < Player.MaxHealth * 0.5) && R.GetDamage(enemy) + GetWDmg(enemy) + Q.GetDamage(enemy) > enemy.Health && Common.ValidUlt(enemy))
                {
                    Program.CastSpell(R, enemy);
                }
            }
            if (Player.Health < Player.MaxHealth * 0.5)
            {
                foreach (var target in HeroManager.Enemies.Where(target => target.IsValidTarget(270) && target.IsMelee && Config.Item("GapCloser" + target.ChampionName).GetValue<bool>()))
                {
                    Program.CastSpell(R, target);
                }
            }
        }

        private void LogicQ()
        {
            var t = Orbwalker.GetTarget() as Obj_AI_Hero;
            if (!t.IsValidTarget())
                t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            if (t.IsValidTarget())
            {
                if ((Program.Combo || (Common.GetBuffCount(t, "varuswdebuff") == 3 && Program.Farm)) && Player.Mana > RMANA + QMANA)
                {
                    Q.StartCharging();
                    if (Q.IsCharging)
                    {
                        Q.Cast(t, true);
                    }
                }
                else if (Program.Farm && Player.Mana > RMANA + EMANA + QMANA + QMANA && Config.Item("harras" + t.ChampionName).GetValue<bool>() && !Player.UnderTurret(true))
                {
                    Q.StartCharging();
                    if (Q.IsCharging)
                    {
                        Q.Cast(t, true);
                    }
                }
                else if (!Program.None && Player.Mana > RMANA + WMANA)
                {
                    foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range) && !Common.CanMove(enemy)))
                    {
                        Q.StartCharging();
                        if (Q.IsCharging)
                        {
                            Q.Cast(enemy, true);
                        }
                    }
                }
            }
            else if (Program.LaneClear && Player.Mana > RMANA + QMANA + WMANA && Player.CountEnemiesInRange(1450) == 0 && ClearEnable)
            {
                if (Config.Item("farmQ", true).GetValue<bool>() && (Player.ManaPercent > Config.Item("Mana", true).GetValue<Slider>().Value || Q.IsCharging) && Q.Range > 1300)
                {
                    var allMinionsQ = Cache.GetMinions(Player.ServerPosition, Q.Range);
                    var Qfarm = Q.GetLineFarmLocation(allMinionsQ, Q.Width);
                    if (Qfarm.MinionsHit > 3 || (Q.IsCharging && Qfarm.MinionsHit > 0))
                    {
                        Q.Cast(Qfarm.Position);
                    }
                }
            }

            foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(1600) && Q.GetDamage(enemy) + GetWDmg(enemy) > enemy.Health))
            {
                if (enemy != null)
                {
                    if (enemy.IsValidTarget(R.Range))
                        CastQ(enemy);
                }
            }
        }

        private void LogicE()
        {
            foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(E.Range) && E.GetDamage(enemy) + GetWDmg(enemy) > enemy.Health))
            {
                Program.CastSpell(E, enemy);
            }
            var t = Orbwalker.GetTarget() as Obj_AI_Hero;
            if (!t.IsValidTarget())
                t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget())
            {
                if ((Common.GetBuffCount(t, "varuswdebuff") == 3) || !Orbwalking.InAutoAttackRange(t))
                {
                    if (Program.Combo && Player.Mana > RMANA + QMANA)
                    {
                        Program.CastSpell(E, t);
                    }
                    else if ((Program.Combo || Program.Farm) && Player.Mana > RMANA + WMANA)
                    {
                        foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(E.Range) && !Common.CanMove(enemy)))
                            E.Cast(enemy);
                    }
                }
            }
        }

        private float GetQEndTime()
        {
            return
                Player.Buffs.OrderByDescending(buff => buff.EndTime - Game.Time)
                    .Where(buff => buff.Name == "VarusQ")
                    .Select(buff => buff.EndTime)
                    .FirstOrDefault() - Game.Time;
        }

        private float GetWDmg(Obj_AI_Base target)
        {
            return (Common.GetBuffCount(target, "varuswdebuff") * W.GetDamage(target, 1));
        }

        private void CastQ(Obj_AI_Base target)
        {
            if (!Q.IsCharging)
            {
                if (target.IsValidTarget(Q.Range - 300))
                    Q.StartCharging();
            }
            else
            {
                if (GetQEndTime() > 1)
                    Program.CastSpell(Q, target);
                else
                    Q.Cast(Q.GetPrediction(target).CastPosition);
                return;
            }
        }

        private void SetMana()
        {
            QMANA = Q.Instance.ManaCost;
            WMANA = W.Instance.ManaCost;
            EMANA = E.Instance.ManaCost;

            if (!R.IsReady())
                RMANA = QMANA - Player.PARRegenRate * Q.Instance.Cooldown;
            else
                RMANA = R.Instance.ManaCost;
        }
    }
}