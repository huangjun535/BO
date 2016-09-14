namespace YuLeOrianna
{
    using System;
    using System.Linq;
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;
    using YuLeLibrary;

    class Orianna
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Spell E, Q, R, W, QR;
        private float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;

        private Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        private Vector3 BallPos;
        private bool Rsmart = false;
        private Obj_AI_Hero best;

        public void Load()
        {
            Q = new Spell(SpellSlot.Q, 800);
            W = new Spell(SpellSlot.W, 210);
            E = new Spell(SpellSlot.E, 1095);
            R = new Spell(SpellSlot.R, 360);
            QR = new Spell(SpellSlot.Q, 825);

            Q.SetSkillshot(0.05f, 70f, 1150f, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(0.25f, 210f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 100f, 1700f, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.4f, 370f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            QR.SetSkillshot(0.5f, 400f, 100f, false, SkillshotType.SkillshotCircle);

            Config.SubMenu("Q技能设置").AddItem(new MenuItem("ComboHarass", "全自动百分百QW对面", true).SetValue(true));
            Config.SubMenu("Q技能设置").AddItem(new MenuItem("comboQ", "连招 Q", true).SetValue(true));
            Config.SubMenu("Q技能设置").AddItem(new MenuItem("harassQ", "骚扰 Q", true).SetValue(true));
            Config.SubMenu("Q技能设置").AddItem(new MenuItem("farmQ", "清线 Q", true).SetValue(true));
            Config.SubMenu("Q技能设置").AddItem(new MenuItem("farmQout", "仅Q不在攻击范围内的小兵", true).SetValue(true));
            Config.SubMenu("Q技能设置").AddItem(new MenuItem("ManaQ", "LaneClear Mana", true).SetValue(new Slider(60, 100, 0)));
            Config.SubMenu("Q技能设置").AddItem(new MenuItem("LCminionsQ", "LaneClear minimum minions", true).SetValue(new Slider(2, 10, 0)));
            Config.SubMenu("Q技能设置").AddItem(new MenuItem("farmQJC", "清野 Q", true).SetValue(true));
            Config.SubMenu("Q技能设置").AddItem(new MenuItem("ManaQJC", "清野最低蓝量", true).SetValue(new Slider(20, 100, 0)));

            Config.SubMenu("W技能设置").AddItem(new MenuItem("W", "出门自动W加速", true).SetValue(false));
            Config.SubMenu("W技能设置").AddItem(new MenuItem("autoW", "自动 W", true).SetValue(true));
            Config.SubMenu("W技能设置").AddItem(new MenuItem("harassW", "骚扰 W 最低蓝量", true).SetValue(new Slider(60, 100, 0)));
            Config.SubMenu("W技能设置").AddItem(new MenuItem("farmW", "LaneClear W", true).SetValue(true));
            Config.SubMenu("W技能设置").AddItem(new MenuItem("ManaW", "LaneClear Mana", true).SetValue(new Slider(60, 100, 0)));
            Config.SubMenu("W技能设置").AddItem(new MenuItem("farmWJC", "清野 W", true).SetValue(true));
            Config.SubMenu("W技能设置").AddItem(new MenuItem("ManaWJC", "清野最低蓝量", true).SetValue(new Slider(20, 100, 0)));

            Config.SubMenu("E技能设置").AddItem(new MenuItem("autoW", "连招自动E", true).SetValue(true));
            Config.SubMenu("E技能设置").AddItem(new MenuItem("hadrCC", "被控制自动E", true).SetValue(true));
            Config.SubMenu("E技能设置").AddItem(new MenuItem("poison", "自身有负面BUFF自动E", true).SetValue(true));
            Config.SubMenu("E技能设置").AddItem(new MenuItem("Wdmg", "E dmg % hp", true).SetValue(new Slider(10, 100, 0)));
            Config.SubMenu("E技能设置").AddItem(new MenuItem("farmE", "LaneClear E", true).SetValue(false));
            Config.SubMenu("E技能设置").AddItem(new MenuItem("ManaE", "LaneClear Mana", true).SetValue(new Slider(60, 100, 0)));
            Config.SubMenu("E技能设置").AddItem(new MenuItem("farmEJC", "清野 E", true).SetValue(true));
            Config.SubMenu("E技能设置").AddItem(new MenuItem("ManaEJC", "清野最低蓝量", true).SetValue(new Slider(60, 100, 0)));
            Config.SubMenu("E技能设置").AddItem(new MenuItem("AGC", "反突进 E", true).SetValue(true));

            Config.SubMenu("R技能设置").AddItem(new MenuItem("rCount", "X个敌人自动R", true).SetValue(new Slider(3, 0, 5)));
            Config.SubMenu("R技能设置").AddItem(new MenuItem("smartR", "Semi-manual cast R key", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("R技能设置").AddItem(new MenuItem("OPTI", "自动R打断技能", true).SetValue(true));
            Config.SubMenu("R技能设置").AddItem(new MenuItem("Rturrent", "自动R拉敌人回塔下", true).SetValue(true));
            Config.SubMenu("R技能设置").AddItem(new MenuItem("Rks", "自动R抢人头", true).SetValue(true));
            Config.SubMenu("R技能设置").AddItem(new MenuItem("Rlifesaver", "自动R保命", true).SetValue(true));
            Config.SubMenu("R技能设置").AddItem(new MenuItem("Rblock", "防止空大", true).SetValue(true));
            Config.SubMenu("R技能设置").AddItem(new MenuItem("Ralwaystarget", "看到以下打钩的敌人直接开R", true));
            foreach (var enemy in HeroManager.Enemies)
                Config.SubMenu("R技能设置").AddItem(new MenuItem("Ralways" + enemy.ChampionName, enemy.ChampionName,true).SetValue(false));

            Config.SubMenu("逃跑设置").AddItem(new MenuItem("Keys", "逃跑按键", true).SetValue(new KeyBind('Z', KeyBindType.Press)));
            Config.SubMenu("逃跑设置").AddItem(new MenuItem("FleeW", "使用 W", true).SetValue(true));
            Config.SubMenu("逃跑设置").AddItem(new MenuItem("FleeE", "使用 E", true).SetValue(true));

            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWard", "启动", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoBuy", "lv9自动买灯泡", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoPink", "自动真眼扫描", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWardCombo", "仅连招模式启动 ", true).SetValue(true));
            new AutoWard().Load();
            new Tracker().Load();

            var SkinMenu = Config.AddSubMenu(new Menu("换肤设置", "换肤设置"));
            {
                SkinMenu.AddItem(new MenuItem("EnableSkin", "启动换肤").SetValue(false));
                SkinMenu.AddItem(new MenuItem("SkinSelect", "选择皮肤").SetValue(new StringList(new[] { "经典", "哥特女仆", "木偶奇遇记", "灵骨工匠", "暗杀星", "冬季仙境", "觅心魔灵" })));
            }

            Config.SubMenu("显示设置").AddItem(new MenuItem("qRange", "Q 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("wRange", "W 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("eRange", "E 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("rRange", "R 范围", true).SetValue(false));

            Game.OnUpdate += Game_OnGameUpdate;
            GameObject.OnCreate += Obj_AI_Base_OnCreate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget +=Interrupter2_OnInterruptableTarget;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!Config.Item("OPTI", true).GetValue<bool>())
                return;

            if (R.IsReady() && sender.Distance(BallPos) < R.Range)
            {
                R.Cast();
            }
            else if (Q.IsReady() && Player.Mana > RMANA + QMANA && sender.IsValidTarget(Q.Range))
                Q.Cast(sender.ServerPosition);
        }


        private void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.R && Config.Item("Rblock", true).GetValue<bool>() &&  CountEnemiesInRangeDeley(BallPos, R.Width, R.Delay) == 0)
                args.Process = false;
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var Target = gapcloser.Sender;
            if (Config.Item("AGC", true).GetValue<bool>() && E.IsReady() && Target.IsValidTarget(800) && Player.Mana > RMANA + EMANA)
                E.CastOnUnit(Player);
            return;
        }
        
        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.HasBuff("Recall") || Player.IsDead)
                return;

            if (Config.Item("Keys", true).GetValue<KeyBind>().Active)
            {
                FleeLogic();
            }

            if (R.IsReady())
                LogicR();

            bool hadrCC = true, poison = true;

            if (Program.LagFree(0))
            {
                SetMana();
                hadrCC = Config.Item("hadrCC", true).GetValue<bool>();
                poison = Config.Item("poison", true).GetValue<bool>();
            }

            best = Player;

            foreach (var ally in HeroManager.Allies.Where(ally => ally.IsValid && !ally.IsDead))
            {
                if (ally.HasBuff("orianaghostself") || ally.HasBuff("orianaghost"))
                    BallPos = ally.ServerPosition;

                if (Program.LagFree(3) )
                {
                    if (E.IsReady() && Player.Mana > RMANA + EMANA && ally.Distance(Player.Position) < E.Range)
                    {
                        var countEnemy = ally.CountEnemiesInRange(800);
                        if (ally.Health < countEnemy * ally.Level * 25)
                        {
                            E.CastOnUnit(ally);
                        }
                        else if (HardCC(ally) && hadrCC && countEnemy > 0)
                        {
                            E.CastOnUnit(ally);
                        }
                        else if (ally.HasBuffOfType(BuffType.Poison))
                        {
                            E.CastOnUnit(ally);
                        }
                    }
                    if (W.IsReady() && Player.Mana > RMANA + WMANA && BallPos.Distance(ally.ServerPosition) < 240 && ally.Health < ally.CountEnemiesInRange(600) * ally.Level * 20)
                        W.Cast();

                    if ((ally.Health < best.Health || ally.CountEnemiesInRange(300) > 0) && ally.Distance(Player.Position) < E.Range && ally.CountEnemiesInRange(700) > 0)
                        best = ally;
                }
                if (Program.LagFree(1) && E.IsReady() && Player.Mana > RMANA + EMANA && ally.Distance(Player.Position) < E.Range && ally.CountEnemiesInRange(R.Width) >= Config.Item("rCount", true).GetValue<Slider>().Value && 0 != Config.Item("rCount", true).GetValue<Slider>().Value)
                {
                    E.CastOnUnit(ally);
                }
            }

            if ((Config.Item("smartR", true).GetValue<KeyBind>().Active || Rsmart) && R.IsReady())
            {
                Rsmart = true;
                var target = TargetSelector.GetTarget(Q.Range + 100, TargetSelector.DamageType.Magical);
                if (target.IsValidTarget())
                {
                    if (CountEnemiesInRangeDeley(BallPos, R.Width, R.Delay) > 1)
                        R.Cast();
                    else if (Q.IsReady())
                        QR.Cast(target, true, true);
                    else if (CountEnemiesInRangeDeley(BallPos, R.Width, R.Delay) > 0)
                        R.Cast();
                }
                else
                    Rsmart = false;
            }
            else
                Rsmart = false;

            if (Program.LagFree(1))
            {
                LogicQ();
            }

            if (Program.LagFree(2) && W.IsReady())
                LogicW();

            if (Program.LagFree(4) && E.IsReady())
                LogicE(best);

            LogicFarm();
        }

        private void FleeLogic()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            if (Config.Item("FleeW", true).GetValue<bool>() && W.IsReady())
            {
                if (BallPos.Distance(Player.Position) <= 200)
                {
                    W.Cast();
                }
            }

            if (Config.Item("FleeE", true).GetValue<bool>() && E.IsReady())
            {
                if (BallPos.Distance(Player.Position) > 300)
                {
                    E.CastOnUnit(Player);
                }
            }
        }

        private void LogicE(Obj_AI_Hero best)
        {
            var ta = TargetSelector.GetTarget(1300, TargetSelector.DamageType.Magical);

            if (Program.Combo && ta.IsValidTarget() && !W.IsReady() && Player.Mana > RMANA + EMANA)
            {
                if (CountEnemiesInRangeDeley(BallPos, 100, 0.1f) > 0)
                    E.CastOnUnit(best);
                var castArea = ta.Distance(best.ServerPosition) * (best.ServerPosition - ta.ServerPosition).Normalized() + ta.ServerPosition;
                if (castArea.Distance(ta.ServerPosition) < ta.BoundingRadius / 2)
                    E.CastOnUnit(best);
            }
        }

        private void LogicR()
        {            
            foreach (var t in HeroManager.Enemies.Where(t => t.IsValidTarget() && BallPos.Distance(Prediction.GetPrediction(t, R.Delay).CastPosition) < R.Width && BallPos.Distance(t.ServerPosition) < R.Width))
            {
                if (Program.Combo && Config.Item("Ralways" + t.ChampionName, true).GetValue<bool>())
                {
                    R.Cast();
                }

                if (Config.Item("Rks", true).GetValue<bool>())
                {
                    var comboDmg = Common.GetKsDamage(t, R);

                    if (t.IsValidTarget(Q.Range))
                        comboDmg += Q.GetDamage(t);
                    if (W.IsReady())
                        comboDmg += W.GetDamage(t);
                    if (Orbwalker.InAutoAttackRange(t))
                        comboDmg += (float)Player.GetAutoAttackDamage(t) * 2;
                    if (t.Health < comboDmg)
                        R.Cast();
                }
                if (Config.Item("Rturrent", true).GetValue<bool>() && BallPos.UnderTurret(false) && !BallPos.UnderTurret(true))
                {
                    R.Cast();
                }
                if (Config.Item("Rlifesaver", true).GetValue<bool>() && Player.Health < Player.CountEnemiesInRange(800) * Player.Level * 20 && Player.Distance(BallPos) > t.Distance(Player.Position))
                {
                    R.Cast();
                }
            }

            int countEnemies=CountEnemiesInRangeDeley(BallPos, R.Width, R.Delay);

            if (countEnemies >= Config.Item("rCount", true).GetValue<Slider>().Value && BallPos.CountEnemiesInRange(R.Width) == countEnemies)
                R.Cast();
        }

        private void LogicW()
        {
            foreach (var t in HeroManager.Enemies.Where(t => t.IsValidTarget() && BallPos.Distance(t.ServerPosition) < 250 && t.Health < W.GetDamage(t)))
            {
                W.Cast();
                return;
            }
            if (CountEnemiesInRangeDeley(BallPos, W.Width, 0f) > 0 && Player.Mana > RMANA + WMANA)
            {
                if (!Config.Item("autoW", true).GetValue<bool>() && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None)
                {
                    return;
                }

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && Player.Mana < Config.Item("harassW", true).GetValue<Slider>().Value)
                {
                    return;
                }

                W.Cast();
                return;
            }
            if (Config.Item("W", true).GetValue<bool>() && !Program.Farm && !Program.Combo && ObjectManager.Player.Mana > Player.MaxMana * 0.95 && Player.HasBuff("orianaghostself"))
                W.Cast();
        }

        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (t.IsValidTarget() && Q.IsReady())
            {
                if (Q.GetDamage(t) + W.GetDamage(t) > t.Health)
                    CastQ(t);
                else if (Program.Combo && Player.Mana > RMANA + QMANA - 10 && Config.Item("comboQ", true).GetValue<bool>())
                    CastQ(t);
                else if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && Player.Mana > RMANA + QMANA + WMANA + EMANA && Config.Item("harassQ", true).GetValue<bool>())
                    CastQ(t);
            }
            if (Config.Item("W", true).GetValue<bool>() && !t.IsValidTarget() && Program.Combo && Player.Mana > RMANA + 3 * QMANA + WMANA + EMANA + WMANA)
            {
                if (W.IsReady() && Player.HasBuff("orianaghostself"))
                {
                    W.Cast();
                }
                else if (E.IsReady() && !Player.HasBuff("orianaghostself"))
                {
                    E.CastOnUnit(Player);
                }
            }
        }

        private void LogicFarm()
        {
            if (!Program.Farm)
                return;

            var allMinions = Cache.GetMinions(Player.ServerPosition, Q.Range);
            if (Config.Item("farmQout", true).GetValue<bool>() && Player.Mana > RMANA + QMANA + WMANA + EMANA)
            {
                foreach (var minion in allMinions.Where(minion => minion.IsValidTarget(Q.Range) && !Orbwalker.InAutoAttackRange(minion) && minion.Health < Q.GetDamage(minion) && minion.Health > minion.FlatPhysicalDamageMod))
                {
                    Q.Cast(minion);
                }
            }

            if (!Program.LaneClear || Player.Mana < RMANA + QMANA)
                return;

            LaneClear();
            JungleClear();
        }

        private void LaneClear()
        {
            var allMinions = Cache.GetMinions(Player.ServerPosition, Q.Range);

            var Qfarm = Q.GetCircularFarmLocation(allMinions, 100);
            var QWfarm = Q.GetCircularFarmLocation(allMinions, W.Width);

            if (Qfarm.MinionsHit + QWfarm.MinionsHit == 0)
                return;

            if (Config.Item("farmQ", true).GetValue<bool>() && Player.ManaPercent > Config.Item("ManaQ", true).GetValue<Slider>().Value)
            {
                if (Qfarm.MinionsHit > Config.Item("LCminionsQ", true).GetValue<Slider>().Value && !W.IsReady() && Q.IsReady())
                {
                    Q.Cast(Qfarm.Position);
                }
                else if (QWfarm.MinionsHit > 2 && Q.IsReady())
                    Q.Cast(QWfarm.Position);
            }

            foreach (var minion in allMinions)
            {
                if (W.IsReady() && minion.Distance(BallPos) < W.Range && minion.Health < W.GetDamage(minion) && Config.Item("farmW", true).GetValue<bool>() && Player.ManaPercent > Config.Item("ManaW", true).GetValue<Slider>().Value)
                    W.Cast();
                if (!W.IsReady() && E.IsReady() && minion.Distance(BallPos) < E.Width && Config.Item("farmE", true).GetValue<bool>() && Player.ManaPercent > Config.Item("ManaE", true).GetValue<Slider>().Value)
                    E.CastOnUnit(Player);
            }
        }

        private void JungleClear()
        {
            var mobs = Cache.GetMinions(Player.ServerPosition, 800, MinionTeam.Neutral);
            if (mobs.Count > 0)
            {
                var mob = mobs[0];
                if (Config.Item("farmQJC", true).GetValue<bool>() && Q.IsReady() && Player.ManaPercent > Config.Item("ManaQJC", true).GetValue<Slider>().Value)
                    Q.Cast(mob.Position);
                if (Config.Item("farmWJC", true).GetValue<bool>() && W.IsReady() && BallPos.Distance(mob.Position) < W.Width && Player.ManaPercent > Config.Item("ManaWJC", true).GetValue<Slider>().Value)
                    W.Cast();
                else if (Config.Item("farmEJC", true).GetValue<bool>() && E.IsReady() && Player.ManaPercent > Config.Item("ManaEJC", true).GetValue<Slider>().Value)
                    E.CastOnUnit(best);
                return;
            }
        }

        private void CastQ(Obj_AI_Hero target)
        {
            float distance = Vector3.Distance(BallPos, target.ServerPosition);
            

            if (E.IsReady() && Player.Mana > RMANA + QMANA + WMANA + EMANA && distance > Player.Distance(target.ServerPosition) + 300)
            {
                E.CastOnUnit(Player);
                return;
            }

            var predInput2 = new YuLeLibarary.Prediction.PredictionInput
            {
                Aoe = true,
                Collision = Q.Collision,
                Speed = Q.Speed,
                Delay = Q.Delay,
                Range = float.MaxValue,
                From = BallPos,
                Radius = Q.Width,
                Unit = target,
                Type = YuLeLibarary.Prediction.SkillshotType.SkillshotCircle
            };
            var prepos5 = YuLeLibarary.Prediction.Prediction.GetPrediction(predInput2);

            if ((int)prepos5.Hitchance > 5)
            {
                if (prepos5.CastPosition.Distance(prepos5.CastPosition) < Q.Range)
                {
                    Q.Cast(prepos5.CastPosition);
                }
            }
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {

            if (sender.IsMe && args.SData.Name == "OrianaIzunaCommand")
                BallPos = args.End;

             if (!E.IsReady() || !sender.IsEnemy || !Config.Item("autoW", true).GetValue<bool>() || Player.Mana < EMANA + RMANA || sender.Distance(Player.Position) > 1600)
                return;

            foreach (var ally in HeroManager.Allies.Where(ally => ally.IsValid && !ally.IsDead && Player.Distance(ally.ServerPosition) < E.Range))
            {
                double dmg = 0;
                if (args.Target != null && args.Target.NetworkId == ally.NetworkId)
                {
                    dmg = dmg + sender.GetSpellDamage(ally, args.SData.Name);
                }
                else
                {
                    var castArea = ally.Distance(args.End) * (args.End - ally.ServerPosition).Normalized() + ally.ServerPosition;
                    if (castArea.Distance(ally.ServerPosition) < ally.BoundingRadius / 2)
                        dmg = dmg + sender.GetSpellDamage(ally, args.SData.Name);
                    else
                        continue;
                }

                double HpLeft = ally.Health - dmg;
                double HpPercentage = (dmg * 100) / ally.Health;
                double shieldValue = 60 + E.Level * 40 + 0.4 * Player.FlatMagicDamageMod;

                if (HpPercentage >= Config.Item("Wdmg", true).GetValue<Slider>().Value || dmg > shieldValue)
                    E.CastOnUnit(ally);
            }   
        }

        private int CountEnemiesInRangeDeley(Vector3 position, float range, float delay)
        {
            int count = 0;
            foreach (var t in HeroManager.Enemies.Where(t => t.IsValidTarget()))
            {
                Vector3 prepos = Prediction.GetPrediction(t, delay).CastPosition;
                if (position.Distance(prepos) < range)
                    count++;
            }
            return count;
        }

        private void Obj_AI_Base_OnCreate(GameObject obj, EventArgs args)
        {
            if (obj.IsValid && obj.IsAlly && obj.Name == "TheDoomBall")
            {
                BallPos = obj.Position;
            }
        }

        private bool HardCC(Obj_AI_Hero target)
        {
            if (target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Knockup) ||
                target.HasBuffOfType(BuffType.Charm) || target.HasBuffOfType(BuffType.Fear) || target.HasBuffOfType(BuffType.Knockback) ||
                target.HasBuffOfType(BuffType.Taunt) || target.HasBuffOfType(BuffType.Suppression) ||
                target.IsStunned )
            {
                return true;

            }
            else
                return false;
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
            if (BallPos.IsValid())
            {
                if (Config.Item("wRange", true).GetValue<bool>())
                {
                    if (W.IsReady())
                        Utility.DrawCircle(BallPos, W.Range, System.Drawing.Color.Orange, 1, 1);
                }

                if (Config.Item("rRange", true).GetValue<bool>())
                {
                    if (R.IsReady())
                        Utility.DrawCircle(BallPos, R.Range, System.Drawing.Color.Gray, 1, 1);
                }
            }

            if (Config.Item("qRange", true).GetValue<bool>())
            {
                if (Q.IsReady())
                    Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan, 1, 1);
            }
            
            if (Config.Item("eRange", true).GetValue<bool>())
            {
                if (E.IsReady())
                    Utility.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Yellow, 1, 1);
            }
        }
    }
}
