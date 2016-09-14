namespace YuLeKogMaw
{
    using LeagueSharp;
    using LeagueSharp.Common;
    using System;
    using System.Linq;
    using YuLeLibrary;

    class KogMaw
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        public Spell Q, W, E, R;
        public float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;
        private Humanizer _humanizer;
        private int _attacksSoFar;
        public bool attackNow = true;
        private Random _rand;

        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        public void Load()
        {
            Q = new Spell(SpellSlot.Q, 980);
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 1200);
            R = new Spell(SpellSlot.R, 1800);

            Q.SetSkillshot(0.25f, 50f, 2000f, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 120f, 1400f, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(1.2f, 120f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            Config.SubMenu("Q 设置").AddItem(new MenuItem("autoQ", "自动 Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("harrasQ", "骚扰 Q", true).SetValue(true));

            Config.SubMenu("W 设置").AddItem(new MenuItem("autoW", "自动 W", true).SetValue(true));
            Config.SubMenu("W 设置").AddItem(new MenuItem("harasW", "骚扰 W", true).SetValue(true));

            Config.SubMenu("E 设置").AddItem(new MenuItem("autoE", "自动 E", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("HarrasE", "骚扰 E", true).SetValue(false));
            Config.SubMenu("E 设置").AddItem(new MenuItem("AGC", "反突进 E", true).SetValue(true));

            Config.SubMenu("R 设置").AddItem(new MenuItem("autoR", "自动 R", true).SetValue(true));
            Config.SubMenu("R 设置").AddItem(new MenuItem("RmaxHp", "目标最大 HP百分比", true).SetValue(new Slider(50, 100, 0)));
            Config.SubMenu("R 设置").AddItem(new MenuItem("comboStack", "最大连招堆叠数", true).SetValue(new Slider(2, 10, 0)));
            Config.SubMenu("R 设置").AddItem(new MenuItem("harasStack", "最大骚扰堆叠数", true).SetValue(new Slider(1, 10, 0)));
            Config.SubMenu("R 设置").AddItem(new MenuItem("Rcc", "R 无法移动目标", true).SetValue(true));
            Config.SubMenu("R 设置").AddItem(new MenuItem("Rslow", "R 减速的目标", true).SetValue(true));
            Config.SubMenu("R 设置").AddItem(new MenuItem("Raoe", "R 打群体伤害", true).SetValue(true));
            Config.SubMenu("R 设置").AddItem(new MenuItem("Raa", "仅R 不在攻击范围内的敌人", true).SetValue(false));

            Config.AddSubMenu(new Menu("杂项 设置", "杂项 设置"));
            Config.SubMenu("杂项 设置").AddItem(new MenuItem("sheen", "反控制", true).SetValue(true));
            Config.SubMenu("杂项 设置").AddItem(new MenuItem("AApriority", "技能使用搭配普攻", true).SetValue(true));
            Config.SubMenu("杂项 设置").AddItem(new MenuItem("Humanizer", "人性化大嘴", false).SetValue(false));
            Config.SubMenu("杂项 设置").AddItem(new MenuItem("HumanizerMinAttacks", "移动一次最少攻击数", true).SetValue(new Slider(2, 1, 10)));
            Config.SubMenu("杂项 设置").AddItem(new MenuItem("HumanizerMovementTime", "移动时间间隔(ms)", true).SetValue(new Slider(200, 0, 1000)));

            Config.AddSubMenu(new Menu("自动 眼位", "自动 眼位"));
            Config.SubMenu("自动 眼位").AddItem(new MenuItem("AutoWard", "启动", true).SetValue(true));
            Config.SubMenu("自动 眼位").AddItem(new MenuItem("AutoBuy", "lv9自动买灯泡", true).SetValue(true));
            Config.SubMenu("自动 眼位").AddItem(new MenuItem("AutoPink", "自动真眼扫描", true).SetValue(true));
            Config.SubMenu("自动 眼位").AddItem(new MenuItem("AutoWardCombo", "仅连招模式启动 ", true).SetValue(true));
            new YuLeLibrary.AutoWard().Load();
            new YuLeLibrary.Tracker().Load();

            Config.SubMenu("显示 设置").AddItem(new MenuItem("ComboInfo", "R killable info", true).SetValue(true));
            Config.SubMenu("显示 设置").AddItem(new MenuItem("qRange", "Q range", true).SetValue(false));
            Config.SubMenu("显示 设置").AddItem(new MenuItem("wRange", "W range", true).SetValue(false));
            Config.SubMenu("显示 设置").AddItem(new MenuItem("eRange", "E range", true).SetValue(false));
            Config.SubMenu("显示 设置").AddItem(new MenuItem("rRange", "R range", true).SetValue(true));

            _rand = new Random();

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.BeforeAttack += BeforeAttack;
            Orbwalking.AfterAttack += afterAttack;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Game.OnUpdate += eventargs => {
                YuLeLibrary.AutoWard.Enable = Config.Item("AutoWard", true).GetValue<bool>();
                YuLeLibrary.AutoWard.AutoBuy = Config.Item("AutoBuy", true).GetValue<bool>();
                YuLeLibrary.AutoWard.AutoPink = Config.Item("AutoPink", true).GetValue<bool>();
                YuLeLibrary.AutoWard.OnlyCombo = Config.Item("AutoWardCombo", true).GetValue<bool>();
                YuLeLibrary.AutoWard.InComboMode = Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo;

                if (HumanizerEnabled)
                {
                    if (_humanizer != null)
                    {
                        _attacksSoFar = 0;
                    }
                    else if (_attacksSoFar >= HumanizerMinAttacks)
                    {
                        _humanizer = new Humanizer(HumanizerMovementTime);
                    }
                    if (!IsWActive())
                    {
                        _humanizer = null;
                        _attacksSoFar = 0;
                    }
                    if (_humanizer != null && _humanizer.ShouldDestroy)
                    {
                        _humanizer = null;
                    }
                    Orbwalker.SetMovement(CanMove());
                    Orbwalker.SetAttack(CanAttack());
                }
                else
                {
                    _humanizer = null;
                    Orbwalker.SetMovement(true);
                    Orbwalker.SetAttack(true);
                }
            };         
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Config.Item("AGC", true).GetValue<bool>() && E.IsReady() && Player.Mana > RMANA + EMANA)
            {
                var Target = (Obj_AI_Hero)gapcloser.Sender;
                if (Target.IsValidTarget(E.Range))
                {
                    E.Cast(Target, true);
                }
            }
            return;
        }

        private bool HumanizerEnabled {
            get{
                return Config.Item("Humanizer", true).GetValue<bool>();
            }
        }

        private int HumanizerMinAttacks {
            get {
                return Config.Item("HumanizerMinAttacks", true).GetValue<Slider>().Value;
            }
        }

        private int HumanizerMovementTime {
            get {
                return Config.Item("HumanizerMovementTime", true).GetValue<Slider>().Value;
            }
        }

        private bool IsWActive() {
            return ObjectManager.Player.HasBuff("KogMawBioArcaneBarrage");
        }

        private float GetAttackRangeAfterWIsApplied() {
            return W.Level > 0 ? new[] { 630, 660, 690, 720, 750 }[W.Level - 1] : 540;
        }

        private bool CanAttack() {
            if (!HumanizerEnabled) return true;
            if (IsWActive())
            {
                return _humanizer == null;
            }
            return true;
        }

        private bool CanMove() {
            if (!HumanizerEnabled)
                return true;
            if (IsWActive() && ObjectManager.Player.AttackSpeedMod / 2 > _rand.Next(167, 230) / 100)
            {
                if ((Program.Combo && ObjectManager.Player.CountEnemiesInRange(GetAttackRangeAfterWIsApplied() - 25) < 1) || (!Program.None && !Program.Combo && (!MinionManager.GetMinions(1000).Any(m => m.IsHPBarRendered && m.Distance(ObjectManager.Player) < GetAttackRangeAfterWIsApplied() - 25) && !MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).Any(m => m.IsHPBarRendered && m.Distance(ObjectManager.Player) < GetAttackRangeAfterWIsApplied() - 25))))
                {
                    return true;
                }
                return _humanizer != null;
            }
            return true;
        }

        private void afterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe)
                return;
            attackNow = true;
        }

        private void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            attackNow = false;
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (Program.LagFree(0))
            {
                R.Range = 800 + 300 * Player.Spellbook.GetSpell(SpellSlot.R).Level;
                W.Range = 650 + 30 * Player.Spellbook.GetSpell(SpellSlot.W).Level;
                SetMana();

            }
            if (Program.LagFree(1) && E.IsReady() && !Player.IsWindingUp && Config.Item("autoE", true).GetValue<bool>())
                LogicE();

            if (Program.LagFree(2) && Q.IsReady() && !Player.IsWindingUp && Config.Item("autoQ", true).GetValue<bool>())
                LogicQ();

            if (Program.LagFree(3) && W.IsReady() && Config.Item("autoW", true).GetValue<bool>())
                LogicW();

            if (Program.LagFree(4) && R.IsReady() && !Player.IsWindingUp)
                LogicR();            
        }

        private void LogicR()
        {
            if (Config.Item("autoR", true).GetValue<bool>() && Sheen())
            {
                var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

                if (target.IsValidTarget(R.Range) && target.HealthPercent < Config.Item("RmaxHp", true).GetValue<Slider>().Value && Common.ValidUlt(target))
                {
                    

                    if (Config.Item("Raa", true).GetValue<bool>() && Orbwalking.InAutoAttackRange(target))
                        return;

                    var harasStack = Config.Item("harasStack", true).GetValue<Slider>().Value;
                    var comboStack = Config.Item("comboStack", true).GetValue<Slider>().Value;
                    var countR = GetRStacks();

                    var Rdmg = R.GetDamage(target);
                    Rdmg = Rdmg + target.CountAlliesInRange(500) * Rdmg;

                    if (R.GetDamage(target) > target.Health - Common.GetIncomingDamage(target))
                        Program.CastSpell(R, target);
                    else if (Program.Combo && Rdmg * 2 > target.Health && Player.Mana > RMANA * 3)
                        Program.CastSpell(R, target);
                    else if (countR < comboStack + 2 && Player.Mana > RMANA * 3)
                    {
                        foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(R.Range) && !Common.CanMove(enemy)))
                        {
                                R.Cast(enemy, true);
                        }
                    }

                    if (target.HasBuffOfType(BuffType.Slow) && Config.Item("Rslow", true).GetValue<bool>() && countR < comboStack + 1 && Player.Mana > RMANA + WMANA + EMANA + QMANA)
                        Program.CastSpell(R, target);
                    else if (Program.Combo && countR < comboStack && Player.Mana > RMANA + WMANA + EMANA + QMANA)
                        Program.CastSpell(R, target);
                    else if (Program.Farm && countR < harasStack && Player.Mana > RMANA + WMANA + EMANA + QMANA)
                        Program.CastSpell(R, target);
                }
            }
        }

        private void LogicW()
        {
            if (Player.CountEnemiesInRange(W.Range) > 0 && Sheen())
            {
                if (Program.Combo)
                    W.Cast();
                else if (Program.Farm && Config.Item("harasW", true).GetValue<bool>() && Player.CountEnemiesInRange(Player.AttackRange) > 0)
                    W.Cast();
            }
        }

        private void LogicQ()
        {
            if (Sheen())
            {
                var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget())
                {
                    var qDmg = Common.GetKsDamage(t, Q);
                    var eDmg = E.GetDamage(t);
                    if (t.IsValidTarget(W.Range) && qDmg + eDmg > t.Health)
                        Program.CastSpell(Q, t);
                    else if (Program.Combo && Player.Mana > RMANA + QMANA * 2 + EMANA)
                        Program.CastSpell(Q, t);
                    else if ((Program.Farm && Player.Mana > RMANA + EMANA + QMANA * 2 + WMANA) && Config.Item("harrasQ", true).GetValue<bool>() && !Player.UnderTurret(true))
                        Program.CastSpell(Q, t);
                    else if ((Program.Combo || Program.Farm) && Player.Mana > RMANA + QMANA + EMANA)
                    {
                        foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range) && !Common.CanMove(enemy)))
                             Q.Cast(enemy, true);

                    }
                }
            }
        }

        private void LogicE()
        {
            if ( Sheen())
            {
                var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget())
                {
                    var qDmg = Q.GetDamage(t);
                    var eDmg = Common.GetKsDamage(t, E);
                    if (eDmg > t.Health)
                        Program.CastSpell(E, t);
                    else if (eDmg + qDmg > t.Health && Q.IsReady())
                        Program.CastSpell(E, t);
                    else if (Program.Combo && ObjectManager.Player.Mana > RMANA + WMANA + EMANA + QMANA)
                        Program.CastSpell(E, t);
                    else if (Program.Farm && Config.Item("HarrasE", true).GetValue<bool>() && Player.Mana > RMANA + WMANA + EMANA + QMANA + EMANA)
                        Program.CastSpell(E, t);
                    else if ((Program.Combo || Program.Farm) && ObjectManager.Player.Mana > RMANA + WMANA + EMANA)
                    {
                        foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(E.Range) && !Common.CanMove(enemy)))
                                E.Cast(enemy, true);
                    }
                }
            }
        }

        private bool Sheen()
        {
            var target = Orbwalker.GetTarget();
            if (!(target is Obj_AI_Hero))
                attackNow = true;
            if (target.IsValidTarget() && Player.HasBuff("sheen") && Config.Item("sheen", true).GetValue<bool>() && target is Obj_AI_Hero)
            {
                return false;
            }
            else if (target.IsValidTarget() && Config.Item("AApriority", true).GetValue<bool>() && target is Obj_AI_Hero && !attackNow)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private int GetRStacks()
        {
            foreach (var buff in ObjectManager.Player.Buffs)
            {
                if (buff.Name == "kogmawlivingartillerycost")
                    return buff.Count;
            }
            return 0;
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

        private void drawText(string msg, Obj_AI_Hero Hero, System.Drawing.Color color)
        {
            var wts = Drawing.WorldToScreen(Hero.Position);
            Drawing.DrawText(wts[0] - (msg.Length) * 5, wts[1], color, msg);
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("ComboInfo", true).GetValue<bool>())
            {
                var combo = "haras";
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget()))
                {
                    if (R.GetDamage(enemy) > enemy.Health)
                    {
                        combo = "KILL R";
                        drawText(combo, enemy, System.Drawing.Color.GreenYellow);
                    }
                    else
                    {
                        combo = (int)(enemy.Health / R.GetDamage(enemy)) + " R";
                        drawText(combo, enemy, System.Drawing.Color.Red);
                    }
                }
            }
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
        }
    }
}
