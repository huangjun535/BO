namespace YuLeMissFortune
{
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;
    using System;
    using System.Linq;
    using YuLeLibrary;

    class MissFortune
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Spell E, Q, Q1, R, W;
        private float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;
        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        private int LastAttackId = 0;
        private float RCastTime = 0;

        public void Load()
        {
            Q = new Spell(SpellSlot.Q, 655f);
            Q1 = new Spell(SpellSlot.Q, 1300f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1000f);
            R = new Spell(SpellSlot.R, 1350f);

            Q1.SetSkillshot(0.25f, 70f, 1500f, true, SkillshotType.SkillshotLine);
            Q.SetTargetted(0.25f, 1400f);
            E.SetSkillshot(0.5f, 200f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.25f, 50f, 3000f, false, SkillshotType.SkillshotCircle);

            Config.SubMenu("Q 设置").AddItem(new MenuItem("autoQ", "自动 Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("harasQ", "骚扰 Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("killQ", "骚扰 Q | 仅在能击杀小兵时使用", true).SetValue(false));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("qMinionMove", "骚扰 Q | 小兵准备位移时禁止使用", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("qMinionWidth", "二段 Q | 弧度", true).SetValue(new Slider(80, 100, 0)));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("farmQ", "清线 Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("farmQMana", "清线 Q | 最低蓝量比", true).SetValue(new Slider(80, 100, 0)));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("jungleQ", "清野 Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("jungleQMana", "清野 Q | 最低蓝量比", true).SetValue(new Slider(80, 100, 0)));

            Config.SubMenu("W 设置").AddItem(new MenuItem("autoW", "自动 W", true).SetValue(true));
            Config.SubMenu("W 设置").AddItem(new MenuItem("harasW", "骚扰 W", true).SetValue(true));
            Config.SubMenu("W 设置").AddItem(new MenuItem("harasWMana", "骚扰 W | 最低蓝量比", true).SetValue(new Slider(80, 100, 0)));
            Config.SubMenu("W 设置").AddItem(new MenuItem("farmW", "清线 W", true).SetValue(true));
            Config.SubMenu("W 设置").AddItem(new MenuItem("farmWMana", "清线 W | 最低蓝量比", true).SetValue(new Slider(80, 100, 0)));
            Config.SubMenu("W 设置").AddItem(new MenuItem("jungleW", "清野 W", true).SetValue(true));
            Config.SubMenu("W 设置").AddItem(new MenuItem("jungleWMana", "清线 W | 最低蓝量比", true).SetValue(new Slider(80, 100, 0)));

            Config.SubMenu("E 设置").AddItem(new MenuItem("autoE", "自动 E", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("farmE", "清线 E", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("farmELCminions", "清线 E | 最低命中数", true).SetValue(new Slider(2, 10, 0)));
            Config.SubMenu("E 设置").AddItem(new MenuItem("farmEMana", "清线 E | 最低蓝量比", true).SetValue(new Slider(80, 100, 0)));
            Config.SubMenu("E 设置").AddItem(new MenuItem("jungleE", "清野 E", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("jungleEMana", "清线 Q | 最低蓝量比", true).SetValue(new Slider(80, 100, 0)));
            Config.SubMenu("E 设置").AddItem(new MenuItem("AGC", "反突进 E", true).SetValue(true));

            Config.SubMenu("R 设置").AddItem(new MenuItem("autoR", "自动 R", true).SetValue(true));
            Config.SubMenu("R 设置").AddItem(new MenuItem("forceBlockMove", "开R的时候禁止移动", true).SetValue(true));
            Config.SubMenu("R 设置").AddItem(new MenuItem("Rturrent", "禁止塔底 R", true).SetValue(true));
            Config.SubMenu("R 设置").AddItem(new MenuItem("disableBlock", "LOL R技能按键(防止按错空大)", true).SetValue(new KeyBind("R".ToCharArray()[0], KeyBindType.Press))); //32 == space
            Config.SubMenu("R 设置").AddItem(new MenuItem("useR", "手动R按键", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press))); //32 == space

            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWard", "启动", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoBuy", "lv9自动买灯泡", true).SetValue(false));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoPink", "自动真眼扫描", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWardCombo", "仅连招模式启动 ", true).SetValue(true));
            new AutoWard().Load();
            new Tracker().Load();

            var SkinMenu = Config.AddSubMenu(new Menu("换肤设置", "换肤设置"));
            {
                SkinMenu.AddItem(new MenuItem("EnableSkin", "启动换肤").SetValue(false));
                SkinMenu.AddItem(new MenuItem("SkinSelect", "选择皮肤").SetValue(new StringList(new[] { "经典", "西部牛仔", "法国皇室", "特工狂花", "圣诞糖果棒", "荒野女警", "黑帮狂花", "电玩女神", "女帝" })));
            }

            Config.SubMenu("被动刷新").AddItem(new MenuItem("PaassiveSwitch", "自动切换攻击目标刷新被动", true).SetValue(true));
            Config.SubMenu("被动刷新").AddItem(new MenuItem("PaassiveHp", "目标的血量低于<= % 不切换目标", true).SetValue(new Slider(20, 100, 0)));

            Config.SubMenu("显示设置").AddItem(new MenuItem("QRange", "Q 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("ERange", "E 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("RRange", "R 范围", true).SetValue(false));

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += afterAttack;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (E.IsReady() && Config.Item("AGC", true).GetValue<bool>() &&  Player.Mana > RMANA + EMANA)
            {
                var Target = gapcloser.Sender;
                if (Target.IsValidTarget(E.Range))
                {
                    E.Cast(gapcloser.End);
                }
                return;
            }
            return;
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == "MissFortuneBulletTime")
            {
                RCastTime = Game.Time;
                Orbwalking.Attack = false;
                Orbwalking.Move = false;

                if (Config.Item("forceBlockMove", true).GetValue<bool>())
                {
                    Common.blockMove = true;
                    Common.blockAttack = true;
                    Common.blockSpells = true;
                }
            }
        }

        private void afterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe)
                return;
            LastAttackId = target.NetworkId;

            var t = target as Obj_AI_Hero;
            if (t != null)
            {
                if (Q.IsReady())
                {
                    if (Q.GetDamage(t) + Player.GetAutoAttackDamage(t) * 3 > t.Health)
                        Q.Cast(t);
                    else if (Program.Combo && Player.Mana > RMANA + QMANA + WMANA)
                        Q.Cast(t);
                    else if (Program.Farm && Player.Mana > RMANA + QMANA + EMANA + WMANA)
                        Q.Cast(t);
                }
                if (W.IsReady())
                {
                    if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && Player.Mana > RMANA + WMANA && Config.Item("autoW", true).GetValue<bool>())
                        W.Cast();
                    else if (Player.Mana > RMANA + WMANA + QMANA && Config.Item("harasW", true).GetValue<bool>() && Player.ManaPercent > Config.Item("harasWMana", true).GetValue<Slider>().Value)
                        W.Cast();
                }
            }
            else if (Program.LaneClear)
            {
                var minions = Cache.GetMinions(Player.ServerPosition, 600);

                if (minions.Count >= Config.Item("LCminions", true).GetValue<Slider>().Value)
                {
                    if (Q.IsReady() && Config.Item("farmQ", true).GetValue<bool>() && Player.ManaPercent > Config.Item("farmQMana", true).GetValue<Slider>().Value && minions.Count > 1)
                        Q.Cast(minions.FirstOrDefault());
                    if (W.IsReady() && Config.Item("farmW", true).GetValue<bool>() && Player.ManaPercent > Config.Item("farmWMana", true).GetValue<Slider>().Value && minions.Count > 1)
                        W.Cast();
                }
            }
        }

        private void Jungle()
        {
            if (Program.LaneClear && Player.Mana > RMANA + QMANA)
            {
                var mobs = Cache.GetMinions(Player.ServerPosition, 600, MinionTeam.Neutral);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    if (Q.IsReady() && Config.Item("jungleQ", true).GetValue<bool>() && Player.ManaPercent > Config.Item("jungleQMana", true).GetValue<Slider>().Value && !Orbwalking.CanAttack() && !Player.IsWindingUp)
                    {
                        Q.Cast(mob);
                        return;
                    }
                    if (W.IsReady() && Config.Item("jungleW", true).GetValue<bool>() && Player.ManaPercent > Config.Item("jungleWMana", true).GetValue<Slider>().Value)
                    {
                        W.Cast();
                        return;
                    }
                    if (E.IsReady() && Config.Item("jungleE", true).GetValue<bool>() && Player.ManaPercent > Config.Item("jungleEMana", true).GetValue<Slider>().Value)
                    {
                        E.Cast(mob.ServerPosition);
                        return;
                    }
                }
            }
        }

        private void setbool()
        {
            AutoWard.Enable = Config.GetBool("AutoWard");
            AutoWard.AutoBuy = Config.GetBool("AutoBuy");
            AutoWard.AutoPink = Config.GetBool("AutoPink");
            AutoWard.OnlyCombo = Config.GetBool("AutoWardCombo");
            AutoWard.InComboMode = Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo;
        }

        private void Skin()
        {
            if (Config.Item("EnableSkin").GetValue<bool>())
            {
                ObjectManager.Player.SetSkin(ObjectManager.Player.CharData.BaseSkinName, Config.Item("SkinSelect").GetValue<StringList>().SelectedIndex);
            }
            else if (!Config.Item("EnableSkin").GetValue<bool>())
            {
                ObjectManager.Player.SetSkin(ObjectManager.Player.CharData.BaseSkinName, 0);
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("disableBlock", true).GetValue<KeyBind>().Active)
            {
                Orbwalking.Attack = true;
                Orbwalking.Move = true;
                Common.blockSpells = false;
                Common.blockAttack = false;
                Common.blockMove = false;
                return;
            }
            else if (Player.IsChannelingImportantSpell() || Game.Time - RCastTime < 0.3)
            {
                if (Config.Item("forceBlockMove", true).GetValue<bool>())
                {
                    Common.blockMove = true;
                    Common.blockAttack = true;
                    Common.blockSpells = true;
                }

                Orbwalking.Attack = false;
                Orbwalking.Move = false;
                return;
            }
            else
            {
                Orbwalking.Attack = true;
                Orbwalking.Move = true;
                if (Config.Item("forceBlockMove", true).GetValue<bool>())
                {
                    Common.blockAttack = false;
                    Common.blockMove = false;
                    Common.blockSpells = false;
                }
                if (R.IsReady() && Config.Item("useR", true).GetValue<KeyBind>().Active)
                {
                    var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                    if (t.IsValidTarget(R.Range))
                    {
                        R.Cast(t, true, true);
                        RCastTime = Game.Time;
                        return;
                    }
                }
            }

            if (Config.Item("PaassiveSwitch", true).GetValue<bool>())
            {
                var orbT = Orbwalker.GetTarget();

                Obj_AI_Hero t2 = null;

                if (orbT != null && orbT is Obj_AI_Hero)
                    t2 = (Obj_AI_Hero)orbT;

                if (t2.IsValidTarget() && t2.NetworkId == LastAttackId && t2.HealthPercent >= Config.Item("PaassiveHp", true).GetValue<Slider>().Value)
                {
                    var ta = HeroManager.Enemies.Where(enemy => 
                        enemy.IsValidTarget() && Orbwalking.InAutoAttackRange(enemy) 
                            && (enemy.NetworkId != LastAttackId || enemy.Health < Player.GetAutoAttackDamage(enemy) * 2) ).FirstOrDefault();

                    if (ta!=null)
                        Orbwalker.ForceTarget(ta);
                }
            }

            setbool();
            Skin();

            if (Program.LagFree(1))
            {
                SetMana();
                Jungle();
            }

            if (Program.LagFree(2) && !Player.IsWindingUp && Q.IsReady() && Config.Item("autoQ", true).GetValue<bool>())
                LogicQ();

            if (Program.LagFree(3) && !Player.IsWindingUp && E.IsReady() && Config.Item("autoE", true).GetValue<bool>())
                LogicE();

            if (Program.LagFree(4) && !Player.IsWindingUp && R.IsReady() && Config.Item("autoR", true).GetValue<bool>())
                LogicR();
            
        }
        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            var t1 = TargetSelector.GetTarget(Q1.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget(Q.Range) && Player.Distance(t.ServerPosition) > 500)
            {
                var qDmg = Common.GetKsDamage(t, Q);
                if (qDmg + Player.GetAutoAttackDamage(t) > t.Health)
                    Q.Cast(t);
                else if (qDmg + Player.GetAutoAttackDamage(t) * 3 > t.Health)
                    Q.Cast(t);
                else if (Program.Combo && Player.Mana > RMANA + QMANA + WMANA)
                    Q.Cast(t);
                else if (Program.Farm && Player.Mana > RMANA + QMANA + EMANA + WMANA)
                    Q.Cast(t);
            }
            else if (t1.IsValidTarget(Q1.Range) && Config.Item("harasQ", true).GetValue<bool>() && Player.Distance(t1.ServerPosition) > Q.Range + 50)
            {
                var minions = Cache.GetMinions(Player.ServerPosition, Q1.Range);

                if (Config.Item("qMinionMove", true).GetValue<bool>())
                {
                    if (minions.Exists(x => x.IsMoving))
                        return;
                }

                var enemyPredictionPos = YuLeLibarary.Prediction.Prediction.GetPrediction(t1, 0.2f).CastPosition;
                foreach(var minion in minions)
                {
                    if (Config.Item("killQ", true).GetValue<bool>() && Q.GetDamage(minion) < minion.Health)
                        continue;

                    var posExt = Player.ServerPosition.Extend(minion.ServerPosition, 420 + Player.Distance(minion));
                    
                    if (InCone(enemyPredictionPos, posExt, minion.ServerPosition, Config.Item("qMinionWidth", true).GetValue<Slider>().Value))
                    {
                        if (minions.Exists(x => 
                        InCone(x.Position, posExt, minion.ServerPosition, Config.Item("qMinionWidth", true).GetValue<Slider>().Value)
                        ))
                            continue;
                        Q.Cast(minion);
                        return;
                    }
                }
            }
        }


        private bool InCone(Vector3 Position, Vector3 finishPos, Vector3 firstPos, int angleSet)
        {
            var range = 420;
            var angle = angleSet * (float)Math.PI / 180;
            var end2 = finishPos.To2D() - firstPos.To2D();
            var edge1 = end2.Rotated(-angle / 2);
            var edge2 = edge1.Rotated(angle);

            var point = Position.To2D() - firstPos.To2D();
            if (point.Distance(new Vector2(), true) < range * range && edge1.CrossProduct(point) > 0 && point.CrossProduct(edge2) > 0)
                return true;

            return false;
        }

        private void LogicE()
        {
            var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (t.IsValidTarget())
            {
                var eDmg = Common.GetKsDamage(t, E);
                if (eDmg > t.Health)
                    Program.CastSpell(E, t);
                else if (eDmg + Q.GetDamage(t) > t.Health && Player.Mana > QMANA + EMANA + RMANA)
                    Program.CastSpell(E, t);
                else if (Program.Combo && Player.Mana > RMANA + WMANA + QMANA + EMANA)
                {
                    if (!Orbwalking.InAutoAttackRange(t) || Player.CountEnemiesInRange(300) > 0 || t.CountEnemiesInRange(250) > 1)
                        Program.CastSpell(E, t);
                    else 
                    {
                        foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(E.Range) && !Common.CanMove(enemy)))
                            E.Cast(enemy, true, true);
                    }
                }
            }
            if (Program.LaneClear && Player.ManaPercent > Config.Item("Mana", true).GetValue<Slider>().Value && Config.Item("farmE", true).GetValue<bool>())
            {
                var minions = Cache.GetMinions(Player.ServerPosition, E.Range);
                var farmPos = E.GetCircularFarmLocation(minions, E.Width);
                if (farmPos.MinionsHit >= Config.Item("LCminions", true).GetValue<Slider>().Value)
                {
                    E.Cast(farmPos.Position);
                }
            }
        }

        private void LogicR()
        {
            if (Player.UnderTurret(true) && Config.Item("Rturrent", true).GetValue<bool>())
                return;

            var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);

            if (t.IsValidTarget(R.Range) && Common.ValidUlt(t))
            {
                var rDmg = R.GetDamage(t) * new double[] { 0.5, 0.75, 1 }[R.Level - 1];

                if (Player.CountEnemiesInRange(700) == 0 && t.CountAlliesInRange(400) == 0)
                {
                    var tDis = Player.Distance(t.ServerPosition);
                    if (rDmg * 7 > t.Health && tDis < 800)
                    {
                        R.Cast(t, true, true);
                        RCastTime = Game.Time;
                    }
                    else if (rDmg * 6 > t.Health && tDis < 900)
                    {
                        R.Cast(t, true, true);
                        RCastTime = Game.Time;
                    }
                    else if (rDmg * 5 > t.Health && tDis < 1000)
                    {
                        R.Cast(t, true, true);
                        RCastTime = Game.Time;
                    }
                    else if (rDmg * 4 > t.Health && tDis < 1100)
                    {
                        R.Cast(t, true, true);
                        RCastTime = Game.Time;
                    }
                    else if (rDmg * 3 > t.Health && tDis < 1200)
                    {
                        R.Cast(t, true, true);
                        RCastTime = Game.Time;
                    }
                    else if (rDmg > t.Health && tDis < 1300)
                    {
                        R.Cast(t, true, true);
                        RCastTime = Game.Time;
                    }
                    return;
                }
                if (rDmg * 8 > t.Health - Common.GetIncomingDamage(t) && rDmg * 2 < t.Health && Player.CountEnemiesInRange(300) == 0 && !Common.CanMove(t))
                {
                    R.Cast(t, true, true);
                    RCastTime = Game.Time;
                    return;
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

        public static void drawLine(Vector3 pos1, Vector3 pos2, int bold, System.Drawing.Color color)
        {
            var wts1 = Drawing.WorldToScreen(pos1);
            var wts2 = Drawing.WorldToScreen(pos2);

            Drawing.DrawLine(wts1[0], wts1[1], wts2[0], wts2[1], bold, color);
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("QRange", true).GetValue<bool>())
            {
                if (Q.IsReady())
                    Utility.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Cyan, 1, 1);
            }
            if (Config.Item("ERange", true).GetValue<bool>())
            {
                if (E.IsReady())
                    Utility.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Orange, 1, 1);
            }
            if (Config.Item("RRange", true).GetValue<bool>())
            {
                if (R.IsReady())
                    Utility.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Gray, 1, 1);
            }
        }
    }
}