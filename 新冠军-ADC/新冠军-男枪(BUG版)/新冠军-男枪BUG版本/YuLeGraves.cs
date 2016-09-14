
namespace YuLeGraves
{
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;
    using System;
    using System.Linq;
    using YuLeLibrary;

    class Graves
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Spell E, Q, Q1, R, W , R1;
        private float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;
        private bool IsAfterAttack = false, loop = false;
        public bool Esmart = false;
        public float OverKill = 0;
        public Obj_AI_Hero Player { get { return ObjectManager.Player; }}
        public static DashLogic Dash;
        private Obj_AI_Hero targets;

        public void Load()
        {
            Q = new Spell(SpellSlot.Q, 900);
            W = new Spell(SpellSlot.W, 950f);
            E = new Spell(SpellSlot.E, 450f);
            R = new Spell(SpellSlot.R, 1000f);
            R1 = new Spell(SpellSlot.R, 1700f);

        
            Q.SetSkillshot(0.25f, 100f, 2100f, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.25f, 120f, 1500f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.25f, 100f, 2100f, false, SkillshotType.SkillshotLine);
            R1.SetSkillshot(0.25f, 100f, 2100f, false, SkillshotType.SkillshotLine);

            Config.SubMenu("Q 设置").AddItem(new MenuItem("autoQ", "连招 Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("harassQ", "骚扰 Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("farmQ", "清线 Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("jungleQ", "清野 Q", true).SetValue(true));

            Config.SubMenu("W 设置").AddItem(new MenuItem("autoW", "连招 W", true).SetValue(true));
            Config.SubMenu("W 设置").AddItem(new MenuItem("jungleW", "清野 W", true).SetValue(true));
            Config.SubMenu("W 设置").AddItem(new MenuItem("AGCW", "反突进 W", true).SetValue(true));

            Config.SubMenu("E 设置").AddItem(new MenuItem("autoE", "自动 E", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("JungleE", "清野 E", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("DashMode", "突进模式", true).SetValue(new StringList(new[] { "鼠标位置", "侧面", "安全距离" }, 2)));
            Config.SubMenu("E 设置").AddItem(new MenuItem("EnemyCheck", "敌人数>= 禁止突进 ", true).SetValue(new Slider(3, 0, 5)));
            Config.SubMenu("E 设置").AddItem(new MenuItem("WallCheck", "禁止E到墙边", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("TurretCheck", "禁止突进到塔下", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("AAcheck", "仅在AA范围内突进", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("GapcloserMode", "反突模式", true).SetValue(new StringList(new[] { "鼠标位置", "安全距离", "关闭" }, 1)));
            Config.SubMenu("E 设置").AddItem(new MenuItem("ASCWSWD", "    反突列表", true));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
                Config.SubMenu("E 设置").AddItem(new MenuItem("EGCchampion" + enemy.ChampionName, enemy.ChampionName, true).SetValue(true));
            Dash = new DashLogic(E);

            Config.SubMenu("R 设置").AddItem(new MenuItem("autoR", "自动 R", true).SetValue(true));
            Config.SubMenu("R 设置").AddItem(new MenuItem("fastR", "击杀 R", true).SetValue(true));
            Config.SubMenu("R 设置").AddItem(new MenuItem("overkillR", "仅击杀使用", true).SetValue(false));
            Config.SubMenu("R 设置").AddItem(new MenuItem("useR", "手动R按键", true).SetValue(new KeyBind("R".ToCharArray()[0], KeyBindType.Press)));

            Config.SubMenu("蓝量管理").AddItem(new MenuItem("Mana", "清线最低蓝量", true).SetValue(new Slider(60, 0, 100)));
            Config.SubMenu("蓝量管理").AddItem(new MenuItem("JungleMana", "清野最低蓝量", true).SetValue(new Slider(20, 0, 100)));

            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWard", "启动", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoBuy", "lv9自动买灯泡", true).SetValue(false));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoPink", "自动真眼扫描", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWardCombo", "仅连招模式启动 ", true).SetValue(true));
            new AutoWard().Load();
            new Tracker().Load();

            var SkinMenu = Config.AddSubMenu(new Menu("换肤设置", "Skin"));
            SkinMenu.AddItem(new MenuItem("EnableSkin", "启动换肤").SetValue(false));
            SkinMenu.AddItem(new MenuItem("SkinSelect", "选择皮肤").SetValue(new StringList(new[] { "原版", "怒之火炮", "杀出重围", "黑帮教父", "防爆士兵", "泳池派对", "无情重炮" })));


            Config.SubMenu("显示设置").AddItem(new MenuItem("qRange", "Q 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("wRange", "W 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("eRange", "E 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("rRange", "R 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("DrawWall", "显示翻墙点", true).SetValue(true));
            Config.AddItem(new MenuItem("爆发设置", "BUG连招默认T键", true));
            Config.AddItem(new MenuItem("QWlogic", "仅没子弹的时候才用QW", true).SetValue(false));
            Config.AddItem(new MenuItem("ClearEnable", "清线技能开关(鼠标滑轮控制)", true).SetValue(true)).Permashow();

            Game.OnWndProc += Game_OnWndProc;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Orbwalking.AfterAttack += Orbwalker_AfterAttack;
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

        public void Orbwalker_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (E.IsReady() && Config.Item("autoE", true).GetValue<bool>())
                LogicE();

            if (unit.IsMe && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Burst && IsAfterAttack == false)
            {
                var t = TargetSelector.GetTarget(550, TargetSelector.DamageType.Physical);

                if (R.IsReady() && E.IsReady() && t.IsValidTarget(550))
                {
                    IsAfterAttack = true;
                    targets = t;
                    loop = true;
                }
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Player.Mana > RMANA + EMANA )
            {
                var t = gapcloser.Sender;
                if (t.IsValidTarget(E.Range) )
                {
                    if (W.IsReady() && Config.Item("AGCW", true).GetValue<bool>())
                    {
                        W.Cast(gapcloser.End);
                    }
                }
            }
        }

        private void BurstLogic()
        {
            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            if (IsAfterAttack == false && !R.IsReady())
            {
                if (Player.Mana > RMANA + QMANA)
                    Program.CastSpell(Q, t);

                if (Player.Mana > RMANA + EMANA && !Player.HasBuff("gravesbasicattackammo2"))
                {
                    var dashPos = Dash.CastDash();
                    if (!dashPos.IsZero)
                    {
                        E.Cast(dashPos);
                    }
                }
            }
        }

        public void Flee()
        {
            if (Config.Item("FleeKEY", true).GetValue<KeyBind>().Active)
            {
                ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

                if (E.IsReady())
                {
                    var curSpot = YuLeGravesWallJump.GetSpot(ObjectManager.Player.ServerPosition);

                    if (curSpot.Start != Vector3.Zero)
                    {
                        E.Cast(curSpot.End);
                        return;
                    }
                    var spot = YuLeGravesWallJump.GetNearest(Game.CursorPos);
                    if (spot.Start != Vector3.Zero)
                    {
                        ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, spot.Start);
                        return;
                    }
                }
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("useR", true).GetValue<KeyBind>().Active && R.IsReady())
            {
                var t = TargetSelector.GetTarget(1800, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget())
                    R1.Cast(t, true);
            }

            if (Program.LagFree(0))
            {
                SetMana();
                Jungle();
            }
            
            if (!Config.Item("QWlogic", true).GetValue<bool>() || !Player.HasBuff("gravesbasicattackammo1"))
            {
                if (Program.LagFree(2) && Q.IsReady() && !Player.IsWindingUp && Config.Item("autoQ", true).GetValue<bool>())
                    LogicQ();
                if (Program.LagFree(3) && W.IsReady() && !Player.IsWindingUp && Config.Item("autoW", true).GetValue<bool>())
                    LogicW();
            }
            if (Program.LagFree(4) && R.IsReady() && Config.Item("autoR", true).GetValue<bool>())
                LogicR();


            Flee();

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Burst)
            {
                if (loop == true && R.IsReady() && targets != null && targets.IsValidTarget())
                {
                    Program.CastSpell(R,targets);
                    Utility.DelayAction.Add(250, () => { E.Cast(targets.Position); IsAfterAttack = false; loop = false; });
                }
                else
                    if (!R.IsReady())
                    BurstLogic();
            }
        }

        private void Jungle()
        {
            if (Program.LaneClear && ClearEnable && Player.ManaPercent >= Config.GetSlider("JungleMana"))
            {
                var mobs = Cache.GetMinions(Player.ServerPosition, 600, MinionTeam.Neutral);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    if (Q.IsReady() && Config.Item("jungleQ", true).GetValue<bool>() )
                    {
                        Q.Cast(mob.Position);
                        return;
                    }
                    if (W.IsReady() && Config.Item("jungleW", true).GetValue<bool>())
                    {
                        W.Cast(mob.Position);
                        return;
                    }
                    if (E.IsReady() && Config.Item("JungleE", true).GetValue<bool>() && !Player.HasBuff("gravesbasicattackammo1") && mob.Health >= Player.GetAutoAttackDamage(mob))
                    {
                        E.Cast(Game.CursorPos);
                    }
                }
            }
        }

        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget())
            {
                var step = t.Distance(Player) / 20;
                for (var i = 0; i < 20; i++)
                {
                    var p = Player.Position.Extend(t.Position, step * i);
                    if (p.IsWall())
                    {
                        return;
                    }
                }

                if (Program.Combo && Player.Mana > RMANA + QMANA)
                    Program.CastSpell(Q, t); 
                else if (Program.Farm && Config.Item("harassQ", true).GetValue<bool>() && Player.Mana > RMANA + EMANA + WMANA + QMANA + QMANA)
                    Program.CastSpell(Q, t);
                else
                {
                    var qDmg = Common.GetKsDamage(t, Q);
                    var rDmg = R.GetDamage(t);
                    if (qDmg > t.Health)
                    {
                        Q.Cast(t, true);
                        OverKill = Game.Time;
                    }
                    else if (qDmg + rDmg > t.Health && R.IsReady() && Player.Mana > RMANA + QMANA)
                    {
                        Program.CastSpell(Q, t);
                        if (Config.Item("fastR", true).GetValue<bool>() && rDmg < t.Health)
                            Program.CastSpell(R, t);
                    }
                }

                if (!Program.None && Player.Mana > RMANA + QMANA + EMANA)
                {
                    foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range) && !Common.CanMove(enemy)))
                        Program.CastSpell(Q, enemy);
                }
            }
            else if (Program.LaneClear && Player.ManaPercent > Config.Item("Mana", true).GetValue<Slider>().Value && Config.Item("farmQ", true).GetValue<bool>() && Player.Mana > RMANA + QMANA && ClearEnable)
            {
                var allMinionsQ = Cache.GetMinions(Player.ServerPosition, Q.Range);
                var Qfarm = Q.GetLineFarmLocation(allMinionsQ, Q.Width);
                if (Qfarm.MinionsHit > 2)
                    Q.Cast(Qfarm.Position);
            }
        }

        private void LogicW()
        {
            var t = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            if (t.IsValidTarget())
            {
                var wDmg = Common.GetKsDamage(t, W);
                if (wDmg > t.Health)
                {
                    W.Cast(t, true, true);
                    return;
                }
                else if (wDmg + Q.GetDamage(t) > t.Health && Player.Mana > QMANA + WMANA + RMANA)
                {
                    W.Cast(t, true, true);
                }
                else if (Program.Combo && Player.Mana > RMANA + WMANA + QMANA)
                {
                    if (!Orbwalking.InAutoAttackRange(t) || Player.CountEnemiesInRange(300) > 0 || t.CountEnemiesInRange(250) > 1 || Player.HealthPercent < 50)
                        W.Cast(t, true, true);
                    else if (Player.Mana > RMANA + WMANA + QMANA + EMANA)
                    {
                        foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(W.Range) && !Common.CanMove(enemy)))
                            W.Cast(enemy, true, true);
                    }
                }
            }
        }

        private void LogicE()
        {
            if (HeroManager.Enemies.Any(target => target.IsValidTarget(270) && target.IsMelee))
            {
                var dashPos = Dash.CastDash(true);
                if (!dashPos.IsZero)
                {
                    E.Cast(dashPos);
                }
            }
            if (Program.Combo && Player.Mana > RMANA + EMANA && !Player.HasBuff("gravesbasicattackammo2"))
            {
                var dashPos = Dash.CastDash();
                if (!dashPos.IsZero)
                {
                    E.Cast(dashPos);
                }
            }
        }

        private void LogicR()
        {
            foreach (var target in HeroManager.Enemies.Where(target => target.IsValidTarget(R1.Range) && Common.ValidUlt(target)))
            {
                double rDmg = Common.GetKsDamage(target,R);

                if (rDmg < target.Health)
                    continue;

                if (Config.Item("overkillR", true).GetValue<bool>() && target.Health < Player.Health)
                {
                    if(Orbwalking.InAutoAttackRange(target))
                        continue;
                    if (target.CountAlliesInRange(400) > 0)
                        continue;
                }

                double rDmg2 = rDmg * 0.8;
                
                if(target.IsValidTarget(R.Range) && !Common.IsSpellHeroCollision(target, R) && rDmg > target.Health)
                {
                    Program.CastSpell(R, target);
                }
                else if (rDmg2 > target.Health )
                {
                    if (!Common.IsSpellHeroCollision(target, R1))
                    {
                        Program.CastSpell(R1, target);
                    }
                    else if (target.IsValidTarget(1200))
                    {
                        Program.CastSpell(R1, target);
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
            if (Config.Item("rRange", true).GetValue<bool>())
            {
                if (R.IsReady())
                    Utility.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.Gray, 1, 1);
            }

            if (Config.GetBool("DrawWall"))
            {
                var curSpot = YuLeGravesWallJump.GetSpot(ObjectManager.Player.ServerPosition);

                if (curSpot.Start != Vector3.Zero)
                {
                    if (Player.ServerPosition.Distance(curSpot.Start) < 1000)
                    {
                        Render.Circle.DrawCircle(curSpot.Start, 200, System.Drawing.Color.AliceBlue, 4);
                    }

                    if (Player.ServerPosition.Distance(curSpot.End) < 1000)
                    {
                        Render.Circle.DrawCircle(curSpot.End, 200, System.Drawing.Color.AliceBlue, 4);
                    }
                }
            }
        }
    }
}
