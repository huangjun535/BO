namespace YuLeAhri
{
    using System;
    using System.Linq;
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;
    using YuLeLibrary;

    class YuLeAhri
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Spell Q, W, E, R;
        private float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;
        public Obj_AI_Hero Player;
        private static GameObject QMissile = null, EMissile = null;
        public Obj_AI_Hero Qtarget = null;
        public static MissileReturn missileManager;

        public void Load()
        {
            Player = ObjectManager.Player;

            Q = new Spell(SpellSlot.Q, 870);
            W = new Spell(SpellSlot.W, 580);
            E = new Spell(SpellSlot.E, 950);
            R = new Spell(SpellSlot.R, 600);

            Q.SetSkillshot(0.25f, 90, 1550, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 60, 1550, true, SkillshotType.SkillshotLine);

            missileManager = new MissileReturn("AhriOrbMissile", "AhriOrbReturn", Q);

            Config.SubMenu("Q 设置").AddItem(new MenuItem("autoQ", "连招 Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("harrasQ", "骚扰 Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("aim", "智能Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("aimQ", "智能Q返回", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("farmQ", "清线 Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("jungleQ", "清野 Q", true).SetValue(true));

            Config.SubMenu("W 设置").AddItem(new MenuItem("autoW", "连招 W", true).SetValue(true));
            Config.SubMenu("W 设置").AddItem(new MenuItem("harrasW", "骚扰 W", true).SetValue(true));
            Config.SubMenu("W 设置").AddItem(new MenuItem("farmW", "清线 W", true).SetValue(false));
            Config.SubMenu("W 设置").AddItem(new MenuItem("jungleW", "清野 W", true).SetValue(true));

            Config.SubMenu("E 设置").AddItem(new MenuItem("autoE", "连招 E", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("harrasE", "骚扰 E", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("harrasE", "禁止E对象", true));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
                Config.SubMenu("E 设置").AddItem(new MenuItem("Eon" + enemy.ChampionName, enemy.ChampionName ,true).SetValue(false));

            Config.SubMenu("R 设置").AddItem(new MenuItem("autoR2", "智能R", true).SetValue(true));
            Config.SubMenu("R 设置").AddItem(new MenuItem("autoR", "自动R击杀 ", true).SetValue(true));

            Config.SubMenu("蓝量管理").AddItem(new MenuItem("Mana", "清线最低蓝量", true).SetValue(new Slider(60, 100, 0)));
            Config.SubMenu("蓝量管理").AddItem(new MenuItem("JungleMana", "清野最低蓝量", true).SetValue(new Slider(40, 100, 0)));

            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
            {
                Config.SubMenu("反突设置").AddItem(new MenuItem("Egapcloser", "    反突对象", true));
                Config.SubMenu("反突设置").AddItem(new MenuItem("Egapcloser" + enemy.ChampionName, enemy.ChampionName, true).SetValue(true));
            }

            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWard", "启动", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoBuy", "lv9自动买灯泡", true).SetValue(false));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoPink", "自动真眼扫描", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWardCombo", "仅连招模式启动 ", true).SetValue(true));
            new AutoWard().Load();
            new Tracker().Load();

            Config.SubMenu("显示设置").AddItem(new MenuItem("qRange", "Q 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("drawHelper", "Q 路径", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("wRange", "W 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("eRange", "E 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("rRange", "R 范围", true).SetValue(false));

            Config.AddItem(new MenuItem("ClearEnable", "清线技能开关(鼠标滑轮控制)", true).SetValue(true)).Permashow();

            Game.OnWndProc += Game_OnWndProc;
            Game.OnUpdate += Game_OnGameUpdate;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += SpellMissile_OnCreateOld;
            GameObject.OnDelete += Obj_SpellMissile_OnDelete;
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

        private void setbool()
        {
            AutoWard.Enable = Config.GetBool("AutoWard");
            AutoWard.AutoBuy = Config.GetBool("AutoBuy");
            AutoWard.AutoPink = Config.GetBool("AutoPink");
            AutoWard.OnlyCombo = Config.GetBool("AutoWardCombo");
            AutoWard.InComboMode = Program.Combo;
        }

        private void Obj_SpellMissile_OnDelete(GameObject sender, EventArgs args)
        {
            if (sender.IsEnemy || sender.Type != GameObjectType.MissileClient || !sender.IsValid<MissileClient>())
                return;

            MissileClient missile = (MissileClient)sender;

            if ( missile.SData.Name != null)
            {
                if(missile.SData.Name == "AhriOrbReturn")
                    QMissile = null;
                if (missile.SData.Name == "AhriSeduceMissile")
                    EMissile = null;
            }
        }

        private void SpellMissile_OnCreateOld(GameObject sender, EventArgs args)
        {
            if (sender.IsEnemy || sender.Type != GameObjectType.MissileClient || !sender.IsValid<MissileClient>())
                return;

            MissileClient missile = (MissileClient)sender;

            if (missile.SData.Name != null )
            {
                if (missile.SData.Name == "AhriOrbMissile" || missile.SData.Name == "AhriOrbReturn")
                {
                    QMissile = sender;
                }
                if (missile.SData.Name == "AhriSeduceMissile")
                {
                    EMissile = sender;
                }
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (E.IsReady() && gapcloser.Sender.IsValidTarget(E.Range) && Config.Item("Egapcloser" + gapcloser.Sender.ChampionName, true).GetValue<bool>())
            {
                E.Cast(gapcloser.Sender);
            }
        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (E.IsReady() && Player.Distance(sender.ServerPosition) < E.Range)
            {
                E.Cast(sender);
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            setbool();

            if (Program.LagFree(0))
            {
                SetMana();
                Jungle();
            }
            
            if (E.IsReady() && Config.Item("autoE", true).GetValue<bool>())
                LogicE();
            if (Program.LagFree(2) && W.IsReady() && Config.Item("autoW", true).GetValue<bool>())
                LogicW();
            if (Program.LagFree(3) && Q.IsReady() && Config.Item("autoQ", true).GetValue<bool>())
                LogicQ();
            if (Program.LagFree(4) && R.IsReady() && Program.Combo)
                LogicR();
        }

        private void LogicR()
        {
            var dashPosition = Player.Position.Extend(Game.CursorPos, 450);

            if (Player.Distance(Game.CursorPos) < 450)
                dashPosition = Game.CursorPos;

            if (dashPosition.CountEnemiesInRange(800) > 2)
                return;

            if (Config.Item("autoR2", true).GetValue<bool>())
            {
                if (Player.HasBuff("AhriTumble"))
                {
                    var BuffTime = Common.GetPassiveTime(Player, "AhriTumble");

                    if (BuffTime < 3)
                    {
                        R.Cast(dashPosition);
                    }

                    var posPred = missileManager.CalculateReturnPos();

                    if (posPred != Vector3.Zero)
                    {
                        if (missileManager.Missile.SData.Name == "AhriOrbReturn" && Player.Distance(posPred) > 200)
                        {
                            R.Cast(posPred);
                        }
                    }
                }
            }

            if (Config.Item("autoR", true).GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(450 + R.Range, TargetSelector.DamageType.Magical);

                if (t.IsValidTarget())
                {
                    var comboDmg = R.GetDamage(t) * 3;

                    if (Q.IsReady())
                    {
                        comboDmg += Q.GetDamage(t) * 2;
                    }

                    if (W.IsReady())
                    {
                        comboDmg += W.GetDamage(t) + W.GetDamage(t, 1);
                    }

                    if (t.CountAlliesInRange(600) < 2 && comboDmg > t.Health && t.Position.Distance(Game.CursorPos) < t.Position.Distance(Player.Position) && dashPosition.Distance(t.ServerPosition) < 500)
                    {
                        R.Cast(dashPosition);
                    }

                    foreach (var target in HeroManager.Enemies.Where(target => target.IsMelee && target.IsValidTarget(300)))
                    {
                        R.Cast(dashPosition);
                    }
                }
            }
        }

        private void LogicW()
        {
            var t = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            if (t.IsValidTarget())
            {
                if (Program.Combo && Player.Mana > RMANA + WMANA)
                    W.Cast();
                else if (Program.Farm && Player.Mana > RMANA + QMANA + WMANA && Config.Item("harrasW", true).GetValue<bool>())
                    W.Cast();
                else if (W.GetDamage(t) + W.GetDamage(t, 1) + Q.GetDamage(t) * 2 > t.Health - Common.GetIncomingDamage(t))
                    W.Cast();
            }
            else if (ClearEnable && Program.LaneClear && QMissile == null && (Player.ManaPercent > Config.Item("Mana", true).GetValue<Slider>().Value && Config.Item("farmW", true).GetValue<bool>() && Player.Mana > RMANA + WMANA))
            {
                var minionList = Cache.GetMinions(Player.ServerPosition, W.Range, MinionTeam.Enemy);

                foreach (var minion in minionList.Where(minion =>  minion.Health < W.GetDamage(minion)))
                {
                    W.Cast();
                }
            }
        }

        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (t.IsValidTarget())
            {
                missileManager.Target = t;

                if (EMissile == null || !EMissile.IsValid)
                {
                    if (Program.Combo && Player.Mana > RMANA + QMANA)
                        Program.CastSpell(Q, t);
                    else if (Program.Farm  && Player.Mana > RMANA + WMANA + QMANA + QMANA && Config.Item("harrasQ", true).GetValue<bool>())
                        Program.CastSpell(Q, t);
                    else if (Q.GetDamage(t) * 2 + Common.GetEchoLudenDamage(t) > t.Health - Common.GetIncomingDamage(t))
                        Q.Cast(t, true);
                }

                if (!Program.None && Player.Mana > RMANA + WMANA)
                {
                    foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range) && !Common.CanMove(enemy)))
                        Q.Cast(enemy, true);
                }
            }
            else if (ClearEnable && Program.LaneClear && Player.ManaPercent > Config.Item("Mana", true).GetValue<Slider>().Value && Config.Item("farmQ", true).GetValue<bool>() && Player.Mana > RMANA + QMANA)
            {
                var minionList = Cache.GetMinions(Player.ServerPosition, Q.Range);
                var farmPosition = Q.GetLineFarmLocation(minionList, Q.Width);

                if (farmPosition.MinionsHit >= 3)
                    Q.Cast(farmPosition.Position);
            }
        }

        private void LogicE()
        {
            foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(E.Range) && E.GetDamage(enemy) + Q.GetDamage(enemy) + W.GetDamage(enemy) + Common.GetEchoLudenDamage(enemy) > enemy.Health))
            {
                Program.CastSpell(E, enemy);
            }
            var t = Orbwalker.GetTarget() as Obj_AI_Hero;
            if (!t.IsValidTarget())
                t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (t.IsValidTarget())
            {
                if (Program.Combo && Player.Mana > RMANA + EMANA && !Config.Item("Eon" + t.ChampionName, true).GetValue<bool>())
                    Program.CastSpell(E, t);
                else if (Program.Farm && Config.Item("harrasE", true).GetValue<bool>() && Player.Mana > RMANA + EMANA + WMANA + EMANA)
                    Program.CastSpell(E, t);
                else if (Common.GetKsDamage(t, E) > t.Health )
                    Program.CastSpell(E, t);
                if (Player.Mana > RMANA + EMANA)
                {
                    foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(E.Range) && !Common.CanMove(enemy) && !Config.Item("Eon" + enemy.ChampionName, true).GetValue<bool>()))
                        E.Cast(enemy);
                }
            }
        }

        private void Jungle()
        {
            if (ClearEnable && Program.LaneClear && Player.Mana > QMANA + RMANA && Config.GetSlider("JungleMana") <= Player.ManaPercent)
            {
                var mobs = Cache.GetMinions(Player.ServerPosition, 600, MinionTeam.Neutral);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    if (W.IsReady() && Config.Item("jungleW", true).GetValue<bool>())
                    {
                        W.Cast();
                        return;
                    }
                    if (Q.IsReady() && Config.Item("jungleQ", true).GetValue<bool>())
                    {
                        Q.Cast(mob.Position);
                        return;
                    }
                }
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

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("qRange", true).GetValue<bool>() && Q.IsReady())
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Cyan, 1);
            }

            if (Config.Item("wRange", true).GetValue<bool>() && W.IsReady())
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, System.Drawing.Color.Orange, 1);
            }

            if (Config.Item("eRange", true).GetValue<bool>() && E.IsReady())
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Yellow, 1);
            }
        }
    }
}
